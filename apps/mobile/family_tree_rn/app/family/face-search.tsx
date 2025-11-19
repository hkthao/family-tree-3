import React from 'react';
import { StyleSheet } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Text, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useFamilyStore } from '../../stores/useFamilyStore'; // Import useFamilyStore

export default function FamilyFaceSearchScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
      alignItems: 'center',
      justifyContent: 'center',
    },
  });

  return (
    <SafeAreaView style={styles.container}>
      <Text variant="headlineMedium">{t('familyDetail.tab.faceSearch')}</Text>
      <Text variant="bodyMedium">Family ID: {currentFamilyId}</Text>
      {/* Add face search content here */}
    </SafeAreaView>
  );
}
