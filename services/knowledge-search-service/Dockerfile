# Stage 1: Builder - for installing all dependencies, including dev/test ones
FROM python:3.11-slim-bookworm as builder

WORKDIR /app

# Install system dependencies needed for some Python packages
RUN apt-get update && apt-get install -y libgomp1 build-essential && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt && rm requirements.txt

# Copy the application code
COPY app/ app/

# Stage 2: Production - lean image with only production dependencies
FROM python:3.11-slim-bookworm as production

WORKDIR /app

# Install only runtime system dependencies
RUN apt-get update && apt-get install -y libgomp1 && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt && rm requirements.txt

# Copy the application code from the builder stage
COPY --from=builder /app/app/ app/

# Make port 8000 available to the world outside this container
EXPOSE 8000

# Run the uvicorn server
# --host 0.0.0.0 makes the server accessible from outside the container
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
