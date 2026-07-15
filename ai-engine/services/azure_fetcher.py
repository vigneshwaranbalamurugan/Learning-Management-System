"""
Azure Blob / SAS URL fetcher.
Downloads blob content as bytes from a SAS (Shared Access Signature) URL.
The .NET backend generates SAS URLs and passes them; Python never holds
permanent Azure credentials.
"""
import httpx
import logging

logger = logging.getLogger(__name__)


async def fetch_bytes_from_sas(sas_url: str, timeout: float = 120.0) -> bytes:
    """
    Download raw bytes from an Azure Blob SAS URL.

    Args:
        sas_url: The pre-signed SAS URL to the blob.
        timeout: HTTP timeout in seconds (default 120s for large files).

    Returns:
        Raw bytes of the blob content.

    Raises:
        httpx.HTTPStatusError: If the server returns a non-2xx status.
        httpx.TimeoutException: If the download times out.
    """
    logger.info("Fetching blob from SAS URL (first 80 chars): %s...", sas_url[:80])
    async with httpx.AsyncClient(timeout=timeout, follow_redirects=True) as client:
        response = await client.get(sas_url)
        response.raise_for_status()
        logger.info("Blob fetched: %d bytes", len(response.content))
        return response.content
