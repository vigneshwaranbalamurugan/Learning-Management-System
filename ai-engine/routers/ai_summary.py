"""
AI Summary router.
Internal endpoint called only by the .NET LMSApi backend.

Routes:
  POST /internal/summary/generate   — generate summary + key points + notes
"""
import json
import logging
from fastapi import APIRouter, HTTPException
from models.schemas import (
    GenerateSummaryRequest, GenerateSummaryResponse,
    LessonType
)
from services import transcription as transcription_svc
from services import pdf_extractor
from services.article_parser import extract_text_from_article
from services import groq_service

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/internal", tags=["internal-summary"])


SUMMARY_SYSTEM_PROMPT = """You are an expert educational content summariser.
Given the full transcript or text of an online course lesson, produce a structured summary.

Respond ONLY with valid JSON in this exact format (no markdown, no explanation outside JSON):
{
  "summary": "A clear 3-5 sentence paragraph summarising the lesson.",
  "key_points": [
    "Key point 1",
    "Key point 2",
    "Key point 3",
    "Key point 4",
    "Key point 5"
  ],
  "notes": "Helpful student notes: important terms, formulas, or tips worth remembering from this lesson."
}

Rules:
- summary: 3-5 concise sentences covering the main topic and what students will learn.
- key_points: 4-7 bullet points, each a single clear sentence.
- notes: 2-4 sentences of study tips, important definitions, or things to remember.
- Write in simple, student-friendly language.
"""


async def _extract_text(req: GenerateSummaryRequest) -> str:
    """Extract lesson text based on lesson type."""
    if req.lesson_type == LessonType.Video:
        if not req.content_url:
            raise HTTPException(400, "content_url required for Video lessons")
        return await transcription_svc.transcribe_video_from_url(req.content_url)

    elif req.lesson_type == LessonType.Pdf:
        if not req.content_url:
            raise HTTPException(400, "content_url required for PDF lessons")
        return await pdf_extractor.extract_text_from_pdf_url(req.content_url)

    elif req.lesson_type == LessonType.Article:
        if not req.content_text:
            raise HTTPException(400, "content_text required for Article lessons")
        return extract_text_from_article(req.content_text)

    raise HTTPException(400, f"Lesson type {req.lesson_type.name} does not support AI summary")


@router.post("/summary/generate", response_model=GenerateSummaryResponse)
async def generate_summary(req: GenerateSummaryRequest):
    """
    Generate a structured AI summary for a lesson.
    Called by Hangfire background job in the .NET backend.
    """
    logger.info("Generating summary for lesson %d (type=%s)", req.lesson_id, req.lesson_type.name)

    try:
        text = await _extract_text(req)

        if not text.strip():
            return GenerateSummaryResponse(
                lesson_id=req.lesson_id,
                summary="No content available to summarise.",
                key_points=[],
                notes="",
                status="error"
            )

        # Truncate very long texts to avoid exceeding context window
        # ~50k chars ≈ ~12k tokens, well within llama3 70B 128k context
        truncated_text = text[:50_000]

        raw_response = groq_service.chat_completion(
            system_prompt=SUMMARY_SYSTEM_PROMPT,
            user_message=f"Here is the lesson content:\n\n{truncated_text}",
            temperature=0.3,
            max_tokens=1500,
            require_json=True
        )

        # Parse JSON response
        parsed = _parse_summary_json(raw_response, req.lesson_id)
        return GenerateSummaryResponse(
            lesson_id=req.lesson_id,
            summary=parsed.get("summary", ""),
            key_points=parsed.get("key_points", []),
            notes=parsed.get("notes", ""),
            status="generated"
        )

    except HTTPException:
        raise
    except Exception as e:
        logger.error("Summary generation failed for lesson %d: %s", req.lesson_id, e, exc_info=True)
        return GenerateSummaryResponse(
            lesson_id=req.lesson_id,
            summary="",
            key_points=[],
            notes="",
            status="error"
        )


def _parse_summary_json(raw: str, lesson_id: int) -> dict:
    """
    Parse the Groq JSON response. Handles cases where the model wraps JSON
    in a markdown code block.
    """
    # Strip possible markdown code fences
    raw = raw.strip()
    if raw.startswith("```"):
        lines = raw.splitlines()
        raw = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])

    try:
        return json.loads(raw)
    except json.JSONDecodeError as e:
        logger.error("Failed to parse summary JSON for lesson %d: %s | raw: %s", lesson_id, e, raw[:300])
        # Fallback: return raw text as summary
        return {
            "summary": raw[:500],
            "key_points": [],
            "notes": ""
        }
