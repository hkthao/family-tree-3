const { Novu } = require('@novu/node');
const config = require('@/config');
const { removeUndefinedProps } = require('@/utils'); // Import the utility

class NovuService {
  constructor() {
    if (!config.novu.apiKey) {
      throw new Error('NOVU_API_KEY is not defined in environment variables.');
    }
    this.novu = new Novu(config.novu.apiKey);
  }

  async identifySubscriber(userId, payload) {
    removeUndefinedProps(payload); // Use the utility function
    return this.novu.subscribers.identify(userId, payload);
  }

  async setExpoCredentials(userId, expoPushTokens) {
    return this.novu.subscribers.setCredentials(userId, 'expo', {
      deviceTokens: expoPushTokens,
    });
  }

  async triggerNotification(workflowId, subscriberId, payload) {
    let isPushEnabled = false;
    try {
      const subscriber = await this.novu.subscribers.getSubscriber(subscriberId);
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
      console.warn(`Could not retrieve subscriber ${subscriberId} details for push check:`, subscriberError.message);
      isPushEnabled = false;
    }

    const updatedPayload = { ...payload, is_push_enabled: isPushEnabled };

    return this.novu.trigger(workflowId, {
      to: {
        subscriberId: subscriberId,
      },
      payload: updatedPayload,
    });
  }

  async getSubscriber(subscriberId) {
    return this.novu.subscribers.getSubscriber(subscriberId);
  }
}

module.exports = new NovuService();
