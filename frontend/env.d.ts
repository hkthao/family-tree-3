/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_APP_USE_MOCK_API: string;
  readonly VITE_API_BASE_URL?: string;
  // Add other environment variables here as needed
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
