import requests

data = {
    "text" : "popka"
}

requests.post("http://localhost:5000/send", json=data)