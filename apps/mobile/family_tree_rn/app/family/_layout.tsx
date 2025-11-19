import React from 'react';
import { Tabs, useLocalSearchParams } from 'expo-router';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';

export default function FamilyDetailLayout() {
  const theme = useTheme();
  const { t } = useTranslation();

  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: theme.colors.primary,
        tabBarInactiveTintColor: theme.colors.onSurfaceVariant,
        tabBarStyle: {
          backgroundColor: theme.colors.surface,
          borderTopColor: theme.colors.outlineVariant,
        },
        headerShown: false, // Hide header for tabs, each tab screen can have its own Appbar
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
  );
}
