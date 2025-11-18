// lib/providers/search_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/services/api_service.dart';
import 'package:family_tree/services/mock_api.dart';
import 'package:family_tree/models/member.dart';
import 'package:family_tree/models/watch.dart';

// Define a union type for search results
typedef Item = dynamic; // Can be Watch or Member

final apiServiceProvider = Provider<ApiService>((ref) {
  return MockApiService(); // Use MockApiService for now
});

final searchQueryProvider = StateProvider<String>((ref) => '');

final searchResultsProvider = FutureProvider<List<Item>>((ref) async {
  final query = ref.watch(searchQueryProvider);
  final apiService = ref.watch(apiServiceProvider);

  if (query.isEmpty) {
    return [];
  }

  // Simulate debouncing
  await Future.delayed(const Duration(milliseconds: 300));

  // Check if the query has changed while waiting
  if (query != ref.read(searchQueryProvider)) {
    return [];
  }

  final results = await apiService.search(query);
  return results.map((item) {
    if (item['type'] == 'watch') {
      return Watch.fromJson(item);
    } else if (item['type'] == 'member') {
      return Member.fromJson(item);
    }
    return item; // Should not happen with current mock data
  }).toList();
});
