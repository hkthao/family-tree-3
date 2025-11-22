/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, any>
  export default component
}

interface ImportMetaEnv {
  readonly VITE_APP_USE_MOCK_API: string;
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_API_PUBLIC_KEY?: string; // New: Public API Key
  // Add other environment variables here as needed
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

interface RuntimeConfig {
  VITE_API_BASE_URL: string;
  VITE_APP_BASE_URL: string; // New: Base URL for the application
  VITE_AUTH0_DOMAIN: string;
  VITE_AUTH0_CLIENT_ID: string;
  VITE_AUTH0_AUDIENCE: string;
  VITE_NOVU_APPLICATION_IDENTIFIER: string;
  VITE_APP_BUILD_DATE: string;
  VITE_APP_ENVIRONMENT: string;
  VITE_APP_COMMIT_ID: string;
  VITE_API_PUBLIC_KEY?: string; // New: Public API Key
}

interface Window {
  runtimeConfig: RuntimeConfig;
}