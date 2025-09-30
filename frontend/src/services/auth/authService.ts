import type { Credentials, User } from '@/types';

export interface AuthService {
  login(credentials: Credentials): Promise<User | null>;
  logout(): Promise<void>;
  register(data: any): Promise<User | null>; // 'any' for now, define specific type later
  getUser(): Promise<User | null>;
  getAccessToken(): Promise<string | null>;
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
