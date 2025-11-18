// lib/models/watch.dart
class Watch {
  final String id;
  final String title;
  final String brand;
  final int year;
  final String thumbnail;

  Watch({
    required this.id,
    required this.title,
    required this.brand,
    required this.year,
    required this.thumbnail,
  });

  factory Watch.fromJson(Map<String, dynamic> json) {
    return Watch(
      id: json['id'],
      title: json['title'],
      brand: json['brand'],
      year: json['year'],
      thumbnail: json['thumbnail'],
    );
  }
}
