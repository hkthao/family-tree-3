import { Tabs, useSegments } from 'expo-router';
import { Appbar, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { View, Share } from 'react-native'; // Import Share, useCallback, and useMemo
import { useNavigation } from '@react-navigation/native'; // Import useNavigation
import { useFamilyStore } from '@/stores/useFamilyStore'; // Import useFamilyStore
import { useCallback, useMemo } from 'react';

export default function FamilyDetailLayout() {
  const theme = useTheme();
  const { t } = useTranslation();
  const segments = useSegments();
  const navigation = useNavigation(); // Get navigation object
  const currentFamilyId = useFamilyStore((state) => state.currentFamilyId); // Get currentFamilyId from store

  // Get the current tab name from segments
  const currentTab = segments[segments.length - 1];

  // Map tab names to their translated titles
  const getTabTitle = (tabName: string) => {
    switch (tabName) {
      case 'dashboard':
        return t('familyDashboard.tab.dashboard');
      case 'members':
        return t('familyDetail.tab.members');
      case 'tree':
        return t('familyDetail.tab.tree');
      case 'face-search':
        return t('familyDetail.tab.faceSearch');
      case 'more':
        return t('familyDetail.tab.more');
      default:
        return t('familyDetail.title'); // Fallback title
    }
  };

  const familyDetailUrl = useMemo(() => {
    const baseUrl = process.env.EXPO_PUBLIC_APP_BASE_URL;
    if (!baseUrl) {
      console.warn('EXPO_PUBLIC_APP_BASE_URL is not defined. Sharing functionality might not work.');
      return '';
    }
    return currentFamilyId ? `${baseUrl}/public/family-tree/${currentFamilyId}` : '';
  }, [currentFamilyId]);

  const onShare = useCallback(async () => {
    if (!familyDetailUrl) {
      console.warn('Cannot share: familyDetailUrl is empty.');
      return;
    }
    try {
      const result = await Share.share({
        message: t('familyTree.shareMessage', { url: familyDetailUrl }),
        url: familyDetailUrl,
      });
      if (result.action === Share.sharedAction) {
        if (result.activityType) {
          console.log('Shared with activity type:', result.activityType);
        } else {
          console.log('Shared successfully');
        }
      } else if (result.action === Share.dismissedAction) {
        console.log('Share dismissed');
      }
    } catch (error: any) {
      console.error('Error sharing:', error.message);
    }
  }, [familyDetailUrl, t]);


  return (
    <View style={{ flex: 1 }}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => navigation.goBack()} />
        <Appbar.Content title={getTabTitle(currentTab)} />
        {currentTab === 'tree' && currentFamilyId && (
          <Appbar.Action icon="share-variant" onPress={onShare} color={theme.colors.onSurface} />
        )}
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
        name="dashboard"
        options={{
          title: t('familyDashboard.tab.dashboard'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="view-dashboard-outline" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="members"
        options={{
          title: t('familyDetail.tab.members'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="account-group-outline" color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="tree"
        options={{
          title: t('familyDetail.tab.tree'),
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="sitemap-outline" color={color} size={size} />
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
            <Tabs.Screen
              name="more"
              options={{
                title: t('familyDetail.tab.more'),
                tabBarIcon: ({ color, size }) => (
                  <MaterialCommunityIcons name="dots-horizontal" color={color} size={size} />
                ),
              }}
            />
          </Tabs>
        </View>
      );
      }
