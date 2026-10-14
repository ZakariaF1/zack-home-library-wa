FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY src/HomeLibrary.slnx ./src/
COPY src/HomeLibrary.Domain ./src/HomeLibrary.Domain
COPY src/HomeLibrary.Application ./src/HomeLibrary.Application
COPY src/HomeLibrary.Infrastructure ./src/HomeLibrary.Infrastructure
COPY src/HomeLibrary.Api ./src/HomeLibrary.Api
RUN dotnet restore src/HomeLibrary.Api/HomeLibrary.Api.csproj
RUN dotnet publish src/HomeLibrary.Api/HomeLibrary.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HomeLibrary.Api.dll"]
