import { DarkTheme, DefaultTheme, ThemeProvider as NavigationThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import 'react-native-reanimated';
import { PaperProvider, Portal } from 'react-native-paper';
import { getPaperTheme } from '@/constants/theme';
import { ThemeProvider, useThemeContext } from '@/context/ThemeContext';

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
          </Stack>
        </NavigationThemeProvider>
      </Portal.Host>
    </PaperProvider>
  );
}
