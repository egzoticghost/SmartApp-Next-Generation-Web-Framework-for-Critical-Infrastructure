FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# We copy the project and restore it
COPY ["SmartWebApi.csproj", "./"]
RUN dotnet restore "SmartWebApi.csproj"
# We copy the rest of the files and build
COPY . .
RUN dotnet build "SmartWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartWebApi.dll"]