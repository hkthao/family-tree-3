FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /backend

# Copy the entire backend directory into the container
COPY . .

# Restore dependencies
RUN dotnet restore backend.sln --verbosity minimal

# Build the application
RUN dotnet build backend.sln -c Release --no-restore

# Publish the application
WORKDIR /backend/src/Web
RUN dotnet publish -c Release -o /app/publish

# Final production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy only the published output from the build stage
COPY --from=build /app/publish .

# Set the entrypoint for the application
ENTRYPOINT ["dotnet", "backend.Web.dll"]