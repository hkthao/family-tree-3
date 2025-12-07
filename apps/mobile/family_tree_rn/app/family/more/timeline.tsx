import React, { useMemo, useCallback } from 'react';
import { View, StyleSheet, Alert } from 'react-native';
import { useTheme, Text } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useIsFocused } from '@react-navigation/native';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { usePublicEventStore } from '@/stores/usePublicEventStore';
import { useFamilyStore } from '@/stores/useFamilyStore';
import type { EventDto, SearchPublicEventsQuery } from '@/types';
import { TimelineListItem } from '@/components/event';
import { PaginatedSearchList } from '@/components/common';
import { usePaginatedSearch, ZustandPaginatedStore } from '@/hooks'; // Import usePaginatedSearch and ZustandPaginatedStore

const TimelineScreen = () => {
  const { t } = useTranslation();
  const theme = useTheme();
  const isFocused = useIsFocused();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  // Define useStore function for usePaginatedSearch
  const useStore = useCallback(() => {
    const { events, loading, error, hasMore, page, fetchEvents, reset, setError } = usePublicEventStore();
    return {
      items: events,
      loading,
      error,
      hasMore,
      page, // Map currentPage to page
      fetch: async (query: SearchPublicEventsQuery, isLoadMore: boolean) => {
        if (!currentFamilyId) {
          Alert.alert(t('common.error'), t('timeline.familyIdNotFound'));
          setError(t('timeline.familyIdNotFound'));
          return null;
        }
        // The fetchEvents function already handles page increment internally if isLoadMore is true
        // It also accepts familyId and query directly
        const result = await fetchEvents(currentFamilyId, query, isLoadMore);
        return result; // fetchEvents now returns PaginatedList<EventDto> | null
      },
      reset,
      setError,
    } as ZustandPaginatedStore<EventDto, SearchPublicEventsQuery>;
  }, [currentFamilyId, t]);

  const { items } = usePaginatedSearch<EventDto, SearchPublicEventsQuery>({
    useStore,
    initialQuery: { searchTerm: '', familyId: '', page: 1, itemsPerPage: 10, sortBy: 'startDate', sortOrder: 'desc' },
    externalDependencies: [currentFamilyId, isFocused],
  });

  const renderEventItem = useCallback(({ item, index }: { item: EventDto, index: number }) => (
    <TimelineListItem
      item={item}
      index={index}
      isFirst={index === 0}
      isLast={index === items.length - 1} // Calculate isLast dynamically
    />
  ), [items.length]); // Add items.length to dependency array

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
    },
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <PaginatedSearchList<EventDto, SearchPublicEventsQuery>
        useStore={useStore}
        searchOptions={{
          initialQuery: { searchTerm: '', familyId: '', page: 1, itemsPerPage: 10, sortBy: 'startDate', sortOrder: 'desc' },
          externalDependencies: [currentFamilyId, isFocused],
        }}
        renderItem={renderEventItem}
        keyExtractor={(item) => item.id}
        searchPlaceholder={t('timeline.searchPlaceholder')}
        containerStyle={styles.container}
        ListEmptyComponent={(
          <View style={styles.container}>
            <Text style={{ textAlign: 'center', marginTop: SPACING_MEDIUM, color: theme.colors.onBackground }}>
              {t('timeline.noEventsFound')}
            </Text>
          </View>
        )}
      />
    </View>
  );
};

export default TimelineScreen;
