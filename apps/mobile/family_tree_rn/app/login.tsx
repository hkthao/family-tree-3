import { StyleSheet, View, ScrollView } from 'react-native';
import { Text, Button, Appbar, useTheme } from 'react-native-paper';
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import { Image } from 'expo-image'; // Re-import Image from expo-image
import { SPACING_LARGE, SPACING_MEDIUM } from '@/constants/dimensions'; // Import spacing constants

export default function LoginScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const { login } = useAuth();
  const theme = useTheme();

  const handleLogin = async () => {
    const success = await login();
    if (success) {
      router.replace('/(tabs)');
    }
  };

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    backgroundImage: {
      position: 'absolute',
      width: '100%',
      height: '100%',
      zIndex: 0, 
      opacity: 0.4
    },
    contentContainer: {
      flexGrow: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_LARGE,
      marginTop: -20,
      zIndex: 2, 
    },
    title: {
      marginBottom: SPACING_MEDIUM,
      textAlign: 'center',
      fontWeight: 'bold',
      color: theme.colors.onSurface,
    },
    description: {
      marginBottom: SPACING_LARGE,
      textAlign: 'center',
      color: theme.colors.onSurfaceVariant,
      paddingHorizontal: SPACING_LARGE,
    },
    loginButton: {
      marginTop: SPACING_LARGE,
      width: '80%',
      borderRadius: theme.roundness
    },
    appBar: {
      backgroundColor: 'transparent'
    }
  });

  return (
    <View style={styles.container}>
      <Image
        source={require('@/assets/images/login_image.png')}
        style={styles.backgroundImage}
        contentFit="cover"
      />
      <Appbar.Header style={styles.appBar}>
        <Appbar.BackAction onPress={() => router.back()} />
      </Appbar.Header>
      <ScrollView contentContainerStyle={styles.contentContainer}>
        <Text variant="headlineLarge" style={styles.title}>{t('login.title')}</Text>
        <Text variant="bodyLarge" style={styles.description}>{t('login.description')}</Text>
        <Button mode="contained" onPress={handleLogin} style={styles.loginButton}>
          {t('login.button')}
        </Button>
      </ScrollView>
    </View>
  );
}
