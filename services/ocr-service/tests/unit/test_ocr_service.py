import unittest
from unittest.mock import patch, MagicMock
from PIL import Image
import io

from app.services.ocr_service import OCRService

class TestOCRService(unittest.TestCase):
    def setUp(self):
        self.ocr_service = OCRService()
        self.mock_image = Image.new('RGB', (100, 100))

    @patch('pytesseract.image_to_string')
    @patch('app.services.ocr_service.preprocess_image_for_ocr')
    @patch('app.services.ocr_service.clean_ocr_text')
    def test_ocr_image_success(self, mock_clean_ocr_text, mock_preprocess_image_for_ocr, mock_image_to_string):
        mock_preprocess_image_for_ocr.return_value = self.mock_image # Return mock image after preprocessing
        mock_image_to_string.return_value = "Mock OCR Text"
        mock_clean_ocr_text.return_value = "Cleaned Mock OCR Text"

        result = self.ocr_service.ocr_image(self.mock_image, lang='eng')

        mock_preprocess_image_for_ocr.assert_called_once_with(self.mock_image)
        mock_image_to_string.assert_called_once_with(self.mock_image, lang='eng')
        mock_clean_ocr_text.assert_called_once_with("Mock OCR Text")
        self.assertEqual(result, "Cleaned Mock OCR Text")

    @patch('pytesseract.image_to_string', side_effect=Exception("Tesseract Error"))
    @patch('app.services.ocr_service.preprocess_image_for_ocr')
    def test_ocr_image_error(self, mock_preprocess_image_for_ocr, mock_image_to_string):
        mock_preprocess_image_for_ocr.return_value = self.mock_image

        with self.assertRaisesRegex(Exception, "Tesseract Error"):
            self.ocr_service.ocr_image(self.mock_image)

    @patch.object(OCRService, 'ocr_image', return_value="Page Text")
    def test_ocr_images_success(self, mock_ocr_image):
        mock_images = [self.mock_image, self.mock_image, self.mock_image]
        with patch('app.services.ocr_service.clean_ocr_text', return_value="Cleaned Combined Text") as mock_clean_ocr_text:
            result = self.ocr_service.ocr_images(mock_images, lang='vie')

            self.assertEqual(mock_ocr_image.call_count, len(mock_images))
            mock_clean_ocr_text.assert_called_once_with("Page Text\nPage Text\nPage Text")
            self.assertEqual(result, "Cleaned Combined Text")

    @patch.object(OCRService, 'ocr_image', side_effect=[Exception("OCR Failed"), "Second Page Text"])
    def test_ocr_images_with_partial_failure(self, mock_ocr_image):
        mock_images = [self.mock_image, self.mock_image]
        with patch('app.services.ocr_service.clean_ocr_text', return_value="Cleaned Combined Text") as mock_clean_ocr_text:
            result = self.ocr_service.ocr_images(mock_images, lang='eng')
            
            self.assertEqual(mock_ocr_image.call_count, len(mock_images))
            # The failed page will result in an empty string being added to the combined text
            mock_clean_ocr_text.assert_called_once_with("\nSecond Page Text")
            self.assertEqual(result, "Cleaned Combined Text")

if __name__ == '__main__':
    unittest.main()
