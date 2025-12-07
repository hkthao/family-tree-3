import { ScrollView, StyleSheet, View } from 'react-native'; // Import View
import { useTranslation } from 'react-i18next'; // Import useTranslation
import { router } from 'expo-router'; // Import router
import '@/118n'; // Import i18n configuration
import { UserAppBar } from '@/components/layout'; // Import UserAppBar
import { Banner, FeaturesSection, HowItWorksSection, MediaPreviewSection, SecondaryCtaSection, PrivacySecuritySection } from '@/components/home'; // Import Home components
import { useTheme } from 'react-native-paper'; // Import useTheme

export default function HomeScreen() {
  const { t, i18n } = useTranslation(); // Use the useTranslation hook
  const theme = useTheme();

  const toggleLanguage = () => {
    i18n.changeLanguage(i18n.language === 'en' ? 'vi' : 'en');
  };

  const backgroundColor1 = theme.colors.background;
  const backgroundColor2 = theme.colors.surfaceVariant;

  return (
    <View style={styles.fullScreenContainer}>
      <UserAppBar />
      <ScrollView showsVerticalScrollIndicator={false}>
        <Banner t={t} toggleLanguage={toggleLanguage} i18n={i18n} backgroundColor={backgroundColor1} router={router} />
        <FeaturesSection t={t} backgroundColor={backgroundColor2} />
        <HowItWorksSection t={t} backgroundColor={backgroundColor1} />
        <MediaPreviewSection t={t} backgroundColor={backgroundColor2} />
        <SecondaryCtaSection t={t} backgroundColor={backgroundColor1} />
        <PrivacySecuritySection t={t} backgroundColor={backgroundColor2} />
        {/* You can add more content below the banner here */}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  fullScreenContainer: {
    flex: 1,
  },
});