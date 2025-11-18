// lib/widgets/result_card.dart
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:family_tree/models/member.dart';
import 'package:family_tree/models/watch.dart';

class ResultCard extends StatelessWidget {
  final dynamic item; // Can be Watch or Member
  final VoidCallback onTap;

  const ResultCard({super.key, required this.item, required this.onTap});

  @override
  Widget build(BuildContext context) {
    String title = '';
    String subtitle = '';
    String imageUrl = '';
    IconData icon = Icons.error;

    if (item is Watch) {
      final watch = item as Watch;
      title = watch.title;
      subtitle = '${watch.brand} - ${watch.year}';
      imageUrl = watch.thumbnail;
      icon = Icons.watch;
    } else if (item is Member) {
      final member = item as Member;
      title = member.name;
      subtitle = member.relation;
      imageUrl = member.avatar;
      icon = Icons.person;
    }

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15.0)),
      elevation: 4,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(15.0),
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(10.0),
                child: imageUrl.isNotEmpty
                    ? CachedNetworkImage(
                        imageUrl: imageUrl,
                        width: 60,
                        height: 60,
                        fit: BoxFit.cover,
                        placeholder: (context, url) => const CircularProgressIndicator(),
                        errorWidget: (context, url, error) => Icon(icon, size: 60),
                      )
                    : Icon(icon, size: 60),
              ),
              const SizedBox(width: 16.0),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4.0),
                    Text(
                      subtitle,
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.grey[600],
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(Icons.arrow_forward_ios, color: Colors.grey, size: 16),
            ],
          ),
        ),
      ),
    );
  }
}
