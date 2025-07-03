FROM hub.nexdev.net/base/python:3.10-alpine AS builder

WORKDIR /app

COPY ./src/FingerPrintEvolutionWebAPI /app

# Install build tools and dependencies
RUN pip3 install -r requirements.txt

ENV TZ="America/La_Paz"

CMD ["gunicorn", "-w", "4", "-b", "0.0.0.0:5000", "wsgi:application"]
