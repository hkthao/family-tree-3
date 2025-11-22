/**
 * Below are the colors that are used in the app. The colors are defined in the light and dark mode.
 * There are many other ways to style your app. For example, [Nativewind](https://www.nativewind.dev/), [Tamagui](https://tamagui.dev/), [unistyles](https://reactnativeunistyles.vercel.app), etc.
 */

import { MD3LightTheme, MD3DarkTheme} from 'react-native-paper';

export const getPaperTheme = (colorScheme: 'light' | 'dark') => {
      const baseTheme = colorScheme === 'dark' ? MD3DarkTheme : MD3LightTheme;
    return {
      ...baseTheme,
      colors: {
        ...baseTheme.colors,
        // primary: colorScheme === 'dark' ? MD3Colors.primary30 : MD3Colors.primary30
      },
      roundness: 8, // Global roundness for components
    };};