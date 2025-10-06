import type { Credentials, User } from '@/types';
import type { RedirectLoginOptions } from '@auth0/auth0-spa-js';

export interface AuthService {
  isAuthenticated(): Promise<boolean>;
  login(options?: RedirectLoginOptions): Promise<void>;
  logout(): Promise<void>;
  register(data: any): Promise<User | null>; // 'any' for now, define specific type later
  getUser(): Promise<User | null>;
  getAccessToken(): Promise<string | null>;
  handleRedirectCallback(): Promise<void>;
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
