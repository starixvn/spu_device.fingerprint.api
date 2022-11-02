FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["./src/Device.FingerPrint.QueueConsumerDCS", "Device.FingerPrint.QueueConsumerDCS/"]

COPY ["./src/Device.FingerPrint", "Device.FingerPrint/"]
COPY ["./src/Device.FingerPrint.Queue", "Device.FingerPrint.Queue/"]
COPY ["./src/Device.FingerPrint.QueueConsumerDCS.Services", "Device.FingerPrint.QueueConsumerDCS.Services/"]


COPY ["./src/Device.FingerPrint.QueueConsumerDCS/Device.FingerPrint.QueueConsumerDCS.csproj", "Device.FingerPrint.QueueConsumerDCS/"]

WORKDIR "/src/Device.FingerPrint.QueueConsumerDCS"

RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.QueueConsumerDCS .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.QueueConsumerDCS.dll"]
