"""
AI Tutor router.
Internal endpoints called only by the .NET LMSApi backend.

Routes:
  POST /internal/tutor/chat         — answer a student question (RAG)
  POST /internal/index/lesson       — index lesson content into ChromaDB
"""
import logging
from fastapi import APIRouter, HTTPException, Request, Depends
from models.schemas import (
    TutorChatRequest, TutorChatResponse,
    IndexLessonRequest, IndexLessonResponse,
    LessonType
)
from services import rag_service
from services import transcription as transcription_svc
from services import pdf_extractor
from services.article_parser import extract_text_from_article

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/internal", tags=["internal-tutor"])


async def _extract_text(req: IndexLessonRequest | TutorChatRequest) -> str:
    """
    Extract plain text from lesson content based on its type.
    Dispatches to the appropriate service.
    """
    lesson_type = req.lesson_type if hasattr(req, "lesson_type") else None

    if lesson_type == LessonType.Video:
        if not req.content_url:
            raise HTTPException(400, "content_url is required for Video lessons")
        return await transcription_svc.transcribe_video_from_url(req.content_url)

    elif lesson_type == LessonType.Pdf:
        if not req.content_url:
            raise HTTPException(400, "content_url is required for PDF lessons")
        return await pdf_extractor.extract_text_from_pdf_url(req.content_url)

    elif lesson_type == LessonType.Article:
        if not req.content_text:
            raise HTTPException(400, "content_text is required for Article lessons")
        return extract_text_from_article(req.content_text)

    elif lesson_type == LessonType.ExternalLink:
        raise HTTPException(400, "ExternalLink lessons are not supported for AI features")

    raise HTTPException(400, f"Unknown lesson type: {lesson_type}")


# ── Index Lesson ──────────────────────────────────────────────────

@router.post("/index/lesson", response_model=IndexLessonResponse)
async def index_lesson(req: IndexLessonRequest):
    """
    Extract text from a lesson and index it into ChromaDB.
    Called by Hangfire background job in the .NET backend after lesson create/update.
    """
    logger.info("Indexing lesson %d (type=%s)", req.lesson_id, req.lesson_type.name)

    try:
        text = await _extract_text(req)
        if not text.strip():
            return IndexLessonResponse(
                lesson_id=req.lesson_id,
                chunks_indexed=0,
                status="skipped"
            )
        chunks_indexed = rag_service.index_lesson_text(req.lesson_id, text)
        return IndexLessonResponse(
            lesson_id=req.lesson_id,
            chunks_indexed=chunks_indexed,
            status="indexed"
        )
    except HTTPException:
        raise
    except Exception as e:
        logger.error("Indexing failed for lesson %d: %s", req.lesson_id, e, exc_info=True)
        return IndexLessonResponse(
            lesson_id=req.lesson_id,
            chunks_indexed=0,
            status=f"error: {str(e)[:200]}"
        )


# ── AI Tutor Chat ─────────────────────────────────────────────────

@router.post("/tutor/chat", response_model=TutorChatResponse)
async def tutor_chat(req: TutorChatRequest):
    """
    Answer a student question using RAG over the lesson's indexed content.
    Called by the .NET AiTutorController on each chat message.
    """
    logger.info("Tutor chat for lesson %d: '%s...'", req.lesson_id, req.question[:60])

    try:
        answer = rag_service.answer_question(
            lesson_id=req.lesson_id,
            question=req.question,
            history=req.history
        )
        return TutorChatResponse(answer=answer, source_lesson_id=req.lesson_id)
    except Exception as e:
        logger.error("Tutor chat failed for lesson %d: %s", req.lesson_id, e, exc_info=True)
        raise HTTPException(status_code=500, detail=f"AI tutor error: {str(e)[:200]}")
