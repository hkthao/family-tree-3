import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api', // Default to /api if not set
  headers: {
    'Content-type': 'application/json',
  },
});

// You can add interceptors here for request/response handling, e.g., for auth tokens

export default apiClient;
