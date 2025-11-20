import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import {
  View,
  StyleSheet,
  FlatList,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { Text, Card, Avatar, IconButton as PaperIconButton, Searchbar, useTheme, Chip, Appbar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';
import { useFamilyStore } from '../../stores/useFamilyStore';
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicMemberStore } from '@/stores/usePublicMemberStore'; // Import usePublicMemberStore
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png'; // Import default family avatar
import { Gender } from '@/types/public.d'; // Import Gender enum

interface MemberFilter {
  gender?: Gender;
  isRootMember?: boolean;
}



export default function MemberSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState('');

  const {
    members,
    page,
    loading,
    error,
    hasMore,
    fetchMembers,
    reset,
    setError, // Destructure setError from store
  } = usePublicMemberStore();

  const [refreshing, setRefreshing] = useState(false);
  const [filters, setFilters] = useState<MemberFilter>({});
  const [showFilterChips, setShowFilterChips] = useState(false);

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
    if (!currentFamilyId) {
      setError(t('memberSearch.errors.noFamilyId'));
      reset(); // Clear any previous data
      return;
    }
    reset(); // Clear data and reset page/hasMore
    fetchMembers({
      familyId: currentFamilyId!, // Use non-null assertion
      page: 1,
      searchTerm: debouncedSearchQuery,
      gender: filters.gender,
      isRoot: filters.isRootMember,
    }, true); // Fetch first page for debounced search
  }, [currentFamilyId, debouncedSearchQuery, filters, fetchMembers, reset, setError, t]);

  const handleRefresh = useCallback(async () => {
    if (!loading) {
      setRefreshing(true);
      try {
        reset(); // Clear data and reset page/hasMore
        await fetchMembers({
          familyId: currentFamilyId!, // Use non-null assertion
          page: 1,
          searchTerm: searchQuery,
          gender: filters.gender,
          isRoot: filters.isRootMember,
        }, true); // Fetch first page for current search query
      } finally {
        setRefreshing(false);
      }
    }
  }, [loading, reset, fetchMembers, currentFamilyId, searchQuery, filters]);

  const isFetchingMore = useRef(false);

  const handleLoadMore = useCallback(async () => {
    if (!loading && hasMore && !isFetchingMore.current) {
      isFetchingMore.current = true;
      await fetchMembers({
        familyId: currentFamilyId!, // Use non-null assertion
        page: page + 1,
        searchTerm: searchQuery,
        gender: filters.gender,
        isRoot: filters.isRootMember,
      }); // Fetch next page
      isFetchingMore.current = false;
    }
  }, [loading, hasMore, page, fetchMembers, currentFamilyId, searchQuery, filters]);

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
      flex: 1,
      borderRadius: theme.roundness,
      backgroundColor: 'transparent', // Make Searchbar background transparent
    },
    searchFilterContainer: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: SPACING_MEDIUM,
      backgroundColor: theme.colors.surfaceVariant, // Set background color for the container
      borderRadius: theme.roundness, // Match Searchbar's border radius
    },
    filterButton: {
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
    },
    flatListEmpty: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    memberCard: {
      marginBottom: SPACING_MEDIUM,
      marginHorizontal: 1
    },
    cardContent: {
      flexDirection: 'row',
      alignItems: 'flex-start',
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
    },
    memberDetailsChips: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      marginLeft: -SPACING_MEDIUM,
    },
    detailChip: {
      backgroundColor: 'transparent',
      paddingHorizontal: 0,
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
  }), [theme]);

  return (
    <View style={styles.container}>
      <View style={styles.searchFilterContainer}>
        <Searchbar
          placeholder={t('memberSearch.placeholder')}
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
          showDivider={true}
          clearIcon={searchQuery.length > 0 ? () => (
            <PaperIconButton
              icon="close-circle"
              size={20}
              onPress={() => setSearchQuery('')}
            />
          ) : undefined}
        />
        <Appbar.Action
          icon={showFilterChips ? "filter-off" : "filter"}
          onPress={() => setShowFilterChips(!showFilterChips)}
          color={theme.colors.onSurfaceVariant}
          size={24}
          style={styles.filterButton}
        />
      </View>

      {showFilterChips && (
        <View style={styles.filterChipsContainer}>
          <Chip
            selected={filters.gender === Gender.Male}
            onPress={() => handleFilterChange('gender', Gender.Male)}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.gender.male')}
          </Chip>
          <Chip
            selected={filters.gender === Gender.Female}
            onPress={() => handleFilterChange('gender', Gender.Female)}
            style={styles.filterChip}
          >
            {t('memberSearch.filter.gender.female')}
          </Chip>
          <Chip
            selected={filters.gender === Gender.Other}
            onPress={() => handleFilterChange('gender', Gender.Other)}
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
        </View>
      )}
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
            router.push(`/member/${item.id}`);
          }}>
            <Card.Content style={styles.cardContent}>
              <Avatar.Image size={48} source={item.avatarUrl ? { uri: item.avatarUrl } : DefaultFamilyAvatar} style={styles.avatar} />
              <View style={styles.cardText}>
                <Text variant="titleMedium">{item.fullName}</Text>
                <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: SPACING_SMALL / 2 }}>
                  {item.occupation && <Text variant="bodySmall">{item.occupation}</Text>}
                  {item.occupation && item.birthDeathYears && <Text variant="bodySmall">|</Text>}
                  {item.birthDeathYears && <Text variant="bodySmall">{item.birthDeathYears}</Text>}
                </View>
                <View style={styles.memberDetailsChips}>
                  {item.gender && (
                    <Chip icon="gender-male-female" style={styles.detailChip} compact={true}>
                      {t(`memberSearch.filter.gender.${item.gender.toLowerCase()}`)}
                    </Chip>
                  )}
                  {item.fatherFullName && (
                    <Chip icon="human-male-boy" style={styles.detailChip} compact={true} >
                      {item.fatherFullName}
                    </Chip>
                  )}
                  {item.motherFullName && (
                    <Chip icon="human-female-girl" style={styles.detailChip} compact={true} >
                      {item.motherFullName}
                    </Chip>
                  )}
                  {item.wifeFullName && (
                    <Chip icon="heart" style={styles.detailChip} compact={true} >
                      {item.wifeFullName}
                    </Chip>
                  )}
                  {item.husbandFullName && (
                    <Chip icon="heart" style={styles.detailChip} compact={true} >
                      {item.husbandFullName}
                    </Chip>
                  )}
                </View>
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
        contentContainerStyle={members.length === 0 && !loading && !error ? styles.flatListEmpty : styles.flatListContent}
      />
    </View>
  );
}
