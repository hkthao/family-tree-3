FROM python:3.9-slim-bullseye

WORKDIR /ocr-service

# Install Tesseract and its language packs for English and Vietnamese
# Also install build dependencies for pip packages
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    tesseract-ocr \
    tesseract-ocr-eng \
    tesseract-ocr-vie \
    libtesseract-dev \
    libleptonica-dev \
    pkg-config \
    build-essential \
    cmake \
    libjpeg-dev \
    zlib1g-dev \
    libpng-dev \
    libtiff-dev \
    libwebp-dev \
    poppler-utils && \
    # Clean up
    rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application code
COPY app ./app

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
