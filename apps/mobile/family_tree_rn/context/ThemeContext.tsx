import React, { createContext, useContext, useState, useEffect, useMemo } from 'react';
import { useColorScheme as useSystemColorScheme } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage'; // Assuming this is installed or will be installed

type ThemePreference = 'light' | 'dark' | 'system';

interface ThemeContextType {
  themePreference: ThemePreference;
  setThemePreference: (preference: ThemePreference) => void;
  colorScheme: 'light' | 'dark';
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const THEME_STORAGE_KEY = 'user-theme-preference';

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const systemColorScheme = useSystemColorScheme();
  const [themePreference, setThemePreferenceState] = useState<ThemePreference>('system');
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    const loadThemePreference = async () => {
      try {
        const storedPreference = await AsyncStorage.getItem(THEME_STORAGE_KEY);
        if (storedPreference === 'light' || storedPreference === 'dark' || storedPreference === 'system') {
          setThemePreferenceState(storedPreference);
        }
      } catch (error) {
        console.error('Failed to load theme preference from AsyncStorage', error);
      } finally {
        setIsLoaded(true);
      }
    };

    loadThemePreference();
  }, []);

  const setThemePreference = async (preference: ThemePreference) => {
    try {
      await AsyncStorage.setItem(THEME_STORAGE_KEY, preference);
      setThemePreferenceState(preference);
    } catch (error) {
      console.error('Failed to save theme preference to AsyncStorage', error);
    }
  };

  const colorScheme = useMemo(() => {
    if (themePreference === 'system') {
      return systemColorScheme || 'light'; // Default to light if system is null (shouldn't happen often)
    }
    return themePreference;
  }, [themePreference, systemColorScheme]);

  const value = useMemo(() => ({
    themePreference,
    setThemePreference,
    colorScheme,
  }), [themePreference, setThemePreference, colorScheme]);

  if (!isLoaded) {
    return null; // Or a loading spinner
  }

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useThemeContext() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useThemeContext must be used within a ThemeProvider');
  }
  return context;
}
