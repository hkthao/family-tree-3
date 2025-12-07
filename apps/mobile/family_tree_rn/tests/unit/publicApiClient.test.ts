import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import { publicApiClient, apiClientWithAuth } from '@/services/api/publicApiClient';
import { authService } from '@/services/authService';

// Mock authService
jest.mock('@/services/authService', () => ({
  authService: {
    getAccessToken: jest.fn(),
  },
}));

describe('publicApiClient', () => {
  let publicClientMock: MockAdapter;
  const originalEnv = process.env;
  let currentPublicApiClient: any;

  beforeEach(() => {
    jest.resetModules();
    process.env = { ...originalEnv };
    process.env.EXPO_PUBLIC_API_BASE_URL = 'http://localhost:3000'; // Set a default base URL for publicApiClient tests
    currentPublicApiClient = require('@/services/api/publicApiClient').publicApiClient;
    publicClientMock = new MockAdapter(currentPublicApiClient);
    jest.clearAllMocks();
  });

  afterEach(() => {
    publicClientMock.restore();
    process.env = originalEnv; // Restore original env
    // The Authorization header is only set temporarily during the request in apiClientWithAuth,
    // so no need to explicitly delete it here for publicApiClient tests as currentPublicApiClient
    // is a fresh instance each time.
  });

  it('should have the correct base URL configured', () => {
    expect(currentPublicApiClient.defaults.baseURL).toBe('http://localhost:3000/api/public');
  });

  it('should set Content-Type header to application/json by default', () => {
    expect(currentPublicApiClient.defaults.headers['Content-Type']).toBe('application/json');
  });

  describe('request interceptor for X-App-Key', () => {
    it('should add X-App-Key header if EXPO_PUBLIC_API_KEY is defined', async () => {
      jest.resetModules();
      process.env.EXPO_PUBLIC_API_BASE_URL = 'http://localhost:3000'; // Ensure base URL is set
      process.env.EXPO_PUBLIC_API_KEY = 'test-api-key';
      const { publicApiClient: reImportedClient } = require('@/services/api/publicApiClient');
      const testMock = new MockAdapter(reImportedClient); // Re-initialize mock for this instance
      testMock.onGet('/test').reply(200);

      await reImportedClient.get('/test');

      expect(testMock.history.get[0].headers?.['X-App-Key']).toBe('test-api-key');
      testMock.restore();
    });

    it('should NOT add X-App-Key header if EXPO_PUBLIC_API_KEY is NOT defined', async () => {
      jest.resetModules();
      process.env.EXPO_PUBLIC_API_BASE_URL = 'http://localhost:3000'; // Ensure base URL is set
      delete process.env.EXPO_PUBLIC_API_KEY;
      const { publicApiClient: reImportedClient } = require('@/services/api/publicApiClient');
      const testMock = new MockAdapter(reImportedClient); // Re-initialize mock for this instance
      testMock.onGet('/test').reply(200);

      await reImportedClient.get('/test');

      expect(testMock.history.get[0].headers?.['X-App-Key']).toBeUndefined();
      testMock.restore();
    });
  });
});

describe('apiClientWithAuth', () => {
  let mock: MockAdapter;

  beforeEach(() => {
    mock = new MockAdapter(publicApiClient);
    jest.clearAllMocks();
  });

  afterEach(() => {
    mock.restore();
    // Reset headers to ensure no side effects
    delete publicApiClient.defaults.headers.common['Authorization'];
  });

  describe('GET method', () => {
    it('should add Authorization header if access token is available', async () => {
      const mockAccessToken = 'mock-access-token';
      (authService.getAccessToken as jest.Mock).mockReturnValue(mockAccessToken);
      mock.onGet('/data').reply(200, { message: 'Success' });

      await apiClientWithAuth.get('/data');

      expect(mock.history.get[0].headers?.['Authorization']).toBe(`Bearer ${mockAccessToken}`);
      // Verify header is removed after request
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });

    it('should NOT add Authorization header if access token is NOT available', async () => {
      (authService.getAccessToken as jest.Mock).mockReturnValue(null);
      mock.onGet('/data').reply(200, { message: 'Success' });

      await apiClientWithAuth.get('/data');

      expect(mock.history.get[0].headers?.['Authorization']).toBeUndefined();
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });
  });

  describe('POST method', () => {
    it('should add Authorization header if access token is available', async () => {
      const mockAccessToken = 'mock-access-token';
      (authService.getAccessToken as jest.Mock).mockReturnValue(mockAccessToken);
      mock.onPost('/data').reply(200, { message: 'Created' });

      await apiClientWithAuth.post('/data', { item: 'new' });

      expect(mock.history.post[0].headers?.['Authorization']).toBe(`Bearer ${mockAccessToken}`);
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });

    it('should NOT add Authorization header if access token is NOT available', async () => {
      (authService.getAccessToken as jest.Mock).mockReturnValue(null);
      mock.onPost('/data').reply(200, { message: 'Created' });

      await apiClientWithAuth.post('/data', { item: 'new' });

      expect(mock.history.post[0].headers?.['Authorization']).toBeUndefined();
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });
  });

  describe('PUT method', () => {
    it('should add Authorization header if access token is available', async () => {
      const mockAccessToken = 'mock-access-token';
      (authService.getAccessToken as jest.Mock).mockReturnValue(mockAccessToken);
      mock.onPut('/data/1').reply(200, { message: 'Updated' });

      await apiClientWithAuth.put('/data/1', { item: 'updated' });

      expect(mock.history.put[0].headers?.['Authorization']).toBe(`Bearer ${mockAccessToken}`);
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });

    it('should NOT add Authorization header if access token is NOT available', async () => {
      (authService.getAccessToken as jest.Mock).mockReturnValue(null);
      mock.onPut('/data/1').reply(200, { message: 'Updated' });

      await apiClientWithAuth.put('/data/1', { item: 'updated' });

      expect(mock.history.put[0].headers?.['Authorization']).toBeUndefined();
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });
  });

  describe('DELETE method', () => {
    it('should add Authorization header if access token is available', async () => {
      const mockAccessToken = 'mock-access-token';
      (authService.getAccessToken as jest.Mock).mockReturnValue(mockAccessToken);
      mock.onDelete('/data/1').reply(200, { message: 'Deleted' });

      await apiClientWithAuth.delete('/data/1');

      expect(mock.history.delete[0].headers?.['Authorization']).toBe(`Bearer ${mockAccessToken}`);
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });

    it('should NOT add Authorization header if access token is NOT available', async () => {
      (authService.getAccessToken as jest.Mock).mockReturnValue(null);
      mock.onDelete('/data/1').reply(200, { message: 'Deleted' });

      await apiClientWithAuth.delete('/data/1');

      expect(mock.history.delete[0].headers?.['Authorization']).toBeUndefined();
      expect(publicApiClient.defaults.headers.common['Authorization']).toBeUndefined();
    });
  });
});