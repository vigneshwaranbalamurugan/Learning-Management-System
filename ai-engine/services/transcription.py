"""
Video transcription service.

Pipeline:
  1. Download video bytes from Azure Blob SAS URL.
  2. Write to a temp file.
  3. Use ffmpeg to extract audio as mono 16kHz MP3 (drastically reduces file size).
  4. If the resulting audio is ≤ WHISPER_MAX_BYTES, send directly to Groq Whisper.
  5. If it is larger, split into chunks and transcribe each chunk, then join.
"""
import os
import tempfile
import subprocess
import logging
import math
from pathlib import Path
from groq import Groq

from services.azure_fetcher import fetch_bytes_from_sas

logger = logging.getLogger(__name__)

WHISPER_MAX_BYTES = int(os.getenv("WHISPER_MAX_BYTES", str(24 * 1024 * 1024)))  # 24 MB
GROQ_WHISPER_MODEL = os.getenv("GROQ_WHISPER_MODEL", "whisper-large-v3")


def _get_groq_client() -> Groq:
    api_key = os.getenv("GROQ_API_KEY")
    if not api_key:
        raise RuntimeError("GROQ_API_KEY is not set.")
    return Groq(api_key=api_key)


async def transcribe_video_from_url(sas_url: str) -> str:
    """
    Download a video from a SAS URL, extract audio with ffmpeg,
    then transcribe with Groq Whisper.

    Returns:
        Full transcript as a plain string.
    """
    logger.info("Starting transcription for SAS URL: %s...", sas_url[:80])
    video_bytes = await fetch_bytes_from_sas(sas_url, timeout=180.0)

    with tempfile.TemporaryDirectory() as tmp_dir:
        video_path = Path(tmp_dir) / "input_video"
        video_path.write_bytes(video_bytes)

        audio_path = Path(tmp_dir) / "audio.mp3"
        _extract_audio(str(video_path), str(audio_path))

        audio_size = audio_path.stat().st_size
        logger.info("Extracted audio size: %.2f MB", audio_size / 1024 / 1024)

        if audio_size <= WHISPER_MAX_BYTES:
            transcript = _transcribe_file(str(audio_path))
        else:
            transcript = _transcribe_chunked(str(audio_path), audio_size, tmp_dir)

    logger.info("Transcription complete: %d chars", len(transcript))
    return transcript


def _extract_audio(video_path: str, audio_out: str) -> None:
    """Use ffmpeg to extract mono 16kHz MP3 audio from a video file."""
    cmd = [
        "ffmpeg", "-y",
        "-i", video_path,
        "-vn",                   # no video
        "-ac", "1",              # mono
        "-ar", "16000",          # 16kHz sample rate
        "-b:a", "32k",           # 32kbps bitrate — small but clear for speech
        audio_out
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        logger.error("ffmpeg error: %s", result.stderr)
        raise RuntimeError(f"ffmpeg audio extraction failed: {result.stderr[:500]}")
    logger.info("ffmpeg audio extraction succeeded.")


def _transcribe_file(audio_path: str) -> str:
    """Send a single audio file to Groq Whisper and return the transcript."""
    client = _get_groq_client()
    with open(audio_path, "rb") as f:
        result = client.audio.transcriptions.create(
            model=GROQ_WHISPER_MODEL,
            file=f,
            response_format="text"
        )
    return str(result)


def _transcribe_chunked(audio_path: str, audio_size: int, tmp_dir: str) -> str:
    """
    Split the audio into ≤ WHISPER_MAX_BYTES segments using ffmpeg segment,
    transcribe each, and concatenate.
    """
    # Estimate a safe chunk duration (seconds) based on file size and a 128kbps assumption
    # We use 32kbps, so 1 second ≈ 4000 bytes → chunk at WHISPER_MAX_BYTES / 4000 seconds
    chunk_duration = int(WHISPER_MAX_BYTES / 4000)
    chunk_duration = max(60, min(chunk_duration, 600))  # clamp between 60s and 600s

    chunk_pattern = str(Path(tmp_dir) / "chunk_%03d.mp3")
    cmd = [
        "ffmpeg", "-y",
        "-i", audio_path,
        "-f", "segment",
        "-segment_time", str(chunk_duration),
        "-c", "copy",
        chunk_pattern
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError(f"ffmpeg chunking failed: {result.stderr[:500]}")

    chunk_files = sorted(Path(tmp_dir).glob("chunk_*.mp3"))
    logger.info("Transcribing %d audio chunks...", len(chunk_files))

    transcripts = []
    for chunk_file in chunk_files:
        transcripts.append(_transcribe_file(str(chunk_file)))

    return " ".join(transcripts)
