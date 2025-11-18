// lib/services/api_service.dart
import 'package:family_tree/models/member.dart';
import 'package:family_tree/models/watch.dart';

abstract class ApiService {
  Future<List<dynamic>> search(String query);
  Future<Watch> getWatchDetail(String id);
  Future<Member> getMemberProfile(String id);
  Future<List<dynamic>> getFamilyTreeData();
}
