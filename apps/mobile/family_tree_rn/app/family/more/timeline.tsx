import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { View, StyleSheet, ActivityIndicator, Text, RefreshControl, Alert } from 'react-native';
import Timeline from 'react-native-timeline-flatlist';
import { useTheme, Searchbar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useIsFocused } from '@react-navigation/native';
import { format } from 'date-fns';

import { SPACING_MEDIUM } from '@/constants/dimensions';
import { usePublicEventStore } from '@/stores/usePublicEventStore';
import { useFamilyStore } from '@/stores/useFamilyStore';
import type { EventDto, SearchPublicEventsQuery } from '@/types';
import TimelineEventDetail from '@/components/event/TimelineEventDetail';


interface TimelineData {
  time: string;
  title: string;
  description: string;
  lineColor?: string;
  circleColor?: string;
  originalEvent: EventDto;
}

const TimelineScreen: React.FC = () => {
  const { t } = useTranslation();
  const theme = useTheme();
  const isFocused = useIsFocused();
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
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState(''); // Corrected line 
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isFetchingMoreData, setIsFetchingMoreData] = useState(false);

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
      time: event.startDate ? format(new Date(event.startDate), 'dd/MM/yyyy') : '',
      title: event.name || t('common.noTitle'),
      description: event.description || t('common.noDescription'),
      originalEvent: event,
      // You can add logic here to set lineColor or circleColor based on event properties
    };
  }, [t]);

  const timelineData = useMemo(() => events.map(mapEventToTimelineData), [events, mapEventToTimelineData]);

  const loadEvents = useCallback(async (isLoadMore: boolean) => {
    if (!currentFamilyId) {
      Alert.alert(t('common.error'), t('timeline.familyIdNotFound'));
      return;
    }

    const { loading: currentLoading, hasMore: currentHasMore } = usePublicEventStore.getState();

    if (currentLoading || isFetchingMoreData) return; // Prevent multiple fetches

    // Prevent loading more if there are no more pages
    if (isLoadMore && !currentHasMore) return;

    try {
      if (!isLoadMore) {
        setIsRefreshing(true);
      } else {
        setIsFetchingMoreData(true); // Set fetching more data state
      }
      const query: SearchPublicEventsQuery = {
        searchTerm: debouncedSearchQuery,
        familyId: currentFamilyId,
        // Add other query parameters if needed
      };
      await fetchEvents(currentFamilyId, query, isLoadMore);
    } catch (err: any) {
      Alert.alert(t('common.error'), err.message || t('timeline.failedToLoadEvents'));
    } finally {
      setIsRefreshing(false);
      setIsFetchingMoreData(false); // Clear fetching more data state
    }
  }, [currentFamilyId, isFetchingMoreData, debouncedSearchQuery, fetchEvents, t]);

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
  }, [isFocused, debouncedSearchQuery, loadEvents, reset, currentFamilyId]);

  const onRefresh = useCallback(() => {
    const { loading: currentLoading } = usePublicEventStore.getState();
    if (!currentLoading) {
      loadEvents(false); // Fetch first page again
    }
  }, [loadEvents]);

  const onEndReached = useCallback(() => {
    console.log('End reached');
    const { loading: currentLoading, hasMore: currentHasMore } = usePublicEventStore.getState();
    if (!currentLoading && !isFetchingMoreData && currentHasMore) { // Use isFetchingMoreData
      loadEvents(true);
    }
  }, [isFetchingMoreData, loadEvents]);

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

  const renderFooter = useCallback(() => {
    if (isFetchingMoreData) {
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
  }, [isFetchingMoreData, hasMore, events.length, styles.footer, styles.footerText, t]);

  const memoizedRefreshControl = useMemo(() => (
    <RefreshControl
      refreshing={isRefreshing}
      onRefresh={onRefresh}
      tintColor={theme.colors.primary}
    />
  ), [isRefreshing, onRefresh, theme.colors.primary]);

  const timelineOptions = useMemo(() => ({
    data: timelineData,
    style: {
      paddingTop: 5,
    },
    refreshControl: memoizedRefreshControl,
    showVerticalScrollIndicator: false,
    onEndReached: onEndReached,
    renderFooter: renderFooter,
    onEndReachedThreshold: 0.5, // Increased threshold
    onEndReachedCalledDuringMomentum: true, // Add this prop
  }), [timelineData, memoizedRefreshControl, onEndReached, renderFooter]);

  if (error) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorText}>{error}</Text>
      </View>
    );
  }

  if (loading && events.length === 0) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" style={{ flex: 1, justifyContent: 'center' }} />
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
        {...timelineOptions}
        listViewContainerStyle={styles.list}
        circleSize={20}
        circleColor={theme.colors.primary}
        lineColor={theme.colors.primary}
        timeContainerStyle={{ minWidth: 86, marginTop: -5 }}
        timeStyle={{ textAlign: 'center',  color: theme.colors.onBackground, padding: 5 }}
        titleStyle={{ color: theme.colors.onSurface, marginTop: -15 }}
        descriptionStyle={{ color: theme.colors.onSurfaceVariant }}
        innerCircle={'dot'}
        renderDetail={(rowData: TimelineData) => (
          <TimelineEventDetail
            event={rowData.originalEvent}
          />
        )}
      />
    </View>
  );
};

export default TimelineScreen;
