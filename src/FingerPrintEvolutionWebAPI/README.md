# FingerPrintEvolutionWebAPI

## Description

A simple Flask API to calculate the similarity ratio between two strings.

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

## Build and Run with Docker

## API Documentation (Swagger)
After starting the application, access the Swagger UI at:  
`http://localhost:8000/apidocs/`

## API Usage

- POST `/api/ratio` with JSON body:

- Response: Similarity ratio as a percentage.

## Swagger UI

Visit [http://localhost:5000/swagger/](http://localhost:5000/swagger/) after running the container.

## Production
Use Gunicorn or another WSGI server:
