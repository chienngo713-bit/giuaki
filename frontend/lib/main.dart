import 'dart:convert';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

const navy = Color(0xFF123E6B);
const paleBlue = Color(0xFFF2F7FD);

void main() => runApp(const ToeicApp());

class ToeicApp extends StatelessWidget {
  const ToeicApp({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
        debugShowCheckedModeBanner: false,
        title: 'TOEIC Practice',
        theme: ThemeData(colorSchemeSeed: navy, scaffoldBackgroundColor: paleBlue, useMaterial3: true),
        home: const HomePage(),
      );
}

class ToeicApi {
  static const base = String.fromEnvironment('API_BASE_URL', defaultValue: 'http://localhost:5000');

  Future<List<dynamic>> exams() async {
    final response = await http.get(Uri.parse('$base/api/toeic/exams'));
    _check(response);
    return jsonDecode(response.body) as List<dynamic>;
  }

  Future<Map<String, dynamic>> questions(int id) async {
    final response = await http.get(Uri.parse('$base/api/toeic/exams/$id/questions'));
    _check(response);
    return Map<String, dynamic>.from(jsonDecode(response.body));
  }

  Future<Map<String, dynamic>> submit(int id, Map<int, String> answers) async {
    final response = await http.post(Uri.parse('$base/api/toeic/exams/$id/submit'),
        headers: {'Content-Type': 'application/json'}, body: jsonEncode({'answers': answers.map((k, v) => MapEntry('$k', v))}));
    _check(response);
    return Map<String, dynamic>.from(jsonDecode(response.body));
  }

  Future<Map<String, dynamic>> validate(Uint8List bytes, String name) async {
    final request = http.MultipartRequest('POST', Uri.parse('$base/api/toeic/exams/validate'))
      ..files.add(http.MultipartFile.fromBytes('file', bytes, filename: name));
    final response = await request.send();
    final body = await response.stream.bytesToString();
    if (response.statusCode >= 400) throw Exception(body);
    return Map<String, dynamic>.from(jsonDecode(body));
  }

  Future<void> importWord(Uint8List bytes, String name) async {
    final request = http.MultipartRequest('POST', Uri.parse('$base/api/toeic/exams/import'))
      ..files.add(http.MultipartFile.fromBytes('file', bytes, filename: name));
    final response = await request.send();
    if (response.statusCode >= 400) throw Exception(await response.stream.bytesToString());
  }

  void _check(http.Response response) {
    if (response.statusCode >= 400) throw Exception('Yêu cầu thất bại (${response.statusCode})');
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});
  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int tab = 0;
  final api = ToeicApi();

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('TOEIC Practice', style: TextStyle(fontWeight: FontWeight.bold)), foregroundColor: navy),
        body: tab == 0 ? StudentPage(api: api) : AdminPage(api: api),
        bottomNavigationBar: NavigationBar(selectedIndex: tab, onDestinationSelected: (value) => setState(() => tab = value), destinations: const [
          NavigationDestination(icon: Icon(Icons.assignment_outlined), label: 'Đề thi'),
          NavigationDestination(icon: Icon(Icons.settings_outlined), label: 'Admin'),
        ]),
      );
}

class StudentPage extends StatefulWidget {
  const StudentPage({required this.api, super.key});
  final ToeicApi api;
  @override
  State<StudentPage> createState() => _StudentPageState();
}

class _StudentPageState extends State<StudentPage> {
  late Future<List<dynamic>> future = widget.api.exams();

  @override
  Widget build(BuildContext context) => RefreshIndicator(
        onRefresh: () async => setState(() => future = widget.api.exams()),
        child: FutureBuilder<List<dynamic>>(
          future: future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) return const Center(child: CircularProgressIndicator());
            if (snapshot.hasError) return Center(child: Text('Không tải được đề thi\n${snapshot.error}', textAlign: TextAlign.center));
            final items = snapshot.data ?? [];
            if (items.isEmpty) return const Center(child: Text('Chưa có đề thi.'));
            return ListView(padding: const EdgeInsets.all(16), children: [
              const Text('LUYỆN THI READING', style: TextStyle(color: navy, fontWeight: FontWeight.bold, letterSpacing: 1)),
              const SizedBox(height: 6), const Text('Sẵn sàng luyện đọc?', style: TextStyle(fontSize: 25, fontWeight: FontWeight.bold, color: navy)),
              const SizedBox(height: 18),
              ...items.map((exam) => ExamCard(api: widget.api, exam: Map<String, dynamic>.from(exam))),
            ]);
          },
        ),
      );
}

class ExamCard extends StatelessWidget {
  const ExamCard({required this.api, required this.exam, super.key});
  final ToeicApi api;
  final Map<String, dynamic> exam;
  @override
  Widget build(BuildContext context) => Card(
        margin: const EdgeInsets.only(bottom: 12),
        child: ListTile(
          title: Text('${exam['examTitle'] ?? exam['examCode'] ?? 'Đề TOEIC'}', style: const TextStyle(fontWeight: FontWeight.bold)),
          subtitle: Text('${exam['totalQuestions'] ?? 0} câu   •   ${exam['timeLimitMinutes'] ?? 75} phút'),
          trailing: FilledButton(onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => ExamPage(api: api, exam: exam))), child: const Text('Bắt đầu')),
        ),
      );
}

