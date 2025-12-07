// apps/mobile/family_tree_rn/services/api/publicApi.ts

import axios from 'axios';

// TODO: Configure this based on your environment (e.g., .env file)
const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL+'/api/public'; // Example: Replace with your backend URL

export const publicApiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to include the API Key
publicApiClient.interceptors.request.use(
  (config) => {
    const apiKey = process.env.EXPO_PUBLIC_API_KEY;
    if (apiKey) {
      config.headers['X-App-Key'] = apiKey; // Add the API Key header
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
