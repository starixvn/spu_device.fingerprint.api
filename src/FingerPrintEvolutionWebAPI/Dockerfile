FROM hub.nexdev.net/base/python:3.10-alpine AS builder

COPY ./src/FingerPrintEvolutionWebAPI /app

WORKDIR /app

RUN pip3 install -r requirements.txt

ENV TZ="America/La_Paz"

CMD [ "python3", "-m" , "flask", "run", "--host=0.0.0.0", "--port=5000"]
