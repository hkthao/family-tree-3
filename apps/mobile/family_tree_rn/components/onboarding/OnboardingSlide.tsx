import React from 'react';
import { View, Text, StyleSheet, Image, Dimensions } from 'react-native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_LARGE, SPACING_MEDIUM } from '@/constants/dimensions';

const { width, height } = Dimensions.get('window');

interface OnboardingSlideProps {
  item: {
    id: string;
    image: any; // Use 'any' for require() images
    title: string;
    description: string;
  };
}

export default function OnboardingSlide({ item }: OnboardingSlideProps) {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = StyleSheet.create({
    slide: {
      width,
      justifyContent: 'center',
      alignItems: 'center',
      paddingHorizontal: SPACING_LARGE,
    },
    image: {
      width: width * 0.8,
      height: height * 0.6,
      resizeMode: 'contain',
      marginBottom: SPACING_LARGE,
      borderRadius: theme.roundness
    },
    title: {
      fontSize: 28,
      fontWeight: 'bold',
      marginBottom: SPACING_MEDIUM,
      color: theme.colors.onBackground,
      textAlign: 'center',
    },
    description: {
      fontSize: 16,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
      paddingHorizontal: SPACING_MEDIUM,
    },
  });

  return (
    <View style={styles.slide}>
      <Image source={item.image} style={styles.image} />
      <Text style={styles.title}>{t(item.title)}</Text>
      <Text style={styles.description}>{t(item.description)}</Text>
    </View>
  );
}
