# Use a slim Python 3.11 image for a smaller footprint
FROM python:3.11-slim-buster

# Set environment variables for RabbitMQ connection (default values provided, can be overridden at runtime)
ENV RABBITMQ_HOST=localhost
ENV RABBITMQ_PORT=5672
ENV RABBITMQ_USER=guest
ENV RABBITMQ_PASS=guest

# Install system dependencies, including Graphviz
# graphviz and graphviz-dev for the 'dot' command and its libraries
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    graphviz \
    graphviz-dev \
    # Clean up APT cache to reduce image size
    && rm -rf /var/lib/apt/lists/*

# Set the working directory in the container
WORKDIR /app

# Create shared volumes for input and output. These will be mounted from the host.
RUN mkdir -p /shared/input /shared/output

# Copy the requirements file and install Python dependencies
COPY services/graphviz-pdf-converter/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application code
COPY services/graphviz-pdf-converter/app.py .

# Command to run the application
# Use 'exec' to ensure signals are properly handled by the Python process
CMD ["python", "app.py"]
