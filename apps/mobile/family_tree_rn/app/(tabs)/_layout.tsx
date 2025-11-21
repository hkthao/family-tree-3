import React from 'react';
import { View } from 'react-native';
import { Tabs } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next'; // Import useTranslation
import { useTheme } from 'react-native-paper'; // Import useTheme

function TabBarIcon({ style, ...rest }: { name: React.ComponentProps<typeof MaterialCommunityIcons>['name']; color: string; style?: object }) {
  return <MaterialCommunityIcons size={28} style={[{ marginBottom: -3 }, style]} {...rest} />;
}

export default function TabLayout() {
  const { t } = useTranslation(); // Initialize useTranslation
  const theme = useTheme();

  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: theme.colors.primary, // Use primary color from react-native-paper theme
        tabBarInactiveTintColor: theme.colors.onSurfaceVariant, // Use secondary text color for inactive tabs
        headerShown: false,
        tabBarBackground: () => (
          <View style={{ flex: 1, backgroundColor: theme.colors.background }} />
        ),
      }}>
      <Tabs.Screen
        name="index"
        options={{
          title: t('tab.home'), // Translated title
          tabBarIcon: ({ color }) => <TabBarIcon name="home" color={color} />,
        }}
      />
      <Tabs.Screen
        name="search"
        options={{
          title: t('tab.search'), // Translated title
          tabBarIcon: ({ color }) => <TabBarIcon name="magnify" color={color} />,
        }}
      />

      <Tabs.Screen
        name="family-dict"
        options={{
          title: t('tab.familyDict'),
          tabBarIcon: ({ color }) => <TabBarIcon name="book-multiple" color={color} />,
        }}
      />
      <Tabs.Screen
        name="donate"
        options={{
          title: t('tab.donate'), // Translated title
          tabBarIcon: ({ color }) => <TabBarIcon name="gift" color={color} />,
        }}
      />

      <Tabs.Screen
        name="settings"
        options={{
          title: t('tab.settings'), // Translated title
          tabBarIcon: ({ color }) => <TabBarIcon name="cog" color={color} />,
        }}
      />
    </Tabs>
  );
}


