import React, { useEffect, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { Appbar, Text, useTheme, Card, ActivityIndicator, Chip, Avatar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { useEventStore } from '@/stores/useEventStore';

export default function EventDetailsScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();
  const { t } = useTranslation();
  const theme = useTheme();

  const { event, loading, error, getEventById } = useEventStore();

  useEffect(() => {
    if (id) {
      const loadEventDetails = async () => {
        const eventId = Array.isArray(id) ? id[0] : id;
        await getEventById(eventId);
      };
      loadEventDetails();
    }
  }, [id, getEventById]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
    },
    appbar: {
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
    errorText: {
      color: theme.colors.onErrorContainer,
      textAlign: 'center',
    },
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    cardContent: {
      // Add specific styling for event details if needed
    },
    profileCardContent: {
      flexDirection: 'column',
      alignItems: 'center',
      paddingBottom: SPACING_MEDIUM,
    },

    titleText: {
      marginBottom: SPACING_SMALL,
      textAlign: 'center',
    },
    eventTypeText: {
      marginBottom: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    dateText: {
      marginBottom: SPACING_SMALL,
      textAlign: 'center',
    },
    locationText: {
      marginBottom: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    chipsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL,
      justifyContent: 'center', // Center chips
      marginTop: SPACING_SMALL,
      marginBottom: SPACING_SMALL,
    },
    chip: {
      marginHorizontal: SPACING_SMALL / 2, // Add some horizontal margin for spacing
    },
    listItemTitle: {
      fontWeight: 'bold',
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
      <View style={{ flex: 1 }}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => router.back()} />
          <Appbar.Content title={t('eventDetail.title')} />
        </Appbar.Header>
        <View style={styles.errorContainer}>
          <Text variant="bodyMedium" style={styles.errorText}>
            {t('common.error_occurred')}: {error}
          </Text>
        </View>
      </View>
    );
  }

  if (!event) {
    return (
      <View style={{ flex: 1 }}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => router.back()} />
          <Appbar.Content title={t('eventDetail.title')} />
        </Appbar.Header>
        <View style={styles.errorContainer}>
          <Text variant="bodyMedium" style={styles.errorText}>
            {t('eventDetail.errors.dataNotAvailable')}
          </Text>
        </View>
      </View>
    );
  }

  const formattedStartDate = event.startDate ? new Date(event.startDate).toLocaleDateString() : t('common.not_available');
  const formattedEndDate = event.endDate ? new Date(event.endDate).toLocaleDateString() : t('common.not_available');

  return (
    <View style={styles.container}>
      <Appbar.Header style={styles.appbar}>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={event.title || t('eventDetail.title')} />
      </Appbar.Header>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* First Card: Key Event Information */}
        <Card style={styles.card}>
          <Card.Content style={styles.profileCardContent}>
            <Avatar.Icon icon="calendar-month" size={80} color={theme.colors.onPrimary}  />
            <Text variant="headlineSmall" style={styles.titleText}>{event.title || t('common.not_available')}</Text>
            <Text variant="bodyMedium" >{event.description || t('common.not_available')}</Text>
            <View style={styles.chipsContainer}>
              <Chip icon="tag" compact={true} style={styles.chip}>{event.eventType || t('common.not_available')}</Chip>
              <Chip icon="calendar-start" compact={true} style={styles.chip}>{formattedStartDate}</Chip>
              {event.endDate && <Chip icon="calendar-end" compact={true} style={styles.chip}>{formattedEndDate}</Chip>}
              {event.location && (
                <Chip icon="map-marker" compact={true} style={styles.chip}>{event.location}</Chip>
              )}

              {event.relatedMembers?.map((memberId, index) => (
                <Chip key={index} compact={true} icon="account-outline">{memberId}</Chip>
              ))}
            </View>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
}
