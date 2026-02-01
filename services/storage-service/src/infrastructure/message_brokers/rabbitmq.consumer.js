const amqp = require('amqplib');
const IMessageBrokerConsumer = require('../../domain/interfaces/IMessageBrokerConsumer');

class RabbitMQConsumer extends IMessageBrokerConsumer {
  constructor(rabbitMqUrl) {
    super();
    this.rabbitMqUrl = rabbitMqUrl;
    this.connection = null;
    this.channel = null;
  }

  async connect() {
    try {
      this.connection = await amqp.connect(this.rabbitMqUrl);
      this.channel = await this.connection.createChannel();
      console.log('Connected to RabbitMQ');
    } catch (error) {
      console.error('Failed to connect to RabbitMQ:', error);
      // Optional: implement retry logic
      throw error;
    }
  }

  async consumeMessages(queueName, callback) {
    if (!this.channel) {
      await this.connect();
    }
    await this.channel.assertQueue(queueName, { durable: true });
    console.log(`Waiting for messages in ${queueName}. To exit press CTRL+C`);

    this.channel.consume(queueName, (msg) => {
      if (msg !== null) {
        callback(msg);
      }
    });
  }

  async closeConnection() {
    try {
      if (this.channel) await this.channel.close();
      if (this.connection) await this.connection.close();
      console.log('RabbitMQ connection closed.');
    } catch (error) {
      console.error('Error closing RabbitMQ connection:', error);
    }
  }
}

module.exports = RabbitMQConsumer;
