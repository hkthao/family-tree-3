import React from 'react';
import { Tabs } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next'; // Import useTranslation

import { useColorScheme } from '@/hooks/use-color-scheme';
import { theme } from '../../src/theme'; // Import theme from react-native-paper config

function TabBarIcon({ style, ...rest }: { name: React.ComponentProps<typeof MaterialCommunityIcons>['name']; color: string }) {
  return <MaterialCommunityIcons size={28} style={[{ marginBottom: -3 }, style]} {...rest} />;
}

export default function TabLayout() {
  const colorScheme = useColorScheme();
  const { t } = useTranslation(); // Initialize useTranslation

  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: theme.colors.primary, // Use primary color from react-native-paper theme
        headerShown: false,
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
        name="story"
        options={{
          title: t('tab.story'), // Translated title
          tabBarIcon: ({ color }) => <TabBarIcon name="book-open-variant" color={color} />,
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


