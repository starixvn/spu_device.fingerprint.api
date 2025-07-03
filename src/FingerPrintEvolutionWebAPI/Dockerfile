FROM hub.nexdev.net/base/python:3.10-alpine AS builder

# Set work directory
WORKDIR /app

# Install dependencies
COPY ./src/FingerPrintEvolutionWebAPI /app
RUN pip install --no-cache-dir -r requirements.txt

# Expose port
EXPOSE 5000

# Use gunicorn for production
CMD ["gunicorn", "--bind", "0.0.0.0:5000", "wsgi:app"]
