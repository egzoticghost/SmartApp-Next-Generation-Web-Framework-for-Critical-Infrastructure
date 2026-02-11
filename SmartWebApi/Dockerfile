FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Kopiujemy projekt i restorujemy
COPY ["SmartWebApp.csproj", "./"]
RUN dotnet restore "SmartWebApp.csproj"
# Kopiujemy resztê plików i budujemy
COPY . .
RUN dotnet build "SmartWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartWebApp.dll"]