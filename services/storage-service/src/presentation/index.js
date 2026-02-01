const express = require('express');
const multer = require('multer');
const router = express.Router();

const upload = multer({ dest: 'uploads/' }); // temporary storage for uploaded files

class StorageController {
  constructor(storageService) {
    this.storageService = storageService;
  }

  async uploadFile(req, res) {
    try {
      if (!req.file) {
        return res.status(400).json({ message: 'No file uploaded' });
      }
      const { originalname, mimetype, path } = req.file;
      const { destination } = req.body; // e.g., 's3', 'cloudinary', 'local'

      const result = await this.storageService.uploadFile({
        filename: originalname,
        mimetype,
        filepath: path,
        destination,
      });

      res.status(200).json(result);
    } catch (error) {
      console.error('Error uploading file:', error);
      res.status(500).json({ message: 'File upload failed', error: error.message });
    }
  }

  async getFile(req, res) {
    try {
      const { filename } = req.params;
      const { destination } = req.query; // e.g., 's3', 'cloudinary', 'local'

      const file = await this.storageService.getFile(filename, destination);

      if (!file) {
        return res.status(404).json({ message: 'File not found' });
      }

      // For local files, send the file. For remote, redirect or send URL.
      if (destination === 'local' && file.filepath) {
        return res.sendFile(file.filepath);
      } else if (file.url) {
        return res.status(200).json({ url: file.url });
      }
      res.status(200).json(file);
    } catch (error) {
      console.error('Error getting file:', error);
      res.status(500).json({ message: 'Failed to retrieve file', error: error.message });
    }
  }
}

module.exports = { StorageController, router, upload };
