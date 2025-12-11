# MyShop - Hệ thống Quản lý Cửa hàng

Ứng dụng WinUI 3 quản lý cửa hàng với PostgreSQL database.

## Yêu cầu hệ thống

- Windows 10/11
- .NET 8.0 SDK
- PostgreSQL Server (local hoặc remote)

## Cài đặt và Chạy

### 1. Clone và Restore packages

```bash
cd myshop
dotnet restore
```

### 2. Build project

```bash
dotnet build -p:Platform=x64
```

### 3. Chạy ứng dụng

- **Visual Studio**: Nhấn F5 hoặc chọn "Start Debugging"
- **Command line**: 
  ```bash
  cd myshop
  dotnet run -p:Platform=x64
  ```

## Kết nối Database

### Cách 1: Dùng Default Config (Nhanh nhất)

Mở file `myshop/Services/DatabaseConfigService.cs` và sửa password mặc định ở dòng 95:

```csharp
Password = "your_password" // Thay đổi password của bạn
```

App sẽ tự động dùng config này nếu chưa có file `database.json`.

### Cách 2: Qua UI ConfigPage

1. Chạy app
2. Vào màn hình **Cài đặt** → **Cấu hình Server**
3. Nhập thông tin:
   - **Server IP/Hostname**: `localhost` (hoặc IP server)
   - **Port**: `5432`
   - **Database**: `myshop_db`
   - **Username**: `postgres`
   - **Password**: Password của bạn
4. Nhấn **"Test kết nối"** để kiểm tra
5. Nhấn **"Lưu cấu hình"** để lưu

### Cách 3: Chỉnh sửa file database.json trực tiếp

1. Chạy app và lưu config một lần (tạo file)
2. Nhấn **"Mở file config"** để mở file `database.json`
3. Chỉnh sửa trực tiếp bằng Notepad:
   ```json
   {
     "Host": "localhost",
     "Port": 5432,
     "Database": "myshop_db",
     "Username": "postgres",
     "Password": "your_password"
   }
   ```
4. Lưu file và restart app

## Setup Database

### Tạo Database

1. Mở pgAdmin hoặc psql
2. Tạo database:
   ```sql
   CREATE DATABASE myshop_db;
   ```

3. Chạy script tạo bảng:
   ```bash
   psql -U postgres -d myshop_db -f myshop-data/Scripts/create_database.sql
   ```

Hoặc copy nội dung file `myshop-data/Scripts/create_database.sql` và chạy trong pgAdmin.

### Docker (Tùy chọn)

```bash
docker run -e POSTGRES_PASSWORD=password -p 5432:5432 postgres
```

## Cấu trúc Project

```
myshop/
├── myshop/              # WinUI 3 Application
│   ├── Views/          # XAML Pages
│   ├── ViewModels/     # MVVM ViewModels
│   ├── Services/       # Business Logic
│   └── Helpers/        # Utilities
│
└── myshop-data/        # Data Access Layer
    ├── Models/         # Entity Models
    ├── Data/           # DbContext
    └── Scripts/        # SQL Scripts
```

## Chức năng chính

- ✅ Đăng nhập với auto-login
- ✅ Dashboard tổng quan
- ✅ Quản lý sản phẩm (CRUD, tìm kiếm, lọc)
- ✅ Quản lý đơn hàng
- ✅ Báo cáo thống kê
- ✅ Cấu hình chương trình

## Tech Stack

- **Frontend**: WinUI 3, C# .NET 8
- **Backend**: Entity Framework Core
- **Database**: PostgreSQL
- **Architecture**: MVVM Pattern

## Troubleshooting

**Lỗi kết nối database?**
- Kiểm tra PostgreSQL đang chạy
- Kiểm tra password trong config
- Kiểm tra firewall/network nếu dùng remote server

**Không tìm thấy file database.json?**
- File chỉ được tạo khi bạn nhấn "Lưu cấu hình" lần đầu
- Hoặc app sẽ dùng default config trong code

## Liên hệ

Project được phát triển cho môn Windows Programming.

