// lib/screens/member/member_profile_screen.dart
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/models/member.dart';

import 'package:family_tree/providers/search_provider.dart';

final memberProfileProvider = FutureProvider.family<Member, String>((ref, id) async {
  final apiService = ref.watch(apiServiceProvider);
  return apiService.getMemberProfile(id);
});

class MemberProfileScreen extends ConsumerWidget {
  final String memberId;

  const MemberProfileScreen({super.key, required this.memberId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final memberProfileAsync = ref.watch(memberProfileProvider(memberId));

    return Scaffold(
      appBar: AppBar(
        title: const Text('Hồ sơ thành viên'),
        centerTitle: true,
      ),
      body: memberProfileAsync.when(
        data: (member) {
          return SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                const SizedBox(height: 20),
                Hero(
                  tag: 'member-${member.id}',
                  child: CircleAvatar(
                    radius: 80,
                    backgroundImage: CachedNetworkImageProvider(member.avatar),
                    onBackgroundImageError: (exception, stackTrace) {
                      // Handle image loading error
                    },
                  ),
                ),
                const SizedBox(height: 20),
                Text(
                  member.name,
                  style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                const SizedBox(height: 8),
                Text(
                  member.relation,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        color: Colors.grey[600],
                      ),
                ),
                const SizedBox(height: 20),
                const Divider(),
                Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Thông tin cá nhân:',
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      const Text('Ngày sinh: 01/01/1990'),
                      const Text('Nơi sinh: Hà Nội'),
                      const Text('Nghề nghiệp: Kỹ sư'),
                      const SizedBox(height: 16),
                      Text(
                        'Dòng thời gian đồng hồ:',
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      // Mock watch timeline/list
                      ListTile(
                        leading: const CircleAvatar(
                          backgroundImage: CachedNetworkImageProvider('https://example.com/img1.jpg'),
                        ),
                        title: const Text('Rolex Submariner'),
                        subtitle: const Text('Sở hữu từ 2010 - 2020'),
                        onTap: () {
                          // Navigate to watch detail
                        },
                      ),
                      ListTile(
                        leading: const CircleAvatar(
                          backgroundImage: CachedNetworkImageProvider('https://example.com/img2.jpg'),
                        ),
                        title: const Text('Omega Speedmaster'),
                        subtitle: const Text('Sở hữu từ 2020 - Hiện tại'),
                        onTap: () {
                          // Navigate to watch detail
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
