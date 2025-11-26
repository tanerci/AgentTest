# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ProductApi.csproj", "."]
RUN dotnet restore "ProductApi.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "ProductApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --uid 10001 appuser
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ProductApi.dll"]
