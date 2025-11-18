// lib/routes.dart
import 'package:go_router/go_router.dart';
import 'package:family_tree/screens/home/landing_screen.dart';
import 'package:family_tree/screens/search/search_screen.dart';
import 'package:family_tree/screens/watch/watch_detail_screen.dart';
import 'package:family_tree/screens/member/member_profile_screen.dart';
import 'package:family_tree/screens/family/tree_screen.dart';

final GoRouter appRouter = GoRouter(
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => const LandingScreen(),
    ),
    GoRoute(
      path: '/search',
      builder: (context, state) => const SearchScreen(),
    ),
    GoRoute(
      path: '/watch/:id',
      builder: (context, state) => WatchDetailScreen(
        watchId: state.params['id']!,
      ),
    ),
    GoRoute(
      path: '/member/:id',
      builder: (context, state) => MemberProfileScreen(
        memberId: state.params['id']!,
      ),
    ),
    GoRoute(
      path: '/tree',
      builder: (context, state) => const TreeScreen(),
    ),
  ],
);
