FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7142

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SmartUni.PublicApi/SmartUni.PublicApi.csproj", "src/SmartUni.PublicApi/"]
RUN dotnet restore "src/SmartUni.PublicApi/SmartUni.PublicApi.csproj"
COPY . .
WORKDIR "/src/src/SmartUni.PublicApi"
RUN dotnet build "SmartUni.PublicApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SmartUni.PublicApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartUni.PublicApi.dll"]
