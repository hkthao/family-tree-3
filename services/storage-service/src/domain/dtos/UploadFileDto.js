class UploadFileDto {
    constructor({ filename, mimetype, filepath, destination }) {
        if (!filename || !mimetype || !filepath || !destination) {
            throw new Error('UploadFileDto requires filename, mimetype, filepath, and destination.');
        }
        this.filename = filename;
        this.mimetype = mimetype;
        this.filepath = filepath; // Local path where the file is temporarily stored
        this.destination = destination; // e.g., 's3', 'cloudinary', 'local'
    }
}

module.exports = UploadFileDto;