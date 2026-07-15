"""
Pydantic schemas for AI Engine request/response models.
"""
from pydantic import BaseModel, Field
from typing import Optional
from enum import IntEnum


# ──────────────────────────────────────────────────────────────
# Shared
# ──────────────────────────────────────────────────────────────

class LessonType(IntEnum):
    Video = 0
    Pdf = 1
    Article = 2
    ExternalLink = 3


class ChatMessage(BaseModel):
    role: str = Field(..., description="'user' or 'assistant'")
    content: str


# ──────────────────────────────────────────────────────────────
# AI Tutor
# ──────────────────────────────────────────────────────────────

class TutorChatRequest(BaseModel):
    lesson_id: int
    question: str = Field(..., min_length=1, max_length=2000)
    history: list[ChatMessage] = Field(default_factory=list)
    # Content is provided by the .NET backend so Python doesn't call Azure
    content_url: Optional[str] = None      # SAS URL for video/pdf
    content_text: Optional[str] = None     # article HTML/markdown


class TutorChatResponse(BaseModel):
    answer: str
    source_lesson_id: int


# ──────────────────────────────────────────────────────────────
# Lesson Indexing
# ──────────────────────────────────────────────────────────────

class IndexLessonRequest(BaseModel):
    lesson_id: int
    lesson_type: LessonType
    content_url: Optional[str] = None      # SAS URL for video/pdf
    content_text: Optional[str] = None     # article HTML/markdown


class IndexLessonResponse(BaseModel):
    lesson_id: int
    chunks_indexed: int
    status: str  # "indexed" | "skipped" | "error"


# ──────────────────────────────────────────────────────────────
# AI Summary
# ──────────────────────────────────────────────────────────────

class GenerateSummaryRequest(BaseModel):
    lesson_id: int
    lesson_type: LessonType
    content_url: Optional[str] = None      # SAS URL for video/pdf
    content_text: Optional[str] = None     # article HTML/markdown


class GenerateSummaryResponse(BaseModel):
    lesson_id: int
    summary: str
    key_points: list[str]
    notes: str
    status: str  # "generated" | "error"
