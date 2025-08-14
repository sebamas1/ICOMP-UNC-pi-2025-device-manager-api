# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/Web/Web.csproj ./src/Web/Web.csproj
COPY src/Business/Business.csproj ./src/Business/Business.csproj
COPY src/Data/Data.csproj ./src/Data/Data.csproj

RUN dotnet restore ./src/Web/Web.csproj

COPY . ./ 

# Publish
RUN dotnet publish ./src/Web/Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Web.dll"]
