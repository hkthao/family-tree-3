import React from 'react';
import { Card, Text, useTheme } from 'react-native-paper';
import { useRouter } from 'expo-router';
import { AgendaEntry } from 'react-native-calendars';
import { SPACING_MEDIUM } from '@/constants/dimensions';

interface EventItem extends AgendaEntry {
  id: string;
  name: string;
  height: number;
  day: string;
}

interface AgendaItemProps {
  reservation: AgendaEntry;
  isFirst: boolean;
}

const AgendaItem = React.memo(({ reservation, isFirst }: AgendaItemProps) => {
  const theme = useTheme();
  const router = useRouter();

  const eventItem = reservation as EventItem;
  const fontSize = isFirst ? 16 : 14;
  const color = isFirst ? theme.colors.onSurface : theme.colors.onSurfaceVariant;

  return (
    <Card
      style={{
        borderRadius: theme.roundness,
        marginRight: SPACING_MEDIUM,
        marginBottom: SPACING_MEDIUM,
        backgroundColor: theme.colors.surface,
      }}
      onPress={() => router.push(`/event/${eventItem.id}`)}
      elevation={1}
    >
      <Card.Content>
        <Text style={{ fontSize, color }}>{eventItem.name}</Text>
      </Card.Content>
    </Card>
  );
});

export default AgendaItem;
