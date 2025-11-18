import { StyleSheet } from 'react-native';
import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';

export default function StoryScreen() {
  return (
    <ThemedView style={styles.container}>
      <ThemedText type="title">Story Tab</ThemedText>
      <ThemedText>This is the Story screen.</ThemedText>
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