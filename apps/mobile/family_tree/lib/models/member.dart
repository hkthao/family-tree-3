// lib/models/member.dart
class Member {
  final String id;
  final String name;
  final String relation;
  final String avatar;

  Member({
    required this.id,
    required this.name,
    required this.relation,
    required this.avatar,
  });

  factory Member.fromJson(Map<String, dynamic> json) {
    return Member(
      id: json['id'],
      name: json['name'],
      relation: json['relation'],
      avatar: json['avatar'],
    );
  }
}
