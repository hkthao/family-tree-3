import React, { useState, useMemo, useCallback } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
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

export default function FamilyEventsScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);

  const [selectedDate, setSelectedDate] = useState('');
  const [loading, setLoading] = useState(false); // Assuming events might load
  const [error, setError] = useState<string | null>(null);

  // Mock marked dates for demonstration
  const markedDates = useMemo(() => {
    return {
      '2025-11-20': { selected: true, marked: true, selectedColor: theme.colors.primary },
      '2025-11-25': { marked: true, dotColor: theme.colors.error },
      '2025-12-01': { marked: true, dotColor: theme.colors.tertiary },
    };
  }, [theme.colors.primary, theme.colors.error, theme.colors.tertiary]);

  const onDayPress = useCallback((day: any) => {
    setSelectedDate(day.dateString);
    // In a real app, you would fetch events for this day
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
      <Appbar.Header style={styles.appbar}>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('familyDetail.tab.events')} />
      </Appbar.Header>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {loading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator animating size="large" color={theme.colors.primary} />
          </View>
        ) : error ? (
          <View style={styles.errorContainer}>
            <Text variant="titleMedium" style={{ color: theme.colors.error }}>{error}</Text>
          </View>
        ) : (
          <>
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
              {t('eventScreen.eventsFor')} {selectedDate || t('eventScreen.noDateSelected')} (Family ID: {currentFamilyId})
            </Text>
            {/* Mock event list for the selected day */}
            <Text style={styles.noEventsText}>{t('eventScreen.noEvents')}</Text>
          </>
        )}
      </ScrollView>
    </View>
  );
}