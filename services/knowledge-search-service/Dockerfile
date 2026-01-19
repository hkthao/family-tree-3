# Use an official Python runtime as a parent image
FROM python:3.10-slim-bookworm

# Set the working directory in the container
WORKDIR /app

# Install any needed packages specified in requirements.txt
COPY requirements.txt .
RUN apt-get update && apt-get install -y libgomp1 && rm -rf /var/lib/apt/lists/*
ENV LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libgomp.so.1
RUN pip install --no-cache-dir -r requirements.txt

# Copy the current directory contents into the container at /app
COPY app/ app/

# Make port 8000 available to the world outside this container
EXPOSE 8000

# Run the uvicorn server
# --host 0.0.0.0 makes the server accessible from outside the container
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
