import React, { useCallback, useMemo } from 'react';
import { StyleSheet, View } from 'react-native';
import { Appbar, Avatar, Card, Chip, Text, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';

import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { useFamilyStore } from '@/stores/useFamilyStore';
import { usePublicFamilyStore } from '@/stores/usePublicFamilyStore';
import { FamilyListDto, SearchPublicFamiliesQuery } from '@/types';
import { PaginatedSearchList } from '@/components/common';


export default function FamilySearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const setCurrentFamilyId = useFamilyStore((state) => state.setCurrentFamilyId);

  // Define useStore function for usePaginatedSearch
  const useStore = useCallback(() => {
    const { families, loading, error, hasMore, page: currentPage, fetchFamilies, reset, setError } = usePublicFamilyStore();
    return useMemo(() => ({
      items: families,
      loading,
      error,
      hasMore,
      page: currentPage,
      fetch: async (query: SearchPublicFamiliesQuery, isLoadMore: boolean) => {
        return fetchFamilies({ ...query, page: query.page || 1 }, isLoadMore);
      },
      reset,
      setError,
    }), [families, loading, error, hasMore, currentPage, fetchFamilies, reset, setError]);
  }, []);

  const renderFamilyItem = useCallback(({ item }: { item: FamilyListDto }) => (
    <Card style={[styles.familyCard, { borderRadius: theme.roundness }]} onPress={() => {
      setCurrentFamilyId(item.id);
      router.push('/family/dashboard');
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
  ), [theme, setCurrentFamilyId, router, t]);

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_SMALL, // Apply padding to PaginatedSearchList's containerStyle
    },
    familyCard: {
      marginBottom: SPACING_SMALL,
      marginHorizontal: 1,
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
      flexWrap: 'wrap',
    },
    chip: {
      marginRight: SPACING_SMALL / 2,
      marginBottom: SPACING_SMALL / 2,
      height: 28,
      justifyContent: 'center',
      borderWidth: 0,
    },
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <Appbar.Header>
        <Appbar.Content title={t('search.title')} />
      </Appbar.Header>
      <PaginatedSearchList<FamilyListDto, SearchPublicFamiliesQuery>
        useStore={useStore}
        searchOptions={{
          initialQuery: { page: 1, itemsPerPage: 10, searchTerm: '' },
        }}
        renderItem={renderFamilyItem}
        keyExtractor={(item) => item.id}
        searchPlaceholder={t('search.placeholder')}
        containerStyle={styles.container}
      // Additional props can be passed here as needed
      />
    </View>
  );
}
