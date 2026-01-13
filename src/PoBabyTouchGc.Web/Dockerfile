# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/PoBabyTouchGc.Web/PoBabyTouchGc.Web.csproj", "src/PoBabyTouchGc.Web/"]
COPY ["src/PoBabyTouchGc.Client/PoBabyTouchGc.Client.csproj", "src/PoBabyTouchGc.Client/"]
COPY ["src/PoBabyTouchGc.Shared/PoBabyTouchGc.Shared.csproj", "src/PoBabyTouchGc.Shared/"]
COPY ["src/PoBabyTouchGc.ServiceDefaults/PoBabyTouchGc.ServiceDefaults.csproj", "src/PoBabyTouchGc.ServiceDefaults/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Restore dependencies
RUN dotnet restore "src/PoBabyTouchGc.Web/PoBabyTouchGc.Web.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/src/PoBabyTouchGc.Web"
RUN dotnet publish "PoBabyTouchGc.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PoBabyTouchGc.Web.dll"]
