# TOEIC Backend

Backend của đề tài được giữ đúng yêu cầu: ASP.NET MVC trên ASP.NET Boilerplate và SQL Server.

Mã nguồn hiện tại nằm trong:

`Chien/10.2.0/aspnet-core`

Các API TOEIC chính:

- `GET /api/toeic/exams`
- `GET /api/toeic/exams/{id}/questions`
- `POST /api/toeic/exams/{id}/submit`
- `POST /api/toeic/exams/validate`
- `POST /api/toeic/exams/import`
- `PUT /api/toeic/exams/questions/{id}`

Chạy backend:

```powershell
cd backend\Chien\10.2.0\aspnet-core
dotnet run --no-build --project src\Chien.Web.Mvc --urls http://localhost:5000
```

