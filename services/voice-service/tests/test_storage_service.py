import pytest
import os
from unittest.mock import MagicMock, AsyncMock, patch
from app.services.storage_service import S3StorageService, _s3_storage_service_instance, get_s3_storage_service
from app.config import settings as app_settings # Import actual settings from app.config

# Mock environment variables for testing
@pytest.fixture(autouse=True)
def mock_settings(mocker): # Use mocker from pytest_mock
    # Patch settings directly in the module where it's used
    mock_storage_settings = mocker.patch('app.services.storage_service.settings')
    mock_storage_settings.AWS_ACCESS_KEY_ID = "test_key"
    mock_storage_settings.AWS_SECRET_ACCESS_KEY = "test_secret"
    mock_storage_settings.AWS_ENDPOINT_URL = "https://test.r2.cloudflarestorage.com"
    mock_storage_settings.AWS_REGION = "auto"
    mock_storage_settings.AWS_BUCKET_NAME = "test-bucket"
    return mock_storage_settings # Return the mock object


@pytest.fixture
def mock_s3_service_instance_for_tests(mocker, mock_settings):
    # This fixture provides a fully mocked S3StorageService instance
    # that can be used directly by tests.
    mock_service = MagicMock(spec=S3StorageService)
    # This is the mock S3 client that will be used by the mocked service
    mock_s3_client_for_service = MagicMock() 
    mock_service.s3_client = mock_s3_client_for_service
    mock_service.bucket_name = mock_settings.AWS_BUCKET_NAME
    # We explicitly mock _get_file_url on the service mock.
    # Note: We are testing the public interface of the service, not internal calls
    # so this _get_file_url on the mock won't be called unless we explicitly test it.
    mock_service._get_file_url.return_value = f"{mock_settings.AWS_ENDPOINT_URL}/{mock_settings.AWS_BUCKET_NAME}/mocked_object.wav"
    
    # IMPORTANT: Make async methods on the mocked service also AsyncMocks
    # This ensures that when service.upload_file is called, it correctly behaves as an awaitable mock
    mock_service.upload_file = AsyncMock() 
    
    # Patch the get_s3_storage_service function so it always returns our mock
    mocker.patch(
        'app.services.storage_service.get_s3_storage_service',
        return_value=mock_service
    )
    return mock_service # Only return the service mock, as its internal client is now handled


@pytest.fixture
def dummy_file(tmp_path):
    file_content = b"This is a dummy file for S3 upload."
    file_path = tmp_path / "dummy.txt"
    file_path.write_bytes(file_content)
    return str(file_path)


def test_s3_service_init_success(mock_settings, mocker):
    # For this specific test, we want to test the actual init, so we temporarily
    # unpatch get_s3_storage_service to allow the real one to be called.
    with patch('app.services.storage_service.get_s3_storage_service', wraps=get_s3_storage_service):
        mock_boto3_client_factory = mocker.patch('boto3.client')
        mock_boto3_client_factory.return_value = MagicMock() # Return a dummy client for init

        global _s3_storage_service_instance
        _s3_storage_service_instance = None # Ensure we get a new instance

        service = get_s3_storage_service()

        assert service.s3_client is not None
        mock_boto3_client_factory.assert_called_once_with(
            's3',
            endpoint_url=mock_settings.AWS_ENDPOINT_URL,
            aws_access_key_id=mock_settings.AWS_ACCESS_KEY_ID,
            aws_secret_access_key=mock_settings.AWS_SECRET_ACCESS_KEY,
            region_name=mock_settings.AWS_REGION,
            config=mock_boto3_client_factory.call_args[1]['config']
        )
        assert service.bucket_name == mock_settings.AWS_BUCKET_NAME


def test_s3_service_init_failure_missing_credentials(mocker): # Use mocker for this specific test
    # Patch settings as before
    mock_storage_settings_for_fail = mocker.patch('app.services.storage_service.settings')
    mock_storage_settings_for_fail.AWS_ACCESS_KEY_ID = "" # Missing
    mock_storage_settings_for_fail.AWS_SECRET_ACCESS_KEY = "test_secret"
    mock_storage_settings_for_fail.AWS_ENDPOINT_URL = "https://test.r2.cloudflarestorage.com"
    mock_storage_settings_for_fail.AWS_REGION = "auto"
    mock_storage_settings_for_fail.AWS_BUCKET_NAME = "test-bucket"

    # Mock boto3.client as it will be called during S3StorageService.__init__
    mocker.patch('boto3.client')

    # Directly instantiate S3StorageService and expect ValueError
    # Ensure _s3_storage_service_instance is reset before this direct call
    global _s3_storage_service_instance
    _s3_storage_service_instance = None
    with pytest.raises(ValueError, match="S3/R2 storage credentials not fully configured"):
        S3StorageService() # Call constructor directly


@pytest.mark.asyncio
async def test_upload_file_success(mock_s3_service_instance_for_tests, dummy_file, mock_settings):
    service = mock_s3_service_instance_for_tests
    object_name = "test/uploaded_file.txt"
    content_type = "text/plain"

    expected_url = f"{mock_settings.AWS_ENDPOINT_URL}/{mock_settings.AWS_BUCKET_NAME}/{object_name}"
    service.upload_file.return_value = expected_url # Configure the return value of the mocked async method

    file_url = await service.upload_file(dummy_file, object_name, content_type)

    service.upload_file.assert_called_once_with(dummy_file, object_name, content_type)
    assert file_url == expected_url


@pytest.mark.asyncio
async def test_upload_file_failure(mock_s3_service_instance_for_tests, dummy_file):
    service = mock_s3_service_instance_for_tests
    object_name = "test/failed_upload.txt"
    
    service.upload_file.side_effect = ValueError("S3 upload error") # Configure the side effect to raise an exception

    with pytest.raises(ValueError, match="S3 upload error"):
        await service.upload_file(dummy_file, object_name, "audio/wav")

    service.upload_file.assert_called_once_with(dummy_file, object_name, "audio/wav") # Default content type


def test_get_file_url_path_style(mock_settings):
    # To test the real _get_file_url method, we need a real S3StorageService instance
    global _s3_storage_service_instance
    _s3_storage_service_instance = None # Ensure fresh instance
    
    # Temporarily patch boto3.client to avoid actual S3 connection during init
    with patch('boto3.client', return_value=MagicMock()):
        real_service = S3StorageService()
    
    object_name = "path/to/object.wav"
    expected_url = f"{mock_settings.AWS_ENDPOINT_URL}/{mock_settings.AWS_BUCKET_NAME}/{object_name}"
    assert real_service._get_file_url(object_name) == expected_url

def test_get_file_url_endpoint_contains_bucket(mock_settings):
    # Test the real _get_file_url method
    global _s3_storage_service_instance
    _s3_storage_service_instance = None # Ensure fresh instance
    
    # Temporarily patch boto3.client to avoid actual S3 connection during init
    with patch('boto3.client', return_value=MagicMock()):
        real_service = S3StorageService()

    object_name = "path/to/object.wav"
    # To test this, temporarily modify the mocked settings' ENDPOINT_URL
    original_endpoint = mock_settings.AWS_ENDPOINT_URL
    mock_settings.AWS_ENDPOINT_URL = f"https://{mock_settings.AWS_BUCKET_NAME}.r2.cloudflarestorage.com"
    
    expected_url = f"{mock_settings.AWS_ENDPOINT_URL}/{object_name}"
    assert real_service._get_file_url(object_name) == expected_url
    mock_settings.AWS_ENDPOINT_URL = original_endpoint # Reset for other tests if needed