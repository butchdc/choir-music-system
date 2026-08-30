FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["choir-music-system.csproj", "./"]
RUN dotnet restore "choir-music-system.csproj"

COPY . .

RUN dotnet publish "choir-music-system.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p \
    /app/Data \
    /app/Storage/Songs \
    /app/Storage/Generated/MusicPacks

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "choir-music-system.dll"]