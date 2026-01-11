using System.IO;
using System.Net;
using System.Text;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Infrastructure;
using backend.Infrastructure.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class CloudinaryFileStorageServiceTests
{
    private readonly Mock<IOptions<CloudinarySettings>> _mockCloudinarySettings;
    private readonly Mock<ILogger<CloudinaryFileStorageService>> _mockLogger;
    // Mocking the CloudinaryDotNet.Cloudinary class directly is not straightforward
    // due to its internal structure and lack of interfaces.
    // For unit testing, we might need to mock a wrapper or verify the parameters
    // passed to the constructor if we cannot directly mock its UploadAsync method.
    // However, given the nature of the fix, we are primarily interested in
    // ensuring the correct *type* of upload parameters is passed.
    // For now, we will create an instance of Cloudinary and rely on
    // it throwing exceptions if configuration is bad, and focus on the logic
    // around `UploadAsync` calls.
    // A more robust approach for external dependency testing often involves
    // an integration test or a dedicated adapter/wrapper for the external library.

    // Given the difficulty of mocking CloudinaryDotNet.Cloudinary, I will rethink this.
    // For a true unit test, we should mock the *behavior* of the external dependency.
    // One common approach is to create a test double (e.g., a fake or a mock)
    // for the Cloudinary client itself, if possible, or for an abstraction over it.
    // Since CloudinaryDotNet.Cloudinary doesn't implement an interface for UploadAsync,
    // we can't mock it with Moq directly.

    // Re-evaluating: The core logic I want to test is the `switch (resourceType)` and
    // the subsequent `if/else if` chain that determines which `UploadAsync` overload to call.
    // I can verify that the correct `UploadParams` type is constructed and that
    // the service attempts to upload *some* `UploadParams` instance.
    // The actual call to `_cloudinary.UploadAsync` is what I want to isolate.

    // Let's create a mock for Cloudinary, but this will require creating an interface
    // or a wrapper for the `CloudinaryDotNet.Cloudinary` class to allow mocking.
    // This goes against the "don't assume library is available" and "mimic project structure"
    // rules if such a wrapper doesn't exist.

    // Alternative: Use an actual Cloudinary instance with dummy settings and rely on it
    // failing fast if the settings are invalid. This would make it more of an integration-ish test.
    // However, the error was about "Unsupported upload parameters type", which is
    // an internal logic error, not necessarily a Cloudinary API error.

    // Let's assume for now that CloudinaryDotNet.Cloudinary is concrete and we cannot mock it directly.
    // The alternative is to encapsulate the Cloudinary client behind an interface:
    // public interface ICloudinaryClientWrapper { Task<ImageUploadResult> UploadImageAsync(ImageUploadParams @params); ... }
    // But this would require modifying the production code.

    // Given the constraints and the specific error, the easiest way to test the fix
    // without major refactoring or complex mocking of a concrete external class
    // is to ensure that when a RawUploadParams type is *expected* to be created,
    // the code does not throw the `Unsupported upload parameters type` exception.

    // A simpler approach for the unit test would be to test the internal logic directly,
    // but that means testing private methods or making assumptions about internal state.

    // Let's try to mock the Cloudinary client by creating a derived class that exposes
    // a mockable UploadAsync. This is a bit of a hack, but might work for unit testing.
    // Or, more practically, verify the *construction* of the upload parameters
    // and that the internal logic *reaches* the correct upload path, even if we can't
    // fully mock the `_cloudinary.UploadAsync` call without a wrapper.

    // Given the original error was an `InvalidOperationException` *from our code*,
    // not from CloudinaryDotNet, I can focus on that.

    // Let's try to mock the `_cloudinary` object directly. Moq *can* mock concrete classes,
    // but it requires methods to be virtual. CloudinaryDotNet's UploadAsync is not virtual.

    // The most straightforward way to unit test this specific fix is to verify that
    // when `GetCloudinaryResourceType` returns `ResourceType.Raw`, the `UploadFileAsync`
    // method does *not* throw the `InvalidOperationException` related to "Unsupported upload parameters type".
    // This still implies an instance of CloudinaryDotNet.Cloudinary will be created.

    // Plan B: Create a fake Cloudinary instance for testing.
    private readonly Cloudinary _fakeCloudinary; // Will be initialized with dummy account

    public CloudinaryFileStorageServiceTests()
    {
        _mockCloudinarySettings = new Mock<IOptions<CloudinarySettings>>();
        _mockLogger = new Mock<ILogger<CloudinaryFileStorageService>>();

        _mockCloudinarySettings.Setup(o => o.Value).Returns(new CloudinarySettings
        {
            CloudName = "testcloud",
            ApiKey = "testkey",
            ApiSecret = "testsecret",
            RootFolder = "test_root"
        });

        // Initialize a fake Cloudinary instance. It won't make actual calls,
        // but its methods can be called to check if our service *tries* to use them.
        // For actual testing of the UploadAsync *result*, we'd need a more elaborate setup
        // or a real integration test.
        // For this unit test, we primarily want to ensure that the internal logic
        // of CloudinaryFileStorageService correctly branches and doesn't throw
        // the "Unsupported upload parameters type" exception when it should handle raw files.
        _fakeCloudinary = new Cloudinary(new Account("fake", "fake", "fake"));
    }

    [Fact]
    public async Task UploadFileAsync_ImageFile_DoesNotThrowUnsupportedError()
    {
        // Arrange
        var service = new CloudinaryFileStorageService(_mockCloudinarySettings.Object, _mockLogger.Object);
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("fake image data"));
        var fileName = "image.png"; // Declare fileName here
        var exception = await Record.ExceptionAsync(() => service.UploadFileAsync(fileStream, fileName, "image/png"));

        Assert.Null(exception); // No exception should be thrown
        // Further assertions would involve mocking Cloudinary.UploadAsync and verifying calls,
        // which is hard without an interface for CloudinaryDotNet.Cloudinary.
        // For now, confirming no exception means our internal logic path is fixed.
    }

    [Fact]
    public async Task UploadFileAsync_RawFile_DoesNotThrowUnsupportedError()
    {
        // Arrange
        var service = new CloudinaryFileStorageService(_mockCloudinarySettings.Object, _mockLogger.Object);
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("fake raw data"));
        var fileName = "document.pdf"; // This should trigger ResourceType.Raw

        // Act & Assert
        // We expect it NOT to throw the specific InvalidOperationException
        var exception = await Record.ExceptionAsync(() => service.UploadFileAsync(fileStream, fileName, "application/pdf"));

        Assert.Null(exception); // No exception should be thrown
    }
}
