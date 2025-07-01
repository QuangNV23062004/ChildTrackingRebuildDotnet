# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the entire project (including appsettings.json, .env, etc.)
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out ./

# Optional: Copy environment files and appsettings if they are needed at runtime
COPY appsettings.json appsettings.*.json .env* ./

# Set environment and port
ENV ASPNETCORE_ENVIRONMENT=RestAPI
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "RestAPI.dll"]
