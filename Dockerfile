FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ToDoBackend.API/ToDoBackend.API.csproj", "ToDoBackend.API/"]
COPY ["ToDoBackend.Application/ToDoBackend.Application.csproj", "ToDoBackend.Application/"]
COPY ["ToDoBackend.Infrastructure/ToDoBackend.Infrastructure.csproj", "ToDoBackend.Infrastructure/"]
COPY ["ToDoBackend.Core/ToDoBackend.Core.csproj", "ToDoBackend.Core/"]

RUN dotnet restore "ToDoBackend.API/ToDoBackend.API.csproj"

COPY . .

WORKDIR "/src/ToDoBackend.API"
RUN dotnet build "ToDoBackend.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ToDoBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "ToDoBackend.API.dll"]
