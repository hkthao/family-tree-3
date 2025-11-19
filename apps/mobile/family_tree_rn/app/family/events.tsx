import React, { useState, useCallback } from 'react';
import { Alert, StyleSheet, Text, View, TouchableOpacity, SectionList } from 'react-native';
import { Agenda, DateData, AgendaEntry, AgendaSchedule } from 'react-native-calendars';

interface EventItem extends AgendaEntry {
  name: string;
  height: number;
  day: string;
}

export default function FamilyEventsScreen() {
  const [items, setItems] = useState<AgendaSchedule>({});

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
    const color = isFirst ? 'black' : '#43515c';

    return (
      <TouchableOpacity
        style={[styles.item, { height: reservation.height }]}
        onPress={() => Alert.alert(reservation.name)}
      >
        <Text style={{ fontSize, color }}>{reservation.name}</Text>
      </TouchableOpacity>
    );
  }, []);

  const renderEmptyDate = useCallback(() => {
    return (
      <View style={styles.emptyDate}>
        <Text>Không có sự kiện nào vào ngày này!</Text>
      </View>
    );
  }, []);

  const rowHasChanged = useCallback((r1: AgendaEntry, r2: AgendaEntry) => {
    return r1.name !== r2.name;
  }, []);

  const timeToString = (time: number) => {
    const date = new Date(time);
    return date.toISOString().split('T')[0];
  };

  const renderList = useCallback((listProps: any) => {
    // Transform listProps.items (object of {date: [items]}) into sections array for SectionList
    const sections = Object.keys(listProps.items).map(date => ({
      title: date,
      data: listProps.items[date]
    }));

    return (
      <SectionList
        sections={sections}
        renderItem={({ item, section, index }) => renderItem(item, index === 0)} // Reuse existing renderItem
        keyExtractor={(item: AgendaEntry, index: number) => item.day + item.name + index}
        renderSectionHeader={({ section: { title } }) => (
          <Text style={styles.sectionHeader}>{title}</Text>
        )}
        // You might want to add other SectionList props like stickySectionHeadersEnabled
      />
    );
  }, [renderItem]); // Add renderItem to dependency array

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
        agendaDayTextColor: 'yellow',
        agendaDayNumColor: 'green',
        agendaTodayColor: 'red',
        agendaKnobColor: 'blue'
      }}
    />
  );
}

const styles = StyleSheet.create({
  item: {
    backgroundColor: 'white',
    flex: 1,
    borderRadius: 5,
    padding: 10,
    marginRight: 10,
    marginTop: 17,
  },
  emptyDate: {
    height: 15,
    flex: 1,
    paddingTop: 30,
    textAlign: 'center',
  },
  sectionHeader: {
    backgroundColor: '#f0f0f0',
    padding: 10,
    fontSize: 16,
    fontWeight: 'bold',
  },
});
