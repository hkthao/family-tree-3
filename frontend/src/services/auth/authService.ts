import type { Credentials } from '@/types/auth';

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  roles?: string[];
}

export interface AuthService {
  login(credentials: Credentials): Promise<AuthUser | null>;
  logout(): Promise<void>;
  register(data: any): Promise<AuthUser | null>; // 'any' for now, define specific type later
  getUser(): Promise<AuthUser | null>;
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
