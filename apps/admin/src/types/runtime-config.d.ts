interface RuntimeConfig {
  VITE_API_BASE_URL: string;
  VITE_AUTH0_DOMAIN: string;
  VITE_AUTH0_CLIENT_ID: string;
  VITE_AUTH0_AUDIENCE: string;
  VITE_NOVU_APPLICATION_IDENTIFIER: string;
  VITE_APP_BUILD_DATE: string;
  VITE_APP_ENVIRONMENT: string;
  VITE_APP_COMMIT_ID: string;
}

interface Window {
  runtimeConfig: RuntimeConfig;
}