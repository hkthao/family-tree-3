require('dotenv').config();

const config = {
  port: process.env.PORT || 3000,
  novu: {
    apiKey: process.env.NOVU_API_KEY,
  },
  rabbitmq: {
    host: process.env.RABBITMQ__HOSTNAME || 'localhost',
    port: process.env.RABBITMQ__PORT || '5672',
    username: process.env.RABBITMQ__USERNAME || 'guest',
    password: process.env.RABBITMQ__PASSWORD || 'guest',
    url: `amqp://${process.env.RABBITMQ__USERNAME || 'guest'}:${process.env.RABBITMQ__PASSWORD || 'guest'}@${process.env.RABBITMQ__HOSTNAME || 'localhost'}:${process.env.RABBITMQ__PORT || '5672'}`,
    exchange: 'notification',
    queue: 'notification_service_queue',
    routingKeys: [
      'notification.user.sync',
      'notification.expo.save',
      'notification.expo.delete',
      'notification.send.*',
    ],
  },
};

// Validate essential configurations
if (!config.novu.apiKey) {
  console.error('Error: NOVU_API_KEY is not defined in environment variables.');
  process.exit(1);
}

module.exports = config;
