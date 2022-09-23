FROM tiangolo/uwsgi-nginx:python3.10 AS builder

COPY ./src/FingerPrintEvolutionWebAPI /app

WORKDIR /app

RUN pip3 install -r requirements.txt

ENV TZ="America/La_Paz"

ENTRYPOINT ["python"]
