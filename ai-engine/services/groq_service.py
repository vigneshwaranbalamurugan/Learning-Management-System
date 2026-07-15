"""
Groq AI service wrapper.
Handles chat completions for tutor and summary use cases.
"""
import os
import logging
from groq import Groq
from models.schemas import ChatMessage

logger = logging.getLogger(__name__)

GROQ_CHAT_MODEL = os.getenv("GROQ_CHAT_MODEL", "llama-3.1-70b-versatile")

_groq_client: Groq | None = None


def _get_client() -> Groq:
    global _groq_client
    if _groq_client is None:
        api_key = os.getenv("GROQ_API_KEY")
        if not api_key:
            raise RuntimeError("GROQ_API_KEY environment variable is not set.")
        _groq_client = Groq(api_key=api_key)
        logger.info("Groq client initialised.")
    return _groq_client


def chat_completion(
    system_prompt: str,
    user_message: str,
    history: list[ChatMessage] | None = None,
    temperature: float = 0.4,
    max_tokens: int = 1500
) -> str:
    """
    Call Groq chat completions API.

    Args:
        system_prompt: System-level instructions for the model.
        user_message: The current user message / query.
        history: Prior conversation turns (for multi-turn chat).
        temperature: Sampling temperature (lower = more deterministic).
        max_tokens: Max tokens in the response.

    Returns:
        Model response as a plain string.
    """
    client = _get_client()
    messages = [{"role": "system", "content": system_prompt}]

    if history:
        for msg in history[-10:]:  # keep last 10 turns to stay within context
            messages.append({"role": msg.role, "content": msg.content})

    messages.append({"role": "user", "content": user_message})

    response = client.chat.completions.create(
        model=GROQ_CHAT_MODEL,
        messages=messages,
        temperature=temperature,
        max_tokens=max_tokens
    )

    answer = response.choices[0].message.content or ""
    logger.info("Groq response: %d chars", len(answer))
    return answer.strip()
