import React, { useMemo } from 'react';
import { StyleSheet, View } from 'react-native';
import { Text, Icon, useTheme } from 'react-native-paper';
import { TFunction } from 'i18next';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';

interface PrivacySecuritySectionProps {
  t: TFunction;
  backgroundColor?: string; // Add backgroundColor prop
}

interface PrivacyFeature {
  icon: string;
  textKey: string;
}

const privacyFeatures: PrivacyFeature[] = [
  {
    icon: 'shield-lock',
    textKey: 'home.privacy_security.feature1',
  },
  {
    icon: 'account-group',
    textKey: 'home.privacy_security.feature2',
  },
  {
    icon: 'database-off',
    textKey: 'home.privacy_security.feature3',
  },
];

export function PrivacySecuritySection({ t, backgroundColor }: PrivacySecuritySectionProps) {
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      padding: SPACING_LARGE,
      backgroundColor: theme.colors.background,
    },
    sectionTitle: {
      textAlign: 'center',
      marginBottom: SPACING_LARGE,
      fontWeight: 'bold',
    },
    featuresList: {
      marginTop: SPACING_MEDIUM,
    },
    featureItem: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: SPACING_MEDIUM,
    },
    featureText: {
      marginLeft: SPACING_MEDIUM,
      flexShrink: 1,
    },
  }), [theme]);

  return (
    <View style={[styles.container, backgroundColor ? { backgroundColor } : {}]}>
      <Text variant="headlineMedium" style={styles.sectionTitle}>
        {t('home.privacy_security.title')}
      </Text>
      <View style={styles.featuresList}>
        {privacyFeatures.map((feature, index) => (
          <View key={index} style={styles.featureItem}>
            <Icon source={feature.icon} size={24} color={theme.colors.primary} />
            <Text variant="bodyLarge" style={styles.featureText}>
              {t(feature.textKey)}
            </Text>
          </View>
        ))}
      </View>
    </View>
  );
}
