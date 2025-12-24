from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from fastapi.responses import JSONResponse
from typing import List, Optional
import logging
import imghdr # For checking image file types

from app.services.ocr_service import OCRService
from app.utils.file_loader import load_image_from_bytes, load_pdf_as_images

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format=(
        '%(asctime)s - %(levelname)s - %(message)s'
    )
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="OCRService",
    description="A FastAPI service for Optical Character Recognition (OCR) using Tesseract.",
    version="1.0.0",
)

ocr_service = OCRService()

@app.post("/ocr", response_model=dict)
async def ocr_file(
    file: UploadFile = File(...),
    lang: str = Form("eng+vie", description="Languages for OCR (e.g., 'eng', 'vie', 'eng+vie')")
):
    logger.info(f"Received request to OCR file: {file.filename}, content_type: {file.content_type}, lang: {lang}")

    allowed_image_types = ["image/jpeg", "image/png", "image/tiff", "image/bmp"]
    allowed_pdf_types = ["application/pdf"]
    
    file_content = await file.read()

    try:
        if file.content_type in allowed_image_types:
            logger.info("Processing as image file.")
            image = load_image_from_bytes(file_content)
            extracted_text = ocr_service.ocr_image(image, lang=lang)
            
        elif file.content_type in allowed_pdf_types:
            logger.info("Processing as PDF file.")
            images = load_pdf_as_images(file_content)
            if not images:
                raise HTTPException(status_code=400, detail="Could not convert PDF to images.")
            extracted_text = ocr_service.ocr_images(images, lang=lang)
            
        else:
            logger.warning(f"Unsupported file type: {file.content_type}")
            raise HTTPException(
                status_code=400,
                detail="Unsupported file type. Only images (JPG, PNG, TIFF, BMP) and PDF are allowed."
            )

        logger.info(f"Successfully OCR'd file. Extracted text length: {len(extracted_text)}")
        return JSONResponse(
            content={"success": True, "text": extracted_text}
        )

    except HTTPException as http_exc:
        raise http_exc
    except Exception as e:
        logger.error(f"Error during OCR processing for file {file.filename}: {e}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"OCR processing failed: {e}"
        )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
