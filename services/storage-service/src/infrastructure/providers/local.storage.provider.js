const IStorageService = require('../../domain/interfaces/IStorageService');
const fs = require('fs').promises;
const path = require('path');

class LocalStorageProvider extends IStorageService {
  constructor(uploadDir) {
    super();
    this.uploadDir = uploadDir;
    // Ensure the upload directory exists
    fs.mkdir(this.uploadDir, { recursive: true }).catch(console.error);
  }

  async uploadFile(fileDto) {
    const filename = `${Date.now()}-${fileDto.filename}`;
    const destinationPath = path.join(this.uploadDir, filename);

    // fileDto.filepath can be a string (path to temp file) or a Buffer (from message broker)
    if (Buffer.isBuffer(fileDto.filepath)) {
      await fs.writeFile(destinationPath, fileDto.filepath);
    } else {
      await fs.copyFile(fileDto.filepath, destinationPath);
      await fs.unlink(fileDto.filepath);
    }

    return {
      filename,
      mimetype: fileDto.mimetype,
      url: `/local/${filename}`, // A URL that the API can use to serve the file
      filepath: destinationPath, // The actual path on the server
    };
  }

  async getFile(filename) {
    const filePath = path.join(this.uploadDir, filename);
    try {
      await fs.access(filePath); // Check if file exists
      return {
        filename,
        filepath: filePath,
        url: `/local/${filename}`,
      };
    } catch (error) {
      if (error.code === 'ENOENT') {
        return null; // File not found
      }
      throw error;
    }
  }

  async deleteFile(filename) {
    const filePath = path.join(this.uploadDir, filename);
    try {
      await fs.unlink(filePath);
      return true;
    } catch (error) {
      if (error.code === 'ENOENT') {
        return false; // File not found, nothing to delete
      }
      throw error;
    }
  }
}

module.exports = LocalStorageProvider;
