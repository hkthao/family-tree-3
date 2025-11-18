// lib/app.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/routes.dart';
import 'package:family_tree/themes/app_theme.dart';

class FamilyTreeApp extends ConsumerWidget {
  const FamilyTreeApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return MaterialApp.router(
      title: 'Family Tree App',
      theme: AppTheme.lightTheme,
      routerConfig: appRouter,
    );
  }
}
