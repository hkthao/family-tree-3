import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  StyleSheet,
  FlatList,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Text, Card, Title, Paragraph, Avatar, IconButton, Searchbar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { PaperTheme } from '@/constants/theme';
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';

// Define a type for Family data (simplified from backend/src/Domain/Entities/Family.cs)
interface Family {
  id: string;
  name: string;
  code: string;
  description?: string;
  avatarUrl?: string;
  totalMembers: number;
  totalGenerations: number;
  visibility: string;
}

// Mock API call function (replace with actual API service)
const fetchFamilies = async (
  query: string,
  page: number,
  pageSize: number,
  signal?: AbortSignal
): Promise<{ data: Family[]; totalCount: number }> => {
  // Simulate API delay
  await new Promise((resolve) => setTimeout(resolve, 1000));

  const allFamilies: Family[] = [
    {
      id: '1',
      name: 'Gia đình Nguyễn',
      code: 'GDN001',
      description: 'Gia đình lớn ở Hà Nội',
      avatarUrl: 'https://picsum.photos/seed/family1/100/100',
      totalMembers: 50,
      totalGenerations: 5,
      visibility: 'Public',
    },
    {
      id: '2',
      name: 'Họ Trần',
      code: 'HTC002',
      description: 'Họ Trần ở Huế',
      avatarUrl: 'https://picsum.photos/seed/family2/100/100',
      totalMembers: 120,
      totalGenerations: 8,
      visibility: 'Public',
    },
    {
      id: '3',
      name: 'Gia đình Lê',
      code: 'GDL003',
      description: 'Gia đình nhỏ ở Sài Gòn',
      avatarUrl: 'https://picsum.photos/seed/family3/100/100',
      totalMembers: 15,
      totalGenerations: 3,
      visibility: 'Private',
    },
    {
      id: '4',
      name: 'Họ Phạm',
      code: 'HPM004',
      description: 'Họ Phạm ở Đà Nẵng',
      avatarUrl: 'https://picsum.photos/seed/family4/100/100',
      totalMembers: 80,
      totalGenerations: 6,
      visibility: 'Public',
    },
    {
      id: '5',
      name: 'Gia đình Hoàng',
      code: 'GDH005',
      description: 'Gia đình Hoàng ở Cần Thơ',
      avatarUrl: 'https://picsum.photos/seed/family5/100/100',
      totalMembers: 30,
      totalGenerations: 4,
      visibility: 'Private',
    },
    {
      id: '6',
      name: 'Họ Đỗ',
      code: 'HD006',
      description: 'Họ Đỗ ở Hải Phòng',
      avatarUrl: 'https://picsum.photos/seed/family6/100/100',
      totalMembers: 90,
      totalGenerations: 7,
      visibility: 'Public',
    },
    {
      id: '7',
      name: 'Gia đình Bùi',
      code: 'GDB007',
      description: 'Gia đình Bùi ở Vũng Tàu',
      avatarUrl: 'https://picsum.photos/seed/family7/100/100',
      totalMembers: 25,
      totalGenerations: 3,
      visibility: 'Private',
    },
    {
      id: '8',
      name: 'Họ Ngô',
      code: 'HNG008',
      description: 'Họ Ngô ở Nha Trang',
      avatarUrl: 'https://picsum.photos/seed/family8/100/100',
      totalMembers: 60,
      totalGenerations: 5,
      visibility: 'Public',
    },
  ];

  const filteredFamilies = allFamilies.filter(
    (f) =>
      f.name.toLowerCase().includes(query.toLowerCase()) ||
      f.code.toLowerCase().includes(query.toLowerCase()) ||
      f.description?.toLowerCase().includes(query.toLowerCase())
  );

  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedFamilies = filteredFamilies.slice(startIndex, endIndex);

  return { data: paginatedFamilies, totalCount: filteredFamilies.length };
};

const PAGE_SIZE = 10;

