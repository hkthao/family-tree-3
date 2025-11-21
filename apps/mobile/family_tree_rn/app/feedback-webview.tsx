import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Appbar, useTheme } from 'react-native-paper';
import { WebView } from 'react-native-webview';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'expo-router';

export default function FeedbackWebViewScreen() {
  const { t } = useTranslation();
  const theme = useTheme();
  const router = useRouter();

  const feedbackUrl = process.env.EXPO_PUBLIC_FEEDBACK_FORM_URL;

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
        <Appbar.Content title={t('settings.helpSupport.feedback')} />
      </Appbar.Header>
      <WebView
        source={{ uri: feedbackUrl as string }}
        style={styles.webview}
        javaScriptEnabled={true}
        domStorageEnabled={true}
        startInLoadingState={true}
        scalesPageToFit={false}
      />
    </View>
  );
}
