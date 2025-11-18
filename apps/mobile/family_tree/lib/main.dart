// lib/main.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/app.dart';

void main() {
  runApp(
    const ProviderScope(
      child: FamilyTreeApp(),
    ),
  );
}