import React, { useEffect, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { Appbar, Text, useTheme, Card, Avatar, ActivityIndicator, Chip, List, Divider } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_LARGE, SPACING_SMALL } from '@/constants/dimensions';
import { usePublicMemberStore } from '@/stores/usePublicMemberStore'; // Import usePublicMemberStore
import { useFamilyStore } from '../../stores/useFamilyStore'; // Import useFamilyStore
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png'; // Import default family avatar

export default function MemberDetailsScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();
  const { t } = useTranslation();
  const theme = useTheme();

  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store
  const { member, loading, error, getMemberById } = usePublicMemberStore();

  useEffect(() => {
    const loadMemberDetails = async () => {
      if (!id) {
        // setError(t('memberDetail.errors.noMemberId')); // Error state is managed by store
        return;
      }
      if (!currentFamilyId) {
        // setError(t('memberDetail.errors.noFamilyId')); // Error state is managed by store
        return;
      }
      const memberId = Array.isArray(id) ? id[0] : id;
      await getMemberById(memberId, currentFamilyId);
    };
    loadMemberDetails();
  }, [id, currentFamilyId, getMemberById]);

  const styles = useMemo(() => StyleSheet.create({
    container: {
      flex: 1,
      padding: SPACING_MEDIUM,
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
      borderWidth: 0, // Remove border
      backgroundColor: 'transparent', // Remove background
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
        <ScrollView style={styles.container} showsVerticalScrollIndicator={false}>
          <Card style={styles.card}>
            <Card.Content style={styles.cardContent}>
              <Avatar.Image size={100} source={member.avatarUrl ? { uri: member.avatarUrl } : DefaultFamilyAvatar} style={styles.avatar} />
              <View style={styles.detailsContainer}>
                <Text variant="headlineMedium" style={{ textAlign: 'center' }}>{member.fullName}</Text>
                {member.occupation && <Text variant="bodyLarge" style={{ textAlign: 'center' }}>{member.occupation}</Text>}
                {member.birthDeathYears && <Text variant="bodyMedium" style={{ textAlign: 'center' }}>{member.birthDeathYears}</Text>}

                <View style={styles.chipsContainer}>
                  {member.gender && (
                    <Chip icon="gender-male-female" style={styles.chip} compact={true}>
                      {t(`memberSearch.filter.gender.${member.gender.toLowerCase()}`)}
                    </Chip>
                  )}
                  {member.isRoot && (
                    <Chip icon="account-star" style={styles.chip} compact={true}>
                      {t('memberDetail.isRoot')}
                    </Chip>
                  )}
                  {member.fatherFullName && (
                    <Chip icon="human-male-boy" style={styles.chip} compact={true}>
                      {member.fatherFullName}
                    </Chip>
                  )}
                  {member.motherFullName && (
                    <Chip icon="human-female-girl" style={styles.chip} compact={true}>
                      {member.motherFullName}
                    </Chip>
                  )}
                  {member.husbandFullName && (
                    <Chip icon="heart" style={styles.chip} compact={true}>
                      {member.husbandFullName}
                    </Chip>
                  )}
                  {member.wifeFullName && (
                    <Chip icon="heart" style={styles.chip} compact={true}>
                      {member.wifeFullName}
                    </Chip>
                  )}
                </View>
              </View>
            </Card.Content>
          </Card>

          <Card style={styles.card}>
            <Card.Content>
              <List.Section>
                {/* Personal Information */}
                <List.Subheader>{t('memberDetail.personalInfo')}</List.Subheader>
                <>
                  <List.Item
                    title={t('memberDetail.lastName')}
                    left={() => <List.Icon icon="account" />}
                    right={() => <Chip compact={true}>{member.lastName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.firstName')}
                    left={() => <List.Icon icon="account" />}
                    right={() => <Chip compact={true}>{member.firstName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.nickname')}
                    left={() => <List.Icon icon="tag" />}
                    right={() => <Chip compact={true}>{member.nickname || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.dateOfBirth')}
                    left={() => <List.Icon icon="calendar-account" />}
                    right={() => <Chip compact={true}>{member.dateOfBirth ? new Date(member.dateOfBirth).toLocaleDateString() : t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.dateOfDeath')}
                    left={() => <List.Icon icon="calendar-remove" />}
                    right={() => <Chip compact={true}>{member.dateOfDeath ? new Date(member.dateOfDeath).toLocaleDateString() : t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.placeOfBirth')}
                    left={() => <List.Icon icon="map-marker" />}
                    right={() => <Chip compact={true}>{member.placeOfBirth || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.placeOfDeath')}
                    left={() => <List.Icon icon="map-marker-off" />}
                    right={() => <Chip compact={true}>{member.placeOfDeath || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.email')}
                    left={() => <List.Icon icon="email" />}
                    right={() => <Chip compact={true}>{member.email || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.phone')}
                    left={() => <List.Icon icon="phone" />}
                    right={() => <Chip compact={true}>{member.phone || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.address')}
                    left={() => <List.Icon icon="home-map-marker" />}
                    right={() => <Chip compact={true}>{member.address || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('memberDetail.occupation')}
                    left={() => <List.Icon icon="briefcase" />}
                    right={() => <Chip compact={true}>{member.occupation || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>

                {/* Family Relationships */}
                <List.Subheader>{t('memberDetail.familyRelationships')}</List.Subheader>
                <>
                  <List.Item
                    title={t('member.father')}
                    left={() => <List.Icon icon="human-male-boy" />}
                    right={() => <Chip compact={true}>{member.fatherFullName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('member.mother')}
                    left={() => <List.Icon icon="human-female-girl" />}
                    right={() => <Chip compact={true}>{member.motherFullName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('member.husband')}
                    left={() => <List.Icon icon="heart" />}
                    right={() => <Chip compact={true}>{member.husbandFullName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
                <>
                  <List.Item
                    title={t('member.wife')}
                    left={() => <List.Icon icon="heart" />}
                    right={() => <Chip compact={true}>{member.wifeFullName || t('common.not_available')}</Chip>}
                  />
                  <Divider />
                </>
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
  );
}
