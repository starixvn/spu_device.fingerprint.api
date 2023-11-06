FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.LogTrail.Consumer", "Device.FingerPrint.LogTrail.Consumer/"]

COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]

COPY ["./src/FPS.Device.FingerPrint.LogTrail.Net.Standard.Client", "FPS.Device.FingerPrint.LogTrail.Net.Standard.Client/"]

COPY ["./src/Device.FingerPrint.LogTrail.Consumer/Device.FingerPrint.LogTrail.Consumer.csproj", "Device.FingerPrint.LogTrail.Consumer/"]

WORKDIR "/Device.FingerPrint.LogTrail.Consumer"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.LogTrail.Consumer .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.LogTrail.Consumer.dll"]
