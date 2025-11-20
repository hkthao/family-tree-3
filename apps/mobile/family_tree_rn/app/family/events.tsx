import { SPACING_MEDIUM } from '@/constants/dimensions';
import React, { useState, useCallback, useMemo, useRef, useEffect } from 'react';
import { StyleSheet, View, ScrollView } from 'react-native';
import { Agenda, DateData, AgendaEntry, AgendaSchedule } from 'react-native-calendars';
import { Divider, useTheme, ActivityIndicator, Text } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { usePublicEventStore } from '@/stores/usePublicEventStore';
import AgendaItem from '@/components/events/AgendaItem';
import { useFamilyStore } from '@/stores/useFamilyStore';
import type { EventDto, EventType } from '@/types/public.d';
import { debounce } from '@/utils/debounce';

interface EventItem extends AgendaEntry {
  id: string;
  name: string;
  height: number;
  day: string;
  type: EventType;
}

export default function FamilyEventsScreen() {
  const [items, setItems] = useState<AgendaSchedule>({});
  const theme = useTheme();
  const { t } = useTranslation();

  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);
  const { loading, error, fetchEvents } = usePublicEventStore();
  const [loadedMonths, setLoadedMonths] = useState<Set<string>>(new Set());
  const loadedMonthsRef = useRef(loadedMonths);

  useEffect(() => {
    loadedMonthsRef.current = loadedMonths;
  }, [loadedMonths]);

  const styles = useMemo(() => StyleSheet.create({
    emptyDate: {
      height: 15,
      flex: 1,
      paddingTop: 30,
      textAlign: 'center',
    },
    sectionHeader: {
      padding: SPACING_MEDIUM,
      fontSize: 16,
      fontWeight: 'bold',
    },
    sectionHeaderWrapper: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingVertical: 5,
      paddingHorizontal: SPACING_MEDIUM,
    },
    sectionHeaderDay: {
      fontSize: 14,
      fontWeight: '300', // Thin font
      marginRight: SPACING_MEDIUM,
      minWidth: 60, // Ensure enough space for day name
      color: theme.colors.onSurfaceVariant,
    },
    sectionHeaderDate: {
      fontSize: 16,
      fontWeight: 'bold',
    },
    sectionRow: {
      flexDirection: 'row',
      minHeight: 100, // Minimum height for a section row
      paddingVertical: SPACING_MEDIUM,
      backgroundColor: theme.colors.background
    },
    sectionLeftColumn: {
      width: 80, // Fixed width for the left column
      marginRight: SPACING_MEDIUM,
      alignItems: 'center',
      justifyContent: 'flex-start', // Align to top
      paddingTop: SPACING_MEDIUM,
    },
    sectionRightColumn: {
      flex: 1,
      justifyContent: 'flex-start', // Align to top
    },
    itemMonth: {
      fontSize: 25,
    },
    itemDayName: {
      fontSize: 14,
      fontWeight: '300',
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
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
    container: {
      flex: 1,
    },
  }), [theme]);

  const timeToString = (time: number) => {
    const date = new Date(time);
    return date.toISOString().split('T')[0];
  };

  const getDayName = (dateString: string) => {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = { weekday: 'long' };
    return new Intl.DateTimeFormat('vi-VN', options).format(date);
  };

  const loadItems = useMemo(() => debounce(async (day: DateData) => {
    if (!currentFamilyId) {
      return;
    }

    const monthString = new Date(day.timestamp).toISOString().slice(0, 7); // YYYY-MM
    if (loadedMonthsRef.current.has(monthString)) {
      return; // Already loaded for this month
    }

    const startDate = '2000-01-01'//timeToString(day.timestamp);
    const endDate = timeToString(day.timestamp + (30 * 24 * 60 * 60 * 1000)); // Load for a month

    const fetchedPaginatedEvents = await fetchEvents({ 
      familyId: currentFamilyId, 
      startDate, 
      endDate, 
      page: 1, 
      itemsPerPage: 100,
      sortBy :"startDate"
    });

    if (fetchedPaginatedEvents) {
      setLoadedMonths(prev => new Set(prev).add(monthString)); // Mark month as loaded
    }

    setItems((prevItems) => {
      const mergedItems = { ...prevItems };
      fetchedPaginatedEvents?.items?.forEach((event: EventDto) => {
        const eventDate = timeToString(new Date(event.startDate).getTime());
        if (!mergedItems[eventDate]) {
          mergedItems[eventDate] = [];
        }
        // Check for duplicates before adding
        if (!(mergedItems[eventDate] as EventItem[]).some(item => item.id === event.id)) {
          mergedItems[eventDate].push({
            id: event.id,
            name: event.name || t('eventDetail.noTitle'),
            height: 80, // Fixed height for now, can be dynamic
            day: eventDate,
            type: event.type,
          } as EventItem);
        }
      });
      return mergedItems;
    });
  }, 300), [currentFamilyId, fetchEvents, t]); // Removed loadedMonths from dependencies

  const renderEmptyDate = useCallback(() => {
    return (
      <View style={[styles.emptyDate, { backgroundColor: theme.colors.background }]}>
        <Text style={{ color: theme.colors.onBackground }}>{t('eventScreen.noEvents')}</Text>
      </View>
    );
  }, [theme.colors, t, styles.emptyDate]);

  const rowHasChanged = useCallback((r1: AgendaEntry, r2: AgendaEntry) => {
    return r1.name !== r2.name;
  }, []);

  const renderList = useCallback((listProps: any) => {
    const sections = Object.keys(listProps.items).map(date => ({
      title: date,
      data: listProps.items[date]
    }));

    return (
      <ScrollView
        showsVerticalScrollIndicator={false}
        style={{
          backgroundColor: theme.colors.background
        }}
      >
        {sections.map(section => {
          const date = new Date(section.title);
          const month = (date.getMonth() + 1).toString().padStart(2, '0'); // MM format
          const dayName = getDayName(section.title);
          return (
            <View key={section.title} style={[styles.sectionRow]}>
              <View style={styles.sectionLeftColumn}>
                <Text style={[styles.itemMonth, { color: theme.colors.onSurfaceVariant }]}>{month}</Text>
                <Text style={[styles.itemDayName, { color: theme.colors.onSurfaceVariant }]}>{dayName}</Text>
              </View>
              <View style={styles.sectionRightColumn}>
                {section.data.map((item: AgendaEntry, index: number) => (
                  <View key={item.day + item.name + index}>
                    <AgendaItem reservation={item as EventItem} isFirst={index === 0} />

                    {index === section.data.length - 1 && section.data.length > 1 && <Divider style={{ margin: SPACING_MEDIUM }} />}
                  </View>
                ))}
              </View>
            </View>
          );
        })}
      </ScrollView>
    );
  }, [theme.colors, styles]);

  if (error) {
    return (
      <View style={styles.container}>
        <View style={styles.errorContainer}>
          <Text variant="bodyMedium" style={styles.errorText}>
            {t('common.error_occurred')}: {error}
          </Text>
        </View>
      </View>
    );
  }

  return (
    <Agenda
      items={items}
      loadItemsForMonth={loadItems}
      selected={new Date().toISOString().split('T')[0]} // Set selected to today's date
      renderEmptyDate={renderEmptyDate}
      rowHasChanged={rowHasChanged}
      showClosingKnob={true}
      renderList={renderList}
      theme={{
        agendaDayTextColor: theme.colors.primary,
        agendaDayNumColor: theme.colors.primary,
        agendaTodayColor: theme.colors.tertiary,
        agendaKnobColor: theme.colors.primary,
        backgroundColor: theme.colors.background,
        calendarBackground: theme.colors.surface,
        dayTextColor: theme.colors.onSurface,
        textSectionTitleColor: theme.colors.onSurfaceVariant,
        textDisabledColor: theme.colors.outline,
        dotColor: theme.colors.primary,
        selectedDotColor: theme.colors.onPrimary,
        monthTextColor: theme.colors.onSurface,
        textDayFontFamily: 'monospace',
        textMonthFontFamily: 'monospace',
        textDayHeaderFontFamily: 'monospace',
        textDayFontSize: 16,
        textMonthFontSize: 16,
        textDayHeaderFontSize: 13
      }}
    />
  );
}

