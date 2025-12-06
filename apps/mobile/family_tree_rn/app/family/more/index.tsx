import React from 'react';
import { StyleSheet, ScrollView } from 'react-native';
import { List, useTheme, Divider } from 'react-native-paper'; // Import Appbar
import { useTranslation } from 'react-i18next';
import { router } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { SPACING_MEDIUM } from '@/constants/dimensions';

export default function MoreOptionsScreen() {
  const { t } = useTranslation();
  const theme = useTheme();

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
      padding: SPACING_MEDIUM,
    },
    listSection: {
      backgroundColor: theme.colors.surface,
      marginVertical: SPACING_MEDIUM,
      paddingHorizontal: SPACING_MEDIUM,
      borderRadius: theme.roundness,
    },
    listItem: {
      paddingLeft: 0,
      paddingRight: 0,
    },
    listIcon: {
      marginRight: SPACING_MEDIUM,
    }
  });

  const navigateTo = (path: string) => {
    router.navigate(path as any);
  };

  return (
    <ScrollView style={styles.container}>
      <List.Section style={styles.listSection}>
        <List.Item
          title={t('more.events')}
          left={() => <MaterialCommunityIcons name="calendar-month-outline" size={24} color={theme.colors.onSurfaceVariant} style={styles.listIcon} />}
          right={() => <List.Icon icon="chevron-right" />}
          onPress={() => navigateTo('/family/more/events')}
          style={styles.listItem}
        />
        <Divider />
        <List.Item
          title={t('more.faceData')}
          left={() => <MaterialCommunityIcons name="face-recognition" size={24} color={theme.colors.onSurfaceVariant} style={styles.listIcon} />}
          right={() => <List.Icon icon="chevron-right" />}
          onPress={() => navigateTo('/family/more/face-data')}
          style={styles.listItem}
        />
        <Divider />
        <List.Item
          title={t('more.memories')}
          left={() => <MaterialCommunityIcons name="image-multiple-outline" size={24} color={theme.colors.onSurfaceVariant} style={styles.listIcon} />}
          right={() => <List.Icon icon="chevron-right" />}
          onPress={() => navigateTo('/family/more/memories')}
          style={styles.listItem}
        />
        <Divider />
        <List.Item
          title={t('more.timeline')}
          left={() => <MaterialCommunityIcons name="timeline-text-outline" size={24} color={theme.colors.onSurfaceVariant} style={styles.listIcon} />}
          right={() => <List.Icon icon="chevron-right" />}
          onPress={() => navigateTo('/family/more/timeline')}
          style={styles.listItem}
        />
        <Divider />
        <List.Item
          title={t('more.privacy')}
          left={() => <MaterialCommunityIcons name="shield-lock-outline" size={24} color={theme.colors.onSurfaceVariant} style={styles.listIcon} />}
          right={() => <List.Icon icon="chevron-right" />}
          onPress={() => navigateTo('/family/more/privacy')}
          style={styles.listItem}
        />
      </List.Section>
    </ScrollView>
  );
}
