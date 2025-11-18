import { Image } from 'expo-image';
import { Platform, StyleSheet } from 'react-native';
import { Button } from 'react-native-paper';

import { HelloWave } from '@/components/hello-wave';
import ParallaxScrollView from '@/components/parallax-scroll-view';
import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { Link } from 'expo-router';

import { useTranslation } from 'react-i18next'; // Import useTranslation
import '../../src/i18n'; // Import i18n configuration
import UserAppBar from '@/components/layout/UserAppBar'; // Import UserAppBar

export default function HomeScreen() {
  const { t, i18n } = useTranslation(); // Use the useTranslation hook

  const toggleLanguage = () => {
    i18n.changeLanguage(i18n.language === 'en' ? 'vi' : 'en');
  };

  return (
    <ThemedView style={styles.fullScreenContainer}> {/* Added a container for UserAppBar and ParallaxScrollView */}
      <UserAppBar /> {/* Add UserAppBar here */}
      <ParallaxScrollView
        headerBackgroundColor={{ light: '#A1CEDC', dark: '#1D3D47' }}
        headerImage={
          <Image
            source={require('@/assets/images/partial-react-logo.png')}
            style={styles.reactLogo}
          />
        }>
        <ThemedView style={styles.titleContainer}>
          <ThemedText type="title">{t('welcome')}</ThemedText> {/* Use translated text */}
          <HelloWave />
        </ThemedView>
        <ThemedView style={styles.stepContainer}>
          <ThemedText type="subtitle">{t('hello')}</ThemedText> {/* Use translated text */}
          <ThemedText>
            {t('description')} {/* Use translated text */}
          </ThemedText>
        </ThemedView>
        <ThemedView style={styles.stepContainer}>
          <Button mode="contained" onPress={toggleLanguage}>
            {t('change_language')}
          </Button>
        </ThemedView>
      </ParallaxScrollView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  fullScreenContainer: { // New style for the full screen container
    flex: 1,
  },
  titleContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  stepContainer: {
    gap: 8,
    marginBottom: 8,
  },
  reactLogo: {
    height: 178,
    width: 290,
    bottom: 0,
    left: 0,
    position: 'absolute',
  },
});
