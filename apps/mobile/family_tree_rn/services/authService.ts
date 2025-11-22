import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import { jwtDecode } from 'jwt-decode'; // Fix: Named import for jwt-decode
import { Platform } from 'react-native';

WebBrowser.maybeCompleteAuthSession();

const AUTH0_DOMAIN = process.env.EXPO_PUBLIC_AUTH0_DOMAIN;
const AUTH0_CLIENT_ID = process.env.EXPO_PUBLIC_AUTH0_CLIENT_ID;

interface UserProfile {
  name: string;
  email: string;
  picture?: string;
  sub: string;
}

class AuthService {
  private user: UserProfile | null = null;
  private accessToken: string | null = null;
  private idToken: string | null = null;

  constructor() {
    if (!AUTH0_DOMAIN || !AUTH0_CLIENT_ID) {
      console.error('Auth0 environment variables are not set.');
    }
  }

  // Get the redirect URL for the platform
  private getRedirectUri(): string {
    if (Platform.OS === 'web') {
      return AuthSession.makeRedirectUri(); // Fix: Removed useProxy
    }
    return AuthSession.makeRedirectUri({
      scheme: 'familytreeapp', // Replace with your scheme
      path: 'auth',
    });
  }

  public async login(): Promise<boolean> {
    const redirectUri = this.getRedirectUri();
    const authUrl = `https://${AUTH0_DOMAIN}/authorize?` +
      `scope=openid profile email&` +
      `audience=https://${AUTH0_DOMAIN}/api/v2/&` +
      `response_type=token id_token&` +
      `client_id=${AUTH0_CLIENT_ID}&` +
      `redirect_uri=${redirectUri}`;

    const result = await WebBrowser.openAuthSessionAsync(authUrl, redirectUri);

    if (result.type === 'success') {
      const parsedUrl = (AuthSession as any).parseUrlQuery(result.url);

      if (parsedUrl.error) {
        console.error('AuthSession error:', parsedUrl.error_description || parsedUrl.error);
        return false;
      }

      this.accessToken = parsedUrl.access_token;
      this.idToken = parsedUrl.id_token;

      if (this.idToken) {
        this.user = jwtDecode<UserProfile>(this.idToken);
        console.log('Logged in user:', this.user);
        return true;
      }
    } else if (result.type === 'cancel') {
      console.log('Authentication cancelled by user.');
    } else if (result.type === 'dismiss') { // WebBrowser returns 'dismiss' on Android back button
      console.log('Authentication dismissed by user.');
    }
    return false;
  }

  public async logout(): Promise<void> {
    if (!this.user) {
      return;
    }

    const redirectUri = AuthSession.makeRedirectUri({
      scheme: 'familytreeapp', // Replace with your scheme
      path: 'logout',
    });
    const logoutUrl = `https://${AUTH0_DOMAIN}/v2/logout?` +
      `client_id=${AUTH0_CLIENT_ID}&` +
      `returnTo=${redirectUri}`;

    await WebBrowser.openAuthSessionAsync(logoutUrl, redirectUri);

    this.user = null;
    this.accessToken = null;
    this.idToken = null;
  }

  public isAuthenticated(): boolean {
    return !!this.user;
  }

  public getUser(): UserProfile | null {
    return this.user;
  }

  public getAccessToken(): string | null {
    return this.accessToken;
  }
}

export const authService = new AuthService();
