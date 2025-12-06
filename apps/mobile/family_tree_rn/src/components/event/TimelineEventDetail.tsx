import React, { useMemo } from 'react';
import { View, Text, StyleSheet, Image, FlatList } from 'react-native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';

import { EventDto, EventType } from '@/types';
import { SPACING_SMALL } from '@/constants/dimensions';
import MemberAvatarChip from '@/components/common/MemberAvatarChip';

interface TimelineEventDetailProps {
  event: EventDto;
}

const TimelineEventDetail: React.FC<TimelineEventDetailProps> = ({ event }) => {
  const { t } = useTranslation();
  const theme = useTheme();

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
    membersContainer: {
      marginTop: SPACING_SMALL * 2,
    },
    membersTitle: {
      color: theme.colors.onSurface,
      fontSize: 14,
      fontWeight: 'bold',
      marginBottom: SPACING_SMALL,
    },
    dateText: {
      color: theme.colors.secondary,
      fontSize: 12,
      marginTop: SPACING_SMALL / 2,
    },
    locationText: {
      color: theme.colors.onSurfaceVariant,
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
      {event.description && event.description !== t('common.noDescription') && (
        <Text style={styles.description}>{event.description}</Text>
      )}
      {event.location && (
        <Text style={styles.locationText}>{t('eventDetail.location')}: {event.location}</Text>
      )}

      {sortedRelatedMembers.length > 0 && (
        <View style={[styles.membersContainer, { flexDirection: 'row', flexWrap: 'wrap' }]}>
          {sortedRelatedMembers.map(item => (
            <MemberAvatarChip
              key={item.id}
              id={item.id}
              fullName={item.fullName}
              avatarUrl={item.avatarUrl}
            />
          ))}
        </View>
      )}
    </View>
  );
};

export default TimelineEventDetail;