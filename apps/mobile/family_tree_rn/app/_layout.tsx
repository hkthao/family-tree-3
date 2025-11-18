import { DarkTheme, DefaultTheme, ThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import 'react-native-reanimated';
import { PaperProvider } from 'react-native-paper'; // Import PaperProvider
import { PaperTheme } from '../constants/theme'; // Import custom theme

import { useColorScheme } from '@/hooks/use-color-scheme';

export const unstable_settings = {
  anchor: '(tabs)',
};

export default function RootLayout() {
  const colorScheme = useColorScheme();

  return (
    <PaperProvider theme={PaperTheme}> {/* Wrap with PaperProvider and apply custom theme */}
      <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
        <Stack>
          <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
          <Stack.Screen name="modal" options={{ presentation: 'modal', title: 'Modal' }} />
        </Stack>
        {/* <StatusBar style="auto" /> Removed as PaperProvider handles status bar */}
      </ThemeProvider>
    </PaperProvider>
  );
}
