
  const cloudinary = require('cloudinary').v2;
const amqp = require('amqplib');

// Mock RabbitMQ channel and connection
const mockChannel = {
  publish: jest.fn(),
  assertExchange: jest.fn(),
  assertQueue: jest.fn(),
  bindQueue: jest.fn(),
  consume: jest.fn(),
  ack: jest.fn(),
  nack: jest.fn(),
};

const mockConnection = {
  createChannel: jest.fn(() => mockChannel),
  close: jest.fn(),
};

jest.mock('amqplib', () => ({
  connect: jest.fn(() => mockConnection),
}));

// Mock Cloudinary
jest.mock('cloudinary', () => ({
  v2: {
    uploader: {
      destroy: jest.fn(),
    },
    config: jest.fn(),
  },
}));

describe('fileUploadConsumer', () => {
  // Now require the consumer *after* the mocks are set up
    const { processFileDeletionRequestedEvent, setChannel, setConnection } = require('../../src/consumers/fileUploadConsumer');
    const cloudinaryModule = require('cloudinary'); // Get the mocked cloudinary module
  
    beforeEach(() => {
      jest.clearAllMocks();
      // Mock cloudinary config to prevent actual network calls during config
      cloudinaryModule.v2.config.mockImplementation(() => {});
      // Set the mocked channel and connection in the consumer module
      setChannel(mockChannel);
      setConnection(mockConnection);
    });

  describe('processFileDeletionRequestedEvent', () => {
    it('should successfully process a deletion request and publish a completion event', async () => {
      const eventData = {
        file_id: 'test-file-id-123',
        file_path: 'https://res.cloudinary.com/example/image/upload/v1234567890/test_public_id.jpg',
        delete_hash: 'test_public_id',
        requested_by: 'user-123',
        family_id: 'family-456',
      };

      cloudinaryModule.v2.uploader.destroy.mockResolvedValueOnce({ result: 'ok' });

      // Call the function
      await processFileDeletionRequestedEvent(eventData);

      // Assert Cloudinary deletion was called correctly
      expect(cloudinaryModule.v2.uploader.destroy).toHaveBeenCalledTimes(1);
      expect(cloudinaryModule.v2.uploader.destroy).toHaveBeenCalledWith('test_public_id', {
        resource_type: 'image',
      });

      // Assert completion event was published
      expect(mockChannel.publish).toHaveBeenCalledTimes(1);
      expect(mockChannel.publish).toHaveBeenCalledWith(
        'file_upload_exchange',
        'file.deletion.completed',
        Buffer.from(
          JSON.stringify({
            file_id: 'test-file-id-123',
            is_success: true,
            error: null,
            family_id: 'family-456',
            requested_by: 'user-123',
          })
        )
      );
    });

    it('should handle deletion failure and publish a completion event with error', async () => {
      const eventData = {
        file_id: 'test-file-id-456',
        file_path: 'https://res.cloudinary.com/example/image/upload/v1234567890/fail_public_id.png',
        delete_hash: 'fail_public_id',
        requested_by: 'user-456',
        family_id: 'family-789',
      };

      cloudinaryModule.v2.uploader.destroy.mockResolvedValueOnce({ result: 'not found' });

      // Call the function
      await processFileDeletionRequestedEvent(eventData);

      // Assert Cloudinary deletion was called
      expect(cloudinaryModule.v2.uploader.destroy).toHaveBeenCalledTimes(1);
      expect(cloudinaryModule.v2.uploader.destroy).toHaveBeenCalledWith('fail_public_id', {
        resource_type: 'image',
      });

      // Assert completion event was published with error
      expect(mockChannel.publish).toHaveBeenCalledTimes(1);
      expect(mockChannel.publish).toHaveBeenCalledWith(
        'file_upload_exchange',
        'file.deletion.completed',
        Buffer.from(
          JSON.stringify({
            file_id: 'test-file-id-456',
            is_success: false,
            error: 'Cloudinary deletion failed: not found',
            family_id: 'family-789',
            requested_by: 'user-456',
          })
        )
      );
    });

    it('should handle deletion failure due to missing public_id/delete_hash and publish a completion event with error', async () => {
        const eventData = {
            file_id: 'test-file-id-789',
            file_path: 'invalid_path', // Malformed URL
            delete_hash: null, // Missing delete hash
            requested_by: 'user-789',
            family_id: 'family-012',
        };

        // Ensure cloudinary.uploader.destroy is NOT called in this case
        cloudinaryModule.v2.uploader.destroy.mockResolvedValueOnce({ result: 'ok' });

        await processFileDeletionRequestedEvent(eventData);

        expect(cloudinaryModule.v2.uploader.destroy).not.toHaveBeenCalled();

        expect(mockChannel.publish).toHaveBeenCalledTimes(1);
        expect(mockChannel.publish).toHaveBeenCalledWith(
            'file_upload_exchange',
            'file.deletion.completed',
            Buffer.from(
                JSON.stringify({
                    file_id: 'test-file-id-789',
                    is_success: false,
                    error: 'Cannot delete from Cloudinary: No public_id or deleteHash provided/found.',
                    family_id: 'family-012',
                    requested_by: 'user-789',
                })
            )
        );
    });

    it('should use extractPublicIdFromCloudinaryUrl if delete_hash is null', async () => {
        const eventData = {
          file_id: 'test-file-id-extracted',
          file_path: 'https://res.cloudinary.com/example/image/upload/v1234567890/extracted_public_id.jpg',
          delete_hash: null,
          requested_by: 'user-extracted',
          family_id: 'family-extracted',
        };
  
        cloudinaryModule.v2.uploader.destroy.mockResolvedValueOnce({ result: 'ok' });
  
        await processFileDeletionRequestedEvent(eventData);
  
        expect(cloudinaryModule.v2.uploader.destroy).toHaveBeenCalledWith('extracted_public_id', expect.any(Object));
      });
  });
});
