"""
This script runs the application using a development server.
It contains the definition of routes and views for the application.
"""
from difflib import SequenceMatcher

from flask import Flask, jsonify
app = Flask(__name__)

@app.route('/api/ratio/<source>/<destination>')
def ratio(source, destination):   
   return jsonify(ratio=SequenceMatcher(None, source, destination).ratio() * 100)

@app.route('/api/healthz')
def healthCheck(): 
    return 'Ok'

if __name__ == '__main__':
    import os
    HOST = os.environ.get('SERVER_HOST', 'localhost')
    try:
        PORT = int(os.environ.get('SERVER_PORT', '5555'))
    except ValueError:
        PORT = 5555
    app.run(HOST, PORT)