class ExamPage extends StatefulWidget {
  const ExamPage({required this.api, required this.exam, super.key});
  final ToeicApi api;
  final Map<String, dynamic> exam;
  @override
  State<ExamPage> createState() => _ExamPageState();
}

class _ExamPageState extends State<ExamPage> {
  Map<String, dynamic>? data;
  final answers = <int, String>{};
  int index = 0;

  @override
  void initState() { super.initState(); widget.api.questions(widget.exam['examId'] as int).then((value) => setState(() => data = value)); }

  List<Map<String, dynamic>> get questions => ((data?['parts'] as List<dynamic>?) ?? []).expand((part) => (part['questions'] as List<dynamic>)).map((q) => Map<String, dynamic>.from(q)).toList();

  @override
  Widget build(BuildContext context) {
    if (data == null) return const Scaffold(body: Center(child: CircularProgressIndicator()));
    final question = questions[index];
    final options = (question['options'] as List<dynamic>).map((o) => Map<String, dynamic>.from(o)).toList();
    return Scaffold(appBar: AppBar(title: Text('Câu ${index + 1}/${questions.length}'), actions: [TextButton(onPressed: () => _submit(), child: const Text('NỘP BÀI'))]), body: ListView(padding: const EdgeInsets.all(16), children: [
      Text(question['questionText']?.toString() ?? '', style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600)), const SizedBox(height: 18),
      ...options.map((option) => RadioListTile<String>(value: option['optionKey'].toString(), groupValue: answers[question['questionId']], onChanged: (value) => setState(() => answers[question['questionId'] as int] = value!), title: Text(option['optionText']?.toString() ?? ''))),
      Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [OutlinedButton(onPressed: index == 0 ? null : () => setState(() => index--), child: const Text('Trước')), FilledButton(onPressed: index == questions.length - 1 ? _submit : () => setState(() => index++), child: Text(index == questions.length - 1 ? 'Nộp bài' : 'Tiếp theo'))]),
    ]));
  }

  Future<void> _submit() async {
    if (answers.length < questions.length && !await _confirm()) return;
    final result = await widget.api.submit(widget.exam['examId'] as int, answers);
    if (!mounted) return;
    await showDialog(context: context, builder: (_) => AlertDialog(title: const Text('Kết quả'), content: Text('Điểm: ${result['score'] ?? 0}/100'), actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('Đóng'))]));
    if (mounted) Navigator.pop(context);
  }

  Future<bool> _confirm() async => await showDialog<bool>(context: context, builder: (_) => AlertDialog(title: const Text('Bạn muốn nộp bài?'), content: Text('Còn ${questions.length - answers.length} câu chưa trả lời.'), actions: [TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Quay lại')), FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Nộp bài'))])) ?? false;
}

class AdminPage extends StatefulWidget {
  const AdminPage({required this.api, super.key});
  final ToeicApi api;
  @override
  State<AdminPage> createState() => _AdminPageState();
}

class _AdminPageState extends State<AdminPage> {
  String status = 'Chọn file Word để kiểm tra và import.';
  bool busy = false;

  Future<void> pickWord() async {
    final result = await FilePicker.platform.pickFiles(withData: true, type: FileType.custom, allowedExtensions: ['docx']);
    if (result == null || result.files.single.bytes == null) return;
    final file = result.files.single;
    setState(() { busy = true; status = 'Đang kiểm tra tags, đáp án và số thứ tự câu...'; });
    try {
      final validation = await widget.api.validate(file.bytes!, file.name);
      if (validation['isValid'] != true) { setState(() => status = 'File không hợp lệ: ${(validation['errors'] as List<dynamic>? ?? []).join(', ')}'); return; }
      await widget.api.importWord(file.bytes!, file.name);
      setState(() => status = 'Import thành công. Đề đã sẵn sàng cho thí sinh.');
    } catch (error) { setState(() => status = 'Import thất bại: $error'); } finally { setState(() => busy = false); }
  }

  @override
  Widget build(BuildContext context) => ListView(padding: const EdgeInsets.all(16), children: [const Text('ADMIN MOBILE', style: TextStyle(color: navy, fontWeight: FontWeight.bold, letterSpacing: 1)), const SizedBox(height: 6), const Text('Quản lý đề thi', style: TextStyle(fontSize: 25, fontWeight: FontWeight.bold, color: navy)), const SizedBox(height: 24), Card(child: Padding(padding: const EdgeInsets.all(18), child: Column(children: [const Icon(Icons.upload_file, size: 44, color: navy), const SizedBox(height: 12), Text(status, textAlign: TextAlign.center), const SizedBox(height: 18), FilledButton.icon(onPressed: busy ? null : pickWord, icon: const Icon(Icons.file_open), label: Text(busy ? 'Đang xử lý...' : 'Chọn file Word'))]))]);
}
