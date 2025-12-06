import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { View, StyleSheet, ActivityIndicator, Text, RefreshControl, Alert } from 'react-native';
import Timeline from 'react-native-timeline-flatlist';
import { useTheme, Searchbar } from 'react-native-paper';
import { useLocalSearchParams } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useIsFocused } from '@react-navigation/native';
import { format } from 'date-fns';

import { SPACING_MEDIUM } from '@/constants/dimensions';
import { usePublicEventStore } from '@/stores/usePublicEventStore';
import { useFamilyStore } from '@/stores/useFamilyStore'; // Import useFamilyStore
import type { EventDto, SearchPublicEventsQuery } from '@/types';


interface TimelineData {
  time: string;
  title: string;
  description: string;
  lineColor?: string;
  circleColor?: string;
}

const TimelineScreen: React.FC = () => {
  const { t } = useTranslation();
  const theme = useTheme();
  const isFocused = useIsFocused();
  // const { familyId } = useLocalSearchParams<{ familyId: string }>(); // Remove this line
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const {
    events,
    loading,
    error,
    hasMore,
    fetchEvents,
    reset,
  } = usePublicEventStore();

  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState('');
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, 400);

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery]);

  const mapEventToTimelineData = useCallback((event: EventDto): TimelineData => {
    return {
      time: event.startDate ? format(new Date(event.startDate), 'HH:mm') : '',
      title: event.name || t('common.noTitle'),
      description: event.description || t('common.noDescription'),
      // You can add logic here to set lineColor or circleColor based on event properties
    };
  }, [t]);

  const timelineData = useMemo(() => events.map(mapEventToTimelineData), [events, mapEventToTimelineData]);

  const loadEvents = useCallback(async (isLoadMore: boolean) => {
    if (!currentFamilyId) { // Use currentFamilyId
      Alert.alert(t('common.error'), t('timeline.familyIdNotFound'));
      return;
    }

    if (loading) return;

    // Prevent loading more if there are no more pages
    if (isLoadMore && !hasMore) return;

    try {
      if (!isLoadMore) {
        setIsRefreshing(true);
      }
      const query: SearchPublicEventsQuery = {
        searchTerm: debouncedSearchQuery,
        // Add other query parameters if needed
      };
      await fetchEvents(currentFamilyId, query, isLoadMore); // Use currentFamilyId
    } catch (err: any) {
      Alert.alert(t('common.error'), err.message || t('timeline.failedToLoadEvents'));
    } finally {
      setIsRefreshing(false);
    }
  }, [currentFamilyId, loading, hasMore, debouncedSearchQuery, fetchEvents, t]); // Update dependencies

  useEffect(() => {
    if (isFocused) {
      // Fetch initial events when component mounts or screen is focused
      loadEvents(false);
    }

    return () => {
      if (!isFocused) {
        reset(); // Reset store when screen loses focus
      }
    };
  }, [isFocused, debouncedSearchQuery, loadEvents, reset, currentFamilyId]); // Update dependencies

  const onRefresh = useCallback(() => {
    if (!loading) {
      loadEvents(false); // Fetch first page again
    }
  }, [loading, loadEvents]);

  const onEndReached = useCallback(() => {
    if (!loading && hasMore) {
      loadEvents(true);
    }
  }, [loading, hasMore, loadEvents]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
      backgroundColor: theme.colors.background,
    },
    searchFilterContainer: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: theme.colors.surfaceVariant,
      borderRadius: theme.roundness,
    },
    searchbar: {
      flex: 1,
      borderRadius: theme.roundness,
      backgroundColor: 'transparent',
    },
    list: {
      flex: 1,
      marginTop: SPACING_MEDIUM,
    },
    footer: {
      marginTop: SPACING_MEDIUM,
      marginBottom: SPACING_MEDIUM,
    },
    footerText: {
      textAlign: 'center',
      color: theme.colors.onSurfaceVariant,
    },
    errorText: {
      color: theme.colors.error,
      textAlign: 'center',
      marginTop: SPACING_MEDIUM,
    },
    emptyListText: {
      color: theme.colors.onBackground,
      textAlign: 'center',
      marginTop: SPACING_MEDIUM,
    }
  }), [theme]);

  const renderFooter = () => {
    if (loading && events.length > 0) { // Only show activity indicator if loading more, not initial load
      return (
        <View style={styles.footer}>
          <ActivityIndicator />
        </View>
      );
    }
    if (!hasMore && events.length > 0) {
      return <Text style={styles.footerText}>{t('timeline.noMoreEvents')}</Text>;
    }
    return <View style={styles.footer} />;
  };

  if (error) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorText}>{error}</Text>
      </View>
    );
  }

  if (loading && events.length === 0) { // Show loading indicator for initial load
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" style={{ flex: 1, justifyContent: 'center' }} />
      </View>
    );
  }

  if (timelineData.length === 0 && !loading) {
    return (
      <View style={styles.container}>
        <Searchbar
          placeholder={t('timeline.searchPlaceholder')}
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
          clearIcon={searchQuery.length > 0 ? 'close-circle' : undefined}
          onClearIconPress={() => setSearchQuery('')}
        />
        <Text style={styles.emptyListText}>{t('timeline.noEventsFound')}</Text>
        <RefreshControl refreshing={isRefreshing} onRefresh={onRefresh} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.searchFilterContainer}>
        <Searchbar
          placeholder={t('timeline.searchPlaceholder')}
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
          clearIcon={searchQuery.length > 0 ? 'close-circle' : undefined}
          onClearIconPress={() => setSearchQuery('')}
        />
      </View>
      <Timeline
        style={styles.list}
        data={timelineData}
        circleSize={20}
        circleColor={theme.colors.primary}
        lineColor={theme.colors.primary}
        timeContainerStyle={{ minWidth: 52, marginTop: -5 }}
        timeStyle={{ textAlign: 'center', backgroundColor: theme.colors.error, color: theme.colors.onError, padding: 5, borderRadius: theme.roundness }}
        titleStyle={{ color: theme.colors.onSurface }}
        descriptionStyle={{ color: theme.colors.onSurfaceVariant }}
        options={
          {
            data: timelineData,
            style: { paddingTop: 5 },
            refreshControl: (
              <RefreshControl
                refreshing={isRefreshing}
                onRefresh={onRefresh}
                tintColor={theme.colors.primary}
              />
            ),
            onEndReached: onEndReached,
            renderFooter: renderFooter,
            onEndReachedThreshold: 0.1, // Load more when 10% from the end
          } as any
        }
        innerCircle={'dot'}
      />
    </View>
  );
};

export default TimelineScreen;
