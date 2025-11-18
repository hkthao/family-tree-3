import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { TFunction } from 'i18next';
import { PaperTheme } from '@/constants/theme';
import { SPACING_LARGE } from '@/constants/dimensions';

interface SecondaryCtaSectionProps {
  t: TFunction;
}

export function SecondaryCtaSection({ t }: SecondaryCtaSectionProps) {
  return (
    <View style={styles.container}>
      <Text variant="titleLarge" style={styles.questionText}>
        {t('home.secondary_cta.question')}
      </Text>
      <Button mode="contained" onPress={() => { /* TODO: Navigate to create family tree screen */ }} style={styles.ctaButton}>
        {t('home.secondary_cta.button')}
      </Button>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    padding: SPACING_LARGE,
    backgroundColor: PaperTheme.colors.onTertiary, // A distinct background color
    alignItems: 'center',
    justifyContent: 'center',
  },
  questionText: {
    textAlign: 'center',
    marginBottom: SPACING_LARGE,
    color: PaperTheme.colors.onPrimaryContainer,
  },
  ctaButton: {
    width: '80%', // Make the button wider
  },
});
