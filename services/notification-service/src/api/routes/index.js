const express = require('express');
const novuService = require('@/services/novu.service');
const { removeUndefinedProps } = require('@/utils');

const router = express.Router();

router.post('/subscribers/sync', async (req, res) => {
  const { userId, firstName, lastName, email, phone, avatar, locale, timezone } = req.body;

  if (!userId) {
    return res.status(400).json({ error: 'Missing required field: userId' });
  }

  try {
    const subscriberPayload = {
      firstName,
      lastName,
      email,
      phone,
      avatar,
      locale,
      data: { timezone },
    };
    removeUndefinedProps(subscriberPayload);

    await novuService.identifySubscriber(userId, subscriberPayload);
    res.status(200).json({ message: 'Subscriber synced successfully' });
  } catch (error) {
    console.error('Error syncing subscriber:', error);
    res.status(500).json({ error: 'Failed to sync subscriber', details: error.message });
  }
});

router.post('/subscribers/expo-token', async (req, res) => {
  const { userId, expoPushTokens } = req.body;

  if (!userId) {
    return res.status(400).json({ error: 'Missing required field: userId' });
  }
  if (!expoPushTokens || !Array.isArray(expoPushTokens)) {
    return res.status(400).json({ error: 'Missing required field: expoPushTokens' });
  }

  try {
    await novuService.setExpoCredentials(userId, expoPushTokens);
    res.status(200).json({ message: 'Expo Push Tokens added successfully' });
  } catch (error) {
    console.error('Error adding Expo Push Token:', error);
    res.status(500).json({ error: 'Failed to add Expo Push Token', details: error.message });
  }
});

router.post('/notifications/send', async (req, res) => {
  const { workflowId, userId, familyId, payload } = req.body;

  if (!workflowId) {
    return res.status(400).json({ error: 'Missing required field: workflowId' });
  }
  if (!userId) {
    return res.status(400).json({ error: 'Missing required field: userId' });
  }
  if (!payload) {
    return res.status(400).json({ error: 'Missing required field: payload' });
  }

  try {
    await novuService.triggerNotification(workflowId, userId, payload);
    res.status(200).json({ message: 'Notification triggered successfully' });
  } catch (error) {
    console.error('Error triggering notification:', error);
    res.status(500).json({ error: 'Failed to trigger notification', details: error.message });
  }
});

module.exports = router;