export default function FamilySearchScreen() {
  const { t } = useTranslation();
  const [searchQuery, setSearchQuery] = useState('');
  const [families, setFamilies] = useState<Family[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);

  const loadFamilies = useCallback(
    async (currentPage: number, isRefreshing: boolean = false) => {
      if (loading) return;

      setLoading(true);
      setError(null);

      try {
        const controller = new AbortController();
        const { data, totalCount: newTotalCount } = await fetchFamilies(
          searchQuery,
          currentPage,
          PAGE_SIZE,
          controller.signal
        );

        if (isRefreshing) {
          setFamilies(data);
        } else {
          setFamilies((prevFamilies) => [...prevFamilies, ...data]);
        }
        setTotalCount(newTotalCount);
        setHasMore(families.length + data.length < newTotalCount);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError('An unknown error occurred');
        }
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [loading, searchQuery, families.length]
  );

  useEffect(() => {
    // Reset and load first page when search query changes
    setFamilies([]);
    setPage(1);
    setHasMore(true);
    loadFamilies(1);
  }, [searchQuery, loadFamilies]);

  const handleRefresh = useCallback(() => {
    setRefreshing(true);
    setPage(1);
    setFamilies([]);
    setHasMore(true);
    loadFamilies(1, true);
  }, [loadFamilies]);

  const handleLoadMore = useCallback(() => {
    if (!loading && hasMore) {
      setPage((prevPage) => prevPage + 1);
      loadFamilies(page + 1);
    }
  }, [loading, hasMore, page, loadFamilies]);

  const renderFooter = () => {
    if (!loading) return null;
    return (
      <View style={styles.footer}>
        <ActivityIndicator animating size="small" color={PaperTheme.colors.primary} />
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

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
      <Searchbar
        placeholder={t('search.placeholder')}
        onChangeText={setSearchQuery}
        value={searchQuery}
        style={styles.searchbar}
        inputStyle={styles.searchbarInput}
        iconColor={PaperTheme.colors.onSurfaceVariant}
        placeholderTextColor={PaperTheme.colors.onSurfaceVariant}
        clearIcon={searchQuery.length > 0 ? () => (
          <IconButton
            icon="close-circle"
            iconColor={PaperTheme.colors.onSurfaceVariant}
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
        data={families}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <Card style={styles.familyCard} onPress={() => console.log('View family', item.id)}>
            <Card.Content style={styles.cardContent}>
              <Avatar.Image size={48} source={{ uri: item.avatarUrl }} style={styles.avatar} />
              <View style={styles.cardText}>
                <Title>{item.name}</Title>
                <Paragraph>{item.description}</Paragraph>
                <View style={styles.detailsRow}>
                  <Text variant="bodySmall">{t('family.members')}: {item.totalMembers}</Text>
                  <Text variant="bodySmall">{t('family.generations')}: {item.totalGenerations}</Text>
                  <Text variant="bodySmall">{t('family.visibility')}: {t(`family.visibility.${item.visibility.toLowerCase()}`)}</Text>
                </View>
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
            colors={[PaperTheme.colors.primary]}
            tintColor={PaperTheme.colors.primary}
          />
        }
        contentContainerStyle={families.length === 0 && !loading && !error ? styles.flatListEmpty : styles.flatListContent}
      />
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: PaperTheme.colors.background,
  },
  container: {
    flex: 1,
    backgroundColor: PaperTheme.colors.background,
    padding: SPACING_MEDIUM,
  },
  searchbar: {
    backgroundColor: PaperTheme.colors.surface,
    borderRadius: 8,
    marginBottom: SPACING_MEDIUM,
    elevation: 2, // Shadow for Android
    shadowColor: '#000', // Shadow for iOS
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.2,
    shadowRadius: 1.41,
  },
  searchbarInput: {
    color: PaperTheme.colors.onSurface,
  },
  errorContainer: {
    padding: SPACING_MEDIUM,
    backgroundColor: PaperTheme.colors.errorContainer,
    borderRadius: 8,
    marginBottom: SPACING_MEDIUM,
  },
  errorText: {
    color: PaperTheme.colors.onErrorContainer,
    textAlign: 'center',
  },
  flatListContent: {
    paddingBottom: SPACING_LARGE, // Ensure space at the bottom
  },
  flatListEmpty: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  familyCard: {
    marginBottom: SPACING_MEDIUM,
    borderRadius: 8,
    elevation: 1,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 0.5 },
    shadowOpacity: 0.1,
    shadowRadius: 1,
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
});
