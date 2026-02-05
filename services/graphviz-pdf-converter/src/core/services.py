import os
import subprocess
import logging

from src.core.config import config
from src.core.domain import RenderRequest

logger = logging.getLogger(__name__)

class GraphvizService:
    @staticmethod
    def render_dot_to_pdf(request: RenderRequest) -> str:
        """
        Renders a Graphviz .dot file to a PDF.

        Args:
            request: A RenderRequest object containing job details.

        Returns:
            The path to the generated PDF file.

        Raises:
            FileNotFoundError: If the input .dot file does not exist or 'dot' command is not found.
            subprocess.TimeoutExpired: If the Graphviz command times out.
            RuntimeError: If the Graphviz command fails for any other reason.
        """
        input_dot_path = os.path.join(config.INPUT_DIR, request.dot_filename)
        output_pdf_path = os.path.join(config.OUTPUT_DIR, f"{request.job_id}.pdf")

        # 1. Validate input .dot file existence
        print(f"DEBUG: Checking existence of input file: {input_dot_path}")
        if not os.path.exists(input_dot_path):
            raise FileNotFoundError(f"Input .dot file not found: {input_dot_path}")

        # 2. Run Graphviz dot command
        command = [
            'dot',
            '-Tpdf',
            f'-Grankdir={request.direction}',
            f'-s{request.page_size}',
            input_dot_path,
            '-o',
            output_pdf_path
        ]
        logger.info("Executing command: %s", ' '.join(command))

        try:
            result = subprocess.run(
                command,
                capture_output=True,
                text=True,
                timeout=config.RENDER_TIMEOUT_SECONDS,
                check=False
            )

            if result.returncode == 0:
                logger.info("Successfully rendered PDF for job %s at %s", request.job_id, output_pdf_path)
                return output_pdf_path
            else:
                error_msg = (
                    f"Graphviz rendering failed for job {request.job_id}. "
                    f"Return code: {result.returncode}. "
                    f"Stdout: {result.stdout.strip()}. "
                    f"Stderr: {result.stderr.strip()}."
                )
                raise RuntimeError(error_msg)

        except subprocess.TimeoutExpired as e:
            raise subprocess.TimeoutExpired(
                cmd=command,
                timeout=config.RENDER_TIMEOUT_SECONDS,
                output=e.output,
                stderr=e.stderr
            )
        except FileNotFoundError:
            raise FileNotFoundError("Graphviz 'dot' command not found. Is Graphviz installed and in PATH?")
        except Exception as e:
            raise RuntimeError(f"An unexpected error occurred during subprocess execution for job {request.job_id}: {e}")
