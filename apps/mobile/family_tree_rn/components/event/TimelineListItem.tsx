import React, { useMemo } from 'react';
import { View, StyleSheet, TextStyle, ViewStyle } from 'react-native';
import { Text, useTheme } from 'react-native-paper';
import { TimelineEventDetail } from '@/components/events';
import { SPACING_SMALL, SPACING_MEDIUM } from '@/constants/dimensions';
import type { EventDto } from '@/types';
import { format } from 'date-fns';

interface TimelineListItemProps {
  item: EventDto;
  index: number;
  isFirst: boolean;
  isLast: boolean;
  // Props to replicate timeline style
  circleSize?: number;
  circleColor?: string;
  lineColor?: string;
  timeContainerStyle?: ViewStyle;
  timeStyle?: TextStyle;
  titleStyle?: TextStyle;
  descriptionStyle?: TextStyle;
  innerCircle?: 'none' | 'dot' | 'icon' | 'element';
}

const TimelineListItem: React.FC<TimelineListItemProps> = ({
  item,
  index,
  isFirst,
  isLast,
  circleSize = 20,
  circleColor,
  lineColor,
  timeContainerStyle,
  timeStyle,
  titleStyle,
  descriptionStyle,
  innerCircle = 'dot',
}) => {
  const theme = useTheme();

  const currentCircleColor = circleColor || theme.colors.primary;
  const currentLineColor = lineColor || theme.colors.primary;

  const formattedTime = useMemo(() => {
    return item.startDate ? format(new Date(item.startDate), 'dd/MM/yyyy') : '';
  }, [item.startDate]);

  const styles = useMemo(() => StyleSheet.create({
    rowContainer: {
      flexDirection: 'row',
      alignItems: 'flex-start', // Align items to the top of the row
    },
    timeColumn: {
      width: 100, // Fixed width for time
      alignItems: 'flex-end', // Align time text to the right
      paddingRight: SPACING_SMALL,
    },
    markerColumn: {
      width: circleSize, // Fixed width for the marker (circle + line)
      alignItems: 'center',
      marginRight: SPACING_MEDIUM,
    },
    timeText: {
      textAlign: 'right', // Align text right in its column
      color: theme.colors.onBackground,
      ...timeStyle,
    },
    circle: {
      width: circleSize,
      height: circleSize,
      borderRadius: circleSize / 2,
      backgroundColor: currentCircleColor,
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 1,
    },
    innerDot: {
      width: circleSize / 2,
      height: circleSize / 2,
      borderRadius: circleSize / 4,
      backgroundColor: theme.colors.onPrimary,
    },
    line: {
      width: 2,
      flex: 1,
      backgroundColor: currentLineColor,
    },
    contentColumn: {
      flex: 1,
    },
  }), [theme, circleSize, currentCircleColor, currentLineColor, timeStyle]);

  return (
    <View style={styles.rowContainer}>
      {/* Column 1: Time */}
      <View style={styles.timeColumn}>
        <Text style={styles.timeText}>{formattedTime}</Text>
      </View>

      {/* Column 2: Timeline Marker (Circle and Line) */}
      <View style={styles.markerColumn}>
        <View style={styles.circle}>
          {innerCircle === 'dot' && <View style={styles.innerDot} />}
        </View>
        {!isLast && <View style={styles.line} />}
      </View>

      {/* Column 3: Event Details */}
      <View style={styles.contentColumn}>
        <TimelineEventDetail event={item} />
      </View>
    </View>
  );
};

export default TimelineListItem;
