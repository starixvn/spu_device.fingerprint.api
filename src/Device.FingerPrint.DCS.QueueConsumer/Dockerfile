FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.DCS.QueueConsumer", "Device.FingerPrint.DCS.QueueConsumer/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]
COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]
COPY ["./src/Device.FingerPrint.Queue", "Device.FingerPrint.Queue/"]
COPY ["./src/Device.FingerPrint.DCS.QueueConsumer.Services", "Device.FingerPrint.DCS.QueueConsumer.Services/"]


COPY ["./src/Device.FingerPrint.DCS.QueueConsumer/Device.FingerPrint.DCS.QueueConsumer.csproj", "Device.FingerPrint.DCS.QueueConsumer/"]

WORKDIR "/src/Device.FingerPrint.DCS.QueueConsumer"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.DCS.QueueConsumer .

COPY ./src/Components/Fanex.Oneyed.Infrastructure.Models.dll Fanex.Oneyed.Infrastructure.Models.dll

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.DCS.QueueConsumer.dll"]
