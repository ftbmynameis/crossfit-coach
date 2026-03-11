# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY server/ .
RUN dotnet restore CrossfitCoach.sln
RUN dotnet publish src/CrossfitCoach.Api/CrossfitCoach.Api.csproj \
    -c Release \
    --no-restore \
    -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /publish .
EXPOSE 8080
ENTRYPOINT ["./CrossfitCoach.Api"]
