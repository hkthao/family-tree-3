import { Stack, router } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { Appbar, useTheme } from 'react-native-paper';
import { View } from 'react-native';

export default function MoreLayout() {
  const { t } = useTranslation();
  const theme = useTheme();

  return (
    <View style={{ flex: 1 }}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('more.title')} />
      </Appbar.Header>
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
        <Stack.Screen name="calendar" options={{ title: t('more.calendar') }} />
        <Stack.Screen name="face-data" options={{ title: t('more.faceData') }} />
        <Stack.Screen name="memories" options={{ title: t('more.memories') }} />
        <Stack.Screen name="timeline" options={{ title: t('more.timeline') }} />
        <Stack.Screen name="privacy" options={{ title: t('more.privacy') }} />
      </Stack>
    </View>
  );
}