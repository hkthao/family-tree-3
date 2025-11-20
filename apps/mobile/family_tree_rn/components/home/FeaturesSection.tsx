import React, { useMemo } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Text, Icon, useTheme } from 'react-native-paper';
import { TFunction } from 'i18next';
import { SPACING_SMALL, SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions'; // Import spacing constants

interface FeaturesSectionProps {
  t: TFunction;
  backgroundColor?: string; // Add backgroundColor prop
}

interface Feature {
  icon: string;
  titleKey: string;
  descriptionKey: string;
}

const features: Feature[] = [
  {
    icon: 'family-tree',
    titleKey: 'home.features.visual_tree.title',
    descriptionKey: 'home.features.visual_tree.description',
  },
  {
    icon: 'image-multiple', // Changed icon
    titleKey: 'home.features.stories_memories.title',
    descriptionKey: 'home.features.stories_memories.description',
  },
  {
    icon: 'account-group', // Changed icon
    titleKey: 'home.features.discover_connections.title',
    descriptionKey: 'home.features.discover_connections.description',
  },
  {
    icon: 'card-account-details', // Changed icon
    titleKey: 'home.features.member_profiles.title',
    descriptionKey: 'home.features.member_profiles.description',
  },
  {
    icon: 'magnify',
    titleKey: 'home.features.smart_search.title',
    descriptionKey: 'home.features.smart_search.description',
  },
  {
    icon: 'shield-lock', // Changed icon
    titleKey: 'home.features.privacy_control.title',
    descriptionKey: 'home.features.privacy_control.description',
  },
];

export function FeaturesSection({ t, backgroundColor }: FeaturesSectionProps) {
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      paddingHorizontal: SPACING_SMALL,
      paddingVertical: SPACING_MEDIUM,
    },
    sectionTitle: {
      textAlign: 'center',
      marginVertical: SPACING_LARGE,
      fontWeight: 'bold',
    },
    featuresGrid: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_MEDIUM, // Use gap for consistent spacing
      justifyContent: 'center', // Center the items
    },
    featureItem: {
      width: '48%', // Roughly two items per row
      alignItems: 'center',
      padding: SPACING_MEDIUM,
      backgroundColor: theme.colors.surface, // Use theme surface color
      borderRadius: theme.roundness,
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.2,
      shadowRadius: 1.41,
      elevation: 2,
      height: 180,
      justifyContent: 'space-between', // Align icon top, text bottom
    },
    featureTextContainer: {
      alignItems: 'center', // Center text horizontally
    },
    featureTitle: {
      marginTop: SPACING_MEDIUM,
      textAlign: 'center',
      fontWeight: 'bold',
    },
    featureDescription: {
      textAlign: 'center',
      marginTop: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant, // Use theme color for secondary text
    },
  }), [theme]);

  return (
    <ScrollView style={[styles.container, backgroundColor ? { backgroundColor } : {}]} showsVerticalScrollIndicator={false}>
      <Text variant="headlineMedium" style={styles.sectionTitle}>
        {t('home.features.title')}
      </Text>
      <View style={styles.featuresGrid}>
        {features.map((feature, index) => (
          <View key={index} style={styles.featureItem}>
            <Icon source={feature.icon} size={40} />
            <View style={styles.featureTextContainer}>
              <Text variant="titleMedium" style={styles.featureTitle} numberOfLines={2} ellipsizeMode="tail">
                {t(feature.titleKey)}
              </Text>
              <Text variant="bodyMedium" style={styles.featureDescription} numberOfLines={2} ellipsizeMode="tail">
                {t(feature.descriptionKey)}
              </Text>
            </View>
          </View>
        ))}
      </View>
    </ScrollView>
  );
}
