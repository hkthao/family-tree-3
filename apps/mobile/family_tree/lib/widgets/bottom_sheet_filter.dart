// lib/widgets/bottom_sheet_filter.dart
import 'package:flutter/material.dart';

class BottomSheetFilter extends StatefulWidget {
  const BottomSheetFilter({super.key});

  @override
  State<BottomSheetFilter> createState() => _BottomSheetFilterState();
}

class _BottomSheetFilterState extends State<BottomSheetFilter> {
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16.0),
      height: 300,
      child: Column(
        children: [
          Text(
            'Lọc kết quả',
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          const Divider(),
          Expanded(
            child: ListView(
              children: const [
                ListTile(
                  title: Text('Loại: Đồng hồ'),
                  trailing: Icon(Icons.check),
                ),
                ListTile(
                  title: Text('Loại: Thành viên'),
                ),
                ListTile(
                  title: Text('Năm: 2000-2010'),
                ),
              ],
            ),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
            },
            child: const Text('Áp dụng'),
          ),
        ],
      ),
    );
  }
}
