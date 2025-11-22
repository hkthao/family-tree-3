import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Text, useTheme } from 'react-native-paper';
import { WebView } from 'react-native-webview';
import { useTranslation } from 'react-i18next';
import { useFamilyStore } from '@/stores/useFamilyStore'; // Import useFamilyStore

export default function FamilyTreeScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
    webview: {
      flex: 1,
      backgroundColor: theme.colors.background,
    },
  });

  const familyDetailUrl = currentFamilyId ? `${process.env.EXPO_PUBLIC_APP_BASE_URL}/public/family-tree/${currentFamilyId}` : '';

  return (
    <View style={styles.container}>
      {currentFamilyId ? (
        <WebView
          source={{ uri: familyDetailUrl }}
          style={styles.webview}
          javaScriptEnabled={true}
          domStorageEnabled={true}
          startInLoadingState={true}
          scalesPageToFit={true}
        />
      ) : (
        <Text variant="headlineMedium">{t('familyDetail.noFamilySelected')}</Text>
      )}
    </View>
  );
}
