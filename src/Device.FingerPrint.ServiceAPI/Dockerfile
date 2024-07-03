FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.ServiceAPI", "Device.FingerPrint.ServiceAPI/"]

COPY ["./src/Core/Device.FingerPrint.Core", "Core/Device.FingerPrint.Core/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]

COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]

COPY ["./src/Device.FingerPrint.Queue", "Device.FingerPrint.Queue/"]

COPY ["./src/Device.FingerPrint.ServiceAPI/Device.FingerPrint.ServiceAPI.csproj", "Device.FingerPrint.ServiceAPI/"]

WORKDIR "/src/Device.FingerPrint.ServiceAPI"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.ServiceAPI .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.ServiceAPI.dll"]
