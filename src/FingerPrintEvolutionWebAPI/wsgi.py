"""
WSGI entry point file for the application.
This allows the application to be run by Gunicorn or other WSGI servers.
"""
from app import app

# Expose the WSGI callable for production servers
if __name__ != "__main__":
    application = app

if __name__ == "__main__":
    app.run()
