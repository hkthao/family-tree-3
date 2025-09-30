import type { AuthService } from './authService';
import type { Credentials, User } from '@/types';

class FakeAuthService implements AuthService {
  private currentUser: User | null = null;
  private currentToken: string | null = null;

  constructor() {
    // Simulate a logged-in user for development
    this.currentUser = {
      id: 'fake-user-123',
      name: 'Fake User',
      email: 'fake@example.com',
      avatar: 'https://i.pravatar.cc/150?u=fake@example.com',
      roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
    };
    this.currentToken = 'fake-jwt-token';
  }

  async login(credentials: Credentials): Promise<User | null> {
    console.log('Fake login attempt with:', credentials);
    if (
      credentials.email === 'test@example.com' &&
      credentials.password === 'password'
    ) {
      this.currentUser = {
        id: 'test-user-456',
        name: 'Test User',
        email: 'test@example.com',
        avatar: 'https://i.pravatar.cc/150?u=test@example.com',
        roles: ['Viewer'],
      };
      this.currentToken = 'test-jwt-token';
      return this.currentUser;
    }
    return null;
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
}

export const fakeAuthService = new FakeAuthService();
