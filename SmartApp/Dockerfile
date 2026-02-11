FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartApp.csproj ./
RUN dotnet restore "./SmartApp.csproj"

COPY . .
RUN dotnet publish "./SmartApp.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY --from=build /app/publish/wwwroot .
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
# ENTRYPOINT ["dotnet", "SmartApp.dll"]