import React, { useEffect, useState, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { Appbar, Text, useTheme, Card, Avatar, ActivityIndicator, Chip, List, Divider } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
// Removed SafeAreaView import
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { fetchMemberDetails, MemberDetail } from '../../data/mockFamilyData'; // Assuming MemberDetail interface and fetchMemberDetails function

export default function MemberDetailsScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();
  const { t } = useTranslation();
  const theme = useTheme();

  const [member, setMember] = useState<MemberDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      const loadMemberDetails = async () => {
        setLoading(true);
        setError(null);
        try {
          const memberId = Array.isArray(id) ? id[0] : id;
          const data = await fetchMemberDetails(memberId);
          if (data) {
            setMember(data);
          } else {
            setError(t('memberDetail.errors.notFound'));
          }
        } catch (err) {
          if (err instanceof Error) {
            setError(err.message);
          } else {
            setError(t('memberDetail.errors.failedToLoad'));
          }
        } finally {
          setLoading(false);
        }
      };
      loadMemberDetails();
    }
  }, [id, t]);

  const styles = useMemo(() => StyleSheet.create({
    safeArea: {
      flex: 1,
    },
    container: {
      flex: 1,
      paddingVertical: SPACING_MEDIUM,
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
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    cardContent: {
      flexDirection: 'column', // Change to column for centering
      alignItems: 'center', // Center items horizontally
      paddingVertical: SPACING_LARGE,
    },
    avatar: {
      marginBottom: SPACING_MEDIUM, // Add margin below avatar
    },
    detailsContainer: {
      alignItems: 'center', // Center text content
      width: '100%', // Take full width for centering
    },
    detailRow: {
      flexDirection: 'row',
      justifyContent: 'space-between', // Align label and value
      marginBottom: SPACING_SMALL / 2,
      width: '100%', // Take full width for alignment
    },
    detailLabel: {
      fontWeight: 'bold',
      marginRight: SPACING_SMALL / 2,
      flexShrink: 0, // Prevent label from shrinking
    },
    detailValue: {
      flex: 1, // Allow value to take remaining space
      textAlign: 'right', // Align value to the right
    },
    chipsContainer: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      marginTop: SPACING_SMALL,
      gap: SPACING_SMALL,
      justifyContent: 'center', // Center chips
    },
    chip: {
      height: 30,
      backgroundColor: 'transparent',
      paddingHorizontal: 0,
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
          <Appbar.Content title={t('memberDetail.title')} />
        </Appbar.Header>
        <View style={styles.container}>
          <View style={styles.errorContainer}>
            <Text variant="bodyMedium" style={styles.errorText}>
              {t('common.error_occurred')}: {error}
            </Text>
          </View>
        </View>
      </View>
    );
  }

  if (!member) {
    return (
      <View style={{ flex: 1 }}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => router.back()} />
          <Appbar.Content title={t('memberDetail.title')} />
        </Appbar.Header>
        <View style={styles.container}>
          <View style={styles.errorContainer}>
            <Text variant="bodyMedium" style={styles.errorText}>
              {t('memberDetail.errors.dataNotAvailable')}
            </Text>
          </View>
        </View>
      </View>
    );
  }

  return (
    <View style={{ flex: 1 }}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={member.fullName || t('memberDetail.title')} />
      </Appbar.Header>
      <View style={styles.safeArea}>
        <ScrollView style={styles.container} showsVerticalScrollIndicator={false}>
          <Card style={styles.card}>
            <Card.Content style={styles.cardContent}>
              <Avatar.Image size={100} source={{ uri: member.avatarUrl || 'https://picsum.photos/100' }} style={styles.avatar} />
              <View style={styles.detailsContainer}>
                <Text variant="headlineMedium" style={{ textAlign: 'center' }}>{member.fullName}</Text>
                {member.occupation && <Text variant="bodyLarge" style={{ textAlign: 'center' }}>{member.occupation}</Text>}
                {member.birthDeathYears && <Text variant="bodyMedium" style={{ textAlign: 'center' }}>{member.birthDeathYears}</Text>}

                <View style={styles.chipsContainer}>
                  {member.gender && (
                    <Chip icon="gender-male-female" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {t(`memberSearch.filter.gender.${member.gender.toLowerCase()}`)}
                    </Chip>
                  )}
                  {member.isRoot && (
                    <Chip icon="account-star" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {t('memberDetail.isRoot')}
                    </Chip>
                  )}
                  {member.father && (
                    <Chip icon="human-male-boy" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {t('member.father')}: {member.father}
                    </Chip>
                  )}
                  {member.mother && (
                    <Chip icon="human-female-girl" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {t('member.mother')}: {member.mother}
                    </Chip>
                  )}
                  {member.husband && (
                    <Chip icon="human-male-boy" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {t('member.husband')}: {member.husband}
                    </Chip>
                  )}
                  {member.wife && (
                    <Chip icon="human-female-girl" style={styles.chip} compact={true} textStyle={{ fontSize: 12 }}>
                      {member.wife}
                    </Chip>
                  )}
                </View>
              </View>
            </Card.Content>
          </Card>



          <Card style={styles.card}>
            <Card.Title title={t('memberDetail.otherDetails')} titleVariant="titleMedium" />
            <Card.Content>
              <List.Section>
                {member.dateOfBirth && (
                  <>
                    <List.Item
                      title={t('memberDetail.dateOfBirth')}
                      left={() => <List.Icon icon="calendar-account" />}
                      right={() => <Text>{new Date(member.dateOfBirth ?? '').toLocaleDateString()}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.dateOfDeath && (
                  <>
                    <List.Item
                      title={t('memberDetail.dateOfDeath')}
                      left={() => <List.Icon icon="calendar-remove" />}
                      right={() => <Text>{new Date(member.dateOfDeath ?? '').toLocaleDateString()}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.placeOfBirth && (
                  <>
                    <List.Item
                      title={t('memberDetail.placeOfBirth')}
                      left={() => <List.Icon icon="map-marker" />}
                      right={() => <Text>{member.placeOfBirth}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.placeOfDeath && (
                  <>
                    <List.Item
                      title={t('memberDetail.placeOfDeath')}
                      left={() => <List.Icon icon="map-marker-off" />}
                      right={() => <Text>{member.placeOfDeath}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.nickname && (
                  <>
                    <List.Item
                      title={t('memberDetail.nickname')}
                      left={() => <List.Icon icon="tag" />}
                      right={() => <Text>{member.nickname}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.father && (
                  <>
                    <List.Item
                      title={t('member.father')}
                      left={() => <List.Icon icon="human-male-boy" />}
                      right={() => <Text>{member.father}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.mother && (
                  <>
                    <List.Item
                      title={t('member.mother')}
                      left={() => <List.Icon icon="human-female-girl" />}
                      right={() => <Text>{member.mother}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.husband && (
                  <>
                    <List.Item
                      title={t('member.husband')}
                      left={() => <List.Icon icon="human-male" />}
                      right={() => <Text>{member.husband}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.wife && (
                  <>
                    <List.Item
                      title={t('member.wife')}
                      left={() => <List.Icon icon="human-female" />}
                      right={() => <Text>{member.wife}</Text>}
                    />
                    <Divider />
                  </>
                )}
                {member.created && (
                  <>
                    <List.Item
                      title={t('memberDetail.created')}
                      left={() => <List.Icon icon="calendar-plus" />}
                      right={() => <Text>{new Date(member.created).toLocaleDateString()}</Text>}
                    />
                  </>
                )}
              </List.Section>
            </Card.Content>
          </Card>

          <Card style={styles.card}>
            <Card.Title title={t('memberDetail.biography')} titleVariant="titleMedium" />
            <Card.Content>
              <Text variant="bodyMedium">{member.biography || t('memberDetail.noBiography')}</Text>
            </Card.Content>
          </Card>
        </ScrollView>
      </View>
    </View>
  );
}
