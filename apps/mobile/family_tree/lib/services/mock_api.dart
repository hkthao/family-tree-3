// lib/services/mock_api.dart
import 'dart:convert';
import 'package:flutter/services.dart' show rootBundle;
import 'package:family_tree/models/member.dart';
import 'package:family_tree/models/watch.dart';
import 'package:family_tree/services/api_service.dart';

class MockApiService implements ApiService {
  @override
  Future<List<dynamic>> search(String query) async {
    await Future.delayed(const Duration(milliseconds: 500)); // Simulate network delay
    final String response = await rootBundle.loadString('assets/fixtures/search_results.json');
    final data = json.decode(response);
    final List<dynamic> items = data['items'];

    return items.where((item) {
      final title = item['title']?.toLowerCase() ?? '';
      final name = item['name']?.toLowerCase() ?? '';
      final lowerQuery = query.toLowerCase();
      return title.contains(lowerQuery) || name.contains(lowerQuery);
    }).toList();
  }

  @override
  Future<Watch> getWatchDetail(String id) async {
    await Future.delayed(const Duration(milliseconds: 300));
    final String response = await rootBundle.loadString('assets/fixtures/search_results.json');
    final data = json.decode(response);
    final List<dynamic> items = data['items'];
    final watchData = items.firstWhere((item) => item['id'] == id && item['type'] == 'watch');
    return Watch.fromJson(watchData);
  }

  @override
  Future<Member> getMemberProfile(String id) async {
    await Future.delayed(const Duration(milliseconds: 300));
    final String response = await rootBundle.loadString('assets/fixtures/search_results.json');
    final data = json.decode(response);
    final List<dynamic> items = data['items'];
    final memberData = items.firstWhere((item) => item['id'] == id && item['type'] == 'member');
    return Member.fromJson(memberData);
  }

  @override
  Future<List<dynamic>> getFamilyTreeData() async {
    await Future.delayed(const Duration(milliseconds: 500));
    // Mock family tree data
    return [
      {'id': 'm1', 'name': 'Huỳnh Kim Thảo', 'relation': 'Chủ gia đình', 'avatar': 'https://example.com/avatar1.jpg', 'parents': [], 'children': ['m2', 'm3']},
      {'id': 'm2', 'name': 'Huỳnh Văn A', 'relation': 'Ông nội', 'avatar': 'https://example.com/avatar2.jpg', 'parents': ['m1'], 'children': []},
      {'id': 'm3', 'name': 'Nguyễn Thị B', 'relation': 'Bà ngoại', 'avatar': 'https://example.com/avatar3.jpg', 'parents': ['m1'], 'children': []},
    ];
  }
}
