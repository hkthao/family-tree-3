class IStorageService {
  async uploadFile(fileDto) {
    throw new Error('Method "uploadFile" must be implemented');
  }

  async getFile(filename, destination) {
    throw new Error('Method "getFile" must be implemented');
  }

  async deleteFile(filename, destination) {
    throw new Error('Method "deleteFile" must be implemented');
  }
}

module.exports = IStorageService;