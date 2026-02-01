const IStorageService = require('../domain/interfaces/IStorageService');
const UploadFileDto = require('../domain/dtos/UploadFileDto');
const fs = require('fs').promises;

class StorageService extends IStorageService {
  constructor(localStorageProvider, s3StorageProvider, cloudinaryStorageProvider, imgbbStorageProvider, imgurStorageProvider) {
    super();
    this.localStorageProvider = localStorageProvider;
    this.s3StorageProvider = s3StorageProvider;
    this.cloudinaryStorageProvider = cloudinaryStorageProvider;
    this.imgbbStorageProvider = imgbbStorageProvider;
    this.imgurStorageProvider = imgurStorageProvider;
  }

  async uploadFile(fileDto) {
    let result;
    switch (fileDto.destination) {
      case 'local':
        result = await this.localStorageProvider.uploadFile(fileDto);
        break;
      case 's3':
        result = await this.s3StorageProvider.uploadFile(fileDto);
        break;
      case 'cloudinary':
        result = await this.cloudinaryStorageProvider.uploadFile(fileDto);
        break;
      case 'imgbb':
        result = await this.imgbbStorageProvider.uploadFile(fileDto);
        break;
      case 'imgur':
        result = await this.imgurStorageProvider.uploadFile(fileDto);
        break;
      case 'imgbb':
        result = await this.imgbbStorageProvider.uploadFile(fileDto);
        break;
      case 'imgur':
        result = await this.imgurStorageProvider.uploadFile(fileDto);
        break;
      default:
        throw new Error('Unsupported storage destination');
    }
    // Clean up the temporary file after upload, only if it was a file path
    if (typeof fileDto.filepath === 'string') {
      await fs.unlink(fileDto.filepath);
    }
    return result;
  }

  async uploadFileFromMessage(messageContent) {
    const { filename, mimetype, filepath, destination, data } = messageContent;
    // For message-based uploads, the 'data' field might contain the file content directly
    // Or we might need to fetch it from a temporary location if 'filepath' is provided and accessible
    // For simplicity, let's assume 'data' is base64 encoded content
    let fileBuffer;
    if (data) {
        fileBuffer = Buffer.from(data, 'base64');
    } else {
        // If data is not provided, we assume filepath is a local path accessible to the service
        fileBuffer = await fs.readFile(filepath);
    }

    const fileDto = new UploadFileDto({
        filename,
        mimetype,
        filepath: fileBuffer, // Pass buffer instead of path for direct upload
        destination,
    });
    // In this case, the filepath in dto is actually the buffer, so we need to adjust
    // the provider's upload method to handle buffer directly.
    // For now, let's just call the uploadFile with a modified DTO or an internal method.
    // This part needs careful design depending on how providers handle file content.
    // For this example, I will modify uploadFile to accept buffer if available.

    let result;
    switch (fileDto.destination) {
      case 'local':
        result = await this.localStorageProvider.uploadFile(fileDto);
        break;
      case 's3':
        result = await this.s3StorageProvider.uploadFile(fileDto);
        break;
      case 'imgur':
        result = await this.imgurStorageProvider.uploadFile(fileDto);
        break;
      default:
        throw new Error('Unsupported storage destination');
    }
    return result;
  }


  async getFile(filename, destination) {
    switch (destination) {
      case 'local':
        return this.localStorageProvider.getFile(filename);
      case 's3':
        return this.s3StorageProvider.getFile(filename);
      case 'cloudinary':
        return this.cloudinaryStorageProvider.getFile(filename);
      case 'imgbb':
        return this.imgbbStorageProvider.getFile(filename);
      case 'imgur':
        return this.imgurStorageProvider.getFile(filename);
      default:
        throw new Error('Unsupported storage destination');
    }
  }

  async deleteFile(filename, destination) {
    switch (destination) {
      case 'local':
        return this.localStorageProvider.deleteFile(filename);
      case 's3':
        return this.s3StorageProvider.deleteFile(filename);
      case 'cloudinary':
        return this.cloudinaryStorageProvider.deleteFile(filename);
      case 'imgbb':
        return this.imgbbStorageProvider.deleteFile(filename);
      case 'imgur':
        return this.imgurStorageProvider.deleteFile(filename);
      default:
        throw new Error('Unsupported storage destination');
    }
  }
}

module.exports = StorageService;
