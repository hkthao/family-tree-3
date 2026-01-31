const request = require('supertest');
const { Novu } = require('@novu/node');

// Define mock objects at a higher scope
const mockSubscribers = {
  identify: jest.fn(),
  setCredentials: jest.fn(),
  getSubscriber: jest.fn(), // Mock getSubscriber as well for triggerNotification logic
};
const mockNovuInstance = {
  subscribers: mockSubscribers,
  trigger: jest.fn(),
};

// Mock Novu
jest.mock('@novu/node', () => {
  return {
    Novu: jest.fn(() => mockNovuInstance),
  };
});

// Mock dotenv config to prevent it from loading actual .env files during tests
jest.mock('dotenv', () => ({
  config: jest.fn(),
}));

// Set a dummy NOVU_API_KEY for testing before importing the app
process.env.NOVU_API_KEY = 'test_api_key';

// Mock the rabbitmq connection to prevent it from trying to connect during tests
jest.mock('../../src/rabbitmq', () => ({
  connectRabbitMQ: jest.fn().mockResolvedValue(true),
  closeRabbitMQ: jest.fn().mockResolvedValue(true),
}));

// Import the app after all mocks and environment variables are set
const app = require('../../src/app'); // Import from the new app.js

describe('Notification Service API', () => {
  let consoleErrorSpy;
  let consoleWarnSpy;

  beforeEach(() => {
    // Clear all mocks for the Novu instance's methods before each test
    mockSubscribers.identify.mockClear();
    mockSubscribers.setCredentials.mockClear();
    mockSubscribers.getSubscriber.mockClear();
    mockNovuInstance.trigger.mockClear();

    // Reset default mock for getSubscriber
    mockSubscribers.getSubscriber.mockResolvedValue({
      data: {
        channel_credentials: [], // Default to no push credentials
      },
    });

    // Silence console.error and console.warn during tests
    consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
    consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});
  });

  afterEach(() => {
    // Restore console.error and console.warn after each test
    consoleErrorSpy.mockRestore();
    consoleWarnSpy.mockRestore();
  });

  describe('POST /subscribers/sync', () => {
    test('should sync a subscriber successfully', async () => {
      mockSubscribers.identify.mockResolvedValueOnce({ data: { subscriberId: 'user123' } });

      const subscriberPayload = {
        userId: 'user123',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        phone: '+1234567890',
        avatar: 'http://example.com/avatar.jpg',
        locale: 'en-US',
        timezone: 'America/New_York',
      };

      const response = await request(app)
        .post('/subscribers/sync')
        .send(subscriberPayload)
        .expect(200);

      expect(response.body.message).toBe('Subscriber synced successfully');
      expect(mockSubscribers.identify).toHaveBeenCalledTimes(1);
      // The payload sent to novu.subscribers.identify will now be cleaned by removeUndefinedProps
      // So we should expect it without undefined/null, and the data object if timezone is present.
      expect(mockSubscribers.identify).toHaveBeenCalledWith('user123', {
        firstName: subscriberPayload.firstName,
        lastName: subscriberPayload.lastName,
        email: subscriberPayload.email,
        phone: subscriberPayload.phone,
        avatar: subscriberPayload.avatar,
        locale: subscriberPayload.locale,
        data: {
          timezone: subscriberPayload.timezone,
        },
      });
    });

    test('should sync a subscriber successfully without optional fields', async () => {
      mockSubscribers.identify.mockResolvedValueOnce({ data: { subscriberId: 'user456' } });

      const subscriberPayload = {
        userId: 'user456',
        firstName: 'Jane',
        email: 'jane.doe@example.com',
      };

      const response = await request(app)
        .post('/subscribers/sync')
        .send(subscriberPayload)
        .expect(200);

      expect(response.body.message).toBe('Subscriber synced successfully');
      expect(mockSubscribers.identify).toHaveBeenCalledTimes(1);
      expect(mockSubscribers.identify).toHaveBeenCalledWith('user456', {
        firstName: subscriberPayload.firstName,
        email: subscriberPayload.email,
      });
    });

    test('should return 400 if userId is missing', async () => {
      const response = await request(app)
        .post('/subscribers/sync')
        .send({})
        .expect(400);

      expect(response.body.error).toBe('Missing required field: userId');
      expect(mockSubscribers.identify).not.toHaveBeenCalled();
    });

    test('should return 500 if Novu identify fails', async () => {
      mockSubscribers.identify.mockRejectedValueOnce(new Error('Novu error'));

      const response = await request(app)
        .post('/subscribers/sync')
        .send({ userId: 'user123' })
        .expect(500);

      expect(response.body.error).toBe('Failed to sync subscriber');
      expect(response.body.details).toBe('Novu error');
    });
  });

  describe('POST /subscribers/expo-token', () => {
    test('should add expo push token successfully', async () => {
      mockSubscribers.setCredentials.mockResolvedValueOnce({});

      const response = await request(app)
        .post('/subscribers/expo-token')
        .send({ userId: 'user123', expoPushTokens: ['ExponentPushToken[abc]'] })
        .expect(200);

      expect(response.body.message).toBe('Expo Push Tokens added successfully');
      expect(mockSubscribers.setCredentials).toHaveBeenCalledTimes(1);
      expect(mockSubscribers.setCredentials).toHaveBeenCalledWith('user123', 'expo', {
        deviceTokens: ['ExponentPushToken[abc]'],
      });
    });

    test('should return 400 if userId is missing', async () => {
      const response = await request(app)
        .post('/subscribers/expo-token')
        .send({ expoPushTokens: ['ExponentPushToken[abc]'] })
        .expect(400);

      expect(response.body.error).toBe('Missing required field: userId');
      expect(mockSubscribers.setCredentials).not.toHaveBeenCalled();
    });

    test('should return 400 if expoPushTokens is missing or not an array', async () => {
      const response1 = await request(app)
        .post('/subscribers/expo-token')
        .send({ userId: 'user123' })
        .expect(400);
      expect(response1.body.error).toBe('Missing required field: expoPushTokens');

      const response2 = await request(app)
        .post('/subscribers/expo-token')
        .send({ userId: 'user123', expoPushTokens: 'not_an_array' })
        .expect(400);
      expect(response2.body.error).toBe('Missing required field: expoPushTokens');

      expect(mockSubscribers.setCredentials).not.toHaveBeenCalled();
    });

    test('should return 500 if Novu setCredentials fails', async () => {
      mockSubscribers.setCredentials.mockRejectedValueOnce(new Error('Novu error'));

      const response = await request(app)
        .post('/subscribers/expo-token')
        .send({ userId: 'user123', expoPushTokens: ['ExponentPushToken[abc]'] })
        .expect(500);

      expect(response.body.error).toBe('Failed to add Expo Push Token');
      expect(response.body.details).toBe('Novu error');
    });
  });

  describe('POST /notifications/send', () => {
    test('should trigger notification successfully with push enabled if credentials exist', async () => {
      mockSubscribers.getSubscriber.mockResolvedValueOnce({
        data: {
          channel_credentials: [
            { provider: 'expo', credentials: { deviceTokens: ['token1'] } },
          ],
        },
      });
      mockNovuInstance.trigger.mockResolvedValueOnce({});

      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', userId: 'user123', payload: { message: 'Hello' } })
        .expect(200);

      expect(response.body.message).toBe('Notification triggered successfully');
      expect(mockNovuInstance.trigger).toHaveBeenCalledTimes(1);
      expect(mockNovuInstance.trigger).toHaveBeenCalledWith('workflow1', {
        to: { subscriberId: 'user123' },
        payload: { message: 'Hello', is_push_enabled: true },
      });
    });

    test('should trigger notification successfully with push disabled if no credentials', async () => {
      // Default mock for getSubscriber returns no credentials
      mockNovuInstance.trigger.mockResolvedValueOnce({});

      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', userId: 'user123', payload: { message: 'Hello' } })
        .expect(200);

      expect(response.body.message).toBe('Notification triggered successfully');
      expect(mockNovuInstance.trigger).toHaveBeenCalledTimes(1);
      expect(mockNovuInstance.trigger).toHaveBeenCalledWith('workflow1', {
        to: { subscriberId: 'user123' },
        payload: { message: 'Hello', is_push_enabled: false },
      });
    });

    test('should trigger notification successfully with push disabled if getSubscriber fails', async () => {
      mockSubscribers.getSubscriber.mockRejectedValueOnce(new Error('Subscriber not found'));
      mockNovuInstance.trigger.mockResolvedValueOnce({});

      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', userId: 'user123', payload: { message: 'Hello' } })
        .expect(200);

      expect(response.body.message).toBe('Notification triggered successfully');
      expect(mockNovuInstance.trigger).toHaveBeenCalledTimes(1);
      expect(mockNovuInstance.trigger).toHaveBeenCalledWith('workflow1', {
        to: { subscriberId: 'user123' },
        payload: { message: 'Hello', is_push_enabled: false },
      });
    });

    test('should return 400 if workflowId is missing', async () => {
      const response = await request(app)
        .post('/notifications/send')
        .send({ userId: 'user123', payload: { message: 'Hello' } })
        .expect(400);

      expect(response.body.error).toBe('Missing required field: workflowId');
      expect(mockNovuInstance.trigger).not.toHaveBeenCalled();
    });

    test('should return 400 if userId is missing', async () => {
      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', payload: { message: 'Hello' } })
        .expect(400);

      expect(response.body.error).toBe('Missing required field: userId');
      expect(mockNovuInstance.trigger).not.toHaveBeenCalled();
    });

    test('should return 400 if payload is missing', async () => {
      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', userId: 'user123' })
        .expect(400);

      expect(response.body.error).toBe('Missing required field: payload');
      expect(mockNovuInstance.trigger).not.toHaveBeenCalled();
    });

    test('should return 500 if Novu trigger fails', async () => {
      mockNovuInstance.trigger.mockRejectedValueOnce(new Error('Novu error'));

      const response = await request(app)
        .post('/notifications/send')
        .send({ workflowId: 'workflow1', userId: 'user123', payload: { message: 'Hello' } })
        .expect(500);

      expect(response.body.error).toBe('Failed to trigger notification');
      expect(response.body.details).toBe('Novu error');
    });
  });
});