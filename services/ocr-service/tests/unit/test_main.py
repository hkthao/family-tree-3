import unittest
from unittest.mock import patch, MagicMock
from fastapi.testclient import TestClient
from PIL import Image
import io

from app.main import app

client = TestClient(app)

class TestOCRMain(unittest.TestCase):
    def setUp(self):
        self.mock_image_bytes = self._create_dummy_image_bytes()
        self.mock_pdf_bytes = b"%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Count 0>>endobj\nxref\n0 3\n0000000000 65535 f\n0000000009 00000 n\n0000000055 00000 n\ntrailer<</Size 3/Root 1 0 R>>startxref\n104\n%%EOF"

    def _create_dummy_image_bytes(self):
        img = Image.new('RGB', (10, 10))
        byte_arr = io.BytesIO()
        img.save(byte_arr, format='PNG')
        return byte_arr.getvalue()

    @patch('app.main.ocr_service.ocr_image', return_value="Image OCR Result")
    @patch('app.main.load_image_from_bytes', side_effect=lambda x: Image.open(io.BytesIO(x)))
    def test_ocr_image_success(self, mock_load_image, mock_ocr_image):
        response = client.post(
            "/ocr",
            files={"file": ("test.png", self.mock_image_bytes, "image/png")},
            data={"lang": "eng"}
        )
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json(), {"success": True, "text": "Image OCR Result"})
        mock_load_image.assert_called_once()
        mock_ocr_image.assert_called_once()

    @patch('app.main.ocr_service.ocr_images', return_value="PDF OCR Result")
    @patch('app.main.load_pdf_as_images', return_value=[Image.new('RGB', (10, 10))])
    def test_ocr_pdf_success(self, mock_load_pdf_images, mock_ocr_images):
        response = client.post(
            "/ocr",
            files={"file": ("test.pdf", self.mock_pdf_bytes, "application/pdf")},
            data={"lang": "vie"}
        )
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json(), {"success": True, "text": "PDF OCR Result"})
        mock_load_pdf_images.assert_called_once()
        mock_ocr_images.assert_called_once()

    def test_ocr_unsupported_file_type(self):
        response = client.post(
            "/ocr",
            files={"file": ("test.txt", b"some text", "text/plain")}
        )
        self.assertEqual(response.status_code, 400)
        self.assertEqual(response.json(), {"detail": "Unsupported file type. Only images (JPG, PNG, TIFF, BMP) and PDF are allowed."})

    @patch('app.main.ocr_service.ocr_image', side_effect=Exception("Internal OCR Error"))
    @patch('app.main.load_image_from_bytes', side_effect=lambda x: Image.open(io.BytesIO(x)))
    def test_ocr_image_internal_error(self, mock_load_image, mock_ocr_image):
        response = client.post(
            "/ocr",
            files={"file": ("test.png", self.mock_image_bytes, "image/png")}
        )
        self.assertEqual(response.status_code, 500)
        self.assertIn("OCR processing failed", response.json()["detail"])
        mock_load_image.assert_called_once()
        mock_ocr_image.assert_called_once()

    @patch('app.main.ocr_service.ocr_images', return_value="PDF OCR Result")
    @patch('app.main.load_pdf_as_images', return_value=[]) # Simulate PDF conversion failure
    def test_ocr_pdf_conversion_failure(self, mock_load_pdf_images, mock_ocr_images):
        response = client.post(
            "/ocr",
            files={"file": ("test.pdf", self.mock_pdf_bytes, "application/pdf")},
            data={"lang": "vie"}
        )
        self.assertEqual(response.status_code, 400)
        self.assertEqual(response.json(), {"detail": "Could not convert PDF to images."})
        mock_load_pdf_images.assert_called_once()
        mock_ocr_images.assert_not_called()

if __name__ == '__main__':
    unittest.main()
