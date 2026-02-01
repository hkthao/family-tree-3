const ImgBBStorageProvider = require('../../src/infrastructure/providers/imgbb.storage.provider');
const UploadFileDto = require('../../src/domain/dtos/UploadFileDto');
const fs = require('fs').promises;
const path = require('path');
const axios = require('axios');

jest.mock('axios'); // Mock axios for API calls

describe('ImgBBStorageProvider', () => {
  const config = { apiKey: 'test_api_key' };
  let provider;
  const tempFilePath = path.join(__dirname, 'temp_imgbb_test_file.txt');

  beforeAll(async () => {
    provider = new ImgBBStorageProvider(config);
    await fs.writeFile(tempFilePath, 'This is a test image content');
  });

  afterAll(async () => {
    await fs.unlink(tempFilePath);
  });

  beforeEach(() => {
    axios.post.mockClear();
    axios.get.mockClear();
  });

  test('should upload a file successfully', async () => {
    const mockResponse = {
      data: {
        data: {
          title: 'test_image.txt',
          mime: 'text/plain',
          url: 'http://test.imgbb.com/test_image.txt',
          delete_url: 'http://test.imgbb.com/delete/test_image.txt',
        },
      },
    };
    axios.post.mockResolvedValueOnce(mockResponse);

    const fileDto = new UploadFileDto({
      filename: 'test_image.txt',
      mimetype: 'text/plain',
      filepath: tempFilePath,
      destination: 'imgbb',
    });

    const result = await provider.uploadFile(fileDto);

    expect(axios.post).toHaveBeenCalledTimes(1);
    expect(result).toEqual({
      filename: 'test_image.txt',
      mimetype: 'text/plain',
      url: 'http://test.imgbb.com/test_image.txt',
      deleteUrl: 'http://test.imgbb.com/delete/test_image.txt',
    });
  });

  test('should handle upload failure', async () => {
    axios.post.mockRejectedValueOnce({ response: { data: { error: { message: 'Upload failed' } } } });

    const fileDto = new UploadFileDto({
      filename: 'fail_image.txt',
      mimetype: 'text/plain',
      filepath: tempFilePath,
      destination: 'imgbb',
    });

    await expect(provider.uploadFile(fileDto)).rejects.toThrow('ImgBB upload failed: Upload failed');
  });

  test('should get a file by URL', async () => {
    const filename = 'http://test.imgbb.com/existing_image.jpg';
    const result = await provider.getFile(filename);
    expect(result).toEqual({ url: filename, filename: 'existing_image.jpg' });
  });

  test('should return null if filename is not a URL', async () => {
    const filename = 'non_url_filename';
    const result = await provider.getFile(filename);
    expect(result).toBeNull();
  });

  test('should delete a file successfully', async () => {
    axios.get.mockResolvedValueOnce({}); // ImgBB delete is a GET request

    const deleteUrl = 'http://test.imgbb.com/delete/test_image.txt';
    const result = await provider.deleteFile(deleteUrl);

    expect(axios.get).toHaveBeenCalledWith(deleteUrl);
    expect(result).toBe(true);
  });

  test('should handle delete failure', async () => {
    axios.get.mockRejectedValueOnce({ response: { data: 'Delete failed' } });

    const deleteUrl = 'http://test.imgbb.com/delete/fail_image.txt';
    const result = await provider.deleteFile(deleteUrl);

    expect(axios.get).toHaveBeenCalledWith(deleteUrl);
    expect(result).toBe(false);
  });

  test('should return false for invalid deleteUrl', async () => {
    const result = await provider.deleteFile('invalid_url');
    expect(result).toBe(false);
  });
});
