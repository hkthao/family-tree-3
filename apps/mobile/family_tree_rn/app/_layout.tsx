import { DarkTheme, DefaultTheme, ThemeProvider as NavigationThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import 'react-native-reanimated';
import { PaperProvider, Portal } from 'react-native-paper';
import { getPaperTheme } from '@/constants/theme';
import { ThemeProvider, useThemeContext } from '@/context/ThemeContext';
import * as SplashScreen from 'expo-splash-screen';
import { useEffect, useState } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import OnboardingScreen from './onboarding';

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

export const unstable_settings = {
  anchor: '(tabs)',
};

export default function RootLayout() {
  const [hasOnboarded, setHasOnboarded] = useState<boolean | null>(null);

  useEffect(() => {
    const checkOnboardingStatus = async () => {
      try {
        const onboarded = await AsyncStorage.getItem('hasOnboarded');
        setHasOnboarded(onboarded === 'true');
      } catch (e) {
        setHasOnboarded(false); // Assume not onboarded on error
      } finally {
        // No longer hiding splash screen here, moved to a separate useEffect
      }
    };

    checkOnboardingStatus();
  }, []);

  useEffect(() => {
    if (hasOnboarded !== null) {
      SplashScreen.hideAsync();
    }
  }, [hasOnboarded]);

  if (hasOnboarded === null) {
    return null
  }

  return hasOnboarded ? <ThemeProvider>
    <AppContent />
  </ThemeProvider> : <OnboardingScreen />;
}

function AppContent() {
  const { colorScheme } = useThemeContext();
  const paperTheme = getPaperTheme(colorScheme);
  return (
    <PaperProvider theme={paperTheme}>
      <Portal.Host>
        <NavigationThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
          <Stack screenOptions={{ headerShown: false }}>
            <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
            <Stack.Screen name="family" options={{ headerShown: false }} />
            <Stack.Screen name="member/[id]" options={{ headerShown: false }} />
            <Stack.Screen name="event/[id]" options={{ headerShown: false }} />
            <Stack.Screen name="legal-webview" options={{ headerShown: false }} />
            <Stack.Screen name="feedback-webview" options={{ headerShown: false }} />
          </Stack>
        </NavigationThemeProvider>
      </Portal.Host>
    </PaperProvider>
  );
}
