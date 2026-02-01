const IStorageService = require('../../domain/interfaces/IStorageService');
const cloudinary = require('cloudinary').v2;
const fs = require('fs').promises;

class CloudinaryStorageProvider extends IStorageService {
  constructor(config) {
    super();
    cloudinary.config({
      cloud_name: config.cloudName,
      api_key: config.apiKey,
      api_secret: config.apiSecret,
    });
  }

  async uploadFile(fileDto) {
    let fileContent;
    let uploadResult;

    if (Buffer.isBuffer(fileDto.filepath)) {
      // Cloudinary expects a data URI or a path for buffer uploads.
      // Convert buffer to data URI for direct upload.
      const base64Image = fileDto.filepath.toString('base64');
      const dataUri = `data:${fileDto.mimetype};base64,${base64Image}`;
      uploadResult = await cloudinary.uploader.upload(dataUri, {
        resource_type: "auto", // Automatically detect file type
        public_id: `${Date.now()}-${fileDto.filename}`,
      });
    } else {
      // For file paths, Cloudinary can directly upload
      uploadResult = await cloudinary.uploader.upload(fileDto.filepath, {
        resource_type: "auto",
        public_id: `${Date.now()}-${fileDto.filename}`,
      });
    }

    return {
      filename: uploadResult.public_id,
      mimetype: uploadResult.format, // Cloudinary provides format, not original mimetype
      url: uploadResult.secure_url,
      eTag: uploadResult.etag,
    };
  }

  async getFile(filename) {
    // Cloudinary does not have a direct 'get file by name' API that returns a downloadable URL easily
    // You typically store the URL returned from upload.
    // If you need to retrieve by public_id, you can construct the URL or use search API.
    // For simplicity, we'll assume the 'filename' here is the public_id from upload.
    // You might need a database to map original filenames to public_ids and their URLs.
    // This is a simplification. A real app might store Cloudinary URLs in its own DB.

    // Cloudinary's fetch API or URL generation:
    const url = cloudinary.url(filename, { secure: true, fetch_format: "auto" });
    if (url) {
        // You might need to make a HEAD request to get content type if not stored
        // For now, returning a generic object
        return {
            filename,
            url,
            mimetype: 'application/octet-stream', // Placeholder, needs actual lookup
        }
    }
    return null;
  }

  async deleteFile(filename) {
    try {
      const result = await cloudinary.uploader.destroy(filename);
      return result.result === 'ok';
    } catch (error) {
      console.error('Cloudinary delete error:', error);
      return false;
    }
  }
}

module.exports = CloudinaryStorageProvider;
