"""
PDF text extractor using PyMuPDF (fitz).
Downloads the PDF from a SAS URL and extracts all text.
"""
import fitz  # PyMuPDF
import logging
from services.azure_fetcher import fetch_bytes_from_sas

logger = logging.getLogger(__name__)


async def extract_text_from_pdf_url(sas_url: str) -> str:
    """
    Download a PDF from a SAS URL and extract its full text.

    Args:
        sas_url: Azure Blob SAS URL pointing to a PDF file.

    Returns:
        Plain text content of the PDF, pages separated by newlines.
    """
    pdf_bytes = await fetch_bytes_from_sas(sas_url)
    return extract_text_from_bytes(pdf_bytes)


def extract_text_from_bytes(pdf_bytes: bytes) -> str:
    """
    Extract text from PDF bytes using PyMuPDF.
    """
    try:
        doc = fitz.open(stream=pdf_bytes, filetype="pdf")
        pages_text = []
        for page_num, page in enumerate(doc):
            text = page.get_text("text")
            if text.strip():
                pages_text.append(f"--- Page {page_num + 1} ---\n{text.strip()}")
        doc.close()
        full_text = "\n\n".join(pages_text)
        logger.info("Extracted %d chars from %d PDF pages", len(full_text), len(pages_text))
        return full_text
    except Exception as e:
        logger.error("PDF extraction failed: %s", e)
        raise
