import React from 'react';
import { View, StyleSheet } from 'react-native';
import { useTheme, Card, List, Divider, Chip } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { FamilyDetailDto, FamilyUserDto, FamilyRole } from '@/types';

interface DetailedInfoCardProps {
  family: FamilyDetailDto;
}

export default function DetailedInfoCard({ family }: DetailedInfoCardProps) {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = StyleSheet.create({
    card: {
      marginBottom: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    chipContainer: {
      marginRight: -SPACING_MEDIUM, // Apply negative margin to pull the chip closer to the right edge
    },
    chipsContainer: {
      gap: 5,
      marginRight: -SPACING_MEDIUM, // Adjust overall right margin for the chips container
    },
  });

  return (
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
                              .filter((fu: FamilyUserDto) => fu.role === FamilyRole.Manager)
                              .map((fu: FamilyUserDto, index: number) => (
                                <Chip key={index} >
                                  {fu.userName || fu.userId} {/* Display userName, fallback to userId */}
                                </Chip>
                              ))}
                          </View>            )}
          />
          <Divider />
          <List.Item
            title={t('familyDetail.details.viewers')}
            left={() => <List.Icon icon="eye-outline" />}
            right={() => (
                          <View style={styles.chipsContainer}>
                            {family.familyUsers
                              .filter((fu: FamilyUserDto) => fu.role === FamilyRole.Viewer)
                              .map((fu: FamilyUserDto, index: number) => (
                                <Chip key={index} >
                                  {fu.userName || fu.userId} {/* Display userName, fallback to userId */}
                                </Chip>
                              ))}
                          </View>            )}
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
  );
}
