FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/Chien.Web.Host/Chien.Web.Host.csproj", "src/Chien.Web.Host/"]
COPY ["src/Chien.Web.Core/Chien.Web.Core.csproj", "src/Chien.Web.Core/"]
COPY ["src/Chien.Application/Chien.Application.csproj", "src/Chien.Application/"]
COPY ["src/Chien.Core/Chien.Core.csproj", "src/Chien.Core/"]
COPY ["src/Chien.EntityFrameworkCore/Chien.EntityFrameworkCore.csproj", "src/Chien.EntityFrameworkCore/"]
WORKDIR "/src/src/Chien.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Chien.Web.Host", "src/Chien.Web.Host"]
COPY ["src/Chien.Web.Core", "src/Chien.Web.Core"]
COPY ["src/Chien.Application", "src/Chien.Application"]
COPY ["src/Chien.Core", "src/Chien.Core"]
COPY ["src/Chien.EntityFrameworkCore", "src/Chien.EntityFrameworkCore"]
WORKDIR "/src/src/Chien.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Chien.Web.Host.dll"]
