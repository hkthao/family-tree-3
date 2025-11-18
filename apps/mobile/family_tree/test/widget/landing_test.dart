// test/widget/landing_test.dart
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:family_tree/screens/home/landing_screen.dart';
import 'package:family_tree/widgets/search_bar.dart';
import 'package:network_image_mock/network_image_mock.dart';

void main() {
  group('LandingScreen', () {
    testWidgets('LandingScreen loads search bar', (tester) async {
      mockNetworkImagesFor(() async {
        await tester.pumpWidget(
          const ProviderScope(
            child: MaterialApp(
              home: LandingScreen(),
            ),
          ),
        );

        expect(find.byType(CustomSearchBar), findsOneWidget);
        expect(find.text('Tìm kiếm và khám phá gia phả của bạn'), findsOneWidget);
      });
    });

    testWidgets('LandingScreen navigates to search screen on button tap', (tester) async {
      mockNetworkImagesFor(() async {
        await tester.pumpWidget(
          ProviderScope(
            child: MaterialApp.router(
              routerConfig: GoRouter(
                routes: [
                  GoRoute(
                    path: '/',
                    builder: (context, state) => const LandingScreen(),
                  ),
                  GoRoute(
                    path: '/search',
                    builder: (context, state) => const Text('Search Screen'),
                  ),
                ],
              ),
            ),
          ),
        );

        await tester.tap(find.text('Tìm kiếm nâng cao'));
        await tester.pumpAndSettle();

        expect(find.text('Search Screen'), findsOneWidget);
      });
    });
  });
}
