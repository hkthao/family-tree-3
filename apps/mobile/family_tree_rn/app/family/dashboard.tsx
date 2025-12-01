import React, { useEffect, useMemo, useState } from 'react';
import { View, StyleSheet, ScrollView, Dimensions } from 'react-native';
import { Text, useTheme, ActivityIndicator, Card, Divider } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { useFamilyStore } from '@/stores/useFamilyStore';
import { usePublicFamilyStore } from '@/stores/usePublicFamilyStore';
import { PieChart, BarChart } from 'react-native-chart-kit';
import { MaterialCommunityIcons } from '@expo/vector-icons'; // Import MaterialCommunityIcons
import ProfileCard from '@/components/family/ProfileCard'; // Import ProfileCard
import DetailedInfoCard from '@/components/family/DetailedInfoCard'; // Import DetailedInfoCard
import MetricCard from '@/components/common/MetricCard'; // Import MetricCard

export default function FamilyDashboardScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const { family, loading, error, getFamilyById } = usePublicFamilyStore();

  // Mock Data for now
  const [dashboardData, setDashboardData] = useState({
    totalMembers: 120,
    totalRelationships: 350,
    totalGenerations: 5,
    averageAge: 45,
    livingMembers: 80,
    deceasedMembers: 40,
    genderDistribution: [
      {
        name: t('common.male'),
        population: 60,
        color: theme.colors.primary,
        legendFontColor: theme.colors.onSurface,
        legendFontSize: 15,
      },
      {
        name: t('common.female'),
        population: 55,
        color: theme.colors.secondary,
        legendFontColor: theme.colors.onSurface,
        legendFontSize: 15,
      },
      {
        name: t('common.other'),
        population: 5,
        color: theme.colors.tertiary,
        legendFontColor: theme.colors.onSurface,
        legendFontSize: 15,
      },
    ],
    membersByGeneration: [
      { generation: 'Gen 1', members: 10 },
      { generation: 'Gen 2', members: 25 },
      { generation: 'Gen 3', members: 40 },
      { generation: 'Gen 4', members: 30 },
      { generation: 'Gen 5', members: 15 },
    ],
  });

  useEffect(() => {
    const loadFamilyDetails = async () => {
      if (!currentFamilyId) {
        return;
      }
      await getFamilyById(currentFamilyId);
      // In a real scenario, you'd process the fetched 'family' data
      // to calculate these dashboard metrics. For now, we use mock data.
    };

    loadFamilyDetails();
  }, [currentFamilyId, getFamilyById]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
    },
    content: {
      padding: SPACING_MEDIUM,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    errorContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_MEDIUM,
    },
    cardsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      justifyContent: 'space-between',
      marginBottom: SPACING_MEDIUM,
    },
    metricCard: {
      width: '48%', // Approx 2 cards per row with some spacing
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
      elevation: 2,
    },
    metricCardContent: {
      alignItems: 'center',
      paddingVertical: SPACING_MEDIUM,
    },
    metricIcon: {
      marginBottom: SPACING_SMALL,
    },
    metricValue: {
      fontSize: 24,
      fontWeight: 'bold',
      color: theme.colors.primary,
      marginBottom: SPACING_SMALL / 2,
    },
    metricLabel: {
      fontSize: 14,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    chartCard: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
      elevation: 2,
    },
    chartCardContent: {
      padding: SPACING_MEDIUM,
      paddingBottom: SPACING_MEDIUM * 2, // Increased padding to prevent overlap
    },
    chartTitle: {
      marginBottom: SPACING_SMALL,
      fontWeight: 'bold',
      color: theme.colors.primary,
      textAlign: 'center',
    },
    chartContainer: {
      alignItems: 'center',
      paddingVertical: SPACING_MEDIUM, // Added vertical padding
    },
    // Styles for bar chart
    barChartLabel: {
      color: theme.colors.onSurface, // Make labels visible
    },
  }), [theme]);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator animating size="large" color={theme.colors.primary} />
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium" style={{ color: theme.colors.error }}>{error}</Text>
      </View>
    );
  }

  if (!family) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium">{t('common.error_occurred')}: {t('familyDetail.errors.dataNotAvailable')}</Text>
      </View>
    );
  }

  const screenWidth = Dimensions.get('window').width;

  const chartConfig = {
    backgroundGradientFrom: theme.colors.background,
    backgroundGradientTo: theme.colors.background,
    color: (opacity = 1) => `rgba(0, 0, 0, ${opacity})`, // Use a neutral color for lines/text
    labelColor: (opacity = 1) => theme.colors.onSurface, // Changed to use theme.colors.onSurface
    strokeWidth: 2,
    barPercentage: 0.5,
    useShadowColorFromDataset: false,
    fillShadowGradient: theme.colors.primary,
    fillShadowGradientOpacity: 0.5,
  };

  return (
    <View style={styles.container}>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Profile Card */}
        <ProfileCard family={family} />

        {/* Metric Cards */}
        <View style={styles.cardsContainer}>
          <MetricCard
            icon="account-multiple"
            value={dashboardData.totalMembers}
            label={t('familyDashboard.totalMembers')}
          />
          <MetricCard
            icon="link-variant"
            value={dashboardData.totalRelationships}
            label={t('familyDashboard.totalRelationships')}
          />
          <MetricCard
            icon="sitemap"
            value={dashboardData.totalGenerations}
            label={t('familyDashboard.totalGenerations')}
          />
          <MetricCard
            icon="account-clock"
            value={dashboardData.averageAge}
            label={t('familyDashboard.averageAge')}
          />
          <MetricCard
            icon="heart-plus-outline"
            value={dashboardData.livingMembers}
            label={t('familyDashboard.livingMembers')}
          />
          <MetricCard
            icon="grave-stone" // No direct outline alternative, keeping as is
            value={dashboardData.deceasedMembers}
            label={t('familyDashboard.deceasedMembers')}
          />
        </View>

        {/* Detailed Info Card */}
        <DetailedInfoCard family={family} />

        {/* Biểu đồ giới tính */}
        <Card style={styles.chartCard}>
          <Card.Content style={styles.chartCardContent}>
            <Text variant="titleMedium" style={styles.chartTitle}>
              {t('familyDashboard.genderRatio')}
            </Text>
            <View style={styles.chartContainer}>
              <PieChart
                data={dashboardData.genderDistribution}
                width={screenWidth - (SPACING_MEDIUM * 2)}
                height={220}
                chartConfig={chartConfig}
                accessor="population"
                backgroundColor="transparent"
                paddingLeft="15"
                // Removed center prop
                absolute
              />
            </View>
          </Card.Content>
        </Card>

        {/* Biểu đồ thành viên theo thế hệ */}
        <Card style={styles.chartCard}>
          <Card.Content style={styles.chartCardContent}>
            <Text variant="titleMedium" style={styles.chartTitle}>
              {t('familyDashboard.membersByGeneration')}
            </Text>
            <View style={styles.chartContainer}>
              <BarChart
                data={{
                  labels: dashboardData.membersByGeneration.map(item => item.generation),
                  datasets: [
                    {
                      data: dashboardData.membersByGeneration.map(item => item.members),
                    },
                  ],
                }}
                width={screenWidth - (SPACING_MEDIUM * 2)}
                height={220}
                yAxisLabel=""
                yAxisSuffix=""
                chartConfig={{
                  ...chartConfig,
                  color: (opacity = 1) => `rgba(26, 255, 146, ${opacity})`, // Example color
                  barPercentage: 0.7,
                  propsForLabels: {
                    fill: theme.colors.onSurface, // Explicitly set fill for labels
                  },
                }}
                verticalLabelRotation={30}
              />
            </View>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
}