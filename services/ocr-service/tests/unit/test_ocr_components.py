import unittest
from unittest.mock import patch, MagicMock
from PIL import Image
import io
import numpy as np

# Import the functions to be tested
from app.utils.file_loader import load_image_from_bytes, load_pdf_as_images
from app.utils.image_preprocess import preprocess_image_for_ocr
from app.utils.text_cleaner import clean_ocr_text

class TestFileLoader(unittest.TestCase):
    def test_load_image_from_bytes(self):
        # Create a dummy image in bytes
        dummy_image = Image.new('RGB', (10, 10))
        byte_arr = io.BytesIO()
        dummy_image.save(byte_arr, format='PNG')
        image_bytes = byte_arr.getvalue()

        # Mock Image.open to return our dummy image
        with patch('PIL.Image.open', return_value=dummy_image) as mock_open:
            loaded_image = load_image_from_bytes(image_bytes)
            mock_open.assert_called_once()
            self.assertIsInstance(loaded_image, Image.Image)
            self.assertEqual(loaded_image.size, (10, 10))

    @patch('app.utils.file_loader.convert_from_bytes')
    def test_load_pdf_as_images(self, mock_convert_from_bytes):
        # Mock convert_from_bytes to return a list of dummy images
        mock_image1 = Image.new('RGB', (20, 20))
        mock_image2 = Image.new('RGB', (20, 20))
        mock_convert_from_bytes.return_value = [mock_image1, mock_image2]

        pdf_bytes = b"%PDF-1.4...\n%%EOF" # Dummy PDF bytes

        loaded_images = load_pdf_as_images(pdf_bytes)

        mock_convert_from_bytes.assert_called_once_with(pdf_bytes, dpi=300)
        self.assertIsInstance(loaded_images, list)
        self.assertEqual(len(loaded_images), 2)
        self.assertIsInstance(loaded_images[0], Image.Image)
        self.assertEqual(loaded_images[0].size, (20, 20))

class TestImagePreprocess(unittest.TestCase):
    def test_preprocess_image_for_ocr(self):
        # Create a dummy grayscale image
        dummy_image = Image.new('L', (100, 100), color=128)
        dummy_image_np = np.array(dummy_image) # Convert to numpy array
        
        # Mock cv2.threshold (since we're testing the wrapper, not cv2 itself)
        # It should return a tuple: retval (threshold value) and a thresholded image
        mock_thresh_image_np = (
            255 * (dummy_image_np > 127)
        ).astype('uint8') # Simulate thresholding to black/white
        
        with patch('cv2.threshold', return_value=(127, mock_thresh_image_np)) as mock_threshold:
            processed_image = preprocess_image_for_ocr(dummy_image)

            self.assertIsInstance(processed_image, Image.Image)
            self.assertEqual(processed_image.mode, 'L') # Should remain grayscale
            mock_threshold.assert_called_once()
            # Further checks could involve comparing pixel data if mock_thresh_image_np was more complex

class TestTextCleaner(unittest.TestCase):
    def test_clean_ocr_text_empty_lines(self):
        dirty_text = "Line 1\n\n  Line 2  \n\n\nLine 3\n"
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Line 1\nLine 2\nLine 3"
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_no_empty_lines(self):
        dirty_text = "Line 1\nLine 2\nLine 3"
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Line 1\nLine 2\nLine 3"
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_only_whitespace(self):
        dirty_text = "   \n\t\n     "
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = ""
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_empty_string(self):
        dirty_text = ""
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = ""
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_leading_trailing_whitespace(self):
        dirty_text = "  Hello \nWorld  \n"
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Hello\nWorld"
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_special_characters_and_spaces(self):
        dirty_text = "T@est S!tring w!th $pec!@l Ch@r@cter$ and   extra    spaces."
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "T est S tring w th pec l Ch r cter and extra spaces."
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_vietnamese_unicode_and_special_chars(self):
        dirty_text = "Tiếng Việt! có_dấu. @#$%^&*() "
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Tiếng Việt có dấu. % ()"
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_unicode_normalization(self):
        # Example of decomposed unicode (NFD) that should be normalized to NFC
        dirty_text = "Tiếng Việt\u0323" # e + combining dot below
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Tiếng Việṭ" # e with dot below as a single character
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_complex_case(self):
        dirty_text = "  Hello, W@rld!  \n\n  Đây là T!ếxt_tiếng Việt.  123$%^  \t\n  New Line.  "
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Hello, W rld\nĐây là T ếxt tiếng Việt. 123 %\nNew Line."
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_repeated_noise(self):
        dirty_text = "Text....with----noise|||here***and"
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "Text with noise here and"
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_unallowed_characters(self):
        dirty_text = "This is some text with @#$^*_ illegal chars. And some % allowed."
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "This is some text with illegal chars. And some % allowed."
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_low_alpha_ratio_removed(self):
        # Line with low alpha ratio (e.g., mostly numbers or symbols) should be removed
        dirty_text = "This is a valid line.\n1234567890\n---++++===\nAnother good line."
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "This is a valid line.\nAnother good line."
        self.assertEqual(cleaned_text, expected_text)

    def test_clean_ocr_text_sufficient_alpha_ratio_kept(self):
        # Line with sufficient alpha ratio should be kept, even if it has numbers
        dirty_text = "This line has 123 numbers and letters."
        cleaned_text = clean_ocr_text(dirty_text)
        expected_text = "This line has 123 numbers and letters."
        self.assertEqual(cleaned_text, expected_text)


if __name__ == '__main__':
    unittest.main()
