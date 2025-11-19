import { ScrollView, StyleSheet, View, Alert } from 'react-native';
import { Text, Button, Switch, Avatar, useTheme, Appbar, List, Divider } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM, SPACING_LARGE } from '@/constants/dimensions';
import { useAuth } from '@/hooks/useAuth';
import { router } from 'expo-router';
import { useEffect, useState, useMemo } from 'react';
import { useThemeContext } from '@/context/ThemeContext'; // Import useThemeContext

export default function SettingsScreen() {
  const { t, i18n } = useTranslation();
  const { logout, user } = useAuth();
  const theme = useTheme();
  const { themePreference, setThemePreference } = useThemeContext(); // Use theme context

  // State for appearance settings
  const [isDarkMode, setIsDarkMode] = useState(themePreference === 'dark'); // Initialize from context

  useEffect(() => {
    setIsDarkMode(themePreference === 'dark');
  }, [themePreference]);

  const handleThemeToggle = () => {
    const newTheme = isDarkMode ? 'light' : 'dark';
    setThemePreference(newTheme);
    setIsDarkMode(!isDarkMode);
  };

  const handleLogout = () => {
    Alert.alert(
      t('settings.logout.confirmTitle'),
      t('settings.logout.confirmMessage'),
      [
        {
          text: t('common.cancel'),
          style: 'cancel',
        },
        {
          text: t('common.logout'),
          onPress: () => {
            logout();
            router.replace('/login'); // Redirect to login after logout
          },
          style: 'destructive',
        },
      ],
      { cancelable: false }
    );
  };

  const handleEditProfile = () => {
    // Navigate to edit profile screen
    console.log('Edit Profile');
  };

  const handleDeleteAccount = () => {
    Alert.alert(
      t('settings.deleteAccount.confirmTitle'),
      t('settings.deleteAccount.confirmMessage'),
      [
        {
          text: t('common.cancel'),
          style: 'cancel',
        },
        {
          text: t('common.delete'),
          onPress: () => {
            // Implement account deletion logic
            console.log('Account Deleted');
            logout(); // Log out after deletion
            router.replace('/login');
          },
          style: 'destructive',
        },
      ],
      { cancelable: false }
    );
  };

  const styles = useMemo(() => StyleSheet.create({
    scrollViewContent: {
      paddingVertical: SPACING_MEDIUM,
    },
    container: {
      flex: 1,
      paddingHorizontal: SPACING_MEDIUM,
    },
    listSection: {
      marginBottom: SPACING_MEDIUM,
      backgroundColor: theme.colors.surface, // Use theme surface color
      borderRadius: theme.roundness, // Use global roundness from theme
      elevation: 2,
    },
    listItem: {
      paddingStart: SPACING_MEDIUM
    },
    rightIcon: {
      marginRight: -SPACING_MEDIUM, // Increased negative margin to pull icon further left
    },
  }), [theme]);

  return (
    <>
      <Appbar.Header>
        <Appbar.Content title={t('settings.title')} />
      </Appbar.Header>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scrollViewContent}>
        <View style={styles.container}>
          {/* 1. Hồ sơ cá nhân (User Profile) */}
          <List.Section style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              title={user?.fullName || t('settings.profile.guestUser')}
              description={user?.email || user?.phoneNumber || 'N/A'}
              left={() => (
                <Avatar.Image size={48} source={{ uri: user?.avatarUrl || 'https://via.placeholder.com/150' }} />
              )}
              onPress={handleEditProfile}
            />
          </List.Section>

          {/* 3. Quyền riêng tư & bảo mật (Privacy & Security) */}
          <List.Section title={t('settings.privacySecurity.title')} style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="download" />}
              title={t('settings.privacySecurity.downloadData')}
              onPress={() => console.log('Download my data')}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="delete" />}
              title={t('settings.privacySecurity.deleteAccount')}
              onPress={handleDeleteAccount}
              titleStyle={{ color: theme.colors.error }}
            />
          </List.Section>

          {/* 4. Tuỳ chỉnh giao diện (App Appearance) */}
          <List.Section title={t('settings.appAppearance.title')} style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="theme-light-dark" />}
              title={t('settings.appAppearance.theme')}
              right={() => <Switch value={isDarkMode} onValueChange={handleThemeToggle} />}
            />
          </List.Section>

          {/* 5. Ngôn ngữ (Language) */}
          <List.Section title={t('settings.language.title')} style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="translate" />}
              title={t('settings.language.vietnamese')}
              onPress={() => i18n.changeLanguage('vi')}
              right={() => i18n.language === 'vi' ? <List.Icon style={styles.rightIcon} icon="check" /> : null}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="translate" />}
              title={t('settings.language.english')}
              onPress={() => i18n.changeLanguage('en')}
              right={() => i18n.language === 'en' ? <List.Icon icon="check" /> : null}
            />
          </List.Section>



          {/* 9. Trung tâm hỗ trợ (Help & Support) */}
          <List.Section title={t('settings.helpSupport.title')} style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="help-circle" />}
              title={t('settings.helpSupport.faq')}
              onPress={() => console.log('FAQ')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="book-open-variant" />}
              title={t('settings.helpSupport.userGuide')}
              onPress={() => console.log('User Guide')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="comment-edit" />}
              title={t('settings.helpSupport.feedback')}
              onPress={() => console.log('Feedback')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
          </List.Section>

          {/* 10. Về ứng dụng (About) */}
          <List.Section title={t('settings.aboutApp.title')} style={styles.listSection}>
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="information" />}
              title={t('settings.aboutApp.versionInfo')}
              right={() => <Text>1.0.0</Text>} // TODO: Get actual version
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="file-document" />}
              title={t('settings.aboutApp.termsOfService')}
              onPress={() => console.log('Terms of Service')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="shield-lock" />}
              title={t('settings.aboutApp.privacyPolicy')}
              onPress={() => console.log('Privacy Policy')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
          </List.Section>

          {/* 11. Đăng xuất */}
          <Button
            mode="contained"
            onPress={handleLogout}
          >
            {t('settings.logout.button')}
          </Button>
        </View>
      </ScrollView>
    </>
  );
}