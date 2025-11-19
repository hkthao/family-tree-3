import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import {
  View,
  StyleSheet,
  FlatList,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Text, Card, Avatar, IconButton, Searchbar, useTheme, Chip } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';
import { useFamilyStore } from '../../stores/useFamilyStore';
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { fetchFamilyMembers, FamilyMember } from '../../data/mockFamilyData'; // Assuming fetchFamilyMembers is updated

interface MemberFilter {
  gender?: 'Male' | 'Female' | 'Other';
  isRootMember?: boolean;
  ordinalNumber?: string; // e.g., "1-10", "11-20" or just a boolean for "has ordinal"
}

const PAGE_SIZE = 10;

export default function MemberSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const [searchQuery, setSearchQuery] = useState('');
  const [members, setMembers] = useState<FamilyMember[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const loadingRef = useRef(loading);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);
  const [filters, setFilters] = useState<MemberFilter>({});

  useEffect(() => {
    loadingRef.current = loading;
  }, [loading]);

  const loadMembers = useCallback(
    async (currentPage: number, isRefreshing: boolean = false) => {
      if (loadingRef.current) {
        return;
      }

      setLoading(true);
      setError(null);

      if (!currentFamilyId) {
        setError(t('memberSearch.errors.noFamilyId'));
        setLoading(false);
        return;
      }

      try {
        const controller = new AbortController();
        // fetchFamilyMembers needs to be updated to accept query and filters
        const { data, totalCount: newTotalCount } = await fetchFamilyMembers(
          currentFamilyId,
          searchQuery,
          filters,
          currentPage,
          PAGE_SIZE,
          controller.signal
        );

        setMembers((prevMembers) => {
          const updatedMembers = isRefreshing ? data : [...prevMembers, ...data];
          setHasMore(updatedMembers.length < newTotalCount);
          return updatedMembers;
        });
        setTotalCount(newTotalCount);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError(t('memberSearch.errors.unknown'));
        }
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [currentFamilyId, searchQuery, filters, setLoading, setError, setMembers, setTotalCount, setHasMore, t]
  );

  useEffect(() => {
    setMembers([]);
    setPage(1);
    setHasMore(true);
    loadMembers(1);
  }, [searchQuery, filters, loadMembers]);

  const handleRefresh = useCallback(() => {
    setRefreshing(true);
    setPage(1);
    setMembers([]);
    setHasMore(true);
    loadMembers(1, true);
  }, [loadMembers]);

  const handleLoadMore = useCallback(() => {
    if (!loading && hasMore) {
      setPage((prevPage) => prevPage + 1);
      loadMembers(page + 1);
    }
  }, [loading, hasMore, page, loadMembers]);

  const handleFilterChange = useCallback((key: keyof MemberFilter, value: any) => {
    setFilters((prevFilters) => {
      if (prevFilters[key] === value) {
        // Toggle off if already selected
        const newFilters = { ...prevFilters };
        delete newFilters[key];
        return newFilters;
      }
      return { ...prevFilters, [key]: value };
    });
  }, []);

  const renderFooter = () => {
    if (!loading) return null;
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
        <Text variant="titleMedium">{t('memberSearch.no_results')}</Text>
        <Text variant="bodyMedium">{t('memberSearch.try_different_query')}</Text>
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
      paddingBottom: SPACING_LARGE,
    },
    searchbar: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    filterChipsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL,
      marginBottom: SPACING_MEDIUM,
    },
    filterChip: {
      // Add any specific chip styling here
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
    memberCard: {
      marginBottom: SPACING_MEDIUM,
    },
    cardContent: {
      flexDirection: 'row',
      alignItems: 'center',
    },
    avatar: {
      marginRight: SPACING_MEDIUM,
    },
    cardText: {
      flex: 1,
    },
    detailsRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginTop: SPACING_SMALL,
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
  }), [theme]);

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <Searchbar
          placeholder={t('memberSearch.placeholder')}
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

        <View style={styles.filterChipsContainer}>
          <Chip
            selected={filters.gender === 'Male'}
            onPress={() => handleFilterChange('gender', 'Male')}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.gender.male')}
          </Chip>
          <Chip
            selected={filters.gender === 'Female'}
            onPress={() => handleFilterChange('gender', 'Female')}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.gender.female')}
          </Chip>
          <Chip
            selected={filters.gender === 'Other'}
            onPress={() => handleFilterChange('gender', 'Other')}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.gender.other')}
          </Chip>
          <Chip
            selected={filters.isRootMember === true}
            onPress={() => handleFilterChange('isRootMember', true)}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.isRootMember')}
          </Chip>
          {/* Ordinal number filter can be more complex, e.g., a dropdown or range input */}
          {/* For simplicity, let's add a placeholder chip for now */}
          <Chip
            selected={filters.ordinalNumber === 'hasOrdinal'}
            onPress={() => handleFilterChange('ordinalNumber', 'hasOrdinal')}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.hasOrdinalNumber')}
          </Chip>
        </View>

        {error && (
          <View style={styles.errorContainer}>
            <Text variant="bodyMedium" style={styles.errorText}>
              {t('common.error_occurred')}: {error}
            </Text>
          </View>
        )}

        <FlatList
          showsVerticalScrollIndicator={false}
          data={members}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <Card style={[styles.memberCard, { borderRadius: theme.roundness }]} onPress={() => {
              // Navigate to member details
              // router.push(`/member/${item.id}`); // Assuming a member detail route
            }}>
              <Card.Content style={styles.cardContent}>
                <Avatar.Image size={48} source={{ uri: item.avatarUrl || 'https://via.placeholder.com/150' }} style={styles.avatar} />
                <View style={styles.cardText}>
                  <Text variant="titleMedium">{item.name}</Text>
                  <Text variant="bodyMedium">{item.relationship}</Text>
                  {/* Add more member details here */}
                </View>
              </Card.Content>
            </Card>
          )}
          ListEmptyComponent={renderEmptyList}
          ListFooterComponent={renderFooter}
          onEndReached={handleLoadMore}
          onEndReachedThreshold={0.5}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={[theme.colors.primary]}
              tintColor={theme.colors.primary}
            />
          }
          contentContainerStyle={members.length === 0 && !loading && !error ? styles.flatListEmpty : styles.flatListContent}
        />
      </View>
    </SafeAreaView>
  );
}
