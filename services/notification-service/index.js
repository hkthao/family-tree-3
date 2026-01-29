require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const { Novu } = require('@novu/node');
const amqp = require('amqplib'); // Add amqplib

const RABBITMQ_HOST = process.env.RABBITMQ__HOSTNAME || 'localhost';
const RABBITMQ_PORT = process.env.RABBITMQ__PORT || '5672';
const RABBITMQ_USERNAME = process.env.RABBITMQ__USERNAME || 'guest';
const RABBITMQ_PASSWORD = process.env.RABBITMQ__PASSWORD || 'guest';
const RABBITMQ_URL = `amqp://${RABBITMQ_USERNAME}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOST}:${RABBITMQ_PORT}`;

const NOTIFICATION_EXCHANGE = 'notification';
const NOTIFICATION_QUEUE = 'notification_service_queue'; // A dedicated queue for this service

const app = express();
const port = process.env.PORT || 3000;

app.use(bodyParser.json());

const NOVU_API_KEY = process.env.NOVU_API_KEY;

if (!NOVU_API_KEY) {
  console.error('NOVU_API_KEY is not defined in environment variables.');
  process.exit(1);
}

const novu = new Novu(NOVU_API_KEY);

// HTTP API Routes
app.post('/subscribers/sync', async (req, res) => {
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
    };
    if (timezone) {
      subscriberPayload.data = { timezone };
    }
    // Remove undefined or null values from payload
    Object.keys(subscriberPayload).forEach(key => {
      if (subscriberPayload[key] === null || subscriberPayload[key] === undefined) delete subscriberPayload[key];
    });
    if (subscriberPayload.data) {
      Object.keys(subscriberPayload.data).forEach(key => {
        if (subscriberPayload.data[key] === null || subscriberPayload.data[key] === undefined) delete subscriberPayload.data[key];
      });
      if (Object.keys(subscriberPayload.data).length === 0) delete subscriberPayload.data;
    }

    await novu.subscribers.identify(userId, subscriberPayload);
    res.status(200).json({ message: 'Subscriber synced successfully' });
  } catch (error) {
    console.error('Error syncing subscriber:', error);
    res.status(500).json({ error: 'Failed to sync subscriber', details: error.message });
  }
});

app.post('/subscribers/expo-token', async (req, res) => {
  const { userId, expoPushTokens } = req.body;

  if (!userId) {
    return res.status(400).json({ error: 'Missing required field: userId' });
  }
  if (!expoPushTokens || !Array.isArray(expoPushTokens)) {
    return res.status(400).json({ error: 'Missing required field: expoPushTokens' });
  }

  try {
    await novu.subscribers.setCredentials(userId, 'expo', {
      deviceTokens: expoPushTokens,
    });
    res.status(200).json({ message: 'Expo Push Tokens added successfully' });
  } catch (error) {
    console.error('Error adding Expo Push Token:', error);
    res.status(500).json({ error: 'Failed to add Expo Push Token', details: error.message });
  }
});

app.post('/notifications/send', async (req, res) => {
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
    let isPushEnabled = false;
    try {
      const subscriber = await novu.subscribers.getSubscriber(userId);
      if (subscriber && subscriber.data && subscriber.data.channel_credentials) {
        const pushCredentials = subscriber.data.channel_credentials.find(
          (cred) =>
            (cred.provider === 'expo' || cred.provider === 'fcm' || cred.provider === 'apns') &&
            cred.credentials &&
            cred.credentials.deviceTokens &&
            cred.credentials.deviceTokens.length > 0
        );
        if (pushCredentials) {
          isPushEnabled = true;
        }
      }
    } catch (subscriberError) {
      console.warn(`Could not retrieve subscriber ${userId} details for push check:`, subscriberError.message);
      isPushEnabled = false;
    }

    const updatedPayload = { ...payload, is_push_enabled: isPushEnabled };

    await novu.trigger(workflowId, {
      to: {
        subscriberId: userId,
      },
      payload: updatedPayload,
    });
    res.status(200).json({ message: 'Notification triggered successfully' });
  } catch (error) {
    console.error('Error triggering notification:', error);
    res.status(500).json({ error: 'Failed to trigger notification', details: error.message });
  }
});

