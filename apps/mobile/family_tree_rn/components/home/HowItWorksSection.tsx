import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Text, Icon, Chip } from 'react-native-paper';
import { TFunction } from 'i18next';

interface HowItWorksSectionProps {
  t: TFunction;
}

interface Step {
  icon: string;
  titleKey: string;
}

const steps: Step[] = [
  {
    icon: 'account-plus', // Placeholder icon
    titleKey: 'home.how_it_works.step1',
  },
  {
    icon: 'family-tree', // Placeholder icon
    titleKey: 'home.how_it_works.step2',
  },
  {
    icon: 'share-variant', // Placeholder icon
    titleKey: 'home.how_it_works.step3',
  },
];

export function HowItWorksSection({ t }: HowItWorksSectionProps) {
  return (
    <View style={styles.container}>
      <Text variant="headlineMedium" style={styles.sectionTitle}>
        {t('home.how_it_works.title')}
      </Text>
      <View style={styles.stepsContainer}>
        {steps.map((step, index) => (
          <View key={index} style={styles.timelineItem}>
            <View style={styles.timelineDot} />
            {index < steps.length - 1 && <View style={styles.timelineLine} />}
            <Chip
              icon={() => <Icon source={step.icon} size={20} />} // Smaller icon size
              style={styles.stepChip}
              textStyle={styles.stepChipText}
            >
              {t(step.titleKey)}
            </Chip>
          </View>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    padding: 20,
    backgroundColor: '#ffffff', // White background for the section
  },
  sectionTitle: {
    textAlign: 'center',
    marginBottom: 20,
    fontWeight: 'bold',
  },
  stepsContainer: {
    alignItems: 'flex-start', // Align timeline to the left
  },
  timelineItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 20,
    position: 'relative',
  },
  timelineDot: {
    width: 12,
    height: 12,
    borderRadius: 6,
    backgroundColor: '#4C662B', // Primary color for the dot
    marginRight: 10,
    zIndex: 1, // Ensure dot is above the line
    marginTop:0
  },
  timelineLine: {
    position: 'absolute',
    left: 5, // Align with the center of the dot
    top: 18, // Start below the dot
    bottom: -40, // Extend to the next item
    width: 2,
    backgroundColor: '#4C662B', // Primary color for the line
  },
  stepChip: {
    flex: 1, // Allow chip to take available space
    height: 'auto', // Adjust height based on content
    paddingVertical: 5,
    justifyContent: 'flex-start', // Align content to start
  },
  stepChipText: {
    textAlign: 'left',
    fontWeight: 'bold',
    marginLeft: 5, // Space between icon and text
  },
});
