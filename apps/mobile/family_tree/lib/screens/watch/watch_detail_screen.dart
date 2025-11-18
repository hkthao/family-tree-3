// lib/screens/watch/watch_detail_screen.dart
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/models/watch.dart';

import 'package:family_tree/providers/search_provider.dart';

final watchDetailProvider = FutureProvider.family<Watch, String>((ref, id) async {
  final apiService = ref.watch(apiServiceProvider);
  return apiService.getWatchDetail(id);
});

class WatchDetailScreen extends ConsumerWidget {
  final String watchId;

  const WatchDetailScreen({super.key, required this.watchId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final watchDetailAsync = ref.watch(watchDetailProvider(watchId));

    return Scaffold(
      appBar: AppBar(
        title: const Text('Chi tiết đồng hồ'),
        centerTitle: true,
      ),
      body: watchDetailAsync.when(
        data: (watch) {
          return SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Hero(
                  tag: 'watch-${watch.id}',
                  child: CachedNetworkImage(
                    imageUrl: watch.thumbnail,
                    width: double.infinity,
                    height: 250,
                    fit: BoxFit.cover,
                    placeholder: (context, url) => const Center(child: CircularProgressIndicator()),
                    errorWidget: (context, url, error) => const Icon(Icons.watch, size: 100),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        watch.title,
                        style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Thương hiệu: ${watch.brand}',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Năm sản xuất: ${watch.year}',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'Thông số kỹ thuật:',
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.',
                        style: TextStyle(fontSize: 16),
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'Thành viên sở hữu:',
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      // Mock list of members who own this watch
                      ListTile(
                        leading: const CircleAvatar(
                          backgroundImage: CachedNetworkImageProvider('https://example.com/avatar1.jpg'),
                        ),
                        title: const Text('Huỳnh Kim Thảo'),
                        subtitle: const Text('Chủ gia đình'),
                        onTap: () {
                          // Navigate to member profile
                        },
                      ),
                      ListTile(
                        leading: const CircleAvatar(
                          backgroundImage: CachedNetworkImageProvider('https://example.com/avatar2.jpg'),
                        ),
                        title: const Text('Huỳnh Văn A'),
                        subtitle: const Text('Ông nội'),
                        onTap: () {
                          // Navigate to member profile
                        },
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (err, stack) => Center(child: Text('Lỗi: $err')),
      ),
    );
  }
}
