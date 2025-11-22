import { Image } from 'expo-image';
import { StyleSheet, View } from 'react-native';
import { Button, Text, useTheme } from 'react-native-paper';
import { LinearGradient } from 'expo-linear-gradient';
import { TFunction } from 'i18next';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions'; // Import spacing constants
import { ExpoRouter } from 'expo-router/types/expo-router'; // Import router type

interface BannerProps {
  t: TFunction;
  toggleLanguage: () => void;
  i18n: any;
  backgroundColor?: string; // Add backgroundColor prop
  router: ExpoRouter.Router; // Add router prop
}

const getStyles = (theme: any) => StyleSheet.create({
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
    padding: SPACING_LARGE,
  },
  languageButton: {
    marginTop: SPACING_LARGE,
    alignSelf: 'flex-start',
  },
  createTreeText: {
    marginTop: SPACING_MEDIUM,
    color: 'gray', // Example color, adjust as needed
  },
});

export function Banner({ t, toggleLanguage, i18n, backgroundColor, router }: BannerProps) {
  const theme = useTheme();
  const styles = getStyles(theme);

  return (
    <View style={[styles.bannerContainer, backgroundColor ? { backgroundColor } : {}]}>
      <Image
        source={{ uri: 'https://picsum.photos/seed/familytree/700/500' }}
        style={styles.bannerImage}
        contentFit="cover"
      />
      <LinearGradient
        colors={[theme.colors.background, 'rgba(255,255,255,0.1)']} // Use theme background color
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 0 }}
        style={styles.leftBannerOverlay}
      >
        <Text variant="headlineLarge">{t('home.banner.title')}</Text>
        <Text variant="bodyLarge">{t('home.banner.description')}</Text>
        <Button mode="contained" onPress={() => router.push('/feature-under-development')} style={styles.languageButton}>
          {t('home.banner.cta_button')}
        </Button>
      </LinearGradient>
    </View>
  );
}
