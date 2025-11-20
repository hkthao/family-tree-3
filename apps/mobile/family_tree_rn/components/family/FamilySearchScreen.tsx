import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import {
  View,
  StyleSheet,
  FlatList,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';

import { Text, Card, Avatar, IconButton, Searchbar, useTheme, Appbar, Chip } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router'; // Import useRouter
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicFamilyStore } from '@/stores/usePublicFamilyStore'; // Import usePublicFamilyStore
import { useFamilyStore } from '@/stores/useFamilyStore'; // Import useFamilyStore
import type { FamilyListDto } from '@/types/public.d'; // Import FamilyListDto
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png'; // Import default family avatar

const PAGE_SIZE = 10; // Re-define PAGE_SIZE

export default function FamilySearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme(); // Get theme from PaperProvider
  const router = useRouter(); // Initialize useRouter
  const setCurrentFamilyId = useFamilyStore((state) => state.setCurrentFamilyId); // Get setCurrentFamilyId from store
  const isFetchingMore = useRef(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState('');
  const [refreshing, setRefreshing] = useState(false);

  const {
    families,
    page,
    loading,
    error,
    hasMore,
    fetchFamilies, // Renamed from searchFamilies
    reset,
  } = usePublicFamilyStore();

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
    reset(); // Clear data and reset page/hasMore
    fetchFamilies({ page: 1, search: debouncedSearchQuery }, true); // Fetch first page for debounced search
  }, [debouncedSearchQuery, fetchFamilies, reset]); // Added reset to dependencies

  const handleRefresh = useCallback(async () => {
    if (!loading) {
      setRefreshing(true);
      try {
        reset(); // Clear data and reset page/hasMore
        await fetchFamilies({ page: 1, search: searchQuery }, true); // Fetch first page for current search query
      } finally {
        setRefreshing(false);
      }
    }
  }, [loading, reset, fetchFamilies, searchQuery]);


  const handleLoadMore = useCallback(async () => {
    if (!loading && hasMore && !isFetchingMore.current) {
      isFetchingMore.current = true;
      await fetchFamilies({ page: page + 1, search: searchQuery }); // Fetch next page
      isFetchingMore.current = false;
    }
  }, [loading, hasMore, page, fetchFamilies, searchQuery]);

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
        <Text variant="titleMedium">{t('search.no_results')}</Text>
        <Text variant="bodyMedium">{t('search.try_different_query')}</Text>
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
      borderRadius: theme.roundness, // Explicitly apply theme roundness
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
    familyCard: {
      marginBottom: SPACING_SMALL,
      marginHorizontal: 1
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
      flexWrap: 'wrap', // Allow chips to wrap to the next line
    },
    chip: {
      marginRight: SPACING_SMALL / 2,
      marginBottom: SPACING_SMALL / 2,
      height: 28, // Adjust chip height
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
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <Appbar.Header>
        <Appbar.Content title={t('search.title')} />
      </Appbar.Header>
      <View style={styles.container}>
        <Searchbar
          placeholder={t('search.placeholder')}
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
          data={families}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <Card style={[styles.familyCard, { borderRadius: theme.roundness }]} onPress={() => {
              setCurrentFamilyId(item.id);
              router.push('/family/details');
            }}>
              <Card.Content style={styles.cardContent}>
                <Avatar.Image size={48} source={item.avatarUrl ? { uri: item.avatarUrl } : DefaultFamilyAvatar} style={styles.avatar} />
                <View style={styles.cardText}>
                  <Text variant="titleMedium">{item.name}</Text>
                  <Text variant="bodyMedium">{item.address}</Text>
                  <View style={styles.detailsRow}>
                    <Chip icon="account-group" mode="outlined" style={styles.chip}>
                      <Text variant="bodySmall">{item.totalMembers}</Text>
                    </Chip>
                    <Chip icon="family-tree" mode="outlined" style={styles.chip}>
                      <Text variant="bodySmall">{item.totalGenerations}</Text>
                    </Chip>
                    <Chip icon={item.visibility.toLowerCase() === 'public' ? 'eye' : 'eye-off'} mode="outlined" style={styles.chip}>
                      <Text variant="bodySmall">{t(`family.visibility.${item.visibility.toLowerCase()}`)}</Text>
                    </Chip>
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
          contentContainerStyle={families.length === 0 && !loading && !error ? styles.flatListEmpty : styles.flatListContent}
        />
      </View>
    </View>
  );
}
