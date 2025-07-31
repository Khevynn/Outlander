from flask import Flask, redirect, request, jsonify, abort
import requests
import base64
import hashlib
import secrets
import urllib.parse
import logging
import os
from threading import Lock
from datetime import datetime, timedelta

app = Flask(__name__)

CLIENT_ID = "RDJRNEJqcnAyb0wzQXk1TC02Zmc6MTpjaQ"
REDIRECT_URI = "https://imaikzz.pythonanywhere.com/callback"

# Use a thread-safe dict or proper DB for concurrency
user_tokens = {}
tokens_lock = Lock()

# Token expiration time (example: 1 hour)
TOKEN_EXPIRES_IN = timedelta(hours=1)

# Setup logging
logging.basicConfig(level=logging.INFO)

def generate_code_verifier():
    return base64.urlsafe_b64encode(secrets.token_bytes(32)).rstrip(b'=').decode()

def generate_code_challenge(verifier):
    challenge = hashlib.sha256(verifier.encode()).digest()
    return base64.urlsafe_b64encode(challenge).rstrip(b'=').decode()

@app.route("/auth")
def auth():
    verifier = generate_code_verifier()
    challenge = generate_code_challenge(verifier)
    state = secrets.token_urlsafe(16)

    with tokens_lock:
        user_tokens[state] = {
            "verifier": verifier,
            "created_at": datetime.utcnow()
        }

    # Compose the Twitter OAuth2 authorization URL
    params = {
        "response_type": "code",
        "client_id": CLIENT_ID,
        "redirect_uri": REDIRECT_URI,
        "scope": "tweet.read tweet.write users.read offline.access",
        "state": state,
        "code_challenge": challenge,
        "code_challenge_method": "S256"
    }
    url = "https://twitter.com/i/oauth2/authorize?" + urllib.parse.urlencode(params)

    logging.info(f"Redirecting user to Twitter OAuth2 endpoint with state={state}")
    return redirect(url)

@app.route("/callback")
def callback():
    code = request.args.get("code")
    state = request.args.get("state")

    if not code or not state:
        abort(400, description="Missing required query parameters.")

    with tokens_lock:
        if state not in user_tokens:
            abort(400, description="Invalid or expired state.")

        user_tokens[state]["code"] = code
        user_tokens[state]["code_received_at"] = datetime.utcnow()

    # Redirect to Unity local listener to pick up the code & state
    unity_callback_url = f"http://localhost:7890/callback?state={state}"
    logging.info(f"Redirecting to Unity local listener with state={state}")
    return redirect(unity_callback_url)

@app.route("/token", methods=["POST"])
def token():
    data = request.json or {}
    state = data.get("state")
    if not state:
        return jsonify({"error": "Missing state"}), 400

    with tokens_lock:
        token_info = user_tokens.get(state)
        if not token_info or "code" not in token_info:
            return jsonify({"error": "Invalid or missing state/code"}), 400

        created_at = token_info.get("created_at")
        if created_at and datetime.utcnow() - created_at > timedelta(minutes=10):
            del user_tokens[state]
            return jsonify({"error": "State expired"}), 400

        verifier = token_info["verifier"]
        code = token_info["code"]

    payload = {
        "grant_type": "authorization_code",
        "code": code,
        "redirect_uri": REDIRECT_URI,
        "client_id": CLIENT_ID,
        "code_verifier": verifier
    }

    headers = {
        "Content-Type": "application/x-www-form-urlencoded"
    }

    try:
        response = requests.post("https://api.twitter.com/2/oauth2/token", data=payload, headers=headers, timeout=10)
        response.raise_for_status()
    except requests.RequestException as e:
        logging.error(f"Token exchange failed: {e}")
        return jsonify({"error": "Token exchange failed", "details": str(e)}), 500

    tokens = response.json()

    with tokens_lock:
        user_tokens[state].update(tokens)
        user_tokens[state]["token_acquired_at"] = datetime.utcnow()

    logging.info(f"Token exchange success for state={state}")
    return jsonify({"status": "token_exchange_success", "tokens": tokens})

@app.route("/tweet", methods=["POST"])
def tweet():
    data = request.json or {}
    state = data.get("state")
    message = data.get("message")

    if not state or not message:
        return jsonify({"error": "Missing state or message"}), 400

    with tokens_lock:
        token_info = user_tokens.get(state)
        if not token_info or "access_token" not in token_info:
            return jsonify({"error": "Invalid or expired state"}), 400

        access_token = token_info["access_token"]

    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
    }

    tweet_payload = {"text": message}

    try:
        response = requests.post("https://api.twitter.com/2/tweets", headers=headers, json=tweet_payload, timeout=10)
        if response.status_code == 403:
            # Handle specific error for duplicate tweets or forbidden content
            detail = response.json().get("detail", "")
            logging.warning(f"Tweet failed with 403 Forbidden: {detail}")
            return jsonify({"error": "Tweet failed", "details": detail}), 403
        response.raise_for_status()
    except requests.RequestException as e:
        logging.error(f"Tweet failed: {e}")
        return jsonify({"error": "Tweet failed", "details": str(e)}), 500

    logging.info(f"Tweet posted successfully for state={state}")
    return jsonify({"status": "Tweeted!", "response": response.json()})

# Optional: add a route for health check
@app.route("/health")
def health():
    return jsonify({"status": "ok"})

if __name__ == "__main__":
    # Use production-ready WSGI server like gunicorn in deployment
    app.run(host="0.0.0.0", port=int(os.getenv("PORT", 5000)), debug=False)
