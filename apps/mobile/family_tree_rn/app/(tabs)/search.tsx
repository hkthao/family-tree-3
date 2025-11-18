import { StyleSheet } from 'react-native';
import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { useTranslation } from 'react-i18next';

export default function SearchScreen() {
  const { t } = useTranslation();
  return (
    <ThemedView style={styles.container}>
      <ThemedText type="title">{t('tab.search')}</ThemedText>
      <ThemedText>This is the Search screen.</ThemedText>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});