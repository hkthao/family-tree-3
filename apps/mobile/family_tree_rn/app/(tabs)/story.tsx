import { StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';

export default function StoryScreen() {
  return (
    <View style={styles.container}>
      <Text variant="headlineMedium">Story Tab</Text>
      <Text variant="bodyMedium">This is the Story screen.</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});