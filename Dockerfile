# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TaxCalculator.slnx .
COPY src/TaxCalculator.Domain/TaxCalculator.Domain.csproj src/TaxCalculator.Domain/
COPY src/TaxCalculator.Application/TaxCalculator.Application.csproj src/TaxCalculator.Application/
COPY src/TaxCalculator.Infrastructure/TaxCalculator.Infrastructure.csproj src/TaxCalculator.Infrastructure/
COPY src/TaxCalculator.API/TaxCalculator.API.csproj src/TaxCalculator.API/

RUN dotnet restore src/TaxCalculator.API/TaxCalculator.API.csproj

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
COPY src/ src/
RUN dotnet publish src/TaxCalculator.API/TaxCalculator.API.csproj -c Release -o /app/publish /p:UseAppHost=false


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaxCalculator.API.dll"]