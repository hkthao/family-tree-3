import { SPACING_MEDIUM } from '@/constants/dimensions';
import React, { useState, useCallback, useMemo } from 'react';
import { StyleSheet, View, ScrollView } from 'react-native';
import { Agenda, DateData, AgendaEntry, AgendaSchedule } from 'react-native-calendars';
import { Divider, Card, useTheme, ActivityIndicator, Text } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useEventStore } from '@/stores/useEventStore';
import { useFamilyStore } from '@/stores/useFamilyStore';
import { useRouter } from 'expo-router';
import type { EventDto } from '@/types/public.d';

interface EventItem extends AgendaEntry {
  id: string;
  name: string;
  height: number;
  day: string;
}

export default function FamilyEventsScreen() {
  const [items, setItems] = useState<AgendaSchedule>({});
  const theme = useTheme();
  const { t } = useTranslation();
  const router = useRouter();

  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId);
  const { events, loading, error, fetchEvents } = useEventStore();

  const styles = useMemo(() => StyleSheet.create({
    item: {
      borderRadius: theme.roundness,
      marginRight: SPACING_MEDIUM,
      marginBottom: SPACING_MEDIUM,
    },
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

  const loadItems = useCallback(async (day: DateData) => {
    if (!currentFamilyId) {
      return;
    }

    const startDate = timeToString(day.timestamp);
    const endDate = timeToString(day.timestamp + (30 * 24 * 60 * 60 * 1000)); // Load for a month

    await fetchEvents({ familyId: currentFamilyId, startDate, endDate });

    const newItems: AgendaSchedule = {};
    events.forEach((event: EventDto) => {
      const eventDate = timeToString(new Date(event.startDate).getTime());
      if (!newItems[eventDate]) {
        newItems[eventDate] = [];
      }
      newItems[eventDate].push({
        id: event.id,
        name: event.title || t('eventDetail.noTitle'),
        height: 80, // Fixed height for now, can be dynamic
        day: eventDate,
      } as EventItem);
    });

    setItems(newItems);
  }, [currentFamilyId, fetchEvents, events, t]);

  const renderItem = useCallback((reservation: AgendaEntry, isFirst: boolean) => {
    const eventItem = reservation as EventItem;
    const fontSize = isFirst ? 16 : 14;
    const color = isFirst ? theme.colors.onSurface : theme.colors.onSurfaceVariant;

    return (
      <Card
        style={[styles.item, { backgroundColor: theme.colors.surface }]}
        onPress={() => router.push(`/event/${eventItem.id}`)} // Navigate to event detail
        elevation={1} // Remove shadow
      >
        <Card.Content>
          <Text style={{ fontSize, color }}>{eventItem.name}</Text>
        </Card.Content>
      </Card>
    );
  }, [theme.colors, router, styles.item]);

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
                    {renderItem(item, index === 0)}

                    {index === section.data.length - 1 && section.data.length > 1 && <Divider style={{ margin: SPACING_MEDIUM }} />}
                  </View>
                ))}
              </View>
            </View>
          );
        })}
      </ScrollView>
    );
  }, [renderItem, theme.colors, styles]);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator animating size="large" color={theme.colors.primary} />
      </View>
    );
  }

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
      renderItem={renderItem}
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

