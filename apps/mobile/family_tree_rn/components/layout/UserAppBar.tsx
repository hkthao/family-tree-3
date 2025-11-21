import React, { useMemo } from 'react';
import { StyleSheet } from 'react-native';
import { Avatar, Appbar, useTheme } from 'react-native-paper'; // Remove Menu, IconButton, Add useTheme
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useFonts } from 'expo-font';

const useAuth = () => {
  const isLoggedIn = true; // Simulate logged in state
  const user = { name: 'John Doe', avatar: 'https://picsum.photos/200' }; // Mock user data
  return { isLoggedIn, user };
};

export default function UserAppBar() {
  const router = useRouter();
  const { t } = useTranslation(); // Re-add useTranslation hook
  const { isLoggedIn, user } = useAuth();
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
  }), [theme, fontsLoaded]);

  const handleAvatarPress = () => {
    router.push('/'); // TODO: Navigate to profile screen
  };

  if (!isLoggedIn || !fontsLoaded) { // Conditionally render
    return null;
  }

  return (
      <Appbar.Header style={styles.appBarHeader}>
        <Appbar.Action // Avatar on the left
          icon={user?.avatar ? () => <Avatar.Image size={32} source={{ uri: user.avatar }} /> : "account"}
          onPress={handleAvatarPress} // Handle press on avatar
          size={32}
          color={theme.colors.primary}
        />
        <Appbar.Content
          title={t('appbar.title')} // Use translated title
          titleStyle={styles.appBarTitle} // Apply the new style
          style={styles.appBarContent}
        />
        <Appbar.Action icon="bell" onPress={() => { /* TODO: Navigate to notifications */ }} color={theme.colors.primary} />
      </Appbar.Header>
  );
}
