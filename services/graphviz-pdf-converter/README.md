# Graphviz PDF Converter Service

This microservice provides functionality to convert Graphviz `.dot` files to PDF format. It offers both a RabbitMQ-based consumer for asynchronous processing and a FastAPI-based HTTP API for synchronous requests.

## Functional Requirements

*   **Input Queue (RabbitMQ)**: `render_request`
    *   **Message Format**: JSON
        ```json
        {
          "job_id": "unique_job_identifier",
          "dot_filename": "example.dot",
          "page_size": "A0",  // Optional, e.g., "A0", "Letter", "Legal". Defaults to A0.
          "direction": "LR"   // Optional, e.g., "LR" (Left-to-Right), "TB" (Top-to-Bottom). Defaults to LR.
        }
        ```
*   **API Endpoint (FastAPI)**: `POST /render`
    *   **Request Body**: JSON (same format as RabbitMQ message)
    *   **Response Body (Success)**: JSON
        ```json
        {
          "job_id": "unique_job_identifier",
          "status": "success",
          "output_file_path": "/shared/output/unique_job_identifier.pdf"
        }
        ```
    *   **Response Body (Failure)**: JSON (HTTP status codes 400, 404, 500, 504)
        ```json
        {
          "job_id": "unique_job_identifier",
          "status": "error",
          "error_message": "Details about the error"
        }
        ```
*   **Input File Location**: Reads `.dot` files from `/shared/input/{dot_filename}`.
*   **Output File Location**: Writes generated PDFs to `/shared/output/{job_id}.pdf`.
*   **Output (Status Update) Exchange (RabbitMQ)**: `graph_generation_exchange`
    *   **Routing Key**: `graph.status.updated`
    *   **Success Message**:
        ```json
        {
          "job_id": "unique_job_identifier",
          "status": "success",
          "output_file_path": "/shared/output/unique_job_identifier.pdf"
        }
        ```
    *   **Failure Message**:
        ```json
        {
          "job_id": "unique_job_identifier",
          "status": "error",
          "error_message": "Details about the error"
        }
        ```
*   **Graphviz Command**: Uses `dot input.dot -Tpdf -o output.pdf`.
*   **Timeout**: Rendering jobs have a timeout of 120 seconds (configurable via `RENDER_TIMEOUT_SECONDS` environment variable).
*   **Validation**: Validates the existence of the input `.dot` file.
*   **Directory Management**: Creates the output folder `/shared/output` if it does not exist.
*   **Logging**: Logs major steps to `stdout` (configurable via `LOG_LEVEL` environment variable).
*   **Graceful Shutdown**: Handles `SIGTERM` signals for graceful termination (RabbitMQ consumer).

## Technical Stack

*   **Language**: Python 3.11
*   **Web Framework**: FastAPI
*   **RabbitMQ Client**: `pika`
*   **Data Validation**: Pydantic
*   **Graphviz**: `dot` command-line tool
*   **Process Management**: `subprocess.run`

## Project Structure

```
services/graphviz-pdf-converter/
├───src/
│   ├───api/
│   │   ├───main.py        # FastAPI application entry point
│   ├───core/
│   │   ├───config.py      # Configuration settings
│   │   ├───domain.py      # Pydantic models for request/response
│   │   └───services.py    # Core Graphviz rendering logic
│   └───messaging/
│       └───consumer.py    # RabbitMQ message consumer
├───Dockerfile             # Docker build instructions
├───README.md              # This documentation
├───requirements.txt       # Python dependency list
└───tests/                 # Unit and integration tests
```

## How to Run with Docker

### 1. Build the Docker Image

Navigate to the root of your project and build the Docker image for the service:

```bash
docker build -t graphviz-pdf-converter -f services/graphviz-pdf-converter/Dockerfile .
```

### 2. Run the Docker Container

The service can run in two modes: as a FastAPI server (default) or as a RabbitMQ consumer.

First, ensure you have a RabbitMQ instance running. If you're using `docker-compose.yml` from the main project, it likely includes a RabbitMQ service.

#### a) Run as FastAPI Server (default)

This will expose the API on port 8000.

```bash
docker run -d \
  --name graphviz-pdf-converter-api \
  --network host \ # Or connect to a specific Docker network
  -p 8000:8000 \ # Map port 8000
  -v /path/to/your/input_files:/shared/input \
  -v /path/to/your/output_pdfs:/shared/output \
  graphviz-pdf-converter
```

