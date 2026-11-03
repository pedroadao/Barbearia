FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Barbearia.slnx ./
COPY src/Core/Domain/Barbearia.Domain.csproj src/Core/Domain/
COPY src/Core/Application/Barbearia.Application.csproj src/Core/Application/
COPY src/Infrastructure/Barbearia.Infrastructure.csproj src/Infrastructure/
COPY src/API/Barbearia.Api.csproj src/API/
COPY tests/Barbearia.Tests/Barbearia.Tests.csproj tests/Barbearia.Tests/

RUN dotnet restore src/API/Barbearia.Api.csproj

COPY src/ src/

RUN dotnet publish src/API/Barbearia.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Barbearia.Api.dll"]
