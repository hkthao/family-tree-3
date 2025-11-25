# Use a specific Python base image with OpenCV pre-installed for easier setup
# python:3.9-slim-buster might be sufficient, but we often need specific system libs for OpenCV/dlib
# A good base for ML/CV is usually something like 'nvcr.io/nvidia/l4t-ml:r32.7.1-py3' for Jetson
# or a generic opencv image. For simplicity, let's use a base Python image and install deps.
FROM python:3.9-slim-bullseye

# Set working directory
WORKDIR /app

# Install system dependencies for dlib/face_recognition and OpenCV
# This part can be tricky and might require iteration based on base image and specific library versions.
# For dlib/face_recognition, build essentials and cmake are often needed.
RUN apt-get update && apt-get install -y \
    build-essential \
    cmake \
    libopenblas-dev \
    liblapack-dev \
    libx11-dev \
    libgtk-3-dev \
    libjpeg-dev \
    libpng-dev \
    libtiff-dev \
    libavcodec-dev \
    libavformat-dev \
    libswscale-dev \
    pkg-config \
    python3-dev \
    python3-pip \
    --no-install-recommends && \
    rm -rf /var/lib/apt/lists/*

# Copy requirements file
COPY requirements.txt ./requirements.txt

# Install Python dependencies
# Use --no-cache-dir to save space
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application code
COPY . ./ 

# Copy the entrypoint script and make it executable
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Expose the port FastAPI will run on
EXPOSE 8000

# Set the entrypoint for the container
# This will run entrypoint.sh, which then decides to start uvicorn or run tests
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Default command if no arguments are passed to the container (will be passed to entrypoint.sh)
CMD ["main:app"]
