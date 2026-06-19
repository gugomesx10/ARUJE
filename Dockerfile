FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Aruje-Back-End/Aruje-Back-End.csproj", "Aruje-Back-End/"]
COPY ["Aruje.Application/Aruje.Application.csproj", "Aruje.Application/"]
COPY ["Aruje.Domain/Aruje.Domain.csproj", "Aruje.Domain/"]
COPY ["Aruje.Infrastructure/Aruje.Infrastructure.csproj", "Aruje.Infrastructure/"]

RUN dotnet restore "Aruje-Back-End/Aruje-Back-End.csproj"

COPY . .

RUN dotnet publish "Aruje-Back-End/Aruje-Back-End.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Aruje-Back-End.dll"]