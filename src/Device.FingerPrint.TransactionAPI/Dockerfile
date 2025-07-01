FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

WORKDIR /src

COPY ["./src/Core/Device.FingerPrint.Core", "Core/Device.FingerPrint.Core/"]

COPY ["./src/Device.FingerPrint.Queue", "Device.FingerPrint.Queue/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]

COPY ["./src/Device.FingerPrint.Redis", "Device.FingerPrint.Redis/"]

COPY ["./src/Device.FingerPrint.TransactionAPI/Device.FingerPrint.TransactionAPI.csproj", "Device.FingerPrint.TransactionAPI/"]

WORKDIR "/src/Device.FingerPrint.TransactionAPI"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.TransactionAPI .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine-25h1 AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.TransactionAPI.dll"]