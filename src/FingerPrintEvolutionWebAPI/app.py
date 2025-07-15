"""
This script defines the Flask application.
It contains the definition of routes and views for the application.
"""
from difflib import SequenceMatcher

from flask import Flask, jsonify, request

# Add Flasgger for Swagger support
from flasgger import Swagger

# Create Flask app instance
app = Flask(__name__)

# Initialize Swagger
swagger_config = {
    "headers": [],
    "specs": [
        {
            "endpoint": 'apispec_1',
            "route": '/swagger/apispec_1.json',
            "rule_filter": lambda rule: True,  # all in
            "model_filter": lambda tag: True,  # all in
        }
    ],
    "static_url_path": "/flasgger_static",
    "swagger_ui": True,
    "specs_route": "/swagger/"
}
swagger = Swagger(app, config=swagger_config)

# Import and register blueprints here
# from .routes.example import example_bp
# app.register_blueprint(example_bp)

@app.route('/api/ratio', methods=['POST'])
def ratio():
    """
    Calculate similarity ratio between two strings.
    ---
    tags:
      - Ratio
    parameters:
      - in: body
        name: body
        required: true
        schema:
          type: object
          required:
            - source
            - destination
          properties:
            source:
              type: string
              example: "hello"
            destination:
              type: string
              example: "h3llo"
    responses:
      200:
        description: Similarity ratio (percentage)
        schema:
          type: number
          example: 85.71428571428571
    """
    requestData = request.get_json()
    source = requestData['source']
    destination = requestData['destination']
    return jsonify(SequenceMatcher(None, source, destination).ratio() * 100)

@app.route('/healthz')
def healthCheck():
    """
    Health check endpoint.
    ---
    tags:
      - Health
    responses:
      200:
        description: Service is healthy
        schema:
          type: string
          example: Healthy
    """
    return 'Healthy'

# Only used when running the file directly, not when imported by a WSGI server
if __name__ == '__main__':
    # For local development only; use gunicorn in production
    app.run(host="0.0.0.0", port=5000, debug=True)
