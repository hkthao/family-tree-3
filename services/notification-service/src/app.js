const express = require('express');
const bodyParser = require('body-parser');
const apiRoutes = require('@/api/routes');
const config = require('@/config'); // Ensure config is loaded first to validate env vars

const app = express();

app.use(bodyParser.json());

// API Routes
app.use('/', apiRoutes);

// Basic health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok', message: 'Notification service is running' });
});

module.exports = app;
