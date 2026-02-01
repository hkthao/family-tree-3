const amqp = require('amqplib');
const cloudinary = require('cloudinary').v2;
const fs = require('fs/promises'); // For async file operations
const path = require('path');

// Configuration constants - these should ideally come from a config file or environment variables
const RABBITMQ_HOSTNAME = process.env.RABBITMQ__HOSTNAME || 'localhost';
const RABBITMQ_USERNAME = process.env.RABBITMQ__USERNAME || 'guest';
const RABBITMQ_PASSWORD = process.env.RABBITMQ__PASSWORD || 'guest';
const RABBITMQ_PORT = process.env.RABBITMQ__PORT || 5672;

const CLOUDINARY_CLOUD_NAME = process.env.CLOUDINARY_CLOUD_NAME;
const CLOUDINARY_API_KEY = process.env.CLOUDINARY_API_KEY;
const CLOUDINARY_API_SECRET = process.env.CLOUDINARY_API_SECRET;
const CLOUDINARY_ROOTFOLDER = process.env.CLOUDINARY_ROOTFOLDER || 'family-tree-uploads';

const FILE_UPLOAD_EXCHANGE = 'file_upload_exchange';
const FILE_UPLOAD_REQUESTED_ROUTING_KEY = 'file.upload.requested';
const FILE_UPLOAD_COMPLETED_ROUTING_KEY = 'file.upload.completed';
const FILE_DELETION_REQUESTED_ROUTING_KEY = 'file.deletion.requested'; // NEW
const FILE_DELETION_COMPLETED_ROUTING_KEY = 'file.deletion.completed'; // NEW

const UPLOAD_QUEUE_NAME = 'storage_service_file_upload_requested_queue'; // Renamed
const DELETION_QUEUE_NAME = 'storage_service_file_deletion_requested_queue'; // NEW

// Configure Cloudinary
cloudinary.config({
  cloud_name: CLOUDINARY_CLOUD_NAME,
  api_key: CLOUDINARY_API_KEY,
  api_secret: CLOUDINARY_API_SECRET,
  secure: true
});

let connection;
let channel;

async function connectRabbitMQ() {
  try {
    connection = await amqp.connect(`amqp://${RABBITMQ_USERNAME}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOSTNAME}:${RABBITMQ_PORT}`);
    channel = await connection.createChannel();

    await channel.assertExchange(FILE_UPLOAD_EXCHANGE, 'topic', { durable: true });

    // Setup for Upload Requests
    await channel.assertQueue(UPLOAD_QUEUE_NAME, { durable: true });
    await channel.bindQueue(UPLOAD_QUEUE_NAME, FILE_UPLOAD_EXCHANGE, FILE_UPLOAD_REQUESTED_ROUTING_KEY);

    console.log(`[Storage Service] Connected to RabbitMQ. Listening for upload requests on exchange "${FILE_UPLOAD_EXCHANGE}" with routing key "${FILE_UPLOAD_REQUESTED_ROUTING_KEY}".`);

    channel.consume(UPLOAD_QUEUE_NAME, async (msg) => {
      if (msg !== null) {
        try {
          const content = JSON.parse(msg.content.toString());
          console.log('[Storage Service] Received FileUploadRequestedEvent:', content);

          await processFileUploadRequestedEvent(content);
          channel.ack(msg);
        } catch (error) {
          console.error('[Storage Service] Error processing upload request:', error);
          channel.nack(msg, false, true); // Requeue = true
        }
      }
    });

    // Setup for Deletion Requests
    await channel.assertQueue(DELETION_QUEUE_NAME, { durable: true });
    await channel.bindQueue(DELETION_QUEUE_NAME, FILE_UPLOAD_EXCHANGE, FILE_DELETION_REQUESTED_ROUTING_KEY);

    console.log(`[Storage Service] Listening for deletion requests on exchange "${FILE_UPLOAD_EXCHANGE}" with routing key "${FILE_DELETION_REQUESTED_ROUTING_KEY}".`);

    channel.consume(DELETION_QUEUE_NAME, async (msg) => {
      if (msg !== null) {
        try {
          const content = JSON.parse(msg.content.toString());
          console.log('[Storage Service] Received FileDeletionRequestedEvent:', content);

          await processFileDeletionRequestedEvent(content);
          channel.ack(msg);
        } catch (error) {
          console.error('[Storage Service] Error processing deletion request:', error);
          channel.nack(msg, false, true); // Requeue = true
        }
      }
    });

  } catch (error) {
    console.error('[Storage Service] Failed to connect to RabbitMQ:', error);
    setTimeout(connectRabbitMQ, 5000); // Retry after 5 seconds
  }
}

