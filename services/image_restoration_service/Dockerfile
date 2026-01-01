# Use an official Python runtime as a parent image
FROM python:3.10-slim-buster

# Set the working directory in the container
WORKDIR /app

# Copy the requirements file into the container
COPY requirements.txt .

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Copy the entire application code to the container
COPY app /app/app

# Expose the port that FastAPI will run on
EXPOSE 8000

# Run the FastAPI application using Uvicorn
# Use 0.0.0.0 to listen on all available network interfaces
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
