import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM } from '@/constants/dimensions';

export default function MorePrivacyScreen() {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      backgroundColor: theme.colors.background,
      padding: SPACING_MEDIUM,
    },
    text: {
      color: theme.colors.onBackground,
      textAlign: 'center',
    },
  });

  return (
    <View style={styles.container}>
      {/* @ts-ignore */}
      <Text style={styles.text} variant="titleLarge">{t('more.privacyPlaceholder')}</Text>
    </View>
  );
}