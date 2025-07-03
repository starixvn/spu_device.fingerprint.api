FROM python:3.11-slim

WORKDIR /app

COPY ./src/FingerPrintEvolutionWebAPI /app

# Install build tools and dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends gcc build-essential libffi-dev && \
    pip install --no-cache-dir -r requirements.txt && \
    apt-get purge -y --auto-remove gcc build-essential && \
    rm -rf /var/lib/apt/lists/*

ENV TZ="America/La_Paz"

CMD ["gunicorn", "-w", "4", "-b", "0.0.0.0:5000", "wsgi:application"]
