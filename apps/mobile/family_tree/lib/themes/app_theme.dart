// lib/themes/app_theme.dart
import 'package:flutter/material.dart';

class AppTheme {
  static final ThemeData lightTheme = ThemeData(
    primarySwatch: Colors.blueGrey,
    primaryColor: const Color(0xFF607D8B), // A soft blue-grey
    hintColor: const Color(0xFFB0BEC5), // Lighter blue-grey for accents
    scaffoldBackgroundColor: const Color(0xFFF5F5F5), // Light grey background
    appBarTheme: const AppBarTheme(
      backgroundColor: Color(0xFFFFFFFF), // White app bar
      foregroundColor: Color(0xFF37474F), // Dark text on app bar
      elevation: 0,
      centerTitle: true,
    ),
    cardTheme: CardThemeData(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: const Color(0xFF607D8B), // Primary color for buttons
        foregroundColor: Colors.white,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: const Color(0xFF607D8B), // Primary color for text buttons
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.grey[100],
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide.none,
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: const Color(0xFF607D8B).withOpacity(0.5), width: 2),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: Colors.grey[300]!, width: 1),
      ),
      contentPadding: const EdgeInsets.symmetric(vertical: 14, horizontal: 16),
    ),
    // Text themes for a modern, clean look
    textTheme: const TextTheme(
      headlineLarge: TextStyle(fontSize: 32.0, fontWeight: FontWeight.bold, color: Color(0xFF37474F)),
      headlineMedium: TextStyle(fontSize: 28.0, fontWeight: FontWeight.bold, color: Color(0xFF37474F)),
      headlineSmall: TextStyle(fontSize: 24.0, fontWeight: FontWeight.bold, color: Color(0xFF37474F)),
      titleLarge: TextStyle(fontSize: 20.0, fontWeight: FontWeight.w600, color: Color(0xFF37474F)),
      titleMedium: TextStyle(fontSize: 18.0, fontWeight: FontWeight.w500, color: Color(0xFF37474F)),
      titleSmall: TextStyle(fontSize: 16.0, fontWeight: FontWeight.w500, color: Color(0xFF37474F)),
      bodyLarge: TextStyle(fontSize: 16.0, color: Color(0xFF455A64)),
      bodyMedium: TextStyle(fontSize: 14.0, color: Color(0xFF455A64)),
      labelLarge: TextStyle(fontSize: 14.0, fontWeight: FontWeight.bold, color: Colors.white),
    ),
    colorScheme: ColorScheme.fromSwatch(primarySwatch: Colors.blueGrey).copyWith(secondary: const Color(0xFFB0BEC5)),
  );
}
