import requests

url = "http://127.0.0.1:8000/recommend"
payload = {
    "query": "Món gà healthy dưới 400 kcal",
    "max_calories": 400,
    "avoid_allergens": ["đậu phộng"]
}

response = requests.post(url, json=payload)
print(response.json())
