"""
ChromaDB vector store service.
One collection per lesson: lesson_{lesson_id}

Uses sentence-transformers (all-MiniLM-L6-v2) running locally for embeddings.
ChromaDB persists to disk at CHROMA_PERSIST_DIR.
"""
import os
import logging
from typing import Optional
import chromadb
from chromadb.utils.embedding_functions import SentenceTransformerEmbeddingFunction

logger = logging.getLogger(__name__)

CHROMA_PERSIST_DIR = os.getenv("CHROMA_PERSIST_DIR", "./chroma_data")
EMBED_MODEL = os.getenv("EMBED_MODEL", "all-MiniLM-L6-v2")
RAG_TOP_K = int(os.getenv("RAG_TOP_K", "5"))

# Singleton clients — initialised once on first use
_chroma_client: Optional[chromadb.PersistentClient] = None
_embedding_fn: Optional[SentenceTransformerEmbeddingFunction] = None


def _get_client() -> chromadb.PersistentClient:
    global _chroma_client
    if _chroma_client is None:
        os.makedirs(CHROMA_PERSIST_DIR, exist_ok=True)
        _chroma_client = chromadb.PersistentClient(path=CHROMA_PERSIST_DIR)
        logger.info("ChromaDB client initialised at: %s", CHROMA_PERSIST_DIR)
    return _chroma_client


def _get_embedding_fn() -> SentenceTransformerEmbeddingFunction:
    global _embedding_fn
    if _embedding_fn is None:
        logger.info("Loading sentence-transformers model: %s", EMBED_MODEL)
        _embedding_fn = SentenceTransformerEmbeddingFunction(model_name=EMBED_MODEL)
        logger.info("Embedding model loaded.")
    return _embedding_fn


def _collection_name(lesson_id: int) -> str:
    return f"lesson_{lesson_id}"


def collection_exists(lesson_id: int) -> bool:
    """Return True if a ChromaDB collection already exists for this lesson."""
    client = _get_client()
    collections = [c.name for c in client.list_collections()]
    return _collection_name(lesson_id) in collections


def index_chunks(lesson_id: int, chunks: list[str]) -> int:
    """
    Store text chunks as embeddings in a per-lesson ChromaDB collection.
    Deletes and re-creates the collection if it already exists (for re-indexing).

    Returns:
        Number of chunks indexed.
    """
    if not chunks:
        logger.warning("No chunks to index for lesson %d", lesson_id)
        return 0

    client = _get_client()
    col_name = _collection_name(lesson_id)

    # Delete existing collection to allow clean re-indexing
    try:
        client.delete_collection(col_name)
        logger.info("Deleted existing collection: %s", col_name)
    except Exception:
        pass  # Collection didn't exist yet

    collection = client.create_collection(
        name=col_name,
        embedding_function=_get_embedding_fn(),
        metadata={"lesson_id": lesson_id}
    )

    # ChromaDB requires string IDs
    ids = [f"{lesson_id}_chunk_{i}" for i in range(len(chunks))]
    collection.add(documents=chunks, ids=ids)

    logger.info("Indexed %d chunks for lesson %d", len(chunks), lesson_id)
    return len(chunks)


def query_chunks(lesson_id: int, question: str, top_k: int = RAG_TOP_K) -> list[str]:
    """
    Retrieve the top-k most relevant text chunks for a question
    from the lesson's ChromaDB collection.

    Returns:
        List of relevant text chunks, or empty list if collection doesn't exist.
    """
    client = _get_client()
    col_name = _collection_name(lesson_id)

    try:
        collection = client.get_collection(
            name=col_name,
            embedding_function=_get_embedding_fn()
        )
    except Exception:
        logger.warning("Collection not found for lesson %d — not indexed yet.", lesson_id)
        return []

    results = collection.query(query_texts=[question], n_results=min(top_k, collection.count()))
    docs = results.get("documents", [[]])[0]
    logger.info("Retrieved %d chunks for lesson %d", len(docs), lesson_id)
    return docs


def delete_collection(lesson_id: int) -> None:
    """Remove a lesson's ChromaDB collection (called on lesson deletion)."""
    client = _get_client()
    try:
        client.delete_collection(_collection_name(lesson_id))
        logger.info("Deleted ChromaDB collection for lesson %d", lesson_id)
    except Exception as e:
        logger.warning("Could not delete collection for lesson %d: %s", lesson_id, e)