async function processFileUploadRequestedEvent(eventData) {
  const { file_id, original_file_name, temp_local_path, content_type, folder, uploaded_by, file_size, family_id } = eventData;

  let finalFileUrl = null;
  let deleteHash = null;
  let uploadError = null;

  try {
    // Read the file from the shared local storage
    const fileBuffer = await fs.readFile(TempLocalPath);
    console.log(`[Storage Service] Read file from local path: ${TempLocalPath}`);

    // Determine the Cloudinary folder
    const cloudinaryFolder = Folder ? path.join(CLOUDINARY_ROOTFOLDER, Folder) : CLOUDINARY_ROOTFOLDER;

    // Upload to Cloudinary
    // Use the buffer directly for upload
    const uploadResult = await cloudinary.uploader.upload(`data:${ContentType};base64,${fileBuffer.toString('base64')}`, {
      resource_type: 'auto', // Automatically determine resource type (image, video, raw)
      public_id: OriginalFileName.split('.')[0] + '-' + FileId, // Use original filename without extension + fileId as public ID
      folder: cloudinaryFolder,
      overwrite: true,
      use_filename: true,
      unique_filename: false,
    });

    console.log('[Storage Service] Cloudinary upload result:', uploadResult);

    if (uploadResult && uploadResult.secure_url) {
      finalFileUrl = uploadResult.secure_url;
      deleteHash = uploadResult.public_id; // For cloudinary, public_id is used for deletion
      console.log(`[Storage Service] File uploaded to Cloudinary: ${finalFileUrl}`);
    } else {
      uploadError = 'Cloudinary upload failed: No secure_url returned.';
      console.error(`[Storage Service] Cloudinary upload failed for ${OriginalFileName}: No secure_url returned.`);
    }

  } catch (error) {
    uploadError = `Error during Cloudinary upload: ${error.message}`;
    console.error(`[Storage Service] Error processing file ${OriginalFileName}:`, error);
  } finally {
    // Clean up the local temporary file
    try {
      await fs.unlink(TempLocalPath);
      console.log(`[Storage Service] Deleted temporary local file: ${TempLocalPath}`);
    } catch (unlinkError) {
      console.error(`[Storage Service] Error deleting temporary local file ${TempLocalPath}:`, unlinkError);
    }
  }

  // Publish FileUploadCompletedEvent back to backend
  const fileUploadCompletedEvent = {
    FileId: FileId,
    OriginalFileName: OriginalFileName,
    FinalFileUrl: finalFileUrl,
    DeleteHash: deleteHash,
    UploadedBy: UploadedBy,
    FamilyId: FamilyId,
    Error: uploadError // Include error if any
  };

  try {
    await channel.publish(FILE_UPLOAD_EXCHANGE, FILE_UPLOAD_COMPLETED_ROUTING_KEY, Buffer.from(JSON.stringify(fileUploadCompletedEvent)));
    console.log('[Storage Service] Published FileUploadCompletedEvent:', fileUploadCompletedEvent);
  } catch (publishError) {
    console.error('[Storage Service] Failed to publish FileUploadCompletedEvent:', publishError);
  }
}

async function processFileDeletionRequestedEvent(eventData) {
  const { FileId, FilePath, DeleteHash, RequestedBy, FamilyId } = eventData;

  let isSuccess = false;
  let deletionError = null;

  try {
    // For Cloudinary, we primarily use the public_id (stored as DeleteHash in our system) for deletion.
    // If DeleteHash is not provided, we might try to extract public_id from FilePath.
    const publicIdToDelete = DeleteHash || extractPublicIdFromCloudinaryUrl(FilePath);

    if (!publicIdToDelete) {
      deletionError = 'Cannot delete from Cloudinary: No public_id or deleteHash provided/found.';
      console.error(`[Storage Service] ${deletionError} FileId: ${FileId}, FilePath: ${FilePath}`);
    } else {
      const deleteResult = await cloudinary.uploader.destroy(publicIdToDelete, {
        resource_type: 'image' // Assuming images for now. Adjust as needed if raw/video types are handled.
      });

      console.log('[Storage Service] Cloudinary deletion result:', deleteResult);

      if (deleteResult && deleteResult.result === 'ok') {
        isSuccess = true;
        console.log(`[Storage Service] File with public_id ${publicIdToDelete} deleted from Cloudinary.`);
      } else {
        deletionError = `Cloudinary deletion failed: ${deleteResult?.result || 'Unknown error'}`;
        console.error(`[Storage Service] Cloudinary deletion failed for public_id ${publicIdToDelete}: ${deleteResult?.result || 'Unknown error'}`);
      }
    }

  } catch (error) {
    deletionError = `Error during Cloudinary deletion: ${error.message}`;
    console.error(`[Storage Service] Error processing file deletion for FileId ${FileId}:`, error);
  }

  // Publish FileDeletionCompletedEvent back to backend
  const fileDeletionCompletedEvent = {
    FileId: FileId,
    IsSuccess: isSuccess,
    Error: deletionError,
    FamilyId: FamilyId,
    RequestedBy: RequestedBy // Pass along for context if needed by backend
  };

  try {
    await channel.publish(FILE_UPLOAD_EXCHANGE, FILE_DELETION_COMPLETED_ROUTING_KEY, Buffer.from(JSON.stringify(fileDeletionCompletedEvent)));
    console.log('[Storage Service] Published FileDeletionCompletedEvent:', fileDeletionCompletedEvent);
  } catch (publishError) {
    console.error('[Storage Service] Failed to publish FileDeletionCompletedEvent:', publishError);
  }
}

function extractPublicIdFromCloudinaryUrl(url) {
  if (!url || typeof url !== 'string') {
    return null;
  }
  // Example Cloudinary URL: https://res.cloudinary.com/<cloud_name>/image/upload/v<version>/<public_id>.<extension>
  // or https://res.cloudinary.com/<cloud_name>/image/upload/<public_id>.<extension>
  const match = url.match(/(?:upload|fetch)\/(?:v\d+\/)?([^\/\.]+)/);
  if (match && match[1]) {
    return match[1];
  }
  return null;
}

// Ensure the directory for consumers exists (This line is not strictly needed for this file)
// const consumersDir = path.join(__dirname, 'consumers');
// if (!fs.existsSync(consumersDir)) {
//   fs.mkdirSync(consumersDir, { recursive: true });
// }

module.exports = {
  connectRabbitMQ
};