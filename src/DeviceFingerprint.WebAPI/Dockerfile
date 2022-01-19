FROM registry.nexdev.net:5050/docker/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]

COPY ["./src/DeviceFingerprint.WebAPI/Device.Fingerprint.WebAPI.csproj", "DeviceFingerprint.WebAPI/"]

WORKDIR "/src/DeviceFingerprint.WebAPI"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/DeviceFingerprint.WebAPI .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM registry.nexdev.net:5050/docker/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.Fingerprint.WebAPI.dll"]
