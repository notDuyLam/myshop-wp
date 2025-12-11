# Database Setup Guide

## Cài đặt PostgreSQL

### Option 1: Cài đặt trực tiếp trên Windows

1. Tải PostgreSQL từ: https://www.postgresql.org/download/windows/
2. Cài đặt với default settings
3. Nhớ password của user `postgres` (mặc định)

### Option 2: Sử dụng Docker

```bash
docker run -e POSTGRES_PASSWORD=password -p 5432:5432 postgres
```

## Tạo Database

1. Mở pgAdmin hoặc psql command line
2. Tạo database mới:
```sql
CREATE DATABASE myshop_db;
```

3. Kết nối đến database `myshop_db` và chạy script `create_database.sql`:
```bash
psql -U postgres -d myshop_db -f create_database.sql
```

Hoặc copy nội dung file `create_database.sql` và chạy trong pgAdmin Query Tool.

## Connection String Format

Sau khi setup xong, connection string sẽ có format:
```
Host=localhost;Port=5432;Database=myshop_db;Username=postgres;Password=your_password
```

Lưu ý: Thay `your_password` bằng password thực tế của bạn.

