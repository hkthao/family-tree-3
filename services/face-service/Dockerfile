FROM python:3.9-slim-bullseye

WORKDIR /face-service

# Install Dlib build dependencies, download tools, and models
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    build-essential \
    cmake \
    libopenblas-dev \
    liblapack-dev \
    libx11-dev \
    python3-dev \
    libgl1-mesa-glx \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender1 \
    libfontconfig1 \
    libjpeg-dev \
    zlib1g-dev \
    pkg-config && \
    # Clean up
    rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
COPY .flake8 .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the new python files along with the app directory
COPY app ./app
COPY app/crop_face.py app/
COPY app/get_emotion.py app/

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
