import React, { useCallback, useMemo } from 'react';
import { View, StyleSheet } from 'react-native';
import { Text, Card, Avatar, Chip, useTheme, Appbar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';

import { useFamilyStore } from '@/stores/useFamilyStore';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicMemberStore } from '@/stores/usePublicMemberStore';
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png';
import { Gender, MemberListDto, SearchPublicMembersQuery } from '@/types';
import { PaginatedSearchList } from '@/components/common';
import { ZustandPaginatedStore } from '@/hooks/usePaginatedSearch'; // Import ZustandPaginatedStore

interface MemberFilterProps {
  filters: SearchPublicMembersQuery;
  setFilters: React.Dispatch<React.SetStateAction<SearchPublicMembersQuery>>;
  toggleFilterVisibility?: () => void;
}

const MemberFilterComponent: React.FC<MemberFilterProps> = ({ filters, setFilters }) => {
  const { t } = useTranslation();
  const theme = useTheme();

  const handleFilterChange = useCallback((key: keyof SearchPublicMembersQuery, value: any) => {
    setFilters((prevFilters) => {
      // Toggle off if already selected
      if (prevFilters[key] === value) {
        const newFilters = { ...prevFilters };
        delete newFilters[key];
        return newFilters;
      }
      return { ...prevFilters, [key]: value };
    });
  }, [setFilters]);

  const styles = useMemo(() => StyleSheet.create({
    filterChipsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL,
      marginBottom: SPACING_MEDIUM,
    },
    filterChip: {
      // Add any specific chip styling here
    },
  }), [theme]);

  return (
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
        selected={filters.isRoot === true}
        onPress={() => handleFilterChange('isRoot', true)}
        style={styles.filterChip}
      >
        {t('memberSearch.filter.isRootMember')}
      </Chip>
    </View>
  );
};


export default function MemberSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  // Define useStore function for usePaginatedSearch
  const useStore = useCallback(() => {
    const { members, loading, error, hasMore, page: currentPage, fetchMembers, reset, setError } = usePublicMemberStore();
    return {
      items: members,
      loading,
      error,
      hasMore,
      page: currentPage,
      fetch: async (query: SearchPublicMembersQuery, isLoadMore: boolean) => {
        if (!currentFamilyId) {
          setError(t('memberSearch.errors.noFamilyId'));
          return null;
        }
        return fetchMembers({ ...query, familyId: currentFamilyId }, isLoadMore);
      },
      reset,
      setError,
    } as ZustandPaginatedStore<MemberListDto, SearchPublicMembersQuery>;
  }, [currentFamilyId, t]);

  const renderMemberItem = useCallback(({ item }: { item: MemberListDto }) => (
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
  ), [router, t, theme]);

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM, // Apply padding to PaginatedSearchList's containerStyle
    },
    memberCard: {
      marginBottom: SPACING_MEDIUM,
      marginHorizontal: 1,
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
    memberDetailsChips: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      marginLeft: -SPACING_MEDIUM,
    },
    detailChip: {
      backgroundColor: 'transparent',
      paddingHorizontal: 0,
      borderWidth: 0,
    },
  }), [theme]);

  return (
    <View style={styles.safeArea}>
      <Appbar.Header>
        <Appbar.Content title={t('familyDetail.tab.members')} />
      </Appbar.Header>
      <PaginatedSearchList<MemberListDto, SearchPublicMembersQuery>
        useStore={useStore}
        searchOptions={{
          initialQuery: { familyId: '', page: 1, itemsPerPage: 10, searchTerm: '', gender: undefined, isRoot: undefined },
          externalDependencies: [currentFamilyId],
        }}
        renderItem={renderMemberItem}
        keyExtractor={(item) => item.id}
        searchPlaceholder={t('memberSearch.placeholder')}
        containerStyle={styles.container}
        showFilterButton={true}
        FilterComponent={MemberFilterComponent}
      />
    </View>
  );
}
