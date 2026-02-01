const IStorageService = require('../../domain/interfaces/IStorageService');
const axios = require('axios');
const FormData = require('form-data');
const fs = require('fs').promises;

class ImgBBStorageProvider extends IStorageService {
  constructor(config) {
    super();
    this.apiKey = config.apiKey;
    this.baseUrl = 'https://api.imgbb.com/1/upload';
  }

  async uploadFile(fileDto) {
    const form = new FormData();
    form.append('key', this.apiKey);

    let fileBuffer;
    if (Buffer.isBuffer(fileDto.filepath)) {
      fileBuffer = fileDto.filepath;
    } else {
      fileBuffer = await fs.readFile(fileDto.filepath);
    }
    
    // ImgBB API expects base64 encoded image data for direct upload
    form.append('image', fileBuffer.toString('base64'));

    try {
      const response = await axios.post(this.baseUrl, form, {
        headers: form.getHeaders(),
        maxContentLength: Infinity,
        maxBodyLength: Infinity,
      });

      const { data } = response.data;
      return {
        filename: data.title,
        mimetype: data.mime,
        url: data.url,
        deleteUrl: data.delete_url, // ImgBB provides a delete URL
      };
    } catch (error) {
      console.error('ImgBB upload error:', error.response ? error.response.data : error.message);
      throw new Error(`ImgBB upload failed: ${error.response ? error.response.data.error.message : error.message}`);
    }
  }

  async getFile(filename) {
    // ImgBB does not provide a direct way to retrieve file metadata or URLs by filename
    // after upload, other than the URL returned at upload time.
    // If 'filename' here refers to the title or ID, you would typically need to store
    // the URL and delete_url in your own database.
    // For this example, if filename is the direct URL, we return it.
    // Otherwise, this method is largely unimplemented for practical purposes without a DB.
    if (filename.startsWith('http')) {
      return { url: filename, filename: filename.split('/').pop() };
    }
    return null; // ImgBB does not support getting by arbitrary filename without prior knowledge
  }

  async deleteFile(deleteUrl) {
    // ImgBB requires a delete URL (provided during upload) to delete an image.
    // The 'deleteUrl' parameter here is expected to be the URL obtained during upload.
    if (!deleteUrl || !deleteUrl.startsWith('http')) {
        console.warn('Invalid deleteUrl for ImgBB delete operation.');
        return false;
    }
    try {
        await axios.get(deleteUrl); // ImgBB delete is a GET request to the delete URL
        return true;
    } catch (error) {
        console.error('ImgBB delete error:', error.response ? error.response.data : error.message);
        return false;
    }
  }
}

module.exports = ImgBBStorageProvider;
