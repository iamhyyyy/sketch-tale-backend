# 🎨 SketchTale Backend API

> **SketchTale** là nền tảng ứng dụng giáo dục và sáng tạo truyện tranh dành cho trẻ em kết hợp trí tuệ nhân tạo (AI). Hệ thống cung cấp giải pháp cho phụ huynh đồng hành cùng con trong quá trình phát triển tư duy ngôn ngữ, trí tưởng tượng qua tranh vẽ và câu chuyện tương tác.

---

## 🏛️ Kiến Trúc Hệ Thống (Architecture)

Dự án được xây dựng theo mô hình **Clean Architecture (Onion Architecture)** 4 tầng chuẩn mực cho các hệ thống enterprise ASP.NET Core, đảm bảo tính độc lập giữa các tầng, dễ mở rộng, dễ bảo trì và dễ viết kiểm thử:

```
+-------------------------------------------------------------+
|                     sketch_tale.API                         |
|     (Presentation Layer: Controllers, Middlewares, Program) |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                  sketch_tale.Application                    |
|      (Use Cases, Business Services, DTOs, Mappings)         |
+-------------------------------------------------------------+
               /                              \
              v                                v
+---------------------------+    +----------------------------+
|    sketch_tale.Domain     |    |  sketch_tale.Infrastructure|
| (Entities, Enums, Common) |<---|  (EF Core, Repos, Services)|
+---------------------------+    +----------------------------+
```

### Các tầng kiến trúc:

1. **`sketch_tale.Domain` (Core Domain Layer):**
   - Tầng trung tâm, **hoàn toàn độc lập** và không phụ thuộc vào bất kỳ thư viện hay tầng nào bên ngoài.
   - Định nghĩa các Entities (`User`, `ParentProfile`, `ChildProfile`, `StoryTemplate`, `Subscription`, v.v.).
   - Định nghĩa `BaseEntity` (chứa các metadata audit: `Id`, `CreatedAt`, `CreateBy`, `UpdatedAt`, `UpdateBy`).
   - Định nghĩa các Enums trạng thái và Interface nền tảng.

2. **`sketch_tale.Application` (Business Logic Layer):**
   - Đóng gói toàn bộ logic nghiệp vụ (Use Cases) của hệ thống.
   - Định nghĩa Interfaces cho Services (`IAuthService`, `IStoryTemplateService`,...) và Repositories (`IParentProfileRepository`, `IUnitOfWork`,...).
   - Data Transfer Objects (DTOs) và cấu hình chuyển đổi dữ liệu qua `AutoMapper`.

3. **`sketch_tale.Infrastructure` (Data Access & External Services Layer):**
   - Quản lý cơ sở dữ liệu với `AppDbContext` (kế thừa `AuditIdentityDbContext`), Migrations và Seed Data.
   - Triển khai cụ thể các Repository (`GenericRepository<T>`, `ParentProfileRepository`, `SubscriptionRepository`,...) và `UnitOfWork`.
   - Tích hợp dịch vụ bên ngoài: Gửi email (`SendGridService`), lưu trữ đám mây (`CloudinaryService`), sinh mã bảo mật (`JwtService`).

4. **`sketch_tale.API` (Presentation Layer):**
   - Điểm vào của ứng dụng (Entry Point).
   - RESTful API Controllers định tuyến HTTP request và trả về response chuẩn hóa.
   - Cấu hình Dependency Injection (DI), Authentication JWT Bearer, Authorization Policies, CORS và Swagger/OpenAPI.

---

## 🛠️ Công Nghệ & Thư Viện Sử Dụng

