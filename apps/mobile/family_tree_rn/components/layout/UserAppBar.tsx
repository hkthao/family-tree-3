import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Avatar, Appbar } from 'react-native-paper'; // Remove Menu, IconButton
import { useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { PaperTheme } from '../../constants/theme';

const useAuth = () => {
  const isLoggedIn = true; // Simulate logged in state
  const user = { name: 'John Doe', avatar: 'https://picsum.photos/200' }; // Mock user data
  return { isLoggedIn, user };
};

export default function UserAppBar() {
  const router = useRouter();
  const { t } = useTranslation(); // Re-add useTranslation hook
  const { isLoggedIn, user } = useAuth();

  const handleAvatarPress = () => {
    router.push('/'); // TODO: Navigate to profile screen
  };

  if (!isLoggedIn) {
    return null;
  }

  return (
      <Appbar.Header style={styles.appBarHeader}>
        <Appbar.Action // Avatar on the left
          icon={user?.avatar ? () => <Avatar.Image size={32} source={{ uri: user.avatar }} /> : "account"}
          onPress={handleAvatarPress} // Handle press on avatar
          size={32}
          color={PaperTheme.colors.primary}
        />
        <Appbar.Content
          title={t('appbar.title')} // Use translated title
          titleStyle={{ color: PaperTheme.colors.primary }} // Apply primary color to title
          style={styles.appBarContent}
        />
      </Appbar.Header>
  );
}

const styles = StyleSheet.create({
  appBarHeader: {
    backgroundColor: 'white',
  },
  appBarContent: {
  },
});
