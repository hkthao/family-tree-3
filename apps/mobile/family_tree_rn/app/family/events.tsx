import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import React, { useState, useCallback, useMemo } from 'react';
import { Alert, StyleSheet, Text, View, TouchableOpacity, ScrollView } from 'react-native';
import { Agenda, DateData, AgendaEntry, AgendaSchedule } from 'react-native-calendars';
import { Divider, Card, useTheme } from 'react-native-paper';

interface EventItem extends AgendaEntry {
  name: string;
  height: number;
  day: string;
}

export default function FamilyEventsScreen() {
  const [items, setItems] = useState<AgendaSchedule>({});
  const theme = useTheme();
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
  }), [theme]);

  const loadItems = useCallback((day: DateData) => {
    setTimeout(() => {
      setItems(prevItems => {
        const updatedItems: AgendaSchedule = { ...prevItems }; // Start with previous items, ensuring it's an object

        for (let i = -15; i < 85; i++) {
          const time = day.timestamp + i * 24 * 60 * 60 * 1000;
          const strTime = timeToString(time);

          if (!updatedItems[strTime]) {
            updatedItems[strTime] = [];
            const numItems = Math.floor(Math.random() * 3 + 1); // Simulate 1-3 events per day
            for (let j = 0; j < numItems; j++) {
              updatedItems[strTime].push({
                name: `Sự kiện gia đình vào ngày ${strTime} #${j + 1}`,
                height: Math.max(50, Math.floor(Math.random() * 150)),
                day: strTime,
              });
            }
          }
        }
        return updatedItems;
      });
    }, 1000);
  }, []); // Removed 'items' from dependency array as we use functional update

  const renderItem = useCallback((reservation: AgendaEntry, isFirst: boolean) => {
    const fontSize = isFirst ? 16 : 14;
    const color = isFirst ? theme.colors.onSurface : theme.colors.onSurfaceVariant;

    return (
      <Card
        style={[styles.item, { backgroundColor: theme.colors.surface }]}
        onPress={() => Alert.alert(reservation.name)}
        elevation={1} // Remove shadow
      >
        <Card.Content>
          <Text style={{ fontSize, color }}>{reservation.name}</Text>
        </Card.Content>
      </Card>
    );
  }, [theme.colors]);

  const renderEmptyDate = useCallback(() => {
    return (
      <View style={[styles.emptyDate, { backgroundColor: theme.colors.background }]}>
        <Text style={{ color: theme.colors.onBackground }}>Không có sự kiện nào vào ngày này!</Text>
      </View>
    );
  }, [theme.colors]);

  const rowHasChanged = useCallback((r1: AgendaEntry, r2: AgendaEntry) => {
    return r1.name !== r2.name;
  }, []);

  const timeToString = (time: number) => {
    const date = new Date(time);
    return date.toISOString().split('T')[0];
  };

  const getDayName = (dateString: string) => {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = { weekday: 'long' };
    return new Intl.DateTimeFormat('vi-VN', options).format(date);
  };

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
  }, [renderItem, theme.colors]);

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

