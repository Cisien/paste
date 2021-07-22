FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Paste.Server/Paste.Server.csproj", "Paste.Server/"]
COPY ["Paste.Shared/Paste.Shared.csproj", "Paste.Shared/"]
RUN dotnet restore "Paste.Server/Paste.Server.csproj"
COPY . .
WORKDIR "/src/Paste.Server"
RUN dotnet build "Paste.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Paste.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Paste.Server.dll"]