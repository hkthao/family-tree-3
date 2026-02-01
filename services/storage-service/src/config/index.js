require('dotenv').config();

module.exports = {
  port: process.env.PORT || 3000,
  rabbitMqUrl: process.env.RABBITMQ_URL || 'amqp://localhost',
  s3: {
    accessKeyId: process.env.AWS_ACCESS_KEY_ID,
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
    region: process.env.AWS_REGION,
    bucketName: process.env.AWS_BUCKET_NAME,
  },
  cloudinary: {
    cloudName: process.env.CLOUDINARY_CLOUD_NAME,
    apiKey: process.env.CLOUDINARY_API_KEY,
    apiSecret: process.env.CLOUDINARY_API_SECRET,
  },
  imgur: {
    clientId: process.env.IMGUR_CLIENT_ID,
  },
  imgbb: {
    apiKey: process.env.IMGBB_API_KEY,
  },
  local: {
    uploadDir: process.env.LOCAL_UPLOAD_DIR || 'uploads/local',
  },
};