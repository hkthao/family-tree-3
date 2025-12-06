import { MaterialCommunityIcons } from '@expo/vector-icons'; // Import MaterialCommunityIcons
import { EventDto, EventType } from '@/types'; // Ensure EventType is imported
import { SPACING_SMALL } from '@/constants/dimensions';
import MemberAvatarChip from '@/components/common/MemberAvatarChip';
import { useMemo, } from 'react';
import { useTranslation } from 'react-i18next';
import { View, StyleSheet } from 'react-native';
import { useTheme, Text } from 'react-native-paper';

interface TimelineEventDetailProps {
  event: EventDto;
}

const getIconForEventType = (eventType: EventType): string => {
  switch (eventType) {
    case EventType.Birth:
      return 'cake-variant';
    case EventType.Marriage:
      return 'ring';
    case EventType.Death:
      return 'cross';
    case EventType.Anniversary:
    case EventType.Other:
    default:
      return 'calendar-blank-outline';
  }
};

const TimelineEventDetail: React.FC<TimelineEventDetailProps> = ({ event }) => {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = useMemo(() => StyleSheet.create({
    container: {
      backgroundColor: theme.colors.surface,
      borderRadius: theme.roundness,
      marginTop: -15,
      paddingVertical: SPACING_SMALL, // Add padding to container
      position: 'relative', // Needed for absolute positioning of icon
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
    },
    eventTypeIcon: { // Style for the icon container
      position: 'absolute',
      top: SPACING_SMALL,
      right: SPACING_SMALL,
      backgroundColor: theme.colors.primary,
      borderRadius: 15, // Half of size for circle
      width: 30,
      height: 30,
      justifyContent: 'center',
      alignItems: 'center',
    },
  }), [theme]);

  const sortedRelatedMembers = useMemo(() => {
    return event.relatedMembers || [];
  }, [event.relatedMembers]);

  const eventTypeIconName = getIconForEventType(event.type);

  return (
    <View style={styles.container}>
      <View style={styles.eventTypeIcon}>
        <MaterialCommunityIcons name={eventTypeIconName as any} size={18} color={theme.colors.onPrimary} />
      </View>
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