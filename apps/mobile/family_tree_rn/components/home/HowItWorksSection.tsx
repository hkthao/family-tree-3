import React, { useMemo } from 'react';
import { StyleSheet, View } from 'react-native';
import { Text, Icon, Chip, useTheme } from 'react-native-paper';
import { TFunction } from 'i18next';
import { SPACING_SMALL, SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions'; // Import spacing constants

interface HowItWorksSectionProps {
  t: TFunction;
  backgroundColor?: string; // Add backgroundColor prop
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

export function HowItWorksSection({ t, backgroundColor }: HowItWorksSectionProps) {
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      padding: SPACING_LARGE,
      backgroundColor: theme.colors.background, // Use theme background color
    },
    sectionTitle: {
      textAlign: 'center',
      marginBottom: SPACING_LARGE,
      fontWeight: 'bold',
    },
    stepsContainer: {
      alignItems: 'flex-start', // Align timeline to the left
    },
    timelineItem: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: SPACING_LARGE,
      position: 'relative',
    },
    timelineDot: {
      width: 12,
      height: 12,
      borderRadius: 6,
      backgroundColor: theme.colors.primary, // Primary color for the dot
      marginRight: SPACING_MEDIUM,
      zIndex: 1, // Ensure dot is above the line
      marginTop:0
    },
    timelineLine: {
      position: 'absolute',
      left: 5, // Align with the center of the dot
      top: 18, // Start below the dot
      bottom: -40, // Extend to the next item
      width: 2,
      backgroundColor: theme.colors.primary, // Primary color for the line
    },
    stepChip: {
      flex: 1, // Allow chip to take available space
      height: 'auto', // Adjust height based on content
      paddingVertical: SPACING_SMALL,
      justifyContent: 'flex-start', // Align content to start
      borderRadius: theme.roundness,
    },
    stepChipText: {
      textAlign: 'left',
      fontWeight: 'bold',
      marginLeft: SPACING_SMALL, // Space between icon and text
    },
  }), [theme]);

  return (
    <View style={[styles.container, backgroundColor ? { backgroundColor } : {}]}>
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
