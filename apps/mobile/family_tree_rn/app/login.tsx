import { StyleSheet, View } from 'react-native';
import { Text, Button } from 'react-native-paper';
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';

export default function LoginScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const { login } = useAuth();

  const handleLogin = async () => {
    await login();
    router.replace('/feature-under-development'); // Redirect to feature under development screen
  };

  return (
    <View style={styles.container}>
      <Text variant="headlineMedium">{t('login.title')}</Text>
      <Text variant="bodyMedium">{t('login.description')}</Text>
      <Button mode="contained" onPress={handleLogin} style={styles.loginButton}>
        {t('login.button')}
      </Button>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  loginButton: {
    marginTop: 20,
  },
});
