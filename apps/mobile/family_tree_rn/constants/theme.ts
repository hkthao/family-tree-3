/**
 * Below are the colors that are used in the app. The colors are defined in the light and dark mode.
 * There are many other ways to style your app. For example, [Nativewind](https://www.nativewind.dev/), [Tamagui](https://tamagui.dev/), [unistyles](https://reactnativeunistyles.vercel.app), etc.
 */

import { Platform } from 'react-native';
import { DefaultTheme } from 'react-native-paper';

const tintColorLight = '#0a7ea4';
const tintColorDark = '#fff';

export const Colors = {
  light: {
    text: '#11181C',
    background: '#fff',
    tint: tintColorLight,
    icon: '#687076',
    tabIconDefault: '#687076',
    tabIconSelected: tintColorLight,
  },
  dark: {
    text: '#ECEDEE',
    background: '#151718',
    tint: tintColorDark,
    icon: '#9BA1A6',
    tabIconDefault: '#9BA1A6',
    tabIconSelected: tintColorDark,
  },
};

export const Fonts = Platform.select({
  ios: {
    /** iOS `UIFontDescriptorSystemDesignDefault` */
    sans: 'system-ui',
    /** iOS `UIFontDescriptorSystemDesignSerif` */
    serif: 'ui-serif',
    rounded: 'ui-rounded',
    mono: 'ui-monospace',
  },
  default: {
    sans: 'normal',
    serif: 'serif',
    rounded: 'normal',
    mono: 'monospace',
  },
  web: {
    sans: "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
    serif: "Georgia, 'Times New Roman', serif",
    rounded: "'SF Pro Rounded', 'Hiragino Maru Gothic ProN', Meiryo, 'MS PGothic', sans-serif",
    mono: "SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace",
  },
});

export const PaperTheme = {
  ...DefaultTheme,
  colors: {
    ...DefaultTheme.colors,
    primary: '#4C662B',
    onPrimary: '#FFFFFF',
    primaryContainer: '#CDEDA3',
    onPrimaryContainer: '#354E16',
    secondary: '#586249',
    onSecondary: '#FFFFFF',
    secondaryContainer: '#DCE7C8',
    onSecondaryContainer: '#404A33',
    tertiary: '#386663',
    onTertiary: '#FFFFFF',
    tertiaryContainer: '#BCECE7',
    onTertiaryContainer: '#1F4E4B',
    error: '#BA1A1A',
    onError: '#FFFFFF',
    errorContainer: '#FFDAD6',
    onErrorContainer: '#93000A',
    background: '#F9FAEF',
    onBackground: '#1A1C16',
    surface: '#F9FAEF',
    onSurface: '#1A1C16',
    surfaceVariant: '#E1E4D5',
    onSurfaceVariant: '#44483D',
    outline: '#75796C',
    outlineVariant: '#C5C8BA',
    shadow: '#000000',
    scrim: '#000000',
    inverseSurface: '#2F312A',
    inverseOnSurface: '#F1F2E6',
    inversePrimary: '#B1D18A',
  },
};