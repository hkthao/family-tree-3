require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const { Novu } = require('@novu/node');

const app = express();
const port = process.env.PORT || 3000;

app.use(bodyParser.json());

const NOVU_API_KEY = process.env.NOVU_API_KEY;

if (!NOVU_API_KEY) {
  console.error('NOVU_API_KEY is not defined in environment variables.');
  process.exit(1);
}

const novu = new Novu(NOVU_API_KEY);

// Middleware để kiểm tra body
const validateBody = (fields) => (req, res, next) => {
  for (const field of fields) {
    if (!req.body[field]) {
      return res.status(400).json({ error: `Missing required field: ${field}` });
    }
  }
  next();
};

/**
 * @route POST /subscribers/sync
 * @description Tạo mới hoặc cập nhật subscriber trong Novu.
 * @body {string} userId - ID của người dùng (subscriberId).
 */
app.post('/subscribers/sync', validateBody(['userId']), async (req, res) => {
  const { userId, firstName, lastName, email, phone, avatar, locale, timezone } = req.body;
  try {
    const subscriberPayload = {
      firstName,
      lastName,
      email,
      phone,
      avatar,
      locale,
      data: {
        timezone,
      },
    };

    // Remove null or undefined properties from the main payload
    Object.keys(subscriberPayload).forEach(key => {
      if (subscriberPayload[key] === null || subscriberPayload[key] === undefined) {
        delete subscriberPayload[key];
      }
    });

    // Remove null or undefined properties from the nested 'data' object
    if (subscriberPayload.data) {
      Object.keys(subscriberPayload.data).forEach(key => {
        if (subscriberPayload.data[key] === null || subscriberPayload.data[key] === undefined) {
          delete subscriberPayload.data[key];
        }
      });
      // If the 'data' object becomes empty after cleaning, remove it too
      if (Object.keys(subscriberPayload.data).length === 0) {
        delete subscriberPayload.data;
      }
    }

    const subscriber = await novu.subscribers.identify(userId, subscriberPayload);
    console.log(`Subscriber synced: ${userId}`);
    res.status(200).json({ message: 'Subscriber synced successfully', subscriber: subscriber.data });
  } catch (error) {
    console.error(`Error syncing subscriber ${userId}:`, error);
    res.status(500).json({ error: 'Failed to sync subscriber', details: error.message, fullError: error.response ? error.response.data : error });
  }
});

/**
 * @route POST /subscribers/expo-token
 * @description Gắn Expo Push Token vào subscriber trong Novu.
 * @body {string} userId - ID của người dùng (subscriberId).
 * @body {string[]} expoPushTokens - Mảng các Expo Push Token.
 */
app.post('/subscribers/expo-token', validateBody(['userId', 'expoPushTokens']), async (req, res) => {
  const { userId, expoPushTokens } = req.body;
  try {
    // Novu tự động quản lý token, chỉ cần thêm device
    await novu.subscribers.setCredentials(userId, 'expo', {
      deviceTokens: expoPushTokens,
    });
    console.log(`Expo Push Tokens added for ${userId}: ${expoPushTokens}`);
    res.status(200).json({ message: 'Expo Push Tokens added successfully' });
  } catch (error) {
    console.error(`Error adding Expo Push Token for ${userId}:`, error);
    res.status(500).json({ error: 'Failed to add Expo Push Token', details: error.message });
  }
});




/**
 * @route POST /notifications/send
 * @description Trigger Novu workflow để gửi thông báo.
 * @body {string} workflowId - ID của workflow Novu.
 * @body {string} userId - ID của người nhận (subscriberId).
 * @body {object} payload - Dữ liệu tùy chỉnh để truyền vào workflow.
 */
app.post('/notifications/send', validateBody(['workflowId', 'userId', 'payload']), async (req, res) => {
  const { workflowId, userId, payload } = req.body;
  try {
    await novu.trigger(workflowId, {
      to: {
        subscriberId: userId,
      },
      payload: payload,
    });
    console.log(`Notification triggered for workflow ${workflowId} to ${userId}`);
    res.status(200).json({ message: 'Notification triggered successfully' });
  } catch (error) {
    console.error(`Error triggering notification for workflow ${workflowId} to ${userId}:`, error);
    res.status(500).json({ error: 'Failed to trigger notification', details: error.message });
  }
});

if (require.main === module) {
  app.listen(port, () => {
    console.log(`Notification service listening at http://localhost:${port}`);
  });
}

module.exports = app;
