class IMessageBrokerConsumer {
    async consumeMessages(queueName, callback) {
        throw new Error('Method "consumeMessages" must be implemented');
    }

    async closeConnection() {
        throw new Error('Method "closeConnection" must be implemented');
    }
}

module.exports = IMessageBrokerConsumer;