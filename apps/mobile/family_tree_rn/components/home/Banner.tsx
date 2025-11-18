import { Image } from 'expo-image';
import { StyleSheet, View } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { LinearGradient } from 'expo-linear-gradient';
import { TFunction } from 'i18next';
import { i18n } from 'i18next';

interface BannerProps {
  t: TFunction;
  toggleLanguage: () => void;
  i18n: any;
}

export function Banner({ t, toggleLanguage, i18n }: BannerProps) {
  return (
    <View style={styles.bannerContainer}>
      <Image
        source={{ uri: 'https://picsum.photos/seed/family/700/500' }}
        style={styles.bannerImage}
        contentFit="cover"
      />
      <LinearGradient
        colors={['rgba(255,255,255,1)', 'rgba(255,255,255,0.1)']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 0 }}
        style={styles.leftBannerOverlay}
      >
        <Text variant="headlineLarge">{t('home.banner.title')}</Text>
        <Text variant="bodyLarge">{t('home.banner.description')}</Text>
        <Button mode="contained" onPress={() => { /* TODO: Navigate to create family tree screen */ }} style={styles.languageButton}>
          {t('home.banner.cta_button')}
        </Button>
      </LinearGradient>
    </View>
  );
}

const styles = StyleSheet.create({
  bannerContainer: {
    height: 250,
    position: 'relative',
  },
  bannerImage: {
    width: '100%',
    height: '100%',
  },
  leftBannerOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    bottom: 0,
    width: '100%',
    justifyContent: 'center',
    padding: 20,
  },
  languageButton: {
    marginTop: 20,
    alignSelf: 'flex-start',
  },
  createTreeText: {
    marginTop: 10,
    color: 'gray', // Example color, adjust as needed
  },
});
