# Giai đoạn 1: Build code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy toàn bộ source code vào trong Docker
COPY . .

# Restore và Publish project chính (eCommerce.Web)
# Lưu ý: Code dưới đây trỏ thẳng vào thư mục eCommerce.Web
RUN dotnet restore "eCommerce.Web/eCommerce.Web.csproj"
RUN dotnet publish "eCommerce.Web/eCommerce.Web.csproj" -c Release -o /app/out

# Giai đoạn 2: Chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Cấu hình cổng 8080 cho Render
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Chạy file DLL của Web (Tên file này được tạo ra từ project eCommerce.Web)
ENTRYPOINT ["dotnet", "eCommerce.Web.dll"]
