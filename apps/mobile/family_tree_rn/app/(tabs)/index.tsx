import { ScrollView, StyleSheet, View } from 'react-native'; // Import View
import { useTranslation } from 'react-i18next'; // Import useTranslation
import '../../src/i18n'; // Import i18n configuration
import UserAppBar from '@/components/layout/UserAppBar'; // Import UserAppBar
import { Banner } from '@/components/home/Banner'; // Import Banner component
import { FeaturesSection } from '@/components/home/FeaturesSection'; // Import FeaturesSection component
import { HowItWorksSection } from '@/components/home/HowItWorksSection'; // Import HowItWorksSection component

export default function HomeScreen() {
  const { t, i18n } = useTranslation(); // Use the useTranslation hook

  const toggleLanguage = () => {
    i18n.changeLanguage(i18n.language === 'en' ? 'vi' : 'en');
  };

  return (
    <View style={styles.fullScreenContainer}>
      <UserAppBar />
      <ScrollView showsVerticalScrollIndicator={false}>
        <Banner t={t} toggleLanguage={toggleLanguage} i18n={i18n} />
        <FeaturesSection t={t} />
        <HowItWorksSection t={t} />
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
