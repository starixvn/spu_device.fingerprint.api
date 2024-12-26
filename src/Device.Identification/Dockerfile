FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

WORKDIR /src

COPY ["./src/Device.Identification/Device.Identification.csproj", "Device.Identification/"]

WORKDIR "/src/Device.Identification"

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.Identification .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.Identification.dll"]
