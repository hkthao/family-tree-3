import React, { useEffect, useState, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { Text, Appbar, useTheme, Avatar, ActivityIndicator, Card } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { fetchFamilyDetails, FamilyDetail } from '../../data/mockFamilyData';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';
import { useFamilyStore } from '../../stores/useFamilyStore'; // Import useFamilyStore

export default function FamilyDetailsScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const { id: routeId } = useLocalSearchParams(); // Keep for debugging if needed, but not used for fetching
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store
  const [family, setFamily] = useState<FamilyDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadFamilyDetails = async () => {
      if (!currentFamilyId) {
        setError('No Family ID available');
        setLoading(false);
        return;
      }
      try {
        const data = await fetchFamilyDetails(currentFamilyId);
        if (data) {
          setFamily(data);
        } else {
          setError('Family not found');
        }
      } catch (err) {
        setError('Failed to load family details');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadFamilyDetails();
  }, [currentFamilyId]); // Depend on currentFamilyId from Zustand

  const router = useRouter();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    appbar: {
      backgroundColor: theme.colors.surface,
    },
    content: {
      padding: SPACING_MEDIUM,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    errorContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_MEDIUM,
    },
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    cardContent: {
      flexDirection: 'row',
      alignItems: 'center',
    },
    avatar: {
      marginRight: SPACING_MEDIUM,
    },
    detailsContainer: {
      flex: 1,
    },
    detailRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginBottom: SPACING_MEDIUM,
    },
    detailItem: {
      flex: 1,
    },
    detailLabel: {
      fontWeight: 'bold',
      color: theme.colors.onSurfaceVariant,
    },
    detailValue: {
      color: theme.colors.onSurface,
    },
    description: {
      marginTop: SPACING_MEDIUM,
      marginBottom: SPACING_MEDIUM,
    },
  }), [theme]);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator animating size="large" color={theme.colors.primary} />
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium" style={{ color: theme.colors.error }}>{error}</Text>
      </View>
    );
  }

  if (!family) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium">{t('common.error_occurred')}: Family data not available.</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Appbar.Header style={styles.appbar}>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('familyDetail.title')} />
      </Appbar.Header>
      <ScrollView contentContainerStyle={styles.content}>
        <Card style={styles.card}>
          <Card.Content style={styles.cardContent}>
            <Avatar.Image size={80} source={{ uri: family.avatarUrl || 'https://via.placeholder.com/150' }} style={styles.avatar} />
            <View style={styles.detailsContainer}>
              <Text variant="headlineSmall">{family.name}</Text>
              <Text variant="titleMedium" style={{ color: theme.colors.onSurfaceVariant }}>{family.code}</Text>
            </View>
          </Card.Content>
        </Card>

        <Card style={styles.card}>
          <Card.Content>
            <Text variant="titleMedium" style={{ marginBottom: SPACING_MEDIUM }}>{t('familyDetail.details.overview')}</Text>
            <Text variant="bodyMedium" style={styles.description}>{family.description}</Text>

            <View style={styles.detailRow}>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('family.members')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{family.totalMembers}</Text>
              </View>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('family.generations')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{family.totalGenerations}</Text>
              </View>
            </View>

            <View style={styles.detailRow}>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('family.visibility')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{t(`family.visibility.${family.visibility.toLowerCase()}`)}</Text>
              </View>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('familyDetail.details.createdBy')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{family.createdBy}</Text>
              </View>
            </View>

            <View style={styles.detailRow}>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('familyDetail.details.createdAt')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{new Date(family.createdAt).toLocaleDateString()}</Text>
              </View>
              <View style={styles.detailItem}>
                <Text variant="bodySmall" style={styles.detailLabel}>{t('familyDetail.details.lastUpdatedAt')}</Text>
                <Text variant="bodyMedium" style={styles.detailValue}>{new Date(family.lastUpdatedAt).toLocaleDateString()}</Text>
              </View>
            </View>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
}
