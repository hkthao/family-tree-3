import React, { useCallback, useMemo } from 'react';
import { View, StyleSheet } from 'react-native';
import { Text, Card, Chip, Appbar, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicFamilyDictStore } from '@/stores/usePublicFamilyDictStore';
import { FamilyDictType, FamilyDictLineage, FamilyDictDto, FamilyDictFilter } from '@/types';
import { PaginatedSearchList } from '@/components/common';
import { ZustandPaginatedStore } from '@/hooks/usePaginatedSearch';

export default function FamilyDictListScreen() {
  const { t } = useTranslation();
  const theme = useTheme();

  // Define useStore function for usePaginatedSearch
  const useStore = useCallback(() => {
    const { familyDicts, loading, error, hasMore, page, fetchFamilyDicts, reset, setError } = usePublicFamilyDictStore();
    return {
      items: familyDicts,
      loading,
      error,
      hasMore,
      page, // currentPage from store is already named 'page'
      fetch: async (filter: FamilyDictFilter, isLoadMore: boolean) => {
        // The fetchFamilyDicts function already handles page increment internally if isLoadMore is true
        // It also accepts page and itemsPerPage directly
        return fetchFamilyDicts(filter, isLoadMore ? page + 1 : 1, 10, !isLoadMore);
      },
      reset,
      setError,
    } as ZustandPaginatedStore<FamilyDictDto, FamilyDictFilter>;
  }, [usePublicFamilyDictStore]);

  const getFamilyDictTypeTitle = useCallback((type: FamilyDictType) => {
    switch (type) {
      case FamilyDictType.Blood: return t('familyDict.type.blood');
      case FamilyDictType.Marriage: return t('familyDict.type.marriage');
      case FamilyDictType.Adoption: return t('familyDict.type.adoption');
      case FamilyDictType.InLaw: return t('familyDict.type.inLaw');
      case FamilyDictType.Other: return t('familyDict.type.other');
      default: return t('common.unknown');
    }
  }, [t]);

  const getFamilyDictLineageTitle = useCallback((lineage: FamilyDictLineage) => {
    switch (lineage) {
      case FamilyDictLineage.Noi: return t('familyDict.lineage.noi');
      case FamilyDictLineage.Ngoai: return t('familyDict.lineage.ngoai');
      case FamilyDictLineage.NoiNgoai: return t('familyDict.lineage.noiNgoai');
      case FamilyDictLineage.Other: return t('familyDict.lineage.other');
      default: return t('common.unknown');
    }
  }, [t]);

  const renderFamilyDictItem = useCallback(({ item }: { item: FamilyDictDto }) => (
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
              return null;
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
                  {Array.isArray(item.namesByRegion.central) && item.namesByRegion.central.map((name: string, index: number) => (
                    <Chip key={`central-${index}`} icon="map-marker-outline" style={[styles.chip, { backgroundColor: theme.colors.secondaryContainer }]}>
                      {name}
                    </Chip>
                  ))}
                  {typeof item.namesByRegion.south === 'string' && item.namesByRegion.south && (
                    <Chip icon="compass" style={[styles.chip, { backgroundColor: theme.colors.tertiaryContainer }]}>
                      {item.namesByRegion.south}
                    </Chip>
                  )}
                  {Array.isArray(item.namesByRegion.south) && item.namesByRegion.south.map((name: string, index: number) => (
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
  ), [theme, getFamilyDictTypeTitle, getFamilyDictLineageTitle, t]);

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
    },
    familyDictCard: {
      marginBottom: SPACING_SMALL,
      marginHorizontal: 1,
    },
    cardContent: {},
    namesByRegionContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL / 2,
      marginTop: SPACING_SMALL,
    },
    chip: {
      justifyContent: 'center',
      borderWidth: 0,
    },
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <Appbar.Header>
        <Appbar.Content title={t('familyDict.list.title')} />
      </Appbar.Header>
      <PaginatedSearchList<FamilyDictDto, FamilyDictFilter>
        useStore={useStore}
        searchOptions={{
          initialQuery: { searchTerm: '', sortBy: 'name', sortOrder: 'asc' },
        }}
        renderItem={renderFamilyDictItem}
        keyExtractor={(item) => item.id}
        searchPlaceholder={t('familyDict.search.search')}
        containerStyle={styles.container}
      />
    </View>
  );
}
