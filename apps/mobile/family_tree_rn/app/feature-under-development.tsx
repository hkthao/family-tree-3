import React from 'react';
import { StyleSheet, View, Linking } from 'react-native';
import { Text, Button, Appbar, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { router } from 'expo-router';
import { SPACING_LARGE } from '@/constants/dimensions';

export default function FeatureUnderDevelopmentScreen() {
  const { t } = useTranslation();
  const theme = useTheme();

  const adminWebUrl = process.env.EXPO_PUBLIC_APP_BASE_URL || 'https://familytree.hkthao.com'; // Fallback URL

  const handleOpenAdminWebsite = () => {
    Linking.openURL(adminWebUrl).catch(err => console.error("Couldn't load page", err));
  };

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    content: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      paddingHorizontal: SPACING_LARGE,
    },
    title: {
      marginBottom: SPACING_LARGE,
      textAlign: 'center',
      fontWeight: 'bold',
      color: theme.colors.primary,
    },
    description: {
      marginBottom: SPACING_LARGE * 2,
      textAlign: 'center',
      color: theme.colors.onSurfaceVariant,
    },
    button: {
      marginTop: SPACING_LARGE,
    },
  });

  return (
    <View style={styles.container}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('featureUnderDevelopment.title')} />
      </Appbar.Header>
      <View style={styles.content}>
        <Text variant="headlineMedium" style={styles.title}>
          {t('featureUnderDevelopment.message')}
        </Text>
        <Text variant="bodyLarge" style={styles.description}>
          {t('featureUnderDevelopment.description')}
        </Text>
        <Button mode="contained" onPress={handleOpenAdminWebsite}>
          {t('featureUnderDevelopment.goToAdminWebsite')}
        </Button>
      </View>
    </View>
  );
}
