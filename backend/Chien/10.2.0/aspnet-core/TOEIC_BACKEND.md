# Backend TOEIC đã thêm

Module được thêm vào project `Chien.Web.Core` và dùng database `TOEICExamDb`.

## API

| Method | URL | Chức năng |
|---|---|---|
| POST | `/api/toeic/exams/validate` | Đọc DOCX, kiểm tra Tags, số câu, 4 đáp án và đáp án đúng; không lưu dữ liệu |
| POST | `/api/toeic/exams/import` | Validate trước, chỉ lưu toàn bộ đề nếu hợp lệ |
| GET | `/api/toeic/exams` | Danh sách đề thi |
| GET | `/api/toeic/exams/{examId}/preview` | Xem trước đề đã import |
| PUT | `/api/toeic/exams/questions/{questionId}` | Chỉnh sửa nội dung câu hỏi và đáp án |

Upload dùng `multipart/form-data` với tên field là `file`.

## Chạy thử

1. Chạy `database/TOEIC_Exam_Database.sql` trong SQL Server để tạo `TOEICExamDb`.
2. Kiểm tra `ConnectionStrings:Toeic` trong `src/Chien.Web.Host/appsettings.json`.
3. Mở solution `Chien.sln`, restore NuGet và chạy project `Chien.Web.Host`.
4. Mở Swagger, gọi `POST /api/toeic/exams/validate` trước.
5. Chỉ gọi `POST /api/toeic/exams/import` khi response validation có `isValid: true`.

Import có tính nguyên tử ở cấp ứng dụng: nếu validation thất bại, không tạo Exam, Part, Question hay Option nào.
