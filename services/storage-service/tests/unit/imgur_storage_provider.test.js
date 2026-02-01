const ImgurStorageProvider = require('../../src/infrastructure/providers/imgur.storage.provider');
const UploadFileDto = require('../../src/domain/dtos/UploadFileDto');
const fs = require('fs').promises;
const path = require('path');
const axios = require('axios');

jest.mock('axios'); // Mock axios for API calls

describe('ImgurStorageProvider', () => {
  const config = { clientId: 'test_client_id' };
  let provider;
  const tempFilePath = path.join(__dirname, 'temp_imgur_test_file.txt');

  beforeAll(async () => {
    provider = new ImgurStorageProvider(config);
    await fs.writeFile(tempFilePath, 'This is a test image content');
  });

  afterAll(async () => {
    await fs.unlink(tempFilePath);
  });

  beforeEach(() => {
    axios.post.mockClear();
    axios.delete.mockClear();
  });

  test('should upload a file successfully', async () => {
    const mockResponse = {
      data: {
        data: {
          id: 'testId123',
          type: 'image/jpeg',
          link: 'http://test.imgur.com/testId123.jpg',
          deletehash: 'testDeleteHash',
        },
        success: true,
        status: 200,
      },
    };
    axios.post.mockResolvedValueOnce(mockResponse);

    const fileDto = new UploadFileDto({
      filename: 'test_image.jpg',
      mimetype: 'image/jpeg',
      filepath: tempFilePath,
      destination: 'imgur',
    });

    const result = await provider.uploadFile(fileDto);

    expect(axios.post).toHaveBeenCalledTimes(1);
    expect(result).toEqual({
      filename: 'testId123',
      mimetype: 'image/jpeg',
      url: 'http://test.imgur.com/testId123.jpg',
      deletehash: 'testDeleteHash',
    });
  });

  test('should handle upload failure', async () => {
    axios.post.mockRejectedValueOnce({ response: { data: { data: { error: 'Upload failed' } } } });

    const fileDto = new UploadFileDto({
      filename: 'fail_image.jpg',
      mimetype: 'image/jpeg',
      filepath: tempFilePath,
      destination: 'imgur',
    });

    await expect(provider.uploadFile(fileDto)).rejects.toThrow('Imgur upload failed: Upload failed');
  });

  test('should get a file by URL', async () => {
    const filename = 'http://test.imgur.com/existing_image.jpg';
    const result = await provider.getFile(filename);
    expect(result).toEqual({ url: filename, filename: 'existing_image.jpg' });
  });

  test('should return null if filename is not a URL', async () => {
    const filename = 'non_url_filename';
    const result = await provider.getFile(filename);
    expect(result).toBeNull();
  });

  test('should delete a file successfully', async () => {
    axios.delete.mockResolvedValueOnce({ data: { success: true } });

    const deletehash = 'testDeleteHash';
    const result = await provider.deleteFile(deletehash);

    expect(axios.delete).toHaveBeenCalledWith(`https://api.imgur.com/3/image/${deletehash}`, expect.any(Object));
    expect(result).toBe(true);
  });

  test('should handle delete failure', async () => {
    axios.delete.mockRejectedValueOnce({ response: { data: { success: false } } });

    const deletehash = 'failDeleteHash';
    const result = await provider.deleteFile(deletehash);

    expect(axios.delete).toHaveBeenCalledWith(`https://api.imgur.com/3/image/${deletehash}`, expect.any(Object));
    expect(result).toBe(false);
  });

  test('should return false for invalid deletehash', async () => {
    const result = await provider.deleteFile(null);
    expect(result).toBe(false);
  });
});
