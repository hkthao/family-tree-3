import React, { useCallback, useMemo, useState } from 'react';
import { FlatList, View, StyleSheet, RefreshControl, ActivityIndicator, StyleProp, ViewStyle, TouchableOpacity } from 'react-native';
import { Searchbar, Text, useTheme, IconButton as PaperIconButton } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { usePaginatedSearch, PaginatedSearchOptions, PaginatedSearchResult } from '@/hooks/usePaginatedSearch';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';

interface PaginatedSearchListProps<T, Q extends { searchTerm?: string }> {
  // Options for the usePaginatedSearch hook
  searchOptions: Omit<PaginatedSearchOptions<T, Q>, 'useStore'>;
  // Specific Zustand store hook for this list
  useStore: PaginatedSearchOptions<T, Q>['useStore'];

  renderItem: ({ item, index }: { item: T; index: number }) => React.ReactElement;
  keyExtractor: (item: T) => string;
  searchPlaceholder?: string;
  ListEmptyComponent?: React.ComponentType | React.ReactElement | null;
  ListHeaderComponent?: React.ComponentType | React.ReactElement | null;
  FilterComponent?: React.ComponentType<{ filters: Q; setFilters: React.Dispatch<React.SetStateAction<Q>>; toggleFilterVisibility?: () => void; }>;
  headerTitle?: string;
  // Style props
  containerStyle?: StyleProp<ViewStyle>;
  contentContainerStyle?: StyleProp<ViewStyle>;
  searchbarContainerStyle?: StyleProp<ViewStyle>;
  showFilterButton?: boolean;
}

export function PaginatedSearchList<T, Q extends { searchTerm?: string }>(
  props: PaginatedSearchListProps<T, Q>
) {
  const {
    searchOptions,
    useStore,
    renderItem,
    keyExtractor,
    searchPlaceholder,
    ListEmptyComponent,
    ListHeaderComponent,
    FilterComponent,
    headerTitle,
    containerStyle,
    contentContainerStyle,
    searchbarContainerStyle,
    showFilterButton = false,
  } = props;

  const { t } = useTranslation();
  const theme = useTheme();

  const [showFilterUI, setShowFilterUI] = useState(false);

  // Use the custom hook to handle search logic
  const {
    items,
    loading,
    error,
    hasMore,
    refreshing,
    searchQuery,
    setSearchQuery,
    filters,
    setFilters,
    handleRefresh,
    handleLoadMore,
  } = usePaginatedSearch<T, Q>({
    useStore,
    initialQuery: searchOptions.initialQuery,
    debounceTime: searchOptions.debounceTime,
    externalDependencies: searchOptions.externalDependencies,
  });

  const toggleFilterVisibility = useCallback(() => {
    setShowFilterUI(prev => !prev);
  }, []);

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
    },
    searchFilterContainer: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: SPACING_MEDIUM,
      backgroundColor: theme.colors.surfaceVariant,
      borderRadius: theme.roundness,
    },
    searchbar: {
      flex: 1,
      borderRadius: theme.roundness,
      backgroundColor: 'transparent',
    },
    filterButton: {
      // styles for the filter icon button
    },
    errorContainer: {
      padding: SPACING_MEDIUM,
      backgroundColor: theme.colors.errorContainer,
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    errorText: {
      color: theme.colors.onErrorContainer,
      textAlign: 'center',
    },
    emptyContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_LARGE,
    },
    footer: {
      paddingVertical: SPACING_MEDIUM,
      alignItems: 'center',
    },
    emptyListText: {
      color: theme.colors.onBackground,
      textAlign: 'center',
      marginTop: SPACING_MEDIUM,
    }
  }), [theme]);

  const DefaultEmptyListComponent = useMemo(() => (
    <View style={styles.emptyContainer}>
      <Text variant="titleMedium" style={styles.emptyListText}>{t('search.no_results')}</Text>
      <Text variant="bodyMedium" style={styles.emptyListText}>{t('search.try_different_query')}</Text>
    </View>
  ), [styles, t]);

  const renderListEmptyComponent = useMemo(() => {
    if (loading || refreshing) return null; // Don't show empty component while loading/refreshing
    if (ListEmptyComponent) {
      return React.isValidElement(ListEmptyComponent) ? ListEmptyComponent : React.createElement(ListEmptyComponent as React.ComponentType);
    }
    return DefaultEmptyListComponent;
  }, [loading, refreshing, ListEmptyComponent, DefaultEmptyListComponent]);

  const renderListFooterComponent = useCallback(() => {
    if (loading && items.length > 0) { // Only show spinner for subsequent loads, not initial
      return (
        <View style={styles.footer}>
          <ActivityIndicator animating size="small" color={theme.colors.primary} />
        </View>
      );
    }
    if (!hasMore && items.length > 0) {
      return <Text style={styles.emptyListText}>{t('common.noMoreItems')}</Text>; // Assuming common.noMoreItems exists
    }
    return null;
  }, [loading, items.length, hasMore, styles.footer, styles.emptyListText, theme.colors.primary, t]);

  return (
    <View style={[styles.safeArea, containerStyle]}>
      {headerTitle && (
        <View style={{ paddingHorizontal: SPACING_MEDIUM, paddingTop: SPACING_MEDIUM }}>
          <Text variant="headlineSmall" style={{ fontWeight: 'bold', textAlign: 'center' }}>{headerTitle}</Text>
        </View>
      )}

      <View style={[styles.container, containerStyle]}>
        <View style={[styles.searchFilterContainer, searchbarContainerStyle]}>
          <Searchbar
            placeholder={searchPlaceholder || t('search.placeholder')}
            onChangeText={setSearchQuery}
            value={searchQuery}
            style={styles.searchbar}
            showDivider={false} // Divider can be managed by container style
            clearIcon={searchQuery.length > 0 ? () => (
              <PaperIconButton
                icon="close-circle"
                size={20}
                onPress={() => setSearchQuery('')}
              />
            ) : undefined}
          />
          {showFilterButton && FilterComponent && (
            <PaperIconButton
              icon={showFilterUI ? "filter-off" : "filter"}
              onPress={toggleFilterVisibility}
              iconColor={theme.colors.onSurfaceVariant}
              size={24}
              style={styles.filterButton}
            />
          )}
        </View>

        {showFilterUI && FilterComponent && (
          <FilterComponent filters={filters} setFilters={setFilters} toggleFilterVisibility={toggleFilterVisibility} />
        )}

        {error && (
          <View style={styles.errorContainer}>
            <Text variant="bodyMedium" style={styles.errorText}>
              {t('common.error_occurred')}: {error}
            </Text>
          </View>
        )}

        {loading && items.length === 0 && !error ? ( // Show initial loading indicator
          <View style={styles.emptyContainer}>
            <ActivityIndicator animating size="large" color={theme.colors.primary} />
          </View>
        ) : (
          <FlatList
            showsVerticalScrollIndicator={false}
            data={items}
            renderItem={renderItem}
            keyExtractor={keyExtractor}
            contentContainerStyle={[styles.container, contentContainerStyle]}
            refreshControl={
              <RefreshControl
                refreshing={refreshing}
                onRefresh={handleRefresh}
                colors={[theme.colors.primary]}
                tintColor={theme.colors.primary}
              />
            }
            onEndReached={handleLoadMore}
            onEndReachedThreshold={0.5}
            ListEmptyComponent={renderListEmptyComponent}
            ListFooterComponent={renderListFooterComponent}
          />
        )}
      </View>
    </View>
  );
}
