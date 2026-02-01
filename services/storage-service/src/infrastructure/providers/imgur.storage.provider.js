const IStorageService = require('../../domain/interfaces/IStorageService');
const axios = require('axios');
const FormData = require('form-data');
const fs = require('fs').promises;

class ImgurStorageProvider extends IStorageService {
  constructor(config) {
    super();
    this.clientId = config.clientId;
    this.baseUrl = 'https://api.imgur.com/3/image';
  }

  async uploadFile(fileDto) {
    const form = new FormData();

    let fileBuffer;
    if (Buffer.isBuffer(fileDto.filepath)) {
      fileBuffer = fileDto.filepath;
    } else {
      fileBuffer = await fs.readFile(fileDto.filepath);
    }
    
    // Imgur expects image data as base64 or binary
    form.append('image', fileBuffer.toString('base64'));
    form.append('type', 'base64');
    form.append('name', fileDto.filename);
    form.append('title', fileDto.filename);

    try {
      const response = await axios.post(this.baseUrl, form, {
        headers: {
          'Authorization': `Client-ID ${this.clientId}`,
          ...form.getHeaders(),
        },
        maxContentLength: Infinity,
        maxBodyLength: Infinity,
      });

      const { data } = response.data;
      return {
        filename: data.id,
        mimetype: data.type,
        url: data.link,
        deletehash: data.deletehash, // Imgur provides a deletehash
      };
    } catch (error) {
      console.error('Imgur upload error:', error.response ? error.response.data : error.message);
      throw new Error(`Imgur upload failed: ${error.response ? error.response.data.data.error : error.message}`);
    }
  }

  async getFile(filename) {
    // Imgur does not provide a direct way to retrieve file metadata or URLs by filename
    // You typically store the URL and deletehash returned at upload time.
    // If 'filename' here refers to the direct URL, we return it.
    if (filename.startsWith('http')) {
      return { url: filename, filename: filename.split('/').pop() };
    }
    return null; // Imgur does not support getting by arbitrary filename without prior knowledge
  }

  async deleteFile(deletehash) {
    // Imgur requires a deletehash (provided during upload) to delete an image.
    if (!deletehash) {
        console.warn('Invalid deletehash for Imgur delete operation.');
        return false;
    }
    try {
        const response = await axios.delete(`${this.baseUrl}/${deletehash}`, {
            headers: {
                'Authorization': `Client-ID ${this.clientId}`,
            },
        });
        return response.data.success;
    } catch (error) {
        console.error('Imgur delete error:', error.response ? error.response.data : error.message);
        return false;
    }
  }
}

module.exports = ImgurStorageProvider;