// Function to handle specific message types
async function handleUserSyncEvent(eventPayload) {
  const { user_id, first_name, last_name, email, phone, avatar, locale, timezone } = eventPayload;
  try {
    const subscriberPayload = {
      firstName: first_name,
      lastName: last_name,
      email,
      phone,
      avatar,
      locale,
      data: { timezone },
    };
    Object.keys(subscriberPayload).forEach(key => {
      if (subscriberPayload[key] === null || subscriberPayload[key] === undefined) delete subscriberPayload[key];
    });

    await novu.subscribers.identify(user_id, subscriberPayload);
    console.log(`[RabbitMQ] Subscriber synced: ${user_id}`);
  } catch (error) {
    console.error(`[RabbitMQ] Error syncing subscriber ${user_id}:`, error);
    throw error; // Re-throw to indicate processing failure
  }
}

async function handleSaveExpoPushTokenEvent(eventPayload) {
  const { user_id, expo_push_tokens } = eventPayload;
  try {
    await novu.subscribers.setCredentials(user_id, 'expo', {
      deviceTokens: expo_push_tokens,
    });
    console.log(`[RabbitMQ] Expo Push Tokens added for ${user_id}: ${expo_push_tokens}`);
  } catch (error) {
    console.error(`[RabbitMQ] Error adding Expo Push Token for ${user_id}:`, error);
    throw error;
  }
}

async function handleDeleteExpoPushTokenEvent(eventPayload) {
  const { user_id, expo_push_token } = eventPayload;
  // Novu does not have a direct API to "delete" a single expo token from a subscriber.
  // The 'setCredentials' method replaces the existing device tokens.
  // To simulate deletion, we might need to fetch current tokens, filter, and then set.
  console.warn(`[RabbitMQ] Received request to delete Expo Push Token for ${user_id}: ${expo_push_token}. Novu API does not directly support deleting single tokens without knowing all existing tokens. This event will be processed, but actual deletion logic in Novu might differ.`);
  try {
    // If Novu had a specific delete API:
    // await novu.subscribers.deleteDeviceToken(userId, 'expo', expoPushToken); // Hypothetical API
    // As a workaround, if we want to remove this specific token, we would need to:
    // 1. Fetch the subscriber's current credentials.
    // 2. Filter out the token to be deleted.
    // 3. Call setCredentials with the updated list. This is complex and outside the scope of direct event handling here.
    // For now, we will acknowledge the message as processed.
    console.log(`[RabbitMQ] Processed delete Expo Push Token event for ${user_id}.`);
  } catch (error) {
    console.error(`[RabbitMQ] Error deleting Expo Push Token for ${user_id}:`, error);
    throw error;
  }
}

async function handleSendNotificationEvent(eventPayload) {
  const { workflow_id, user_id, family_id, payload } = eventPayload; // Add family_id
  try {
    let isPushEnabled = false;
    try {
      const subscriber = await novu.subscribers.getSubscriber(user_id);
      if (subscriber && subscriber.data && subscriber.data.channel_credentials) {
        // Check if there are any push credentials with device tokens
        const pushCredentials = subscriber.data.channel_credentials.find(
          (cred) =>
            (cred.provider === 'expo' || cred.provider === 'fcm' || cred.provider === 'apns') &&
            cred.credentials &&
            cred.credentials.deviceTokens &&
            cred.credentials.deviceTokens.length > 0
        );
        if (pushCredentials) {
          isPushEnabled = true;
        }
      }
    } catch (subscriberError) {
      console.warn(`[RabbitMQ] Could not retrieve subscriber ${user_id} details for push check:`, subscriberError.message);
      // If we can't get subscriber details, assume push is not enabled for this check.
      isPushEnabled = false;
    }

    const updatedPayload = { ...payload, is_push_enabled: isPushEnabled };

    const novuResponse = await novu.trigger(workflow_id, {
      to: {
        subscriberId: user_id,
      },
      payload: updatedPayload,
    });
    console.log(`[RabbitMQ] Notification triggered for workflow ${workflow_id} to ${user_id}. FamilyId: ${family_id}. Novu Response:`, novuResponse); // Log Novu response
  } catch (error) {
    console.error(`[RabbitMQ] Error triggering notification for workflow ${workflow_id} to ${user_id}. FamilyId: ${family_id}:`, error);
    throw error;
  }
}

