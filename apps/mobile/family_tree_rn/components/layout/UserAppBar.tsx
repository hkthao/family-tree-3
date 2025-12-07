import React, { useMemo } from 'react';
import { StyleSheet } from 'react-native';
import { Avatar, Appbar, useTheme } from 'react-native-paper';
import FamilyAvatar from '@/assets/images/familyAvatar.png'; // Import the default avatar image
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useFonts } from 'expo-font';
import { useAuth } from '@/hooks/useAuth'; // Import the real useAuth hook
import { useUserProfileStore } from '@/stores/useUserProfileStore'; // Import user profile store

export default function UserAppBar() {
  const router = useRouter();
  const { t } = useTranslation();
  const { isLoggedIn } = useAuth(); // Use the real useAuth hook
  const { userProfile } = useUserProfileStore();
  const theme = useTheme();

  const [fontsLoaded] = useFonts({
    'DancingScript-Regular': require('../../assets/fonts/DancingScript-Regular.ttf'),
  });

  const styles = useMemo(() => StyleSheet.create({
    appBarHeader: {
      backgroundColor: theme.colors.background, // Use theme background color
      shadowColor: '#000', // iOS shadow
      shadowOffset: { width: 0, height: 4 }, // iOS shadow
      shadowOpacity: 0.2, // iOS shadow
      shadowRadius: 2, // iOS shadow
      elevation: 4, // Android shadow
    },
    appBarContent: {
      marginLeft: 0, // Remove default left margin
      justifyContent: 'flex-start', // Align title to the left
    },
    appBarTitle: { // New style for the title text
      fontFamily: 'DancingScript-Regular',
      fontSize: 24, // Adjust as needed
    },
  }), [theme]);

  const handleAvatarPress = () => {
    // Navigate to login or profile based on login status
    if (isLoggedIn) {
      router.push('/settings'); // Navigate to profile screen (or settings)
    } else {
      router.push('/login'); // Navigate to login screen
    }
  };

  if (!fontsLoaded) { // Conditionally render
    return null;
  }

  return (
      <Appbar.Header style={styles.appBarHeader}>
        {isLoggedIn ? ( // Conditionally render avatar and bell if logged in
          <Appbar.Action // Avatar on the left
            icon={userProfile?.avatar ? () => <Avatar.Image size={32} source={{ uri: userProfile.avatar }} /> : () => <Avatar.Image size={32} source={FamilyAvatar} />}
            onPress={handleAvatarPress} // Handle press on avatar
            size={32}
            color={theme.colors.primary}
          />
        ) : (
          <Appbar.Action // Placeholder or guest icon
            icon="account-circle"
            onPress={handleAvatarPress}
            size={32}
            color={theme.colors.primary}
          />
        )}
        <Appbar.Content
          title={t('appbar.title')} // Use translated title
          titleStyle={styles.appBarTitle} // Apply the new style
          style={styles.appBarContent}
        />
        {isLoggedIn && ( // Conditionally render bell if logged in
          <Appbar.Action icon="bell" onPress={() => { /* TODO: Navigate to notifications */ }} color={theme.colors.primary} />
        )}
      </Appbar.Header>
  );
}
