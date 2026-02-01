const LocalStorageProvider = require('../../src/infrastructure/providers/local.storage.provider');
const UploadFileDto = require('../../src/domain/dtos/UploadFileDto');
const fs = require('fs').promises;
const path = require('path');

describe('LocalStorageProvider', () => {
  const testUploadDir = path.join(__dirname, 'test_uploads');
  let provider;

  beforeAll(async () => {
    await fs.mkdir(testUploadDir, { recursive: true });
    provider = new LocalStorageProvider(testUploadDir);
  });

  afterAll(async () => {
    await fs.rm(testUploadDir, { recursive: true, force: true });
  });

  beforeEach(async () => {
    // Clear the test directory before each test
    await fs.rm(testUploadDir, { recursive: true, force: true });
    await fs.mkdir(testUploadDir, { recursive: true });
  });

  test('should upload a file from a temporary path', async () => {
    const tempFilePath = path.join(__dirname, 'temp_test_file.txt');
    await fs.writeFile(tempFilePath, 'Hello, local storage!');

    const fileDto = new UploadFileDto({
      filename: 'test_file.txt',
      mimetype: 'text/plain',
      filepath: tempFilePath,
      destination: 'local',
    });

    const result = await provider.uploadFile(fileDto);

    expect(result).toHaveProperty('filename');
    expect(result).toHaveProperty('filepath');
    expect(result.filepath).toContain(testUploadDir);
    expect(result.url).toMatch(/^\/local\//);

    const uploadedContent = await fs.readFile(result.filepath, 'utf8');
    expect(uploadedContent).toBe('Hello, local storage!');

    // Ensure temporary file is removed by the service layer or explicitly here if not in service
    // In actual service, the temp file cleanup happens after provider.uploadFile
    // For unit testing provider, we check if the temp file exists after upload (it shouldn't be moved yet)
    // If the provider moves it, then it should not exist at tempFilePath.
    // Given the current implementation, provider renames/moves it.
    await expect(fs.access(tempFilePath)).rejects.toHaveProperty('code', 'ENOENT');
  });

  test('should upload a file from a buffer', async () => {
    const fileBuffer = Buffer.from('Hello from buffer!', 'utf8');

    const fileDto = new UploadFileDto({
      filename: 'buffer_test.txt',
      mimetype: 'text/plain',
      filepath: fileBuffer,
      destination: 'local',
    });

    const result = await provider.uploadFile(fileDto);

    expect(result).toHaveProperty('filename');
    expect(result).toHaveProperty('filepath');
    expect(result.filepath).toContain(testUploadDir);
    expect(result.url).toMatch(/^\/local\//);

    const uploadedContent = await fs.readFile(result.filepath, 'utf8');
    expect(uploadedContent).toBe('Hello from buffer!');
  });

  test('should get a file', async () => {
    const filename = `test_get_${Date.now()}.txt`;
    const content = 'Content to retrieve';
    const filePath = path.join(testUploadDir, filename);
    await fs.writeFile(filePath, content);

    const result = await provider.getFile(filename);

    expect(result).toHaveProperty('filename', filename);
    expect(result).toHaveProperty('filepath', filePath);
    expect(result.url).toMatch(/^\/local\//);
  });

  test('should return null when getting a non-existent file', async () => {
    const result = await provider.getFile('non_existent_file.txt');
    expect(result).toBeNull();
  });

  test('should delete a file', async () => {
    const filename = `test_delete_${Date.now()}.txt`;
    const filePath = path.join(testUploadDir, filename);
    await fs.writeFile(filePath, 'Content to delete');

    const deleted = await provider.deleteFile(filename);
    expect(deleted).toBe(true);

    await expect(fs.access(filePath)).rejects.toHaveProperty('code', 'ENOENT');
  });

  test('should return false when deleting a non-existent file', async () => {
    const deleted = await provider.deleteFile('non_existent_delete.txt');
    expect(deleted).toBe(false);
  });
});