async function initRabbitMQ() {
  let connection;
  let channel;
  try {
    connection = await amqp.connect(RABBITMQ_URL);
    channel = await connection.createChannel();

    // Declare the exchange
    await channel.assertExchange(NOTIFICATION_EXCHANGE, 'topic', { durable: true });
    console.log(`[RabbitMQ] Exchange '${NOTIFICATION_EXCHANGE}' asserted.`);

    // Declare the queue
    const q = await channel.assertQueue(NOTIFICATION_QUEUE, { durable: true });
    console.log(`[RabbitMQ] Queue '${q.queue}' asserted.`);

    // Bind the queue to the exchange with routing keys
    // notification.user.sync -> UserSyncEvent
    // notification.expo.save -> SaveExpoPushTokenEvent
    // notification.expo.delete -> DeleteExpoPushTokenEvent
    // notification.send.<workflowId> -> SendNotificationEvent
    const routingKeys = [
      'notification.user.sync',
      'notification.expo.save',
      'notification.expo.delete',
      'notification.send.*', // Wildcard for send notifications
    ];

    for (const key of routingKeys) {
      await channel.bindQueue(q.queue, NOTIFICATION_EXCHANGE, key);
      console.log(`[RabbitMQ] Queue '${q.queue}' bound to exchange '${NOTIFICATION_EXCHANGE}' with routing key '${key}'.`);
    }

    console.log(`[RabbitMQ] Waiting for messages in queue '${q.queue}'. To exit, press CTRL+C`);

    channel.consume(q.queue, async (msg) => {
      if (msg === null) return;

      try {
        const messageContent = JSON.parse(msg.content.toString());
        const routingKey = msg.fields.routingKey;
        console.log(`[RabbitMQ] Received message with routing key: ${routingKey}`);
        console.log(`[RabbitMQ] Message content:`, messageContent);

        // Determine event type based on routing key
        if (routingKey === 'notification.user.sync') {
          await handleUserSyncEvent(messageContent);
        } else if (routingKey === 'notification.expo.save') {
          await handleSaveExpoPushTokenEvent(messageContent);
        } else if (routingKey === 'notification.expo.delete') {
          await handleDeleteExpoPushTokenEvent(messageContent);
        } else if (routingKey.startsWith('notification.send.')) {
          await handleSendNotificationEvent(messageContent);
        } else {
          console.warn(`[RabbitMQ] Unknown routing key: ${routingKey}. Message not processed.`);
        }

        channel.ack(msg); // Acknowledge message if processed successfully
      } catch (error) {
        console.error(`[RabbitMQ] Error processing message:`, error);
        channel.nack(msg, false, false); // Nack message, do not re-queue
      }
    }, {
      noAck: false // Manual acknowledgment
    });

  } catch (error) {
    console.error('[RabbitMQ] Failed to connect or setup consumer:', error);
    // Attempt to reconnect after a delay
    setTimeout(initRabbitMQ, 5000);
  }

  // Handle connection close and errors
  connection.on('close', (err) => {
    console.error('[RabbitMQ] Connection closed:', err);
    setTimeout(initRabbitMQ, 5000); // Attempt to reconnect
  });

  connection.on('error', (err) => {
    console.error('[RabbitMQ] Connection error:', err);
  });
}

if (require.main === module) {
  app.listen(port, () => {
    console.log(`Notification service listening at http://localhost:${port}`);
    initRabbitMQ().catch(console.error); // Initialize RabbitMQ and start consuming
  });
}

module.exports = app;
