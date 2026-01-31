const amqp = require('amqplib');
const config = require('@/config');
const {
  handleUserSyncEvent,
  handleSaveExpoPushTokenEvent,
  handleDeleteExpoPushTokenEvent,
  handleSendNotificationEvent,
} = require('@/rabbitmq/handlers');

let connection = null;
let channel = null;

async function connectRabbitMQ() {
  try {
    connection = await amqp.connect(config.rabbitmq.url);
    channel = await connection.createChannel();

    await channel.assertExchange(config.rabbitmq.exchange, 'topic', { durable: true });
    console.log(`[RabbitMQ] Exchange '${config.rabbitmq.exchange}' asserted.`);

    const q = await channel.assertQueue(config.rabbitmq.queue, { durable: true });
    console.log(`[RabbitMQ] Queue '${q.queue}' asserted.`);

    for (const key of config.rabbitmq.routingKeys) {
      await channel.bindQueue(q.queue, config.rabbitmq.exchange, key);
      console.log(`[RabbitMQ] Queue '${q.queue}' bound to exchange '${config.rabbitmq.exchange}' with routing key '${key}'.`);
    }

    console.log(`[RabbitMQ] Waiting for messages in queue '${q.queue}'.`);

    channel.consume(q.queue, async (msg) => {
      if (msg === null) return;

      try {
        const messageContent = JSON.parse(msg.content.toString());
        const routingKey = msg.fields.routingKey;
        console.log(`[RabbitMQ] Received message with routing key: ${routingKey}`);
        console.log(`[RabbitMQ] Message content:`, messageContent);

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

        channel.ack(msg);
      } catch (error) {
        console.error(`[RabbitMQ] Error processing message:`, error);
        channel.nack(msg, false, false);
      }
    }, {
      noAck: false
    });

    connection.on('close', (err) => {
      console.error('[RabbitMQ] Connection closed:', err);
      if (connection) {
        // Only attempt to reconnect if connection was previously established and not intentionally closed
        console.log('[RabbitMQ] Attempting to reconnect...');
        setTimeout(connectRabbitMQ, 5000);
      }
    });

    connection.on('error', (err) => {
      console.error('[RabbitMQ] Connection error:', err);
      if (connection) {
        // Only attempt to reconnect if connection was previously established and not intentionally closed
        console.log('[RabbitMQ] Attempting to reconnect...');
        setTimeout(connectRabbitMQ, 5000);
      }
    });

  } catch (error) {
    console.error('[RabbitMQ] Failed to connect or setup consumer:', error);
    console.log('[RabbitMQ] Attempting to reconnect...');
    setTimeout(connectRabbitMQ, 5000);
  }
}

async function closeRabbitMQ() {
  if (channel) {
    await channel.close();
    channel = null;
  }
  if (connection) {
    await connection.close();
    connection = null;
  }
  console.log('[RabbitMQ] Connection closed gracefully.');
}

module.exports = {
  connectRabbitMQ,
  closeRabbitMQ,
};
