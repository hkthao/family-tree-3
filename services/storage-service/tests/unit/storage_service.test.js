const StorageService = require('../../src/application/storage.service');
const UploadFileDto = require('../../src/domain/dtos/UploadFileDto');
const IStorageService = require('../../src/domain/interfaces/IStorageService');
const fs = require('fs').promises;
const path = require('path');

// Mock storage providers
class MockLocalStorageProvider extends IStorageService {
  async uploadFile(fileDto) {
    return { url: `mock_local_url/${fileDto.filename}`, filepath: fileDto.filepath };
  }
  async getFile(filename) { return { url: `mock_local_url/${filename}` }; }
  async deleteFile(filename) { return true; }
}

class MockS3StorageProvider extends IStorageService {
  async uploadFile(fileDto) {
    return { url: `mock_s3_url/${fileDto.filename}` };
  }
  async getFile(filename) { return { url: `mock_s3_url/${filename}` }; }
  async deleteFile(filename) { return true; }
}

class MockCloudinaryStorageProvider extends IStorageService {
  async uploadFile(fileDto) {
    return { url: `mock_cloudinary_url/${fileDto.filename}` };
  }
  async getFile(filename) { return { url: `mock_cloudinary_url/${filename}` }; }
  async deleteFile(filename) { return true; }
}

describe('StorageService', () => {
  let localStorageProvider;
  let s3StorageProvider;
  let cloudinaryStorageProvider;
  let storageService;

  beforeEach(() => {
    localStorageProvider = new MockLocalStorageProvider();
    s3StorageProvider = new MockS3StorageProvider();
    cloudinaryStorageProvider = new MockCloudinaryStorageProvider();

    jest.spyOn(localStorageProvider, 'uploadFile');
    jest.spyOn(s3StorageProvider, 'uploadFile');
    jest.spyOn(cloudinaryStorageProvider, 'uploadFile');
    jest.spyOn(localStorageProvider, 'getFile');
    jest.spyOn(s3StorageProvider, 'getFile');
    jest.spyOn(cloudinaryStorageProvider, 'getFile');
    jest.spyOn(localStorageProvider, 'deleteFile');
    jest.spyOn(s3StorageProvider, 'deleteFile');
    jest.spyOn(cloudinaryStorageProvider, 'deleteFile');


    storageService = new StorageService(
      localStorageProvider,
      s3StorageProvider,
      cloudinaryStorageProvider
    );
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  const tempFileName = 'temp_test_file.txt';
  const tempFilePath = path.join(__dirname, tempFileName);

  beforeEach(async () => {
    // Create a fresh temp file for each test
    await fs.writeFile(tempFilePath, 'Temporary file content');
  });

  afterEach(async () => {
    // Clean up the temp file after each test
    await fs.unlink(tempFilePath).catch(() => {}); // Catch error if file already deleted
  });


  test('should upload file to local storage and clean up temp file', async () => {
    const fileDto = new UploadFileDto({
      filename: 'test.txt',
      mimetype: 'text/plain',
      filepath: tempFilePath,
      destination: 'local',
    });

    const result = await storageService.uploadFile(fileDto);

    expect(localStorageProvider.uploadFile).toHaveBeenCalledWith(fileDto);
    expect(result.url).toBe(`mock_local_url/${fileDto.filename}`);
    await expect(fs.access(tempFilePath)).rejects.toHaveProperty('code', 'ENOENT'); // Temp file should be deleted
  });

  test('should upload file to S3 and clean up temp file', async () => {
    const fileDto = new UploadFileDto({
      filename: 'test.jpg',
      mimetype: 'image/jpeg',
      filepath: tempFilePath,
      destination: 's3',
    });

    const result = await storageService.uploadFile(fileDto);

    expect(s3StorageProvider.uploadFile).toHaveBeenCalledWith(fileDto);
    expect(result.url).toBe(`mock_s3_url/${fileDto.filename}`);
    await expect(fs.access(tempFilePath)).rejects.toHaveProperty('code', 'ENOENT');
  });

  test('should upload file to Cloudinary and clean up temp file', async () => {
    const fileDto = new UploadFileDto({
      filename: 'test.png',
      mimetype: 'image/png',
      filepath: tempFilePath,
      destination: 'cloudinary',
    });

    const result = await storageService.uploadFile(fileDto);

    expect(cloudinaryStorageProvider.uploadFile).toHaveBeenCalledWith(fileDto);
    expect(result.url).toBe(`mock_cloudinary_url/${fileDto.filename}`);
    await expect(fs.access(tempFilePath)).rejects.toHaveProperty('code', 'ENOENT');
  });

  test('should get file from local storage', async () => {
    const filename = 'get_local.txt';
    const destination = 'local';

    const result = await storageService.getFile(filename, destination);

    expect(localStorageProvider.getFile).toHaveBeenCalledWith(filename);
    expect(result.url).toBe(`mock_local_url/${filename}`);
  });

  test('should get file from S3', async () => {
    const filename = 'get_s3.jpg';
    const destination = 's3';

    const result = await storageService.getFile(filename, destination);

    expect(s3StorageProvider.getFile).toHaveBeenCalledWith(filename);
    expect(result.url).toBe(`mock_s3_url/${filename}`);
  });

  test('should get file from Cloudinary', async () => {
    const filename = 'get_cloudinary.png';
    const destination = 'cloudinary';

    const result = await storageService.getFile(filename, destination);

    expect(cloudinaryStorageProvider.getFile).toHaveBeenCalledWith(filename);
    expect(result.url).toBe(`mock_cloudinary_url/${filename}`);
  });

  test('should delete file from local storage', async () => {
    const filename = 'delete_local.txt';
    const destination = 'local';

    const result = await storageService.deleteFile(filename, destination);

    expect(localStorageProvider.deleteFile).toHaveBeenCalledWith(filename);
    expect(result).toBe(true);
  });

  test('should delete file from S3', async () => {
    const filename = 'delete_s3.jpg';
    const destination = 's3';

    const result = await storageService.deleteFile(filename, destination);

    expect(s3StorageProvider.deleteFile).toHaveBeenCalledWith(filename);
    expect(result).toBe(true);
  });

  test('should delete file from Cloudinary', async () => {
    const filename = 'delete_cloudinary.png';
    const destination = 'cloudinary';

    const result = await storageService.deleteFile(filename, destination);

    expect(cloudinaryStorageProvider.deleteFile).toHaveBeenCalledWith(filename);
    expect(result).toBe(true);
  });

  test('should handle unsupported destination for uploadFile', async () => {
    const fileDto = new UploadFileDto({
      filename: 'test.txt',
      mimetype: 'text/plain',
      filepath: tempFilePath,
      destination: 'unsupported',
    });

    await expect(storageService.uploadFile(fileDto)).rejects.toThrow('Unsupported storage destination');
  });

  test('should handle unsupported destination for getFile', async () => {
    await expect(storageService.getFile('file', 'unsupported')).rejects.toThrow('Unsupported storage destination');
  });

  test('should handle unsupported destination for deleteFile', async () => {
    await expect(storageService.deleteFile('file', 'unsupported')).rejects.toThrow('Unsupported storage destination');
  });
});
