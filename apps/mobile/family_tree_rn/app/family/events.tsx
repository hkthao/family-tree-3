import React, { useState, useMemo, useCallback } from 'react';
import { View, StyleSheet, ScrollView, FlatList, RefreshControl, Pressable } from 'react-native';
import { Appbar, Text, useTheme, ActivityIndicator } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';
import { Calendar, LocaleConfig } from 'react-native-calendars';
import { useFamilyStore } from '../../stores/useFamilyStore';
import { SPACING_MEDIUM } from '@/constants/dimensions';

// Configure Vietnamese locale for react-native-calendars
LocaleConfig.locales['vi'] = {
  monthNames: [
    'Tháng Một',
    'Tháng Hai',
    'Tháng Ba',
    'Tháng Tư',
    'Tháng Năm',
    'Tháng Sáu',
    'Tháng Bảy',
    'Tháng Tám',
    'Tháng Chín',
    'Tháng Mười',
    'Tháng Mười Một',
    'Tháng Mười Hai',
  ],
  monthNamesShort: [
    'Thg 1',
    'Thg 2',
    'Thg 3',
    'Thg 4',
    'Thg 5',
    'Thg 6',
    'Thg 7',
    'Thg 8',
    'Thg 9',
    'Thg 10',
    'Thg 11',
    'Thg 12',
  ],
  dayNames: ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'],
  dayNamesShort: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
  today: 'Hôm nay',
};
LocaleConfig.defaultLocale = 'vi';

interface EventDetail {
  id: string;
  title: string;
  time: string;
  description?: string;
  color?: string;
}

const mockEventDetails: { [date: string]: EventDetail[] } = {
  '2025-11-20': [
    {
      id: 'e1',
      title: 'Sinh nhật ông nội',
      time: '10:00 AM',
      description: 'Tiệc sinh nhật tại nhà hàng ABC',
      color: '#FF0000',
    },
    {
      id: 'e2',
      title: 'Họp mặt gia đình',
      time: '02:00 PM',
      description: 'Họp mặt thường niên của gia đình',
      color: '#0000FF',
    },
  ],
  '2025-11-25': [
    {
      id: 'e3',
      title: 'Giỗ tổ Hùng Vương',
      time: '09:00 AM',
      description: 'Lễ giỗ tổ tại đền Hùng',
      color: '#00FF00',
    },
  ],
  '2025-12-01': [
    {
      id: 'e4',
      title: 'Đám cưới cô út',
      time: '11:00 AM',
      description: 'Lễ cưới tại trung tâm hội nghị XYZ',
      color: '#FFA500',
    },
  ],
};

const getEventsForDate = (dateString: string): EventDetail[] => {
  return mockEventDetails[dateString] || [];
};

