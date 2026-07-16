"""
RAG (Retrieval-Augmented Generation) service.

Orchestrates:
  1. Content extraction (from video/pdf/article)
  2. Text chunking
  3. Indexing into ChromaDB
  4. Retrieval + Groq AI answer generation
"""
import logging
import re
from models.schemas import LessonType, ChatMessage
from services import vector_store
from services import groq_service

logger = logging.getLogger(__name__)

# Chunk configuration
CHUNK_SIZE = 800       # characters per chunk
CHUNK_OVERLAP = 100    # overlap between consecutive chunks


# ────────────────────────────────────────────────────────────────
# Text Chunking
# ────────────────────────────────────────────────────────────────

def chunk_text(text: str, chunk_size: int = CHUNK_SIZE, overlap: int = CHUNK_OVERLAP) -> list[str]:
    """
    Split text into overlapping chunks.
    Tries to split on sentence boundaries first, then falls back to character slicing.
    """
    if not text or not text.strip():
        return []

    # Split on sentence boundaries
    sentences = re.split(r'(?<=[.!?])\s+', text)
    chunks = []
    current = ""

    for sentence in sentences:
        if len(current) + len(sentence) + 1 <= chunk_size:
            current = current + " " + sentence if current else sentence
        else:
            if current:
                chunks.append(current.strip())
                # Start next chunk with overlap
                words = current.split()
                overlap_words = words[-max(1, int(len(words) * overlap / chunk_size)):]
                current = " ".join(overlap_words) + " " + sentence
            else:
                # Single sentence longer than chunk_size — hard split
                for i in range(0, len(sentence), chunk_size - overlap):
                    chunks.append(sentence[i:i + chunk_size].strip())
                current = ""

    if current.strip():
        chunks.append(current.strip())

    # Filter out very short chunks
    chunks = [c for c in chunks if len(c) > 50]
    logger.info("Chunked text into %d chunks", len(chunks))
    return chunks


# ────────────────────────────────────────────────────────────────
# Indexing
# ────────────────────────────────────────────────────────────────

def index_lesson_text(lesson_id: int, text: str) -> int:
    """
    Chunk and index lesson text into ChromaDB.

    Returns:
        Number of chunks indexed.
    """
    chunks = chunk_text(text)
    if not chunks:
        logger.warning("No indexable content for lesson %d", lesson_id)
        return 0
    return vector_store.index_chunks(lesson_id, chunks)


# ────────────────────────────────────────────────────────────────
# RAG Query
# ────────────────────────────────────────────────────────────────

TUTOR_SYSTEM_PROMPT = """You are an expert AI tutor for an online learning platform.
You help students understand course lesson content clearly and thoroughly.

CRITICAL RULES:
1. You MUST ONLY answer questions that are directly related to the provided Lesson Context.
2. Do NOT use outside knowledge. If a user asks to combine the lesson with an outside technology (e.g., asking for React code in an HTML lesson), you MUST refuse.
3. If the user asks about an unrelated topic, reply with exactly one short sentence politely declining to answer (e.g., "I can only help with questions related to this lesson's content."). Do not explain why or offer alternatives.
4. Keep your responses extremely minimal and concise. Do not write long paragraphs. Answer the question as briefly as possible.
5. If the context does not contain enough information to answer a relevant question, say so honestly.
6. Format your answer in markdown when it improves readability (lists, code blocks).
7. Be encouraging but brief.

Lesson Context:
{context}
"""


def answer_question(
    lesson_id: int,
    question: str,
    history: list[ChatMessage] | None = None
) -> str:
    """
    RAG pipeline: retrieve relevant chunks → build context → call Groq AI.

    Returns:
        AI-generated answer string, or a 'not indexed yet' message.
    """
    if not vector_store.collection_exists(lesson_id):
        return (
            "⏳ The AI tutor is still processing this lesson's content. "
            "Please try again in a few minutes."
        )

    chunks = vector_store.query_chunks(lesson_id, question)
    if not chunks:
        return (
            "I don't have enough context from this lesson to answer that question. "
            "Please try rephrasing or ask a more specific question about the lesson content."
        )

    context = "\n\n---\n\n".join(chunks)
    system_prompt = TUTOR_SYSTEM_PROMPT.format(context=context)

    answer = groq_service.chat_completion(
        system_prompt=system_prompt,
        user_message=question,
        history=history,
        temperature=0.3,
        max_tokens=600
    )
    return answer
