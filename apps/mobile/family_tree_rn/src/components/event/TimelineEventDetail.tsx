import React, { useMemo } from 'react';
import { View, Text, StyleSheet, Image, FlatList } from 'react-native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';

import { EventDto, EventType } from '@/types';
import { SPACING_SMALL } from '@/constants/dimensions';

interface TimelineEventDetailProps {
  event: EventDto;
}

const TimelineEventDetail: React.FC<TimelineEventDetailProps> = ({ event }) => {
  const { t } = useTranslation();
  const theme = useTheme();

  const getEventTypeName = (eventType: EventType): string => {
    switch (eventType) {
      case EventType.Birth:
        return t('eventType.birth');
      case EventType.Marriage:
        return t('eventType.marriage');
      case EventType.Death:
        return t('eventType.death');
      case EventType.Anniversary:
        return t('eventType.anniversary');
      case EventType.Other:
      default:
        return t('eventType.other');
    }
  };

  const styles = useMemo(() => StyleSheet.create({
    container: {
      backgroundColor: theme.colors.surface,
      borderRadius: theme.roundness,
      marginTop: -15
    },
    title: {
      color: theme.colors.onSurface,
      fontSize: 16,
      fontWeight: 'bold',
    },
    description: {
      color: theme.colors.onSurfaceVariant,
      marginTop: SPACING_SMALL,
    },
    eventType: {
      color: theme.colors.primary,
      fontSize: 12,
      fontWeight: '600',
      marginTop: SPACING_SMALL,
    },
    membersContainer: {
      marginTop: SPACING_SMALL * 2,
    },
    membersTitle: {
      color: theme.colors.onSurface,
      fontSize: 14,
      fontWeight: 'bold',
      marginBottom: SPACING_SMALL,
    },
    memberItem: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: SPACING_SMALL,
    },
    avatar: {
      width: 32,
      height: 32,
      borderRadius: 16,
      marginRight: SPACING_SMALL,
      backgroundColor: theme.colors.surfaceVariant, // Placeholder background
    },
    memberName: {
      color: theme.colors.onSurface,
      fontSize: 14,
    },
    dateText: {
      color: theme.colors.secondary,
      fontSize: 12,
      marginTop: SPACING_SMALL / 2,
    }
  }), [theme]);

  const sortedRelatedMembers = useMemo(() => {
    return event.relatedMembers || [];
  }, [event.relatedMembers]);

  return (
    <View style={styles.container}>
      <Text style={styles.title}>{event.name || t('common.noTitle')}</Text>
      <Text style={styles.dateText}>
        {event.startDate ? format(new Date(event.startDate), 'dd/MM/yyyy HH:mm') : t('common.noDate')}
        {event.endDate && ` - ${format(new Date(event.endDate), 'dd/MM/yyyy HH:mm')}`}
      </Text>
      <Text style={styles.eventType}>{getEventTypeName(event.type)}</Text>
      <Text style={styles.description}>{event.description || t('common.noDescription')}</Text>

      {sortedRelatedMembers.length > 0 && (
        <View style={styles.membersContainer}>
          <Text style={styles.membersTitle}>{t('timeline.relatedMembers')}</Text>
          <FlatList
            data={sortedRelatedMembers}
            keyExtractor={(item) => item.id}
            renderItem={({ item }) => (
              <View style={styles.memberItem}>
                {/* TODO: Replace with a proper generic member avatar asset or use conditional logic for male/female/default */}
                <Image
                  source={item.avatarUrl ? { uri: item.avatarUrl } : { uri: 'https://via.placeholder.com/32' }}
                  style={styles.avatar}
                />
                <Text style={styles.memberName}>{item.fullName}</Text>
              </View>
            )}
            scrollEnabled={false} // Disable scrolling for FlatList inside scroll view
          />
        </View>
      )}
    </View>
  );
};

export default TimelineEventDetail;