export default function FamilyEventsScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const initialSelectedDate = useMemo(() => {
    const today = new Date().toISOString().split('T')[0];
    const firstEventDate = Object.keys(mockEventDetails).sort()[0];
    return firstEventDate || today;
  }, []);

  const [selectedDate, setSelectedDate] = useState(initialSelectedDate);
  const [eventsForSelectedDate, setEventsForSelectedDate] = useState<EventDetail[]>(() => getEventsForDate(initialSelectedDate));
  const [loading, setLoading] = useState(false); // Assuming events might load
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    // Simulate fetching data
    await new Promise(resolve => setTimeout(resolve, 1000));
    setEventsForSelectedDate(getEventsForDate(selectedDate)); // Re-fetch events for selected date
    setRefreshing(false);
  }, [selectedDate]);

  const markedDates = useMemo(() => {
    const dates: { [key: string]: any } = {};
    Object.keys(mockEventDetails).forEach((date) => {
      dates[date] = {
        marked: true,
        dotColor: theme.colors.primary, // Default dot color
        ...(date === selectedDate && { selected: true, selectedColor: theme.colors.primary }),
      };
    });
    // Ensure the selected date is always marked, even if it has no events
    if (selectedDate && !dates[selectedDate]) {
      dates[selectedDate] = { selected: true, selectedColor: theme.colors.primary };
    }
    return dates;
  }, [mockEventDetails, selectedDate, theme.colors.primary]);

  const onDayPress = useCallback((day: any) => {
    setSelectedDate(day.dateString);
    setEventsForSelectedDate(getEventsForDate(day.dateString));
    console.log('Selected day', day);
  }, []);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
    },
    appbar: {
    },
    content: {
      flex: 1,
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
    calendarContainer: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
      overflow: 'hidden', // Ensures border radius is applied to children
    },
    eventListTitle: {
      marginBottom: SPACING_MEDIUM,
    },
    noEventsText: {
      textAlign: 'center',
      color: theme.colors.onSurfaceVariant,
    },
    eventItem: {
      padding: SPACING_MEDIUM,
      marginBottom: SPACING_MEDIUM,
      backgroundColor: theme.colors.surfaceVariant,
      borderRadius: theme.roundness,
      borderLeftWidth: 5,
      // borderLeftColor will be set dynamically
    },
    flatListContent: {
      padding: SPACING_MEDIUM, // Apply padding to the FlatList content
    },
  }), [theme]);

  if (!currentFamilyId) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium" style={{ color: theme.colors.error }}>{t('familyDetail.errors.noFamilyId')}</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {loading ? (
        <View style={styles.loadingContainer}>
          <ActivityIndicator animating size="large" color={theme.colors.primary} />
        </View>
      ) : error ? (
        <View style={styles.errorContainer}>
          <Text variant="titleMedium" style={{ color: theme.colors.error }}>{error}</Text>
        </View>
      ) : (
        <FlatList
          data={eventsForSelectedDate}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <Pressable onPress={() => router.push(`/event/${item.id}`)}>
              <View style={[styles.eventItem, { borderLeftColor: item.color || theme.colors.primary }]}>
                <Text variant="titleMedium">{item.title}</Text>
                <Text variant="bodyMedium">{item.time}</Text>
                {item.description && <Text variant="bodySmall" style={{ color: theme.colors.onSurfaceVariant }}>{item.description}</Text>}
              </View>
            </Pressable>
          )}
          ListHeaderComponent={
            <View style={styles.content}>
              <View style={styles.calendarContainer}>
                <Calendar
                  onDayPress={onDayPress}
                  markedDates={markedDates}
                  theme={{
                    backgroundColor: theme.colors.surface,
                    calendarBackground: theme.colors.surface,
                    textSectionTitleColor: theme.colors.onSurfaceVariant,
                    selectedDayBackgroundColor: theme.colors.primary,
                    selectedDayTextColor: theme.colors.onPrimary,
                    todayBackgroundColor: theme.colors.secondaryContainer,
                    todayTextColor: theme.colors.onSecondaryContainer,
                    dayTextColor: theme.colors.onSurface,
                    textDisabledColor: theme.colors.onSurfaceDisabled,
                    dotColor: theme.colors.primary,
                    selectedDotColor: theme.colors.onPrimary,
                    arrowColor: theme.colors.primary,
                    monthTextColor: theme.colors.onSurface,
                    textDayFontFamily: theme.fonts.bodyMedium.fontFamily,
                    textMonthFontFamily: theme.fonts.titleMedium.fontFamily,
                    textDayHeaderFontFamily: theme.fonts.labelMedium.fontFamily,
                    textDayFontSize: 16,
                    textMonthFontSize: 18,
                    textDayHeaderFontSize: 14,
                  }}
                />
              </View>
              <Text variant="titleMedium" style={styles.eventListTitle}>
                {t('eventScreen.eventsFor')} {selectedDate || t('eventScreen.noDateSelected')}
              </Text>
              {eventsForSelectedDate.length === 0 && (
                <Text style={styles.noEventsText}>{t('eventScreen.noEvents')}</Text>
              )}
            </View>
          }
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              colors={[theme.colors.primary]}
              tintColor={theme.colors.primary}
            />
          }
          contentContainerStyle={styles.flatListContent}
        />
      )}
    </View>
  );
}