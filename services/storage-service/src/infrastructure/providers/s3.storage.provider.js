const IStorageService = require('../../domain/interfaces/IStorageService');
const AWS = require('aws-sdk');
const fs = require('fs').promises;

class S3StorageProvider extends IStorageService {
  constructor(config) {
    super();
    this.s3 = new AWS.S3({
      accessKeyId: config.accessKeyId,
      secretAccessKey: config.secretAccessKey,
      region: config.region,
    });
    this.bucketName = config.bucketName;
  }

  async uploadFile(fileDto) {
    const filename = `${Date.now()}-${fileDto.filename}`;
    let fileContent;

    if (Buffer.isBuffer(fileDto.filepath)) {
      fileContent = fileDto.filepath;
    } else {
      fileContent = await fs.readFile(fileDto.filepath);
    }

    const params = {
      Bucket: this.bucketName,
      Key: filename,
      Body: fileContent,
      ContentType: fileDto.mimetype,
      ACL: 'public-read', // Or adjust based on your needs
    };

    const data = await this.s3.upload(params).promise();
    return {
      filename,
      mimetype: fileDto.mimetype,
      url: data.Location,
      eTag: data.ETag,
    };
  }

  async getFile(filename) {
    const params = {
      Bucket: this.bucketName,
      Key: filename,
    };

    try {
      const headObject = await this.s3.headObject(params).promise();
      const url = this.s3.getSignedUrl('getObject', {
        Bucket: this.bucketName,
        Key: filename,
        Expires: 60 * 5, // URL expires in 5 minutes
      });
      return {
        filename,
        mimetype: headObject.ContentType,
        url,
      };
    } catch (error) {
      if (error.code === 'NotFound') {
        return null;
      }
      throw error;
    }
  }

  async deleteFile(filename) {
    const params = {
      Bucket: this.bucketName,
      Key: filename,
    };

    try {
      await this.s3.deleteObject(params).promise();
      return true;
    } catch (error) {
      if (error.code === 'NotFound') {
        return false;
      }
      throw error;
    }
  }
}

module.exports = S3StorageProvider;
