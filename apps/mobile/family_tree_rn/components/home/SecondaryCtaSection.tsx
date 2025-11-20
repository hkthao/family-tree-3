import React, { useMemo } from 'react';
import { StyleSheet, View } from 'react-native';
import { Button, Text, useTheme } from 'react-native-paper';
import { TFunction } from 'i18next';
import { SPACING_LARGE } from '@/constants/dimensions';

interface SecondaryCtaSectionProps {
  t: TFunction;
  backgroundColor?: string; // Add backgroundColor prop
}

export function SecondaryCtaSection({ t, backgroundColor }: SecondaryCtaSectionProps) {
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      padding: SPACING_LARGE,
      backgroundColor: theme.colors.onTertiary, // A distinct background color
      alignItems: 'center',
      justifyContent: 'center',
    },
    questionText: {
      textAlign: 'center',
      marginBottom: SPACING_LARGE,
      color: theme.colors.onPrimaryContainer,
    },
    ctaButton: {
      width: '80%', // Make the button wider
    },
  }), [theme]);

  return (
    <View style={[styles.container, backgroundColor ? { backgroundColor } : {}]}>
      <Text variant="titleLarge" style={styles.questionText}>
        {t('home.secondary_cta.question')}
      </Text>
      <Button mode="contained" onPress={() => { /* TODO: Navigate to create family tree screen */ }} style={styles.ctaButton}>
        {t('home.secondary_cta.button')}
      </Button>
    </View>
  );
}
