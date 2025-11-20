import React from 'react';
import { Card, Text, useTheme, Chip } from 'react-native-paper';
import { useRouter } from 'expo-router';
import { AgendaEntry } from 'react-native-calendars';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { EventType } from '@/types/public.d'; // Import EventType
import { useTranslation } from 'react-i18next'; // Import useTranslation
import { TouchableOpacity } from 'react-native';

interface EventItem extends AgendaEntry {
  id: string;
  name: string;
  height: number;
  day: string;
  type: EventType; // Add eventType here
}

interface AgendaItemProps {
  reservation: EventItem;
  isFirst: boolean;
}

const eventTypeIconMap: Record<EventType, string> = {
  [EventType.Birth]: 'cake-variant',
  [EventType.Death]: 'cross',
  [EventType.Marriage]: 'ring',
  [EventType.Anniversary]: 'calendar-heart',
  [EventType.Other]: 'information',
};

const AgendaItem = React.memo(({ reservation, isFirst }: AgendaItemProps) => {
  const theme = useTheme();
  const router = useRouter();
  const { t } = useTranslation(); // Initialize useTranslation

  const eventTypeStringMap: Record<EventType, string> = {
    [EventType.Birth]: t('eventType.birth'),
    [EventType.Death]: t('eventType.death'),
    [EventType.Marriage]: t('eventType.marriage'),
    [EventType.Anniversary]: t('eventType.anniversary'),
    [EventType.Other]: t('eventType.other'),
  };

  const eventItem = reservation as EventItem;
  const fontSize = isFirst ? 16 : 14;
  const color = isFirst ? theme.colors.onSurface : theme.colors.onSurfaceVariant;

  return (
    <TouchableOpacity
      style={{
        borderRadius: theme.roundness,
        marginRight: SPACING_MEDIUM,
        padding: SPACING_MEDIUM,
        backgroundColor: "transparent",
        borderWidth: 0.5,
        borderColor: theme.colors.onSurface
      }}
      onPress={() => router.push(`/event/${eventItem.id}`)}
    >
      <Card.Content>
        <Text style={{ fontSize, color }}>{eventItem.name}</Text>
        <Chip
          icon={eventTypeIconMap[eventItem.type]}
          style={{
            marginTop: SPACING_MEDIUM / 2,
            alignSelf: 'flex-end',
            backgroundColor: "transparent"
          }}
          textStyle={{ color: theme.colors.onPrimaryContainer }}
        >
          {eventTypeStringMap[eventItem.type]}
        </Chip>
      </Card.Content>
    </TouchableOpacity>
  );
});

AgendaItem.displayName = 'AgendaItem';

export default AgendaItem;
