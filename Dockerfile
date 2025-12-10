# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CreditoAPI.csproj", "./"]
RUN dotnet restore "CreditoAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "CreditoAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CreditoAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreditoAPI.dll"]
