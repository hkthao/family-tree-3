import type { AuthService } from './authService';
import type { User } from '@/types';
import type { RedirectLoginOptions } from '@auth0/auth0-spa-js';
import type { AppState } from '@/types/auth';

class FakeAuthService implements AuthService {
  private currentUser: User | null = null;
  private currentToken: string | null = null;

  constructor() {
    this.currentUser = {
      id: 'fake-user-123',
      auth0UserId: 'auth0|fake-user-123',
      name: 'Fake User',
      email: 'fake@example.com',
      avatar: 'https://i.pravatar.cc/150?u=fake@example.com',
      roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
    };
    this.currentToken = 'fake-jwt-token';
  }

  async isAuthenticated(): Promise<boolean> {
    return !!this.currentUser;
  }

  async login(options?: RedirectLoginOptions): Promise<void> {
    console.log('Fake login initiated with options:', options);
    // Simulate a successful login after a redirect.
    // In a real scenario, this would trigger a redirect.
    this.currentUser = {
      id: 'fake-user-123',
      auth0UserId: 'auth0|fake-user-123',
      name: 'Fake User',
      email: 'fake@example.com',
      avatar: 'https://i.pravatar.cc/150?u=fake@example.com',
      roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
    };
    this.currentToken = 'fake-jwt-token';
    // No return value as per AuthService interface
  }

  async logout(): Promise<void> {
    console.log('Fake logout');
    this.currentUser = null;
    this.currentToken = null;
  }

  async register(data: any): Promise<User | null> {
    console.log('Fake register attempt with:', data);
    // Simulate successful registration
    this.currentUser = {
      id: 'new-fake-user',
      auth0UserId: 'auth0|new-fake-user',
      name: data.name || 'New Fake User',
      email: data.email,
      avatar: 'https://i.pravatar.cc/150?u=new@example.com',
      roles: ['Viewer'],
    };
    this.currentToken = 'new-fake-jwt-token';
    return this.currentUser;
  }

  async getUser(): Promise<User | null> {
    return this.currentUser;
  }

  async getAccessToken(): Promise<string | null> {
    return this.currentToken;
  }

  async handleRedirectCallback(): Promise<AppState> {
    console.log('Fake handleRedirectCallback called');
    return {}; // Return an empty AppState for now
  }
}

export const fakeAuthService = new FakeAuthService();
