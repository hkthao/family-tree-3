import { Stack } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useTheme } from 'react-native-paper';

export default function MoreLayout() {
  const { t } = useTranslation();
  const theme = useTheme();

  return (
    <Stack
      screenOptions={{
        headerStyle: {
          backgroundColor: theme.colors.surface,
        },
        headerTintColor: theme.colors.onSurface,
        headerTitleStyle: {
          fontWeight: 'bold',
        },
        headerShown: false,
        contentStyle: {
          backgroundColor: theme.colors.background,
        },
      }}
    >
      <Stack.Screen name="index" options={{ title: t('more.title') }} />
      <Stack.Screen name="events" options={{ title: t('more.events') }} />
      <Stack.Screen name="face-data" options={{ title: t('more.faceData') }} />
      <Stack.Screen name="memories" options={{ title: t('more.memories') }} />
      <Stack.Screen name="timeline" options={{ title: t('more.timeline') }} />
      <Stack.Screen name="privacy" options={{ title: t('more.privacy') }} />
    </Stack>
  );
}