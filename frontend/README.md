# TOEIC Frontend

Frontend mobile được tách riêng bằng Flutter/Dart. Ứng dụng gọi API ASP.NET trong thư mục `../backend/Chien/10.2.0/aspnet-core`.

Chạy:

```powershell
cd frontend
flutter pub get
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000
```

Trên trình duyệt hoặc thiết bị thật, thay `10.0.2.2` bằng địa chỉ máy chạy backend.

