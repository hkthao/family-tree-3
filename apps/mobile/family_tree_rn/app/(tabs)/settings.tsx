import { ScrollView, StyleSheet, View, Alert, ActivityIndicator } from 'react-native';
import { Text, Button, Switch, Avatar, useTheme, Appbar, List, Divider } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { SPACING_MEDIUM } from '@/constants/dimensions';
import { useAuth } from '@/hooks/useAuth';
import { router } from 'expo-router';
import { useEffect, useState, useMemo } from 'react';
import { useThemeContext } from '@/context/ThemeContext'; // Import useThemeContext
import FamilyAvatar from '@/assets/images/familyAvatar.png'; // Import the default avatar image
import { useUserProfileStore } from '@/stores/useUserProfileStore'; // Import user profile store

export default function SettingsScreen() {
  const { t, i18n } = useTranslation();
  const { logout, isLoggedIn } = useAuth(); // Destructure isLoggedIn, no longer need `user` directly
  const theme = useTheme();
  const { themePreference, setThemePreference } = useThemeContext(); // Use theme context

  const { userProfile: fetchedUserProfile, loading: loadingProfile, error: errorProfile, fetchUserProfile, clearUserProfile } = useUserProfileStore();

  // State for appearance settings
  const [isDarkMode, setIsDarkMode] = useState(themePreference === 'dark'); // Initialize from context

  useEffect(() => {
    setIsDarkMode(themePreference === 'dark');
  }, [themePreference]);

  useEffect(() => {
    if (isLoggedIn) {
      fetchUserProfile();
    } else {
      clearUserProfile();
    }
  }, [isLoggedIn, fetchUserProfile, clearUserProfile]);

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
    if (isLoggedIn) {
      // Navigate to edit profile screen
      console.log('Edit Profile');
    } else {
      router.push('/login'); // Navigate to login if not logged in
    }
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
    errorText: {
      color: theme.colors.error,
      textAlign: 'center',
      marginTop: SPACING_MEDIUM,
    }
  }), [theme]);

  return (
    <>
      <Appbar.Header>
        <Appbar.Content title={t('settings.title')} />
      </Appbar.Header>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scrollViewContent}>
        <View style={styles.container}>
          {loadingProfile ? (
            <ActivityIndicator animating={true} color={theme.colors.primary} style={{ marginTop: SPACING_MEDIUM }} />
          ) : errorProfile ? (
            <Text style={styles.errorText}>{errorProfile}</Text>
          ) : isLoggedIn && fetchedUserProfile ? (
            <List.Section style={styles.listSection}>
              <List.Item
                style={styles.listItem}
                title={fetchedUserProfile.name || t('settings.profile.guestUser')}
                description={fetchedUserProfile.email || fetchedUserProfile.phone || 'N/A'}
                left={() => (
                  <Avatar.Image size={48} source={fetchedUserProfile.avatar ? { uri: fetchedUserProfile.avatar } : FamilyAvatar} />
                )}
                onPress={handleEditProfile}
              />
            </List.Section>
          ) : (
            <List.Section style={styles.listSection}>
              <List.Item
                style={styles.listItem}
                title={t('settings.profile.loginRegister')}
                description={t('settings.profile.loginRegisterDescription')}
                left={() => <List.Icon icon="account-circle-outline" />}
                onPress={() => router.push('/login')}
              />
            </List.Section>
          )}

          {isLoggedIn && ( // Only show Privacy & Security if logged in
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
          )}

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
              onPress={() => router.push('/faq-webview')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
            <Divider />
            <List.Item
              style={styles.listItem}
              left={() => <List.Icon icon="comment-edit" />}
              title={t('settings.helpSupport.feedback')}
              onPress={() => router.push('/feedback-webview')}
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
              left={() => <List.Icon icon="file-document-multiple" />}
              title={t('settings.aboutApp.legalInfo')}
              onPress={() => router.push('/legal-webview')}
              right={() => <List.Icon icon="chevron-right" style={styles.rightIcon} />}
            />
          </List.Section>

          {isLoggedIn && ( // Only show Logout button if logged in
            <Button
              mode="contained"
              style={
                {
                  borderRadius: theme.roundness
                }
              }
              onPress={handleLogout}
            >
              {t('settings.logout.button')}
            </Button>
          )}
        </View>
      </ScrollView>
    </>
  );
}