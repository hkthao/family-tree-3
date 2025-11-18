import { Image } from 'expo-image';
import { StyleSheet, View } from 'react-native'; // Import View
import { Button, Text } from 'react-native-paper';
import { LinearGradient } from 'expo-linear-gradient'; // Import LinearGradient

import { useTranslation } from 'react-i18next'; // Import useTranslation
import '../../src/i18n'; // Import i18n configuration
import UserAppBar from '@/components/layout/UserAppBar'; // Import UserAppBar

export default function HomeScreen() {
  const { t, i18n } = useTranslation(); // Use the useTranslation hook

  const toggleLanguage = () => {
    i18n.changeLanguage(i18n.language === 'en' ? 'vi' : 'en');
  };

  return (
    <View style={styles.fullScreenContainer}>
      <UserAppBar />
      <View style={styles.bannerContainer}>
        <Image
          source={{ uri: 'https://picsum.photos/seed/family/700/500' }} // Placeholder family image
          style={styles.bannerImage}
          contentFit="cover"
        />
        <LinearGradient
          colors={['rgba(255,255,255,1)', 'rgba(255,255,255,0.1)']} // White to transparent gradient
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 0 }}
          style={styles.leftBannerOverlay}
        >
          <Text variant="headlineLarge">{t('home.banner.title')}</Text>
          <Text variant="bodyLarge">{t('home.banner.description')}</Text>
          <Button mode="contained" onPress={toggleLanguage} style={styles.languageButton}>
            {t('change_language')}
          </Button>
        </LinearGradient>
      </View>
      {/* You can add more content below the banner here */}
    </View>
  );
}

const styles = StyleSheet.create({
  fullScreenContainer: {
    flex: 1,
  },
  bannerContainer: {
    height: 200, // Fixed height for the banner
    position: 'relative', // Needed for absolute positioning of children
  },
  bannerImage: {
    width: '100%', // Image takes full width
    height: '100%', // Image takes full height
  },
  leftBannerOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    bottom: 0,
    width: '100%', // Left section covers 60% of the banner
    justifyContent: 'center',
    padding: 20,
  },
  languageButton: {
    marginTop: 20,
    alignSelf: 'flex-start',
  },
});
