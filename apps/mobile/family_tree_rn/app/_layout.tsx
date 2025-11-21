import { DarkTheme, DefaultTheme, ThemeProvider as NavigationThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import 'react-native-reanimated';
import { PaperProvider, Portal } from 'react-native-paper';
import { getPaperTheme } from '@/constants/theme';
import { ThemeProvider, useThemeContext } from '@/context/ThemeContext';
import * as SplashScreen from 'expo-splash-screen';
import { useEffect } from 'react';

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

export const unstable_settings = {
  anchor: '(tabs)',
};

export default function RootLayout() {
  return (
    <ThemeProvider>
      <AppContent />
    </ThemeProvider>
  );
}

function AppContent() {
  const { colorScheme } = useThemeContext();
  const paperTheme = getPaperTheme(colorScheme);

  useEffect(() => {
    setTimeout(() => {
      SplashScreen.hideAsync();
    }, 3000);
  }, []);

  return (
    <PaperProvider theme={paperTheme}>
      <Portal.Host>
        <NavigationThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
          <Stack>
            <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
            <Stack.Screen name="modal" options={{ presentation: 'modal', title: 'Modal' }} />
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
