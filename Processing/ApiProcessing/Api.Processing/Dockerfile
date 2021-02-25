#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Api.Processing/Api.Processing.csproj", "Api.Processing/"]
RUN dotnet restore "Api.Processing/Api.Processing.csproj"
COPY . .
WORKDIR "/src/Api.Processing"
RUN dotnet build "Api.Processing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.Processing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.Processing.dll"]