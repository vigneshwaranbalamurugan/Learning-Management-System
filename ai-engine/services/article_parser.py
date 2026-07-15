"""
Article text parser.
Strips HTML tags and converts Markdown-like content to plain text
for Article-type lessons whose content is stored as HTML or Markdown.
"""
import re
import logging
from bs4 import BeautifulSoup

logger = logging.getLogger(__name__)


def extract_text_from_article(content: str) -> str:
    """
    Convert HTML or Markdown article content to clean plain text.

    Args:
        content: HTML string or Markdown string from the lesson's Content field.

    Returns:
        Clean plain text suitable for embedding and RAG.
    """
    if not content or not content.strip():
        return ""

    # If it looks like HTML (contains < tags), parse with BeautifulSoup
    if "<" in content and ">" in content:
        soup = BeautifulSoup(content, "html.parser")
        # Remove script/style elements
        for tag in soup(["script", "style", "head", "meta", "link"]):
            tag.decompose()
        text = soup.get_text(separator="\n")
    else:
        # Treat as Markdown — strip common markdown syntax
        text = _strip_markdown(content)

    # Normalize whitespace
    lines = [line.strip() for line in text.splitlines()]
    non_empty = [l for l in lines if l]
    result = "\n".join(non_empty)

    logger.info("Article text extracted: %d chars", len(result))
    return result


def _strip_markdown(text: str) -> str:
    """Remove common Markdown formatting symbols."""
    # Remove code blocks
    text = re.sub(r"```[\s\S]*?```", "", text)
    text = re.sub(r"`[^`]*`", "", text)
    # Remove headers
    text = re.sub(r"^#{1,6}\s+", "", text, flags=re.MULTILINE)
    # Remove bold/italic
    text = re.sub(r"\*{1,3}(.*?)\*{1,3}", r"\1", text)
    text = re.sub(r"_{1,3}(.*?)_{1,3}", r"\1", text)
    # Remove links
    text = re.sub(r"\[([^\]]+)\]\([^\)]+\)", r"\1", text)
    # Remove images
    text = re.sub(r"!\[.*?\]\(.*?\)", "", text)
    # Remove horizontal rules
    text = re.sub(r"^---+$", "", text, flags=re.MULTILINE)
    # Remove bullet list markers
    text = re.sub(r"^[\*\-\+]\s+", "", text, flags=re.MULTILINE)
    # Remove numbered list markers
    text = re.sub(r"^\d+\.\s+", "", text, flags=re.MULTILINE)
    return text
