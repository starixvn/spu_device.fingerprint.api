# FingerPrintEvolutionWebAPI

## Overview
A Python web API for fingerprint operations.

## Structure
- `app.py`: Main application, modularized with Blueprints.
- `wsgi.py`: WSGI entry point for production servers.
- `requirements.txt`: Pinned dependencies.
- `Dockerfile`: Production-ready containerization.

## Development
1. Install dependencies: `pip install -r requirements.txt`
2. Run locally: `python app.py`

## API Documentation (Swagger)
After starting the application, access the Swagger UI at:  
`http://localhost:8000/apidocs/`

## Production
Use Gunicorn or another WSGI server:
