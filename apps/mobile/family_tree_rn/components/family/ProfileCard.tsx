import React from 'react';
import { View, StyleSheet } from 'react-native';
import { Text, useTheme, Avatar, Card } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_SMALL } from '@/constants/dimensions';
import DefaultFamilyAvatar from '@/assets/images/familyAvatar.png';
import { FamilyDetailDto } from '@/types/public.d'; // Assuming this is the type for family

interface ProfileCardProps {
  family: FamilyDetailDto;
}

export default function ProfileCard({ family }: ProfileCardProps) {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = StyleSheet.create({
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
      color: theme.colors.onSurfaceVariant,
      marginBottom: SPACING_SMALL,
    },
    locationText: {
      color: theme.colors.onSurfaceVariant,
    },
  });

  return (
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
  );
}
