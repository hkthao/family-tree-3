import React from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { Text, Icon } from 'react-native-paper';
import { TFunction } from 'i18next';

interface FeaturesSectionProps {
  t: TFunction;
}

interface Feature {
  icon: string;
  titleKey: string;
  descriptionKey: string;
}

const features: Feature[] = [
  {
    icon: 'family-tree', // Placeholder icon, need to check available icons
    titleKey: 'home.features.visual_tree.title',
    descriptionKey: 'home.features.visual_tree.description',
  },
  {
    icon: 'camera-iris', // Placeholder icon
    titleKey: 'home.features.stories_memories.title',
    descriptionKey: 'home.features.stories_memories.description',
  },
  {
    icon: 'link-variant', // Placeholder icon
    titleKey: 'home.features.discover_connections.title',
    descriptionKey: 'home.features.discover_connections.description',
  },
  {
    icon: 'account-details', // Placeholder icon
    titleKey: 'home.features.member_profiles.title',
    descriptionKey: 'home.features.member_profiles.description',
  },
  {
    icon: 'magnify', // Placeholder icon
    titleKey: 'home.features.smart_search.title',
    descriptionKey: 'home.features.smart_search.description',
  },
  {
    icon: 'lock', // Placeholder icon
    titleKey: 'home.features.privacy_control.title',
    descriptionKey: 'home.features.privacy_control.description',
  },
];

export function FeaturesSection({ t }: FeaturesSectionProps) {
  return (
    <ScrollView style={styles.container} showsVerticalScrollIndicator={false}>
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

const styles = StyleSheet.create({
  container: {
  },
  sectionTitle: {
    textAlign: 'center',
    marginVertical: 20,
    fontWeight: 'bold',
  },
  featuresGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-around',
  },
  featureItem: {
    width: '45%', // Roughly two items per row
    alignItems: 'center',
    marginBottom: 20,
    padding: 10,
    backgroundColor: 'white',
    borderRadius: 8,
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
    marginTop: 10,
    textAlign: 'center',
    fontWeight: 'bold',
  },
  featureDescription: {
    textAlign: 'center',
    marginTop: 5,
    color: 'gray',
  },
});
