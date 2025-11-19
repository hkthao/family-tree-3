import React from 'react';
import { View, StyleSheet } from 'react-native';
import { Text, Appbar, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useLocalSearchParams } from 'expo-router';

export default function FamilyMembersScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const { id } = useLocalSearchParams();

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
      alignItems: 'center',
      justifyContent: 'center',
    },
  });

  return (
    <>
      <Appbar.Header>
        <Appbar.Content title={t('familyDetail.tab.members')} />
      </Appbar.Header>
      <View style={styles.container}>
        <Text variant="headlineMedium">{t('familyDetail.tab.members')}</Text>
        <Text variant="bodyMedium">Family ID: {id}</Text>
        {/* Add family members content here */}
      </View>
    </>
  );
}
