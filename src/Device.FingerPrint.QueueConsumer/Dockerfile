FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.QueueConsumer", "Device.FingerPrint.QueueConsumer/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]
COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]
COPY ["./src/Device.FingerPrint.Queue", "Device.FingerPrint.Queue/"]
COPY ["./src/Core/Device.FingerPrint.Core", "Core/Device.FingerPrint.Core/"]


COPY ["./src/Device.FingerPrint.QueueConsumer/Device.FingerPrint.QueueConsumer.csproj", "Device.FingerPrint.QueueConsumer/"]

WORKDIR "/src/Device.FingerPrint.QueueConsumer"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.QueueConsumer .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.QueueConsumer.dll"]
