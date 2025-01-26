# Define the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY EducationCourse.csproj .
RUN dotnet restore EducationCourse.csproj

# Copy the rest of the source code
COPY . .

# Publish the application in Release configuration
RUN dotnet publish EducationCourse.csproj -c Release -o /app/publish

# Define the final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the working directory in the final container
WORKDIR /app

# Expose necessary ports
EXPOSE 8080
EXPOSE 8081

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Define the entry point to run the application
ENTRYPOINT ["dotnet", "EducationCourse.dll"]
