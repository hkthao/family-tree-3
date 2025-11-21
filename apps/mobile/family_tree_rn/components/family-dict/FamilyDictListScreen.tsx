import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import {
  View,
  StyleSheet,
  FlatList,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';

import { Text, Card, Searchbar, useTheme, Appbar, Chip, IconButton } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicFamilyDictStore } from '@/stores/usePublicFamilyDictStore';
import { FamilyDictType, FamilyDictLineage, FamilyDictFilter } from '@/types/public.d';

export default function FamilyDictListScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const isFetchingMore = useRef(false);

  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState('');
  const [refreshing, setRefreshing] = useState(false);

  // Filters state
  const [filters, setFilters] = useState<Omit<FamilyDictFilter, 'page' | 'itemsPerPage'>>({
    searchQuery: '',
    sortBy: 'name',
    sortOrder: 'asc',
  });

  const {
    familyDicts,
    page,
    loading,
    error,
    hasMore,
    fetchFamilyDicts,
    reset,
  } = usePublicFamilyDictStore();

  // Debounce search query
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, 400); // 400ms debounce

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery]);

  // Effect for initial load and search query changes
  useEffect(() => {
    setFilters(prev => ({ ...prev, searchQuery: debouncedSearchQuery })); // Update searchQuery only
    reset(); // Clear data and reset page/hasMore
    // Pass filters object, then page and itemsPerPage separately
    fetchFamilyDicts({ ...filters, searchQuery: debouncedSearchQuery }, 1, 10, true);
  }, [debouncedSearchQuery, fetchFamilyDicts]);

  const handleRefresh = useCallback(async () => {
    if (!loading) {
      setRefreshing(true);
      try {
        reset(); // Clear data and reset page/hasMore
        await fetchFamilyDicts(filters, 1, 10, true);
      } finally {
        setRefreshing(false);
      }
    }
  }, [loading, reset, fetchFamilyDicts, filters]);


  const handleLoadMore = useCallback(async () => {
    if (!loading && hasMore && !isFetchingMore.current) {
      isFetchingMore.current = true;
      await fetchFamilyDicts(filters, page + 1, 10, false); // Fetch next page, not refreshing
      isFetchingMore.current = false;
    }
  }, [loading, hasMore, page, fetchFamilyDicts, filters]);

  const getFamilyDictTypeTitle = (type: FamilyDictType) => {
    switch (type) {
      case FamilyDictType.Blood: return t('familyDict.type.blood');
      case FamilyDictType.Marriage: return t('familyDict.type.marriage');
      case FamilyDictType.Adoption: return t('familyDict.type.adoption');
      case FamilyDictType.InLaw: return t('familyDict.type.inLaw');
      case FamilyDictType.Other: return t('familyDict.type.other');
      default: return t('common.unknown');
    }
  };

  const getFamilyDictLineageTitle = (lineage: FamilyDictLineage) => {
    switch (lineage) {
      case FamilyDictLineage.Noi: return t('familyDict.lineage.noi');
      case FamilyDictLineage.Ngoai: return t('familyDict.lineage.ngoai');
      case FamilyDictLineage.NoiNgoai: return t('familyDict.lineage.noiNgoai');
      case FamilyDictLineage.Other: return t('familyDict.lineage.other');
      default: return t('common.unknown');
    }
  };

  const renderFooter = () => {
    if (!loading || page === 1) return null; // Only show spinner for subsequent loads
    return (
      <View style={styles.footer}>
        <ActivityIndicator animating size="small" color={theme.colors.primary} />
      </View>
    );
  };

  const renderEmptyList = () => {
    if (loading || refreshing) return null;
    return (
      <View style={styles.emptyContainer}>
        <Text variant="titleMedium">{t('familyDict.search.no_results')}</Text>
        <Text variant="bodyMedium">{t('familyDict.search.try_different_query')}</Text>
      </View>
    );
  };


  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
    },
    container: {
      flex: 1,
      padding: SPACING_MEDIUM,
    },
    searchbar: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    errorContainer: {
      padding: SPACING_MEDIUM,
      backgroundColor: theme.colors.errorContainer,
      marginBottom: SPACING_MEDIUM,
    },
    errorText: {
      color: theme.colors.onErrorContainer,
      textAlign: 'center',
    },
    flatListContent: {
      // paddingBottom: SPACING_LARGE, // Ensure space at the bottom
    },
    flatListEmpty: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    familyDictCard: {
      marginBottom: SPACING_SMALL,
      marginHorizontal: 1,
    },
    cardContent: {
      // flexDirection: 'row', // Not needed for FamilyDict
      // alignItems: 'center', // Not needed for FamilyDict
    },
    namesByRegionContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL / 2, // Half the normal small spacing
      marginTop: SPACING_SMALL,
    },
    chip: {
      justifyContent: 'center', // Center content vertically
      borderWidth: 0, // Remove border
    },
    footer: {
      paddingVertical: SPACING_MEDIUM,
      alignItems: 'center',
    },
    emptyContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_LARGE,
    },
    cardText: {
      // flex: 1,
    }
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <Appbar.Header>
        <Appbar.Content title={t('familyDict.list.title')} />
      </Appbar.Header>
      <View style={styles.container}>
        <Searchbar
          placeholder={t('familyDict.search.search')}
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
          showDivider={true}
          clearIcon={searchQuery.length > 0 ? () => (
            <IconButton
              icon="close-circle"
              iconColor={theme.colors.onSurfaceVariant}
              size={20}
              onPress={() => setSearchQuery('')}
            />
          ) : undefined}
        />

        {error && (
          <View style={styles.errorContainer}>
            <Text variant="bodyMedium" style={styles.errorText}>
              {t('common.error_occurred')}: {error}
            </Text>
          </View>
        )}

        <FlatList
          showsVerticalScrollIndicator={false}
          data={familyDicts}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <Card style={[styles.familyDictCard, { borderRadius: theme.roundness }]}>
              <Card.Content style={styles.cardContent}>
                <Text variant="titleMedium">{item.name}</Text>
                <Text variant="bodyMedium" numberOfLines={2}>{item.description}</Text>
                <View style={styles.namesByRegionContainer}>
                  {(() => {
                    const northName = item.namesByRegion.north || '';
                    const centralValue = item.namesByRegion.central;
                    const southValue = item.namesByRegion.south;

                    const centralName = typeof centralValue === 'string'
                      ? centralValue
                      : Array.isArray(centralValue)
                        ? centralValue.sort().join(', ')
                        : '';
                    const southName = typeof southValue === 'string'
                      ? southValue
                      : Array.isArray(southValue)
                        ? southValue.sort().join(', ')
                        : '';

                    const areAllRegionsEmpty = !northName && !centralName && !southName;
                    const areNamesIdentical = northName === centralName && centralName === southName && !areAllRegionsEmpty;
                    const areNamesIdenticalToItemName = areNamesIdentical && northName === (item.name || '');

                    if (areNamesIdenticalToItemName) {
                      return null; // Don't render any region chips
                    } else if (areNamesIdentical) {
                      return (
                        <Chip icon="map-marker-multiple-outline" style={[styles.chip, { backgroundColor: theme.colors.primaryContainer }]}>
                          {northName}
                        </Chip>
                      );
                    } else {
                      return (
                        <>
                          {item.namesByRegion.north && (
                            <Chip icon="compass-outline" style={[styles.chip, { backgroundColor: theme.colors.primaryContainer }]}>
                              {item.namesByRegion.north}
                            </Chip>
                          )}
                          {typeof item.namesByRegion.central === 'string' && item.namesByRegion.central && (
                            <Chip icon="map-marker-outline" style={[styles.chip, { backgroundColor: theme.colors.secondaryContainer }]}>
                              {item.namesByRegion.central}
                            </Chip>
                          )}
                          {Array.isArray(item.namesByRegion.central) && item.namesByRegion.central.map((name, index) => (
                            <Chip key={`central-${index}`} icon="map-marker-outline" style={[styles.chip, { backgroundColor: theme.colors.secondaryContainer }]}>
                              {name}
                            </Chip>
                          ))}
                          {typeof item.namesByRegion.south === 'string' && item.namesByRegion.south && (
                            <Chip icon="compass" style={[styles.chip, { backgroundColor: theme.colors.tertiaryContainer }]}>
                              {item.namesByRegion.south}
                            </Chip>
                          )}
                          {Array.isArray(item.namesByRegion.south) && item.namesByRegion.south.map((name, index) => (
                            <Chip key={`south-${index}`} icon="compass" style={[styles.chip, { backgroundColor: theme.colors.tertiaryContainer }]}>
                              {name}
                            </Chip>
                          ))}
                        </>
                      );
                    }
                  })()}
                  <Chip icon="tag" style={[styles.chip, { backgroundColor: theme.colors.surfaceVariant }]}>
                    {getFamilyDictTypeTitle(item.type)}
                  </Chip>
                  <Chip icon="account-group" style={[styles.chip, { backgroundColor: theme.colors.surfaceVariant }]}>
                    {getFamilyDictLineageTitle(item.lineage)}
                  </Chip>
                  {item.specialRelation && (
                    <Chip icon="star" style={[styles.chip, { backgroundColor: theme.colors.surfaceVariant }]}>
                      {t('familyDict.form.specialRelation')}
                    </Chip>
                  )}
                </View>
              </Card.Content>
            </Card>
          )}
          ListEmptyComponent={renderEmptyList}
          ListFooterComponent={renderFooter}
          onEndReached={handleLoadMore}
          onEndReachedThreshold={0.3}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={[theme.colors.primary]}
              tintColor={theme.colors.primary}
            />
          }
          contentContainerStyle={familyDicts.length === 0 && !loading && !error ? styles.flatListEmpty : styles.flatListContent}
        />
      </View>
    </View>
  );
}
