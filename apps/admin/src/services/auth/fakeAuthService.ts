import type { AuthService } from './authService';
import type { UserProfile } from '@/types';
import type { RedirectLoginOptions } from '@auth0/auth0-spa-js';
import type { AppState } from '@/types';

class FakeAuthService implements AuthService {
  private currentUser: UserProfile | null = null;
  private currentToken: string | null = null;

  constructor() {
    this.currentUser = {
      id: 'fake-user-123',
      externalId: 'auth0|fake-user-123',
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

  async login(_options?: RedirectLoginOptions): Promise<void> {

    // Simulate a successful login after a redirect.
    // In a real scenario, this would trigger a redirect.
    this.currentUser = {
      id: 'fake-user-123',
      externalId: 'auth0|fake-user-123',
      name: 'Fake User',
      email: 'fake@example.com',
      avatar: 'https://i.pravatar.cc/150?u=fake@example.com',
      roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
    };
    this.currentToken = 'fake-jwt-token';
    // No return value as per AuthService interface
  }

  async logout(): Promise<void> {

    this.currentUser = null;
    this.currentToken = null;
  }

  async register(data: any): Promise<UserProfile | null> {

    // Simulate successful registration
    this.currentUser = {
      id: 'new-fake-user',
      externalId: 'auth0|new-fake-user',
      name: data.name || 'New Fake User',
      email: data.email,
      avatar: 'https://i.pravatar.cc/150?u=new@example.com',
      roles: ['Viewer'],
    };
    this.currentToken = 'new-fake-jwt-token';
    return this.currentUser;
  }

  async getUser(): Promise<UserProfile | null> {
    return this.currentUser;
  }

  async getAccessToken(): Promise<string | null> {
    return this.currentToken;
  }

  async handleRedirectCallback(): Promise<AppState> {

    return {}; // Return an empty AppState for now
  }
}

export const fakeAuthService = new FakeAuthService();
