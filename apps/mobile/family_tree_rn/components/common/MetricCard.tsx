import React from 'react';
import { StyleSheet } from 'react-native';
import { Card, Text, useTheme } from 'react-native-paper';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { SPACING_SMALL, SPACING_MEDIUM } from '@/constants/dimensions';

// Get the type for MaterialCommunityIcons icon names
type IconName = React.ComponentProps<typeof MaterialCommunityIcons>['name'];

interface MetricCardProps {
  icon: IconName; // Use the specific icon name type
  value: number | string;
  label: string;
}

export default function MetricCard({ icon, value, label }: MetricCardProps) {
  const theme = useTheme();

  const styles = StyleSheet.create({
    metricCard: {
      width: '48%', // Approx 2 cards per row with some spacing
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
      elevation: 2,
    },
    metricCardContent: {
      alignItems: 'center',
      paddingVertical: SPACING_MEDIUM,
    },
    metricIcon: {
      marginBottom: SPACING_SMALL,
    },
    metricValue: {
      fontSize: 24,
      fontWeight: 'bold',
      color: theme.colors.primary,
      marginBottom: SPACING_SMALL / 2,
    },
    metricLabel: {
      fontSize: 14,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
  });

  return (
    <Card style={styles.metricCard}>
      <Card.Content style={styles.metricCardContent}>
        <MaterialCommunityIcons name={icon} size={30} color={theme.colors.primary} style={styles.metricIcon} />
        <Text style={styles.metricValue}>{value}</Text>
        <Text style={styles.metricLabel}>{label}</Text>
      </Card.Content>
    </Card>
  );
}
