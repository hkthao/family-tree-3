import React, { useEffect, useMemo } from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { Text, useTheme, Avatar, ActivityIndicator, Card, List, Divider, Chip } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import { useFamilyStore } from '@/stores/useFamilyStore'; // Import useFamilyStore
import { usePublicFamilyStore } from '@/stores/usePublicFamilyStore'; // Import usePublicFamilyStore
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png'; // Import default family avatar
import { FamilyRole } from '@/types/public.d'; // Import FamilyRole

export default function FamilyDetailsScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store

  const { family, loading, error, getFamilyById } = usePublicFamilyStore();

  useEffect(() => {
    const loadFamilyDetails = async () => {
      if (!currentFamilyId) {
        // setError(t('familyDetail.errors.noFamilyId')); // Error state is managed by store
        // setLoading(false); // Loading state is managed by store
        return;
      }
      await getFamilyById(currentFamilyId);
    };

    loadFamilyDetails();
  }, [currentFamilyId, getFamilyById]); // Depend on currentFamilyId from Zustand and getFamilyById action



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
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    profileCardContent: {
      flexDirection: 'column',
      alignItems: 'center',
      paddingBottom: SPACING_MEDIUM,
    },
    avatar: {
      marginBottom: SPACING_MEDIUM,
    },
    nameText: {
      marginBottom: SPACING_SMALL,
    },
    codeText: {
      marginBottom: SPACING_SMALL,
    },
    locationText: {
      color: theme.colors.onSurfaceVariant,
    },
    chipContainer: {
      marginRight: -SPACING_MEDIUM, // Apply negative margin to pull the chip closer to the right edge
    },
    chipsContainer: {
      gap: 5,
      marginRight: -SPACING_MEDIUM, // Adjust overall right margin for the chips container
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
      <View style={styles.errorContainer}>
        <Text variant="titleMedium" style={{ color: theme.colors.error }}>{error}</Text>
      </View>
    );
  }

  if (!family) {
    return (
      <View style={styles.errorContainer}>
        <Text variant="titleMedium">{t('common.error_occurred')}: {t('familyDetail.errors.dataNotAvailable')}</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* First Card: Profile-like information */}
        <Card style={styles.card}>
          <Card.Content style={styles.profileCardContent}>
            <Avatar.Image size={80} source={family.avatarUrl ? { uri: family.avatarUrl } : DefaultFamilyAvatar} style={styles.avatar} />
            <Text variant="headlineSmall" style={styles.nameText}>{family.name}</Text>
            <Text variant="titleMedium" style={[styles.codeText, { color: theme.colors.onSurfaceVariant }]}>{family.code}</Text>
            <Text variant="bodyMedium" style={styles.locationText}>
              {family.address || t('common.not_available')}
            </Text>
          </Card.Content>
        </Card>

        {/* Second Card: Detailed information using List.Item */}
        <Card style={styles.card}>
          <Card.Content>
            <List.Section>
              <List.Item
                title={t('family.members')}
                left={() => <List.Icon icon="account-group" />}
                right={() => (
                  <View style={styles.chipContainer}>
                    <Chip>{family.totalMembers}</Chip>
                  </View>
                )}
              />
              <Divider />
              <List.Item
                title={t('family.generations')}
                left={() => <List.Icon icon="family-tree" />}
                right={() => (
                  <View style={styles.chipContainer}>
                    <Chip>{family.totalGenerations}</Chip>
                  </View>
                )}
              />
              <Divider />
              <List.Item
                title={t('family.visibility')}
                left={() => <List.Icon icon="eye" />}
                right={() => (
                  <View style={styles.chipContainer}>
                    <Chip>{t(`family.visibility.${family.visibility.toLowerCase()}`)}</Chip>
                  </View>
                )}
              />
              <Divider />
              <List.Item
                title={t('familyDetail.details.manager')}
                left={() => <List.Icon icon="account-tie" />}
                right={() => (
                  <View style={styles.chipsContainer}>
                    {family.familyUsers
                      .filter(fu => fu.role === FamilyRole.Manager)
                      .map((fu, index) => (
                        <Chip key={index} >
                          {fu.userName || fu.userId} {/* Display userName, fallback to userId */}
                        </Chip>
                      ))}
                  </View>
                )}
              />
              <Divider />
              <List.Item
                title={t('familyDetail.details.viewers')}
                left={() => <List.Icon icon="eye-outline" />}
                right={() => (
                  <View style={styles.chipsContainer}>
                    {family.familyUsers
                      .filter(fu => fu.role === FamilyRole.Viewer)
                      .map((fu, index) => (
                        <Chip key={index} >
                          {fu.userName || fu.userId} {/* Display userName, fallback to userId */}
                        </Chip>
                      ))}
                  </View>
                )}
              />
              <Divider />
              <List.Item
                title={t('familyDetail.details.createdAt')}
                left={() => <List.Icon icon="calendar-plus" />}
                right={() => (
                  <View style={styles.chipContainer}>
                    <Chip>{new Date(family.created).toLocaleDateString()}</Chip>
                  </View>
                )}
              />
            </List.Section>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
}
