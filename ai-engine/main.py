"""
AI Engine — FastAPI application entry point.

This server is INTERNAL ONLY. It is not exposed to the internet.
It should only be reachable by the .NET LMSApi backend on the same network/host.

Security:
  - All routes require the X-Internal-Api-Key header matching INTERNAL_API_KEY env var.
  - No public routes are registered.

Run with:
  uvicorn main:app --host 0.0.0.0 --port 8001
"""
import os
import logging
from contextlib import asynccontextmanager
from dotenv import load_dotenv

# Load .env before anything else
load_dotenv()

from fastapi import FastAPI, Request, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from routers import ai_tutor, ai_summary

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger(__name__)

INTERNAL_API_KEY = os.getenv("INTERNAL_API_KEY", "")


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Startup: pre-load embedding model so first request isn't slow."""
    logger.info("AI Engine starting up…")
    if not os.getenv("GROQ_API_KEY"):
        logger.warning("GROQ_API_KEY is not set. AI features will fail.")
    if not INTERNAL_API_KEY:
        logger.warning("INTERNAL_API_KEY is not set. All requests will be rejected.")

    # Pre-load sentence-transformers model on startup
    try:
        from services.vector_store import _get_embedding_fn
        _get_embedding_fn()
        logger.info("Embedding model pre-loaded.")
    except Exception as e:
        logger.error("Failed to pre-load embedding model: %s", e)

    yield
    logger.info("AI Engine shutting down.")


app = FastAPI(
    title="LMS AI Engine",
    description="Internal AI service for LMS: RAG tutor + lesson summary generation.",
    version="1.0.0",
    lifespan=lifespan,
    # Disable public docs in production — only enable in dev
    docs_url="/docs" if os.getenv("ENV", "production") == "development" else None,
    redoc_url=None,
)

# No CORS needed — this server is internal only.
# Only allow calls from the same host/network.
app.add_middleware(
    CORSMiddleware,
    allow_origins=[],  # No browser access allowed
    allow_methods=["POST", "GET"],
    allow_headers=["*"],
)


# ── Security Middleware ────────────────────────────────────────────────────────

@app.middleware("http")
async def verify_internal_api_key(request: Request, call_next):
    """
    Reject any request that doesn't carry the correct X-Internal-Api-Key header.
    """
    if not INTERNAL_API_KEY:
        raise HTTPException(status_code=503, detail="Server misconfigured: INTERNAL_API_KEY not set.")

    provided_key = request.headers.get("X-Internal-Api-Key", "")
    if provided_key != INTERNAL_API_KEY:
        logger.warning("Rejected request with invalid API key from %s", request.client)
        raise HTTPException(status_code=401, detail="Unauthorized: invalid internal API key.")

    return await call_next(request)


# ── Routers ───────────────────────────────────────────────────────────────────

app.include_router(ai_tutor.router)
app.include_router(ai_summary.router)


@app.get("/health")
async def health():
    return {"status": "ok", "service": "lms-ai-engine"}
