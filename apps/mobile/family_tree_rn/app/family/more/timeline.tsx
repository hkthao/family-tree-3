import React, { useState, useEffect, useMemo } from 'react';
import { View, StyleSheet, ActivityIndicator, Text, RefreshControl } from 'react-native'; // Added ActivityIndicator, Text for renderFooter
import Timeline from 'react-native-timeline-flatlist';
import { useTheme, Searchbar } from 'react-native-paper'; // Import useTheme, Searchbar

import { SPACING_MEDIUM } from '@/constants/dimensions';

interface TimelineData {
  time: string;
  title: string;
  description: string;
  lineColor?: string;
  circleColor?: string;
}

const DUMMY_DATA: TimelineData[] = [
  { time: '09:00', title: 'Archery Training', description: 'The Beginner Archery and Beginner Crossbow course does not require you to bring any equipment, since everything you need will be provided for the course. ' },
  { time: '10:45', title: 'Play Badminton', description: 'Badminton is a racquet sport played using racquets to hit a shuttlecock across a net.' },
  { time: '12:00', title: 'Lunch', description: 'Lunch time' }, // Removed icon, added description to fit TimelineData
  { time: '14:00', title: 'Watch Soccer', description: 'Team sport played between two teams of eleven players with a spherical ball. ' },
  { time: '16:30', title: 'Go to Fitness center', description: 'Look out for the Best Gym & Fitness Centers around me :)' },
];

const TimelineScreen: React.FC = () => {
  const theme = useTheme(); // Get theme from react-native-paper
  const [data, setData] = useState<TimelineData[]>(DUMMY_DATA);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [waiting, setWaiting] = useState(false);

  // Search state and debounce logic
  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState('');

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, 400); // 400ms debounce

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery]);

  // Filter data based on search query
  const filteredData = useMemo(() => {
    if (!debouncedSearchQuery) {
      return data;
    }
    const lowercasedQuery = debouncedSearchQuery.toLowerCase();
    return data.filter(item =>
      item.title.toLowerCase().includes(lowercasedQuery) ||
      item.description.toLowerCase().includes(lowercasedQuery)
    );
  }, [data, debouncedSearchQuery]);


  const onRefresh = () => {
    setIsRefreshing(true);
    // Simulate fetching new data
    setTimeout(() => {
      setData(DUMMY_DATA); // Reset to initial data
      setIsRefreshing(false);
    }, 2000);
  };

  const onEndReached = () => {
    if (!waiting) {
      setWaiting(true);
      // Simulate loading more data
      setTimeout(() => {
        const newData: TimelineData[] = [
          { time: '18:00', title: 'Load more data', description: 'append event at bottom of timeline' },
          { time: '18:00', title: 'Load more data', description: 'append event at bottom of timeline' },
          { time: '18:00', title: 'Load more data', description: 'append event at bottom of timeline' },
          { time: '18:00', title: 'Load more data', description: 'append event at bottom of timeline' },
          { time: '18:00', title: 'Load more data', description: 'append event at bottom of timeline' }
        ];
        setData([...data, ...newData]);
        setWaiting(false);
      }, 2000);
    }
  };

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
      backgroundColor: theme.colors.background,
    },
    searchFilterContainer: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: theme.colors.surfaceVariant, // Set background color for the container
      borderRadius: theme.roundness, // Match Searchbar's border radius
    },
    searchbar: {
      flex: 1,
      borderRadius: theme.roundness,
      backgroundColor: 'transparent', // Make Searchbar background transparent
    },
    list: {
      flex: 1,
      marginTop: SPACING_MEDIUM, // Add some top margin after searchbar
    },
    footer: {
      marginTop: 10,
      marginBottom: 20,
    },
    footerText: {
      textAlign: 'center',
      color: theme.colors.onSurfaceVariant,
    }
  }), [theme]);

  const renderFooter = () => {
    if (!waiting) return <Text style={styles.footerText}>~</Text>;
    return (
      <View style={styles.footer}>
        <ActivityIndicator />
      </View>
    );
  };

  return (
    <View style={styles.container}>
      <View style={styles.searchFilterContainer}>
        <Searchbar
          placeholder="Tìm kiếm sự kiện" // Placeholder text
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
          clearIcon={searchQuery.length > 0 ? 'close-circle' : undefined}
          onClearIconPress={() => setSearchQuery('')}
        />
      </View>
      <Timeline
        style={styles.list}
        data={filteredData} // Use filtered data
        circleSize={20}
        circleColor={theme.colors.primary}
        lineColor={theme.colors.primary}
        timeContainerStyle={{ minWidth: 52, marginTop: -5 }} // Changed to -5
        timeStyle={{ textAlign: 'center', backgroundColor: theme.colors.error, color: theme.colors.onError, padding: 5, borderRadius: theme.roundness }}
        titleStyle={{ color: theme.colors.onSurface }} // Removed marginTop
        descriptionStyle={{ color: theme.colors.onSurfaceVariant }}
        options={
          {
            data: filteredData, // Use filtered data for options too
            style: { paddingTop: 5 },
            refreshControl: (
              <RefreshControl
                refreshing={isRefreshing}
                onRefresh={onRefresh}
              />
            ),
            onEndReached: onEndReached,
            renderFooter: renderFooter,
          } as any
        }
        innerCircle={'dot'}
      />
    </View>
  );
};

export default TimelineScreen;
