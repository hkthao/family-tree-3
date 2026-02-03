# Graphviz PDF Converter Service

This microservice listens for Graphviz `.dot` file rendering requests via RabbitMQ, converts them to PDF using the `dot` command-line tool, and publishes the results back to RabbitMQ.

## Functional Requirements

*   **Input Queue**: `render_request`
    *   **Message Format**: JSON
        ```json
        {
          "job_id": "unique_job_identifier",
          "dot_filename": "example.dot",
          "page_size": "A0",  // Optional, e.g., "A0", "Letter", "Legal". Defaults to A0.
          "direction": "LR"   // Optional, e.g., "LR" (Left-to-Right), "TB" (Top-to-Bottom). Defaults to LR.
        }
        ```
*   **Input File Location**: Reads `.dot` files from `/shared/input/{dot_filename}`.
*   **Output File Location**: Writes generated PDFs to `/shared/output/{job_id}.pdf`.
*   **Output Queue**: `render_response`
    *   **Success Message**:
        ```json
        {
          "job_id": "unique_job_identifier",
          "status": "success",
          "pdf_path": "/shared/output/unique_job_identifier.pdf"
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
*   **Timeout**: Rendering jobs have a timeout of 120 seconds.
*   **Validation**: Validates the existence of the input `.dot` file.
*   **Directory Management**: Creates the output folder `/shared/output` if it does not exist.
*   **Logging**: Logs major steps to `stdout`.
*   **Graceful Shutdown**: Handles `SIGTERM` signals for graceful termination.

## Technical Stack

*   **Language**: Python 3.11
*   **RabbitMQ Client**: `pika`
*   **Graphviz**: `dot` command-line tool
*   **Process Management**: `subprocess.run`

## Deliverables

*   `app.py`: Main consumer service logic.
*   `requirements.txt`: Python dependency list.
*   `Dockerfile`: Docker build instructions.
*   `README.md`: This documentation.

## How to Run with Docker

### 1. Build the Docker Image

Navigate to the root of your project and build the Docker image for the service:

```bash
docker build -t graphviz-pdf-converter -f services/graphviz-pdf-converter/Dockerfile .
```

### 2. Run the Docker Container

You can run the container, linking it to a RabbitMQ instance and mounting the shared directories for input and output.

First, ensure you have a RabbitMQ instance running. If you're using `docker-compose.yml` from the main project, it likely includes a RabbitMQ service.

To run the converter service independently (e.g., for testing):

```bash
docker run -d \
  --name graphviz-pdf-converter \
  --network host \# Or connect to a specific Docker network where RabbitMQ is available
  -v /path/to/your/input_files:/shared/input \
  -v /path/to/your/output_pdfs:/shared/output \
  -e RABBITMQ_HOST=your_rabbitmq_host \
  -e RABBITMQ_PORT=5672 \
  -e RABBITMQ_USER=guest \
  -e RABBITMQ_PASS=guest \
  graphviz-pdf-converter
```

**Explanation of parameters:**

*   `-d`: Run the container in detached mode.
*   `--name graphviz-pdf-converter`: Assign a name to your container.
*   `--network host`: Allows the container to use the host's network stack. This is often convenient for local development but might need adjustment for production environments (e.g., connecting to a specific Docker network).
*   `-v /path/to/your/input_files:/shared/input`: Mounts a local directory (`/path/to/your/input_files`) as the `/shared/input` volume inside the container. Place your `.dot` files here.
*   `-v /path/to/your/output_pdfs:/shared/output`: Mounts a local directory (`/path/to/your/output_pdfs`) as the `/shared/output` volume. The generated PDFs will appear here.
*   `-e RABBITMQ_HOST=your_rabbitmq_host`: Set the RabbitMQ host. Replace `your_rabbitmq_host` with the actual hostname or IP of your RabbitMQ server.
*   `-e RABBITMQ_USER`, `-e RABBITMQ_PASS`: Set RabbitMQ credentials if they are not the defaults.

### 3. Example Usage

1.  **Prepare a `.dot` file**:
    Create a file named `my_graph.dot` in your mounted input directory (`/path/to/your/input_files`):
    ```dot
    digraph G {
        A -> B;
        B -> C;
        C -> A;
    }
    ```

2.  **Send a message to `render_request` queue**:
    Using a RabbitMQ client (e.g., `pika` in Python, or RabbitMQ Management UI), publish a message like this:

    ```json
    {
      "job_id": "my_test_job",
      "dot_filename": "my_graph.dot",
      "page_size": "Letter",
      "direction": "TB"
    }
    ```

3.  **Check for the PDF**:
    The service will process the request, and if successful, you should find `my_test_job.pdf` in your mounted output directory (`/path/to/your/output_pdfs`).

4.  **Monitor responses**:
    You can also consume messages from the `render_response` queue to see the status of your rendering jobs.

## Error Handling

The service provides detailed error messages in the `render_response` queue if a rendering job fails due to:
*   Missing input `.dot` file.
*   Graphviz command errors (e.g., invalid `.dot` syntax).
*   Timeout during rendering.
*   Invalid incoming JSON message.
