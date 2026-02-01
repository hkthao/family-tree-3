require('dotenv/config'); // Load environment variables from .env file
const express = require('express');
const app = express();
const fileUploadConsumer = require('./consumers/fileUploadConsumer'); // Import the consumer

app.use(express.json());

// Basic health check endpoint
app.get('/health', (req, res) => {
  res.status(200).send('Storage service is running');
});

const PORT = process.env.PORT || 8000;

app.listen(PORT, async () => {
  console.log(`Storage service listening on port ${PORT}`);
  await fileUploadConsumer.connectRabbitMQ(); // Connect to RabbitMQ when the server starts
});