import type { AppState, UserProfile } from '@/types';
import type { RedirectLoginOptions } from '@auth0/auth0-spa-js';

export interface AuthService {
  isAuthenticated(): Promise<boolean>;
  login(options?: RedirectLoginOptions): Promise<void>;
  logout(): Promise<void>;
  register(data: any): Promise<UserProfile | null>; // 'any' for now, define specific type later
  getUser(): Promise<UserProfile | null>;
  getAccessToken(): Promise<string | null>;
  handleRedirectCallback(): Promise<AppState>;
}

let authServiceInstance: AuthService | null = null;

export function setAuthService(service: AuthService) {
  authServiceInstance = service;
}

export function useAuthService(): AuthService {
  if (!authServiceInstance) {
    throw new Error('AuthService not initialized. Call setAuthService first.');
  }
  return authServiceInstance;
}
