FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.Integration.Site/Device.FingerPrint.Integration.Site.csproj", "Device.FingerPrint.Integration.Site/"]

WORKDIR "/src/Device.FingerPrint.Integration.Site"

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.Integration.Site .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine-25h1 AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.Integration.Site.dll"]
