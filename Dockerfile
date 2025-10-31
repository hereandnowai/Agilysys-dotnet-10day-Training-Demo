# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["First_Sample_Project_Prompting/First_Sample_Project_Prompting.csproj", "First_Sample_Project_Prompting/"]
RUN dotnet restore "First_Sample_Project_Prompting/First_Sample_Project_Prompting.csproj"

# Copy all files and build
COPY . .
WORKDIR "/src/First_Sample_Project_Prompting"
RUN dotnet build "First_Sample_Project_Prompting.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "First_Sample_Project_Prompting.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "First_Sample_Project_Prompting.dll"]
