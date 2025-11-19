import React, { useEffect, useState, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { Appbar, Text, useTheme, Card, ActivityIndicator, Chip, Avatar } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';

interface EventDetail {
  id: string;
  title: string;
  eventType: string;
  startDate: string;
  endDate?: string;
  time: string;
  location?: string;
  description?: string;
  color?: string;
  relatedMembers?: string[];
}

const mockEventDetailsData: EventDetail[] = [
  {
    id: 'e1',
    title: 'Sinh nhật ông nội',
    eventType: 'Sinh nhật',
    startDate: '2025-11-20',
    endDate: '2025-11-20',
    time: '10:00 AM',
    location: 'Nhà hàng ABC',
    description: 'Tiệc sinh nhật tại nhà hàng ABC với sự tham gia của toàn thể gia đình và bạn bè thân thiết. Sẽ có nhiều món ăn ngon và quà tặng bất ngờ.',
    color: '#FF0000',
    relatedMembers: ['Nguyễn Văn A', 'Trần Thị B'],
  },
  {
    id: 'e2',
    title: 'Họp mặt gia đình',
    eventType: 'Họp mặt',
    startDate: '2025-11-20',
    endDate: '2025-11-20',
    time: '02:00 PM',
    location: 'Nhà riêng',
    description: 'Họp mặt thường niên của gia đình để bàn bạc về các kế hoạch sắp tới và ôn lại kỷ niệm xưa. Mọi thành viên đều được khuyến khích tham gia.',
    color: '#0000FF',
    relatedMembers: ['Nguyễn Văn A', 'Trần Thị B', 'Lê Văn C'],
  },
  {
    id: 'e3',
    title: 'Giỗ tổ Hùng Vương',
    eventType: 'Lễ hội',
    startDate: '2025-11-25',
    endDate: '2025-11-25',
    time: '09:00 AM',
    location: 'Đền Hùng',
    description: 'Lễ giỗ tổ tại đền Hùng, tưởng nhớ các vị vua Hùng đã có công dựng nước. Nghi lễ trang trọng và linh thiêng.',
    color: '#00FF00',
    relatedMembers: [],
  },
  {
    id: 'e4',
    title: 'Đám cưới cô út',
    eventType: 'Đám cưới',
    startDate: '2025-12-01',
    endDate: '2025-12-01',
    time: '11:00 AM',
    location: 'Trung tâm hội nghị XYZ',
    description: 'Lễ cưới của cô út tại trung tâm hội nghị XYZ. Hân hạnh đón tiếp quý khách đến chung vui cùng gia đình.',
    color: '#FFA500',
    relatedMembers: ['Lê Thị D', 'Phạm Văn E'],
  },
];

const fetchEventDetails = async (eventId: string): Promise<EventDetail | null> => {
  await new Promise(resolve => setTimeout(resolve, 500)); // Simulate API call
  return mockEventDetailsData.find(event => event.id === eventId) || null;
};

export default function EventDetailsScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();
  const { t } = useTranslation();
  const theme = useTheme();

  const [event, setEvent] = useState<EventDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      const loadEventDetails = async () => {
        setLoading(true);
        setError(null);
        try {
          const eventId = Array.isArray(id) ? id[0] : id;
          const data = await fetchEventDetails(eventId);
          if (data) {
            setEvent(data);
          } else {
            setError(t('eventDetail.errors.notFound'));
          }
        } catch (err) {
          if (err instanceof Error) {
            setError(err.message);
          } else {
            setError(t('eventDetail.errors.failedToLoad'));
          }
        } finally {
          setLoading(false);
        }
      };
      loadEventDetails();
    }
  }, [id, t]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
    },
    appbar: {
    },
    content: {
      padding: SPACING_MEDIUM,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    errorContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: SPACING_MEDIUM,
    },
    errorText: {
      color: theme.colors.onErrorContainer,
      textAlign: 'center',
    },
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    cardContent: {
      // Add specific styling for event details if needed
    },
    profileCardContent: {
      flexDirection: 'column',
      alignItems: 'center',
      paddingBottom: SPACING_MEDIUM,
    },

    titleText: {
      marginBottom: SPACING_SMALL,
      textAlign: 'center',
    },
    eventTypeText: {
      marginBottom: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    dateText: {
      marginBottom: SPACING_SMALL,
      textAlign: 'center',
    },
    locationText: {
      marginBottom: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    timeText: {
      marginBottom: SPACING_SMALL,
      color: theme.colors.onSurfaceVariant,
      textAlign: 'center',
    },
    chipsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: SPACING_SMALL,
      justifyContent: 'center', // Center chips
      marginTop: SPACING_SMALL,
      marginBottom: SPACING_SMALL,
    },
    chip: {
      marginHorizontal: SPACING_SMALL / 2, // Add some horizontal margin for spacing
    },
    listItemTitle: {
      fontWeight: 'bold',
    },
  }), [theme]);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator animating size="large" color={theme.colors.primary} />
      </View>
    );
  }

  if (error) {
    return (
      <View style={{ flex: 1 }}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => router.back()} />
          <Appbar.Content title={t('eventDetail.title')} />
        </Appbar.Header>
        <View style={styles.errorContainer}>
          <Text variant="bodyMedium" style={styles.errorText}>
            {t('common.error_occurred')}: {error}
          </Text>
        </View>
      </View>
    );
  }

  if (!event) {
    return (
      <View style={{ flex: 1 }}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => router.back()} />
          <Appbar.Content title={t('eventDetail.title')} />
        </Appbar.Header>
        <View style={styles.errorContainer}>
          <Text variant="bodyMedium" style={styles.errorText}>
            {t('eventDetail.errors.dataNotAvailable')}
          </Text>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Appbar.Header style={styles.appbar}>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={event.title || t('eventDetail.title')} />
      </Appbar.Header>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* First Card: Key Event Information */}
        <Card style={styles.card}>
          <Card.Content style={styles.profileCardContent}>
            <Avatar.Icon icon="calendar-month" size={80} color={theme.colors.onPrimary}  />
            <Text variant="headlineSmall" style={styles.titleText}>{event.title}</Text>
            <Text variant="bodyMedium" >{event.description}</Text>
            <View style={styles.chipsContainer}>
              <Chip icon="tag" compact={true} style={styles.chip}>{event.eventType}</Chip>
              <Chip icon="calendar-start" compact={true} style={styles.chip}>{event.startDate}</Chip>
              {event.endDate && <Chip icon="calendar-end" compact={true} style={styles.chip}>{event.endDate}</Chip>}
              {event.location && (
                <Chip icon="map-marker" compact={true} style={styles.chip}>{event.location}</Chip>
              )}
              <Chip icon="clock-outline" compact={true} style={styles.chip}>{event.time}</Chip>

              {event.relatedMembers?.map((member, index) => (
                <Chip key={index} compact={true} icon="account-outline">{member}</Chip>
              ))}
            </View>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
}
