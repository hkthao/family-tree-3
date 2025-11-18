import { StyleSheet } from 'react-native';
import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';

export default function DonateScreen() {
  return (
    <ThemedView style={styles.container}>
      <ThemedText type="title">Donate Tab</ThemedText>
      <ThemedText>This is the Donate screen.</ThemedText>
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