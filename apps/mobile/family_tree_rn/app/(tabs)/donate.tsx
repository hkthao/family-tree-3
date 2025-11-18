import { StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';

export default function DonateScreen() {
  return (
    <View style={styles.container}>
      <Text variant="headlineMedium">Donate Tab</Text>
      <Text variant="bodyMedium">This is the Donate screen.</Text>
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