# Hướng dẫn đồng bộ TOÀN BỘ dự án cho bạn

## Vấn đề hiện tại
- Bạn của bạn deploy lên https://viettech.onrender.com nhưng:
  - ❌ Thiếu nhiều chức năng
  - ❌ Code cũ, không mới
  - ❌ Lỗi VNPay, voucher không có
  - ❌ Thiếu hình ảnh sản phẩm

## Nguyên nhân
Thiếu 3 thành phần:
1. **Code mới nhất** (đã push ✓)
2. **Database có đầy đủ data** (products, vouchers, users...)
3. **Hình ảnh** (wwwroot/images)

---

## CÁCH 1: ĐƠN GIẢN NHẤT - Dùng PostgreSQL trên Render

### Bước 1: Bạn của bạn tạo PostgreSQL Database trên Render

1. Vào https://dashboard.render.com/
2. Click **New +** → **PostgreSQL**
3. Đặt tên: `viettech-db`
4. Chọn **Free Plan**
5. Click **Create Database**
6. Copy **Internal Database URL**

### Bước 2: Cài PostgreSQL provider

Bảo bạn chạy:
```bash
dotnet add eCommerce.Infrastructure/eCommerce.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Bước 3: Set Environment Variables trên Render

Vào service VietTech → Environment → Add:

```
ASPNETCORE_ENVIRONMENT=Production
DatabaseProvider=PostgreSQL
ConnectionStrings__VietTechConnection=<Internal Database URL từ bước 1>
VnPay__TmnCode=UUX88K9W
VnPay__HashSecret=RA7XT5LCN57930GIPZPNBTAN8G7XIIC3
VnPay__ReturnUrl=https://viettech.onrender.com/Payment/Result
```

### Bước 4: Sửa Program.cs hỗ trợ PostgreSQL

Thêm vào Program.cs (sau dòng UseSqlServer):

```csharp
else if (dbProvider == "PostgreSQL")
{
    options.UseNpgsql(connectionString);
    Console.WriteLine("--> Using PostgreSQL DB");
}
```

### Bước 5: Migration database

```bash
dotnet ef migrations add InitialPostgreSQL --project eCommerce.Infrastructure --startup-project eCommerce.Web
dotnet ef database update --project eCommerce.Infrastructure --startup-project eCommerce.Web
```

### Bước 6: Seed data

Chạy app local kết nối PostgreSQL production, đăng nhập admin, thêm:
- Categories
- Brands  
- Products (upload hình)
- Vouchers

---

## CÁCH 2: NÂNG CAO - Backup/Restore SQL Server

### A. TRÊN MÁY BẠN (Export data)

#### 1. Export database ra file SQL

```powershell
# Cài SQL Server module
Install-Module -Name SqlServer -Force -Scope CurrentUser

# Export data
$tables = @("AspNetUsers", "AspNetRoles", "AspNetUserRoles", "Brands", "Categories", "Products", "Vouchers", "Orders", "OrderItems", "LoyaltyPoints", "Reviews")

$output = "-- VietTech2 Data Export`n`n"

foreach ($table in $tables) {
    $data = Invoke-Sqlcmd -ServerInstance "(localdb)\MSSQLLocalDB" -Database "VietTech2" -Query "SELECT * FROM [$table]" -ErrorAction SilentlyContinue
    
    if ($data) {
        $output += "-- $table`nSET IDENTITY_INSERT [$table] ON;`n"
        
        foreach ($row in $data) {
            # (Code to generate INSERT statements)
        }
        
        $output += "SET IDENTITY_INSERT [$table] OFF;`n`n"
    }
}

$output | Out-File "database_export.sql" -Encoding UTF8
```

#### 2. Nén wwwroot/images

```powershell
Compress-Archive -Path "eCommerce.Web\wwwroot\images\*" -DestinationPath "images_backup.zip" -Force
```

#### 3. Gửi cho bạn

- `database_export.sql`
- `images_backup.zip`

### B. TRÊN MÁY BẠN BẠN (Import data)

#### 1. Pull code mới

```bash
git pull origin main
```

#### 2. Chạy migration

```bash
dotnet ef database update --project eCommerce.Infrastructure --startup-project eCommerce.Web
```

#### 3. Import data

```bash
# Dùng SQL Server Management Studio hoặc:
sqlcmd -S (localdb)\MSSQLLocalDB -d VietTech2 -i database_export.sql
```

#### 4. Extract hình ảnh

```powershell
Expand-Archive -Path images_backup.zip -DestinationPath eCommerce.Web\wwwroot\images -Force
```

---

## CÁCH 3: NHANH NHẤT - Chia sẻ LocalDB file

### A. TRÊN MÁY BẠN

```powershell
# Dừng app
# Copy file database
Copy-Item "C:\Users\ASUS\VietTech2.mdf" -Destination "VietTech2.mdf"
Copy-Item "C:\Users\ASUS\VietTech2_log.ldf" -Destination "VietTech2_log.ldf"

# Nén và gửi
Compress-Archive -Path "VietTech2.*" -DestinationPath "database_files.zip"
```

### B. TRÊN MÁY BẠN BẠN

```powershell
# Extract
Expand-Archive database_files.zip

# Attach database
sqlcmd -Q "CREATE DATABASE VietTech2 ON (FILENAME = 'C:\Path\VietTech2.mdf'), (FILENAME = 'C:\Path\VietTech2_log.ldf') FOR ATTACH"
```

---

## TÓM TẮT: Nên dùng CÁCH 1 (PostgreSQL)

**Ưu điểm:**
- ✅ Miễn phí trên Render
- ✅ Không cần backup/restore phức tạp
- ✅ Dễ quản lý
- ✅ Bạn seed data trực tiếp lên production

**Bước tiếp theo:**
1. Bạn push code mới nhất lên GitHub (đã xong ✓)
2. Bạn của bạn pull code
3. Bạn của bạn tạo PostgreSQL trên Render
4. Set Environment Variables
5. Deploy
6. Bạn login admin → seed data (categories, products, vouchers...)

---

## Checklist cuối cùng

- [ ] Code mới nhất đã push lên GitHub
- [ ] Bạn bạn đã pull code
- [ ] Database production đã setup (PostgreSQL hoặc SQL Server)
- [ ] Environment variables đã set đúng
- [ ] Images đã upload (hoặc seed lại qua admin panel)
- [ ] Test đầy đủ:
  - [ ] Đăng nhập
  - [ ] Xem sản phẩm (có hình)
  - [ ] Voucher hiển thị
  - [ ] Thanh toán VNPay OK
  - [ ] Order được tạo

Done! 🎉
