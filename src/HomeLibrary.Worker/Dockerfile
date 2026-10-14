FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY src/HomeLibrary.slnx ./src/
COPY src/HomeLibrary.Domain ./src/HomeLibrary.Domain
COPY src/HomeLibrary.Application ./src/HomeLibrary.Application
COPY src/HomeLibrary.Infrastructure ./src/HomeLibrary.Infrastructure
COPY src/HomeLibrary.Worker ./src/HomeLibrary.Worker
RUN dotnet restore src/HomeLibrary.Worker/HomeLibrary.Worker.csproj
RUN dotnet publish src/HomeLibrary.Worker/HomeLibrary.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HomeLibrary.Worker.dll"]
