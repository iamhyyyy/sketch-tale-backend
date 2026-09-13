# ==========================================
# Step 1: Base - Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
# Render cần biết container chạy ở port nào. .NET 8+ mặc định là 8080.
EXPOSE 8080
EXPOSE 8081

# ==========================================
# Step 2: Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy tất cả các file .csproj vào đúng thư mục để phục hồi thư viện (Restore)
COPY ["sketch_tale.API/sketch_tale.API.csproj", "sketch_tale.API/"]
COPY ["sketch_tale.Application/sketch_tale.Application.csproj", "sketch_tale.Application/"]
COPY ["sketch_tale.Domain/sketch_tale.Domain.csproj", "sketch_tale.Domain/"]
COPY ["sketch_tale.Infrastructure/sketch_tale.Infrastructure.csproj", "sketch_tale.Infrastructure/"]

# Khôi phục các gói NuGet
RUN dotnet restore "sketch_tale.API/sketch_tale.API.csproj"

# Copy toàn bộ code còn lại vào container
COPY . .
WORKDIR "/src/sketch_tale.API"
RUN dotnet build "sketch_tale.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# Step 3: Publish
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "sketch_tale.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# Step 4: Final - Run
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sketch_tale.API.dll"]