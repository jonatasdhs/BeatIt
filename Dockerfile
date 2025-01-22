FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BeatIt.sln ./
COPY BeatIt/BeatIt.csproj BeatIt/
COPY BeatIt.Tests/BeatIt.Tests.csproj  BeatIt.Tests/

RUN dotnet restore

COPY wait-for-it.sh /app/wait-for-it.sh
RUN chmod +x /app/wait-for-it.sh

COPY . .

WORKDIR /src/BeatIt
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .

EXPOSE 5000
EXPOSE 5001

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--", "dotnet", "BeatIt.dll"]