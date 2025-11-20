import { Tabs, useSegments } from 'expo-router';
import { Appbar, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { View } from 'react-native'; // Import View for wrapping
import { useNavigation } from '@react-navigation/native'; // Import useNavigation

export default function FamilyDetailLayout() {
  const theme = useTheme();
  const { t } = useTranslation();
  const segments = useSegments();
  const navigation = useNavigation(); // Get navigation object

  // Get the current tab name from segments
  const currentTab = segments[segments.length - 1];

  // Map tab names to their translated titles
  const getTabTitle = (tabName: string) => {
    switch (tabName) {
      case 'details':
        return t('familyDetail.tab.details');
      case 'members':
        return t('familyDetail.tab.members');
      case 'tree':
        return t('familyDetail.tab.tree');
      case 'events':
        return t('familyDetail.tab.events');
      case 'face-search':
        return t('familyDetail.tab.faceSearch');
      default:
        return t('familyDetail.title'); // Fallback title
    }
  };

  return (
    <View style={{ flex: 1 }}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => navigation.goBack()} />
        <Appbar.Content title={getTabTitle(currentTab)} />
      </Appbar.Header>
      <Tabs
        screenOptions={{
          tabBarActiveTintColor: theme.colors.primary,
          tabBarInactiveTintColor: theme.colors.onSurfaceVariant,
          tabBarStyle: {
            backgroundColor: theme.colors.surface,
            borderTopColor: theme.colors.outlineVariant,
          },
          headerShown: false, // Hide header for tabs, as we have a custom Appbar
        }}
      >
      <Tabs.Screen
        name="details"
        options={{
          title: t('familyDetail.tab.details'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="information" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="members"
        options={{
          title: t('familyDetail.tab.members'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="account-group" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="tree"
        options={{
          title: t('familyDetail.tab.tree'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="family-tree" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="events"
        options={{
          title: t('familyDetail.tab.events'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="calendar-month" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="face-search"
        options={{
          title: t('familyDetail.tab.faceSearch'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="face-recognition" color={color} size={size} />
          ),
        }}
      />
      </Tabs>
    </View>
  );
}
