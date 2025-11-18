import { ScrollView, StyleSheet, View, Linking } from 'react-native';
import { Text, Card } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Image } from 'expo-image';
import { PaperTheme } from '@/constants/theme';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';

export default function DonateScreen() {
  const { t } = useTranslation();

  const handleLinkPress = (url: string) => {
    Linking.openURL(url).catch((err) => console.error("Couldn't load page", err));
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView style={styles.scrollViewContent}>
        <View style={styles.container}>
          <Card style={styles.mainCard}>
            <Card.Content>
              <Text variant="headlineMedium" style={styles.title}>
                {t('about.donate.title')}
              </Text>
              <Text variant="bodyLarge" style={styles.description}>
                {t('about.donate.description')}
              </Text>
              <Text variant="bodyLarge" style={styles.description}>
                {t('about.donate.reason')}
              </Text>

              <View style={styles.donationMethodsContainer}>
                <Card style={styles.donationCard}>
                  <Card.Content>
                    <Text variant="titleMedium" style={styles.donationMethodTitle}>
                      {t('donate.buyMeACoffee')}
                    </Text>
                    <Image
                      source={require('../../../infra/bmc_qr.png')} // Adjust path as needed
                      alt="Buy Me A Coffee QR Code"
                      style={styles.qrCodeImage}
                      contentFit="contain"
                    />
                    <Text
                      style={styles.link}
                      onPress={() => handleLinkPress('https://www.buymeacoffee.com/thaohk90e')}
                    >
                      https://www.buymeacoffee.com/thaohk90e
                    </Text>
                  </Card.Content>
                </Card>

                <Card style={styles.donationCard}>
                  <Card.Content>
                    <Text variant="titleMedium" style={styles.donationMethodTitle}>
                      {t('donate.momo')}
                    </Text>
                    <Image
                      source={require('../../../infra/momo.jpg')} // Adjust path as needed
                      alt="MoMo QR Code"
                      style={styles.qrCodeImage}
                      contentFit="contain"
                    />
                    <Text variant="bodyLarge" style={styles.momoNumber}>
                      0946351139
                    </Text>
                  </Card.Content>
                </Card>
              </View>

              <Text variant="bodyLarge" style={styles.thankYouMessage}>
                {t('about.donate.thankYou')}
              </Text>
            </Card.Content>
          </Card>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: PaperTheme.colors.background,
  },
  scrollViewContent: {
    flexGrow: 1,
    backgroundColor: PaperTheme.colors.background,
  },
  container: {
    flex: 1,
    padding: SPACING_MEDIUM,
    backgroundColor: PaperTheme.colors.background,
  },
  mainCard: {
    elevation: 2,
    borderRadius: 8,
  },
  title: {
    textAlign: 'center',
    marginBottom: SPACING_MEDIUM,
    fontWeight: 'bold',
    color: PaperTheme.colors.primary,
  },
  description: {
    marginBottom: SPACING_MEDIUM,
    textAlign: 'center',
  },
  donationMethodsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-around',
    marginTop: SPACING_LARGE,
    marginBottom: SPACING_LARGE,
    gap: SPACING_MEDIUM,
  },
  donationCard: {
    flex: 1,
    minWidth: 150, // Ensure cards don't get too small on smaller screens
    maxWidth: '48%', // Roughly two cards per row
    elevation: 1,
    borderRadius: 8,
    alignItems: 'center',
    paddingVertical: SPACING_MEDIUM,
  },
  donationMethodTitle: {
    textAlign: 'center',
    marginBottom: SPACING_MEDIUM,
    fontWeight: 'bold',
  },
  qrCodeImage: {
    width: 150,
    height: 150,
    alignSelf: 'center',
    marginBottom: SPACING_MEDIUM,
    borderRadius: 8,
  },
  link: {
    color: PaperTheme.colors.primary,
    textAlign: 'center',
    textDecorationLine: 'underline',
    marginTop: SPACING_MEDIUM,
  },
  momoNumber: {
    textAlign: 'center',
    fontWeight: 'bold',
    marginTop: SPACING_MEDIUM,
  },
  thankYouMessage: {
    marginTop: SPACING_LARGE,
    textAlign: 'center',
    fontStyle: 'italic',
  },
});