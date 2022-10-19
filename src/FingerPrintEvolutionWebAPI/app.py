"""
This script runs the application using a development server.
It contains the definition of routes and views for the application.
"""
from difflib import SequenceMatcher

from flask import Flask, jsonify, request
app = Flask(__name__)

@app.route('/api/ratio', methods=['POST'])
def ratio():   
    requestData = request.get_json()
    source = requestData['source']
    destination = requestData['destination']
    return jsonify(SequenceMatcher(None, source, destination).ratio() * 100)

@app.route('/healthz')
def healthCheck(): 
    return 'Healthy'

if __name__ == '__main__':
    app.run(host="localhost", port=8000, debug=True)
