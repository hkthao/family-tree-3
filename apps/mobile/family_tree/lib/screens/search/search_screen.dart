// lib/screens/search/search_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lottie/lottie.dart';
import 'package:family_tree/providers/search_provider.dart';
import 'package:family_tree/widgets/search_bar.dart';
import 'package:family_tree/widgets/result_card.dart';
import 'package:family_tree/models/watch.dart';
import 'package:family_tree/models/member.dart';

class SearchScreen extends ConsumerWidget {
  const SearchScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final searchResults = ref.watch(searchResultsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Tìm kiếm'),
        centerTitle: true,
      ),
      body: Column(
        children: [
          const CustomSearchBar(),
          Expanded(
            child: searchResults.when(
              data: (items) {
                if (items.isEmpty) {
                  return Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Lottie.network(
                          'https://assets10.lottiefiles.com/packages/lf20_aP0GZg.json', // Empty state animation
                          width: 200,
                          height: 200,
                          fit: BoxFit.cover,
                        ),
                        const Text('Không tìm thấy kết quả nào.'),
                      ],
                    ),
                  );
                }
                return ListView.builder(
                  itemCount: items.length,
                  itemBuilder: (context, index) {
                    final item = items[index];
                    return ResultCard(
                      item: item,
                      onTap: () {
                        if (item is Watch) {
                          context.go('/watch/${item.id}');
                        } else if (item is Member) {
                          context.go('/member/${item.id}');
                        }
                      },
                    );
                  },
                );
              },
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (err, stack) => Center(child: Text('Lỗi: $err')),
            ),
          ),
        ],
      ),
    );
  }
}
