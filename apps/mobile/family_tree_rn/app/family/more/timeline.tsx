import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { View, StyleSheet, RefreshControl, Alert, FlatList } from 'react-native';
import { useTheme, Searchbar, ActivityIndicator, Text } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useIsFocused } from '@react-navigation/native';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { usePublicEventStore } from '@/stores/usePublicEventStore';
import { useFamilyStore } from '@/stores/useFamilyStore';
import type { EventDto, SearchPublicEventsQuery } from '@/types';
import { TimelineListItem } from '@/components/event';

const TimelineScreen = () => {
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

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, 400);

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery]);

  const loadEvents = useCallback(async (isLoadMore: boolean) => {
    if (!currentFamilyId) {
      Alert.alert(t('common.error'), t('timeline.familyIdNotFound'));
      return;
    }

    const { hasMore: currentHasMore } = usePublicEventStore.getState();

    if (isLoadMore && !currentHasMore) return;

    if (!isLoadMore) {
      setIsRefreshing(true);
      // Reset store for fresh fetch
      reset();
    }

    try {
      const query: SearchPublicEventsQuery = {
        searchTerm: debouncedSearchQuery,
        familyId: currentFamilyId,
        // Add other query parameters if needed
      };
      await fetchEvents(currentFamilyId, query, isLoadMore);
    } catch (err: any) {
      Alert.alert(t('common.error'), err.message || t('timeline.failedToLoadEvents'));
    } finally {
      if (!isLoadMore) {
        setIsRefreshing(false);
      }
    }
  }, [currentFamilyId, debouncedSearchQuery, fetchEvents, t, reset]);

  useEffect(() => {
    if (isFocused && currentFamilyId) {
      loadEvents(false);
    }
  }, [isFocused, debouncedSearchQuery, currentFamilyId, loadEvents]);

  const onRefresh = useCallback(() => {
    console.log('Refreshing timeline');

    loadEvents(false); // Fetch first page again
  }, [loadEvents]);

  const onEndReached = useCallback(() => {
    console.log('End reached');
    const { loading: currentLoading, hasMore: currentHasMore } = usePublicEventStore.getState();
    if (!currentLoading && currentHasMore) {
      loadEvents(true);
    }
  }, [loadEvents]);

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
    if (loading) { // Use store's loading state
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
  }, [loading, hasMore, events.length, styles.footer, styles.footerText, t]);

  const renderItem = useCallback(({ item, index }: { item: EventDto, index: number }) => (
    <TimelineListItem
      item={item}
      index={index}
      isFirst={index === 0}
      isLast={index === events.length - 1}
    />
  ), [events.length]);


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
      <FlatList
        data={events}
        renderItem={renderItem}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl
            refreshing={isRefreshing}
            onRefresh={onRefresh}
            tintColor={theme.colors.primary}
          />
        }
        onEndReached={onEndReached}
        onEndReachedThreshold={0.5}
        ListFooterComponent={renderFooter}
      />
    </View>
  );
};

export default TimelineScreen;
