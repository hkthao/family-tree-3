import { StyleSheet, View } from 'react-native';
import { Text, Button, Appbar, useTheme } from 'react-native-paper'; // Import Appbar and useTheme
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';

export default function LoginScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const { login } = useAuth();
  const theme = useTheme(); // Use theme for styling

  const handleLogin = async () => {
    await login();
    router.push('/feature-under-development'); // Redirect to feature under development screen
  };

  return (
    <View style={{ flex: 1, backgroundColor: theme.colors.background }}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('login.title')} />
      </Appbar.Header>
      <View style={styles.content}>
        <Text variant="headlineMedium">{t('login.title')}</Text>
        <Text variant="bodyMedium">{t('login.description')}</Text>
        <Button mode="contained" onPress={handleLogin} style={styles.loginButton}>
          {t('login.button')}
        </Button>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  content: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  loginButton: {
    marginTop: 20,
  },
});