- **Framework:** .NET 8 (C# 12)
- **Database:** PostgreSQL (Neon Cloud Database)
- **ORM:** Entity Framework Core 8 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication & Identity:** ASP.NET Core Identity & JWT Bearer Token (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Object Mapping:** AutoMapper
- **Third-Party Integrations:**
  - **SendGrid:** Gửi email kích hoạt tài khoản & khôi phục mật khẩu.
  - **Cloudinary:** Lưu trữ và quản lý tài nguyên hình ảnh / âm thanh.
  - **Audit.NET (`Audit.EntityFramework.Core`):** Tự động ghi vết kiểm toán (Audit Trail) cho toàn bộ thay đổi dữ liệu.
- **API Documentation:** Swagger / OpenAPI UI

---

## 💡 Các Nguyên Tắc & Quy Ước Lập Trình (Best Practices)

1. **Repository & Unit of Work Pattern:**
   - Các Business Services (như `AuthService`, `SubscriptionService`,...) **không truy vấn LINQ to Entities trực tiếp vào Database Context**, mọi truy vấn dữ liệu theo điều kiện nghiệp vụ được đưa vào Repository tương ứng để đảm bảo tính đóng gói và hiệu năng.
   - Các thao tác thêm/sửa/xóa qua nhiều repository được commit đồng thời thông qua `_unitOfWork.CompleteAsync()`.

2. **Quy chuẩn múi giờ Việt Nam (UTC+7):**
   - Hệ thống áp dụng thống nhất thời gian theo múi giờ `UTC+7` cho tất cả các giao dịch và dữ liệu:
     ```csharp
     DateTime.UtcNow.AddHours(7);
     ```
   - Trong `AppDbContext.cs`, phương thức `SaveChangesAsync` tự động gán `CreatedAt` và `UpdatedAt` bằng thời gian `UTC+7` mỗi khi thêm hoặc cập nhật bản ghi.

3. **Cơ chế Phân Quyền (Role-Based Access Control - RBAC):**
   - Hỗ trợ 4 vai trò chính:
     - `Admin`: Quản trị toàn bộ hệ thống, bảng giá, phân quyền và kiểm duyệt.
     - `ContentManager`: Quản lý nội dung bản mẫu truyện, chủ đề, câu đố và từ vựng.
     - `Parent`: Quản lý gia đình, tạo hồ sơ con, đăng ký gói dịch vụ và theo dõi tiến trình học của bé.
     - `Child`: Tham gia đọc truyện, giải đố, vẽ tranh và sáng tạo nội dung.
   

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy (Getting Started)

### 1. Yêu cầu hệ thống
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Cơ sở dữ liệu PostgreSQL (hoặc tài khoản Neon PostgreSQL)

### 2. Cấu hình chuỗi kết nối
Mở file `sketch_tale.API/appsettings.Development.json` và cấu hình thông tin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<host>; Database=<db>; Username=<user>; Password=<pwd>; SSL Mode=VerifyFull; Channel Binding=Require;"
  },
  "JwtSettings": {
    "Secret": "<YOUR_SECRET_KEY>",
    "ExpiryMinutes": 60
  },
  "SendGridSettings": {
    "ApiKey": "<YOUR_SENDGRID_API_KEY>"
  }
}
```

### 3. Cài đặt và biên dịch dự án
```powershell
# Restore các package NuGet
dotnet restore

# Biên dịch dự án
dotnet build
```

### 4. Khởi chạy ứng dụng
```powershell
# Khởi chạy dự án API
dotnet run --project sketch_tale.API
```
> Khi khởi động lần đầu, hệ thống sẽ tự động chạy các migration và nạp sẵn dữ liệu khởi tạo (`SeedData.cs`).

- **Swagger UI:** [http://localhost:8080/swagger](http://localhost:8080/swagger) hoặc [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 👥 Tài Khoản Mặc Định Thử Nghiệm (Seed Accounts)

| Vai trò | Username | Mật khẩu mặc định | Mục đích sử dụng |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `Admin@123` | Quản trị viên hệ thống |
| **Content Manager** | `manager` | `Manager@123` | Biên tập viên nội dung truyện |
| **Parent** | `parent` | `Parent@123` | Tài khoản phụ huynh mẫu |
| **Child** | `child` | `Child@123` | Tài khoản trẻ em mẫu |
