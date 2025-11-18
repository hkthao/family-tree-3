import { ScrollView, StyleSheet, View, Linking } from 'react-native';
import { Text, Card, Button } from 'react-native-paper'; // Import Button
import { useTranslation } from 'react-i18next';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Image } from 'expo-image';
import { PaperTheme } from '@/constants/theme';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';
import UserAppBar from '@/components/layout/UserAppBar'; // Import UserAppBar
import * as Clipboard from 'expo-clipboard'; // Import Clipboard from expo-clipboard

export default function DonateScreen() {
  const { t } = useTranslation();

  const handleLinkPress = (url: string) => {
    Linking.openURL(url).catch((err) => console.error("Couldn't load page", err));
  };

  const handleCopyLink = (text: string) => {
    Clipboard.setString(text);
    // Optionally, show a toast message or similar to indicate success
    console.log('Link copied to clipboard:', text);
  };

  const handleCopyMomoNumber = (number: string) => {
    Clipboard.setString(number);
    // Optionally, show a toast message or similar to indicate success
    console.log('MoMo number copied to clipboard:', number);
  };

  return (
    <View style={styles.fullScreenContainer}>
      <UserAppBar />
      <ScrollView showsVerticalScrollIndicator={false} >
        <View style={styles.container}>
          <Text variant="headlineMedium" style={styles.title}>
            {t('donate.mainTitle')} 
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
                  source={require('@/assets/images/bmc_qr.png')} 
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
                <Button
                  mode="contained"
                  onPress={() => handleCopyLink('https://www.buymeacoffee.com/thaohk90e')}
                  style={styles.copyButton}
                  labelStyle={styles.copyButtonLabel}
                >
                  {t('donate.copyLink')}
                </Button>
              </Card.Content>
            </Card>

            <Card style={styles.donationCard}>
              <Card.Content>
                <Text variant="titleMedium" style={styles.donationMethodTitle}>
                  {t('donate.momo')}
                </Text>
                <Image
                  source={require('@/assets/images/momo.jpg')} // Adjust path as needed
                  alt="MoMo QR Code"
                  style={styles.qrCodeImage}
                  contentFit="contain"
                />
                <Text variant="bodyLarge" style={styles.momoNumber}>
                  0946351139
                </Text>
                <Button
                  mode="contained"
                  onPress={() => handleCopyMomoNumber('0946351139')}
                  style={styles.copyButton}
                  labelStyle={styles.copyButtonLabel}
                >
                  {t('donate.copyPhoneNumber')}
                </Button>
              </Card.Content>
            </Card>
          </View>

          <Text variant="bodyLarge" style={styles.thankYouMessage}>
            {t('about.donate.thankYou')}
          </Text>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  fullScreenContainer: {
    flex: 1,
  },
  safeArea: {
    flex: 1,
  },
  container: {
    flex: 1,
    padding: SPACING_MEDIUM,
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
    textAlign: 'left',
  },
  donationMethodsContainer: {
    flexDirection: 'column',
    flexWrap: 'wrap',
    justifyContent: 'space-around',
    marginTop: SPACING_LARGE,
    marginBottom: SPACING_LARGE,
    gap: SPACING_MEDIUM,
  },
  donationCard: {
    flex: 1,
    width: '100%', // Roughly two cards per row
    elevation: 1,
    borderRadius: 8,
    backgroundColor: '#FFFFFF', // Explicitly set background to white
    borderWidth: 1,
    borderColor: '#F7F7F7', // Subtle border color
  },
  donationMethodTitle: {
    textAlign: 'center',
    marginBottom: SPACING_MEDIUM,
    fontWeight: 'bold',
  },
  qrCodeImage: {
    width: 240,
    height: 240,
    alignSelf: 'center',
    objectFit: "cover",
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
  },
  copyButton: {
    marginTop: SPACING_MEDIUM,
    width: '80%',
    alignSelf: 'center',
  },
  copyButtonLabel: {
    color: '#FFFFFF', // White text for the button label
  },
  thankYouMessage: {
    textAlign: 'left',
    marginBottom: SPACING_MEDIUM,
  },
});