**Access API Docs**: [http://localhost:8000/docs](http://localhost:8000/docs) or [http://localhost:8000/redoc](http://localhost:8000/redoc)

#### b) Run as RabbitMQ Consumer

To run the consumer, you need to override the `CMD` in the Dockerfile.

```bash
docker run -d \
  --name graphviz-pdf-converter-consumer \
  --network host \ # Or connect to a specific Docker network where RabbitMQ is available
  -v /path/to/your/input_files:/shared/input \
  -v /path/to/your/output_pdfs:/shared/output \
  -e RABBITMQ_HOST=your_rabbitmq_host \
  -e RABBITMQ_PORT=5672 \
  -e RABBITMQ_USER=guest \
  -e RABBITMQ_PASS=guest \
  graphviz-pdf-converter python -m src.messaging.consumer
```

**Explanation of common parameters:**

*   `-d`: Run the container in detached mode.
*   `--network host`: Allows the container to use the host's network stack. This is often convenient for local development but might need adjustment for production environments.
*   `-v /path/to/your/input_files:/shared/input`: Mounts a local directory (`/path/to/your/input_files`) as the `/shared/input` volume inside the container. Place your `.dot` files here.
*   `-v /path/to/your/output_pdfs:/shared/output`: Mounts a local directory (`/path/to/your/output_pdfs`) as the `/shared/output` volume. The generated PDFs will appear here.
*   `-e RABBITMQ_HOST=your_rabbitmq_host`: Set the RabbitMQ host. Replace `your_rabbitmq_host` with the actual hostname or IP of your RabbitMQ server.
*   `-e RABBITMQ_USER`, `-e RABBITMQ_PASS`: Set RabbitMQ credentials if they are not the defaults.

### 3. Example Usage (FastAPI)

1.  **Prepare a `.dot` file**:
    Create a file named `my_graph.dot` in your mounted input directory (`/path/to/your/input_files`):
    ```dot
    digraph G {
        A -> B;
        B -> C;
        C -> A;
    }
    ```

2.  **Send a POST request to `/render`**:
    ```bash
    curl -X POST "http://localhost:8000/render" \
         -H "Content-Type: application/json" \
         -d '{
               "job_id": "my_test_job_api",
               "dot_filename": "my_graph.dot",
               "page_size": "Letter",
               "direction": "TB"
             }'
    ```

3.  **Send a POST request to `/render-and-download` (File Upload)**:
    This endpoint allows you to upload a `.dot` file directly.
    ```bash
    curl -X POST "http://localhost:8000/render-and-download" \
         -H "Content-Type: multipart/form-data" \
         -F "job_id=my_uploaded_job" \
         -F "dot_file=@/path/to/your/input_files/my_graph.dot;type=text/plain" \
         -F "page_size=A4" \
         -F "direction=LR" \
         --output /path/to/your/downloads/my_uploaded_job.pdf
    ```
    *Replace `/path/to/your/input_files/my_graph.dot` with the actual path to your `.dot` file.*
    *Replace `/path/to/your/downloads/my_uploaded_job.pdf` with the desired output path for the downloaded PDF.*

4.  **Send a POST request to `/render-and-download` (Using existing path)**:
    This endpoint allows you to specify a path to an existing `.dot` file on the server's mounted input directory.
    ```bash
    curl -X POST "http://localhost:8000/render-and-download" \
         -H "Content-Type: multipart/form-data" \
         -F "job_id=my_path_job" \
         -F "dot_filename_path=my_graph.dot" \
         -F "page_size=A3" \
         -F "direction=TB" \
         --output /path/to/your/downloads/my_path_job.pdf
    ```

5.  **Check for the PDF**:
    For `/render` endpoint, the service will process the request, and if successful, you should find `my_test_job_api.pdf` in your mounted output directory (`/path/to/your/output_pdfs`).
    For `/render-and-download` endpoint, the PDF will be directly returned as a file download.

### 4. Example Usage (RabbitMQ Consumer)

1.  **Prepare a `.dot` file**: (Same as above)
    Create a file named `my_graph_mq.dot` in your mounted input directory (`/path/to/your/input_files`):
    ```dot
    digraph G {
        D -> E;
        E -> F;
        F -> D;
    }
    ```

2.  **Send a message to `render_request` queue**:
    Using a RabbitMQ client (e.g., `pika` in Python, or RabbitMQ Management UI), publish a message like this:

    ```json
    {
      "job_id": "my_test_job_mq",
      "dot_filename": "my_graph_mq.dot",
      "page_size": "Letter",
      "direction": "TB"
    }
    ```

3.  **Check for the PDF**:
    The service will process the request, and if successful, you should find `my_test_job_mq.pdf` in your mounted output directory (`/path/to/your/output_pdfs`).

4.  **Monitor responses**:
    You can consume messages from the `graph_generation_exchange` with routing key `graph.status.updated` to see the status of your rendering jobs.

## Error Handling

The service publishes detailed error messages to the `graph_generation_exchange` (RabbitMQ) or returns them in the API response (FastAPI) if a rendering job fails due to:
*   Missing input `.dot` file.
*   Graphviz command errors (e.g., invalid `.dot` syntax).
*   Timeout during rendering.
*   Invalid incoming JSON message.
*   Graphviz `dot` command not found on the server.