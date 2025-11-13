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
    wget \
    bzip2 && \
    # Create models directory
    mkdir -p app/models && \
    # Download and extract dlib models
    wget http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2 -O app/models/shape_predictor_68_face_landmarks.dat.bz2 && \
    wget http://dlib.net/files/dlib_face_recognition_resnet_model_v1.dat.bz2 -O app/models/dlib_face_recognition_resnet_model_v1.dat.bz2 && \
    bzip2 -d app/models/shape_predictor_68_face_landmarks.dat.bz2 && \
    bzip2 -d app/models/dlib_face_recognition_resnet_model_v1.dat.bz2 && \
    # Clean up
    rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
COPY .flake8 .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
