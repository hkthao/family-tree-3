import pytest
from unittest.mock import AsyncMock
from uuid import uuid4

# Import the actual functions/classes to be tested
from app.api import _run_restoration_pipeline, jobs
from app.models.job import RestorationJob, RestorationStatus

# Import the getter functions for the services
from app.services.replicate_service import get_replicate_service, ReplicateService
from app.services.backend_api_service import get_backend_api_service, BackendApiService

# Fixture to clear jobs before each test
@pytest.fixture(autouse=True)
def clear_jobs():
    jobs.clear()

# Fixture for mocking the getter functions for services
@pytest.fixture
def mock_getters(mocker):
    mock_replicate_service_instance = mocker.Mock(spec=ReplicateService)
    mock_backend_api_service_instance = mocker.Mock(spec=BackendApiService)

    # Mock the getter functions to return our mock instances
    mocker.patch('app.api.get_replicate_service', return_value=mock_replicate_service_instance)
    mocker.patch('app.api.get_backend_api_service', return_value=mock_backend_api_service_instance)
    
    # Mock methods on the mock instances
    mock_replicate_service_instance.restore_image_pipeline = AsyncMock()
    mock_backend_api_service_instance.update_image_restoration_job_status = AsyncMock()
    mock_backend_api_service_instance.client = mocker.Mock() # Add this line
    mock_backend_api_service_instance.client.patch = AsyncMock() # Also mock httpx client's patch method

    return mock_replicate_service_instance, mock_backend_api_service_instance

@pytest.mark.asyncio
async def test_successful_restoration_updates_backend(mock_getters):
    mock_replicate_service, mock_backend_api_service = mock_getters
    job_id = uuid4()
    image_url = "http://example.com/original.jpg"
    restored_url = "http://example.com/restored.jpg"

    mock_replicate_service.restore_image_pipeline.return_value = {
        "restoredUrl": restored_url,
        "pipeline": ["GFPGAN", "Real-ESRGAN"],
        "status": RestorationStatus.COMPLETED.value,
        "error": None,
    }

    jobs[job_id] = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )

    await _run_restoration_pipeline(
        job_id,
        image_url,
        mock_replicate_service, # Pass the mock instance
        mock_backend_api_service # Pass the mock instance
    )

    mock_backend_api_service.update_image_restoration_job_status.assert_not_called()
    assert jobs[job_id].status == RestorationStatus.COMPLETED
    assert jobs[job_id].restored_url == restored_url
    assert jobs[job_id].error is None

@pytest.mark.asyncio
async def test_failed_restoration_updates_backend(mock_getters):
    mock_replicate_service, mock_backend_api_service = mock_getters
    job_id = uuid4()
    image_url = "http://example.com/original.jpg"
    error_message = "GFPGAN failed: some error"

    mock_replicate_service.restore_image_pipeline.return_value = {
        "restoredUrl": None,
        "pipeline": ["GFPGAN"],
        "status": RestorationStatus.FAILED.value,
        "error": error_message,
    }

    jobs[job_id] = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )

    await _run_restoration_pipeline(
        job_id,
        image_url,
        mock_replicate_service,
        mock_backend_api_service
    )

    mock_backend_api_service.update_image_restoration_job_status.assert_not_called()
    assert jobs[job_id].status == RestorationStatus.FAILED
    assert jobs[job_id].restored_url is None
    assert jobs[job_id].error == error_message

@pytest.mark.asyncio
async def test_exception_during_restoration_updates_backend(mock_getters):
    mock_replicate_service, mock_backend_api_service = mock_getters
    job_id = uuid4()
    image_url = "http://example.com/original.jpg"
    exception_message = "Unexpected error during pipeline"

    mock_replicate_service.restore_image_pipeline.side_effect = Exception(exception_message)

    jobs[job_id] = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )

    await _run_restoration_pipeline(
        job_id,
        image_url,
        mock_replicate_service,
        mock_backend_api_service
    )

    mock_backend_api_service.update_image_restoration_job_status.assert_not_called()
    assert jobs[job_id].status == RestorationStatus.FAILED
    assert jobs[job_id].restored_url is None
    assert jobs[job_id].error == exception_message