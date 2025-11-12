import { Auth0Client, createAuth0Client } from '@auth0/auth0-spa-js';
import type { RedirectLoginOptions, LogoutOptions } from '@auth0/auth0-spa-js';
import type { AuthService } from './authService';
import type { AppState, User } from '@/types';

const AUTH0_DOMAIN = import.meta.env.VITE_AUTH0_DOMAIN;
const AUTH0_CLIENT_ID = import.meta.env.VITE_AUTH0_CLIENT_ID;
const AUTH0_AUDIENCE = import.meta.env.VITE_AUTH0_AUDIENCE;

let auth0: Auth0Client | null = null;

const initAuth0 = async () => {
  if (auth0) return auth0;

  auth0 = await createAuth0Client({
    domain: AUTH0_DOMAIN,
    clientId: AUTH0_CLIENT_ID,
    authorizationParams: {
      redirect_uri: window.location.origin,
      audience: AUTH0_AUDIENCE
    }
  });

  return auth0;
};

export const auth0Service: AuthService = {
  isAuthenticated: async () => {
    const client = await initAuth0();
    return client.isAuthenticated();
  },
  getUser: async () => {
    const client = await initAuth0();
    const auth0User = await client.getUser();
    if (auth0User) {
      return {
        id: auth0User.sub || '',
        externalId: auth0User.sub || '',
        name: auth0User.name || auth0User.nickname || '',
        email: auth0User.email || '',
        avatar: auth0User.picture || undefined,
        // Map roles or other custom claims if available
        roles: (auth0User['https://familytree.com/roles'] as string[]) || [],
      };
    }
    return null;
  },
  register: async (): Promise<User | null> => {
    const client = await initAuth0();
    await client.loginWithRedirect({
      appState: { target: '/' }, // Redirect to home after signup
      authorizationParams: {
        ui_locales: 'vi',
      },
      // You might pass other parameters from 'data' if Auth0 supports them
    });
    return null; // User data will be available after redirect
  },
  login: async (options?: RedirectLoginOptions) => {
    const client = await initAuth0();
    await client.loginWithRedirect({
      ...options,
      authorizationParams: {
        ui_locales: 'vi',
        ...options?.authorizationParams,
      },
    });
  },
  logout: async (options?: LogoutOptions) => {
    const client = await initAuth0();
    client.logout({
      ...options,
      logoutParams: {
        returnTo: window.location.origin + '/logout',
        ...options?.logoutParams,
      },
    });
  },
  getAccessToken: async () => {
    const client = await initAuth0();
    const token = await client.getTokenSilently();
    return token;
  },
  handleRedirectCallback: async (): Promise<AppState> => {
    const client = await initAuth0();
    const appState = await client.handleRedirectCallback();
    return appState as AppState;
  },
}