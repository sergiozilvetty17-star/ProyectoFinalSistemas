# ================================
# BUILD
# ================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["EcommerceApp.csproj", "./"]

RUN dotnet restore "EcommerceApp.csproj"

COPY . .

RUN dotnet publish "EcommerceApp.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ================================
# RUNTIME
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "EcommerceApp.dll"]
