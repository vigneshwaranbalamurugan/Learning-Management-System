FROM python:3.11-slim

# Install system dependencies: ffmpeg for audio extraction
RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy application code
COPY . .

# Create ChromaDB data directory
RUN mkdir -p /app/chroma_data

# Expose internal port
EXPOSE 8001

# Pre-download the sentence-transformers model at build time so first request is fast
RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('all-MiniLM-L6-v2')"

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8001"]
