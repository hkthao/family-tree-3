import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Appbar, useTheme } from 'react-native-paper';
import { WebView } from 'react-native-webview';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';

export default function FaqWebViewScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();

  const faqUrl = `${process.env.EXPO_PUBLIC_APP_BASE_URL}/public/faq`;

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

  return (
    <View style={styles.container}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => router.back()} />
        <Appbar.Content title={t('settings.helpSupport.faq')} />
      </Appbar.Header>
      <WebView
        source={{ uri: faqUrl }}
        style={styles.webview}
        javaScriptEnabled={true}
        domStorageEnabled={true}
        startInLoadingState={true}
        scalesPageToFit={true}
      />
    </View>
  );
}
