# TOEIC Reading — Backend (ASP.NET Boilerplate) + Frontend (Flutter)

Đồ án ôn luyện kỹ năng Reading TOEIC. Repo gồm 2 phần độc lập:

| Thư mục | Nội dung | Công nghệ |
| --- | --- | --- |
| `backend/` | Web MVC + Web API, CSDL đề thi | ASP.NET Boilerplate 10.2.0, ASP.NET Core MVC, EF Core, SQL Server |
| `frontend/` | Ứng dụng di động gọi API backend | Flutter / Dart |

## 1. Cấu trúc thư mục

```
backend/
  Chien/10.2.0/aspnet-core/     # Solution ABP (Chien.sln)
    src/
      Chien.Core/               # Domain, entity, authorization
      Chien.Application/        # Application service
      Chien.EntityFrameworkCore/# DbContext, migrations, seed
      Chien.Web.Core/           # Controller API dùng chung (JWT)
      Chien.Web.Host/           # Host Web API  -> /api/toeic/...
      Chien.Web.Mvc/            # Giao diện quản trị MVC
      Chien.Migrator/           # Chạy migration + seed
    test/                       # Unit test / integration test
  database/                     # TOEIC_Exam_Database.sql + sơ đồ CSDL
  assets/                       # Đề thi & dữ liệu mẫu (.docx)
  tools/                        # Script build/import dữ liệu tagged
frontend/
  lib/main.dart                 # Ứng dụng Flutter
  pubspec.yaml
```

## 2. Yêu cầu môi trường

- .NET SDK 8.0 trở lên (theo `Chien.Web.Host` / `Chien.Web.Mvc`)
- SQL Server (LocalDB hoặc SQL Server Express/Developer)
- Flutter SDK 3.x + Dart 3.x
- (tuỳ chọn) Node.js + gulp nếu muốn build lại `wwwroot/libs`

## 3. Chuẩn bị cơ sở dữ liệu

Chuỗi kết nối nằm trong `appsettings.json` của `Chien.Web.Host` và `Chien.Web.Mvc`
(`ConnectionStrings:Default` cho CSDL ABP, `ConnectionStrings:Toeic` cho CSDL đề thi).

```powershell
cd backend\Chien\10.2.0\aspnet-core

# Tạo/migrate CSDL ABP (ChienDb)
dotnet run --project src\Chien.Migrator

# Tạo CSDL đề thi TOEIC từ script
sqlcmd -S localhost -U sa -P <mật-khẩu> -i ..\..\..\database\TOEIC_Exam_Database.sql
```

## 4. Chạy backend

```powershell
cd backend\Chien\10.2.0\aspnet-core

# Web API (Swagger ở /swagger)
dotnet run --project src\Chien.Web.Host --urls http://localhost:5000

# Giao diện MVC (tuỳ chọn)
dotnet run --project src\Chien.Web.Mvc --urls http://localhost:5001
```

Các API TOEIC chính:

| Method | Endpoint |
| --- | --- |
| GET | `/api/toeic/exams` |
| GET | `/api/toeic/exams/{id}/questions` |
| POST | `/api/toeic/exams/{id}/submit` |
| POST | `/api/toeic/exams/validate` |
| POST | `/api/toeic/exams/import` |
| PUT | `/api/toeic/exams/questions/{id}` |

## 5. Chạy frontend Flutter

```powershell
cd frontend
flutter pub get

# Android emulator: backend trên máy host là 10.0.2.2
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000
```

Thiết bị thật hoặc chạy web thì thay `10.0.2.2` bằng IP máy đang chạy backend.

## 6. Ghi chú về repo

- Không commit thư mục build: `bin/`, `obj/`, `buildcheck/`, `.dart_tool/` — đã có trong `.gitignore`.
- Không commit thông tin máy cá nhân: `secrets.json`, `appsettings.*.local.json`.
- Line ending được chuẩn hoá bằng `.gitattributes` (LF trong repo, CRLF khi checkout trên Windows).
- Script tạo lại dữ liệu tagged: `backend/tools/build-tagged.ps1`.
