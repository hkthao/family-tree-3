// lib/screens/family/tree_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:family_tree/models/member.dart';

import 'package:family_tree/providers/search_provider.dart';
import 'package:family_tree/widgets/node_card.dart';

final familyTreeDataProvider = FutureProvider<List<dynamic>>((ref) async {
  final apiService = ref.watch(apiServiceProvider);
  return apiService.getFamilyTreeData();
});

class TreeScreen extends ConsumerWidget {
  const TreeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final familyTreeAsync = ref.watch(familyTreeDataProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Cây Gia Phả'),
        centerTitle: true,
      ),
      body: familyTreeAsync.when(
        data: (data) {
          final Map<String, Member> members = {};
          for (var item in data) {
            final member = Member.fromJson(item);
            members[member.id] = member;
          }

          // Simple layout for demonstration
          // In a real app, you'd have a more sophisticated tree layout algorithm
          final Map<String, Offset> positions = {
            'm1': const Offset(200, 50),
            'm2': const Offset(100, 250),
            'm3': const Offset(300, 250),
          };

          return InteractiveViewer(
            boundaryMargin: const EdgeInsets.all(100),
            minScale: 0.1,
            maxScale: 4.0,
            child: Stack(
              children: [
                CustomPaint(
                  painter: TreePainter(members, positions),
                  child: Container(),
                ),
                ...members.entries.map((entry) {
                  final member = entry.value;
                  final position = positions[member.id];
                  if (position == null) return const SizedBox.shrink();

                  return Positioned(
                    left: position.dx - 60, // Half of NodeCard width
                    top: position.dy - 40, // Half of NodeCard height
                    child: NodeCard(member: member),
                  );
                }).toList(),
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

class TreePainter extends CustomPainter {
  final Map<String, Member> members;
  final Map<String, Offset> positions;

  TreePainter(this.members, this.positions);

  @override
  void paint(Canvas canvas, Size size) {
    final Paint paint = Paint()
      ..color = Colors.blueGrey
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    // Draw lines between members based on mock family tree data
    // This is a simplified representation; a real tree would need a proper graph structure
    final Map<String, List<String>> relationships = {
      'm1': ['m2', 'm3'], // m1 is parent of m2 and m3
    };

    relationships.forEach((parentId, childrenIds) {
      final parentPos = positions[parentId];
      if (parentPos != null) {
        for (var childId in childrenIds) {
          final childPos = positions[childId];
          if (childPos != null) {
            // Draw line from parent to child
            canvas.drawLine(parentPos, childPos, paint);
          }
        }
      }
    });
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) {
    return false;
  }
}
