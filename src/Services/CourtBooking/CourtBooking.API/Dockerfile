# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/CourtBooking/CourtBooking.API/CourtBooking.API.csproj", "src/Services/CourtBooking/CourtBooking.API/"]
COPY ["src/Services/CourtBooking/CourtBooking.Application/CourtBooking.Application.csproj", "src/Services/CourtBooking/CourtBooking.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/BuildingBlocks/"]
COPY ["src/Services/CourtBooking/CourtBooking.Domain/CourtBooking.Domain.csproj", "src/Services/CourtBooking/CourtBooking.Domain/"]
COPY ["src/Services/CourtBooking/CourtBooking.Infrastructure/CourtBooking.Infrastructure.csproj", "src/Services/CourtBooking/CourtBooking.Infrastructure/"]
RUN dotnet restore "./src/Services/CourtBooking/CourtBooking.API/CourtBooking.API.csproj"
COPY . .
WORKDIR "/src/src/Services/CourtBooking/CourtBooking.API"
RUN dotnet build "./CourtBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CourtBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourtBooking.API.dll"]