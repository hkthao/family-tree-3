import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import { jwtDecode } from 'jwt-decode';
import { Platform } from 'react-native';
import { nanoid } from 'nanoid';
import * as Crypto from 'expo-crypto'; // Import expo-crypto
import AsyncStorage from '@react-native-async-storage/async-storage'; // Add AsyncStorage import

// Constants for AsyncStorage keys
const ACCESS_TOKEN_KEY = 'accessToken';
const ID_TOKEN_KEY = 'idToken';
const USER_PROFILE_KEY = 'userProfile';

// Polyfill global.crypto for nanoid
if (typeof global.crypto === 'undefined' || !global.crypto.getRandomValues) {
  global.crypto = {
    ...(global.crypto || {}),
    getRandomValues: Crypto.getRandomValues as unknown as <T extends ArrayBufferView>(array: T) => T,
  };
}

WebBrowser.maybeCompleteAuthSession();

const AUTH0_DOMAIN = process.env.EXPO_PUBLIC_AUTH0_DOMAIN;
const AUTH0_CLIENT_ID = process.env.EXPO_PUBLIC_AUTH0_CLIENT_ID;
const AUTH0_AUDIENCE = process.env.EXPO_PUBLIC_AUTH0_AUDIENCE;

interface IdTokenPayload {
  name: string;
  email: string;
  picture?: string;
  sub: string;
  nonce?: string; // Add nonce to UserProfile interface
}

class AuthService {
  private user: IdTokenPayload | null = null;
  private accessToken: string | null = null;
  private idToken: string | null = null;
  private nonce: string | null = null; // Store nonce

  constructor() {
    if (!AUTH0_DOMAIN || !AUTH0_CLIENT_ID) {
      console.error('Auth0 environment variables are not set. Check EXPO_PUBLIC_AUTH0_DOMAIN and EXPO_PUBLIC_AUTH0_CLIENT_ID');
    }
  }

  // Public method to initialize the session
  public async initSession(): Promise<void> {
    await this._loadSession();
  }

  // Persist authentication data to AsyncStorage
  private async _saveSession() {
    if (this.accessToken) {
      await AsyncStorage.setItem(ACCESS_TOKEN_KEY, this.accessToken);
    }
    if (this.idToken) {
      await AsyncStorage.setItem(ID_TOKEN_KEY, this.idToken);
    }
    if (this.user) {
      await AsyncStorage.setItem(USER_PROFILE_KEY, JSON.stringify(this.user));
    }
  }

  // Load authentication data from AsyncStorage
  private async _loadSession() {
    this.accessToken = await AsyncStorage.getItem(ACCESS_TOKEN_KEY);
    this.idToken = await AsyncStorage.getItem(ID_TOKEN_KEY);
    const userProfileString = await AsyncStorage.getItem(USER_PROFILE_KEY);
    if (userProfileString) {
      this.user = JSON.parse(userProfileString) as IdTokenPayload;
    }
  }

  // Clear authentication data from AsyncStorage
  private async _clearSession() {
    await AsyncStorage.removeItem(ACCESS_TOKEN_KEY);
    await AsyncStorage.removeItem(ID_TOKEN_KEY);
    await AsyncStorage.removeItem(USER_PROFILE_KEY);
    this.user = null;
    this.accessToken = null;
    this.idToken = null;
  }

  // Get the redirect URL for the platform
  private getRedirectUri(): string {
    const redirectUri = AuthSession.makeRedirectUri({
      scheme: 'familytreeapp', // Replace with your scheme
      path: 'auth',
    });
    console.log('Generated redirectUri:', redirectUri);
    return redirectUri;
  }

  public async login(): Promise<boolean> {
    console.log('Attempting Auth0 login...');
    console.log('AUTH0_DOMAIN:', AUTH0_DOMAIN);
    console.log('AUTH0_CLIENT_ID:', AUTH0_CLIENT_ID);
    const redirectUri = this.getRedirectUri();

    // Generate nonce
    this.nonce = nanoid();

    const authUrl = `https://${AUTH0_DOMAIN}/authorize?` +
      `scope=openid profile email&` +
      `audience=${AUTH0_AUDIENCE}&` +
      `response_type=token id_token&` +
      `client_id=${AUTH0_CLIENT_ID}&` +
      `redirect_uri=${redirectUri}&` +
      `nonce=${this.nonce}`; // Add nonce to authUrl
    console.log('Constructed Auth URL:', authUrl);

    const result = await WebBrowser.openAuthSessionAsync(authUrl, redirectUri);
    console.log('WebBrowser result:', result);

    if (result.type === 'success') {
      const url = new URL(result.url);
      const hashParams = new URLSearchParams(url.hash.substring(1)); // Remove '#' and parse

      const parsedUrl: { [key: string]: string | undefined } = {};
      for (const [key, value] of hashParams.entries()) {
        parsedUrl[key] = value;
      }
      
      if (parsedUrl.error) {
        console.error('AuthSession error:', parsedUrl.error_description || parsedUrl.error);
        return false;
      }

      this.accessToken = parsedUrl.access_token ?? null;
      this.idToken = parsedUrl.id_token ?? null;

      if (this.idToken) {
        const decodedIdToken = jwtDecode<IdTokenPayload>(this.idToken);

        // Verify nonce
        if (!decodedIdToken.nonce || decodedIdToken.nonce !== this.nonce) {
          console.error('Nonce mismatch or missing nonce in ID token. Potential replay attack.');
          this.nonce = null; // Clear nonce after verification attempt
          return false;
        }

        this.user = decodedIdToken;
        console.log('Logged in user:', this.user);
        this.nonce = null; // Clear nonce after successful verification
        await this._saveSession(); // Save session
        return true;
      }
    } else if (result.type === 'cancel') {
      console.log('Authentication cancelled by user.');
    } else if (result.type === 'dismiss') {
      console.log('Authentication dismissed by user.');
    }
    this.nonce = null; // Clear nonce in case of failure or dismissal
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
    await this._clearSession(); // Clear session
  }

  public isAuthenticated(): boolean {
    return !!this.user;
  }

  public getUser(): IdTokenPayload | null {
    return this.user;
  }

  public getAccessToken(): string | null {
    return this.accessToken;
  }
}

export const authService = new AuthService();
