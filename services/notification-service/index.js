require('module-alias/register');
const app = require('./src/app');
const config = require('./src/config');
const { connectRabbitMQ } = require('./src/rabbitmq');

const port = config.port;

const startServer = async () => {
  app.listen(port, () => {
    console.log(`Notification service listening at http://localhost:${port}`);
  });

  // Initialize RabbitMQ and start consuming messages
  await connectRabbitMQ();
};

startServer().catch(error => {
  console.error('Failed to start server or connect to RabbitMQ:', error);
  process.exit(1);
});

module.exports = app; // Export app for testing purposes