import requests, json

url = "http://127.0.0.1:8000/recommend"
payload = {
    "query": "món gà healthy dưới 400 kcal",
    "max_calories": 400,
    "avoid_allergens": ["đậu phộng"]
}

resp = requests.post(url, json=payload, timeout=60)

print("STATUS:", resp.status_code)
print("CONTENT-TYPE:", resp.headers.get("content-type"))

# Thử parse JSON an toàn
try:
    data = resp.json()
    print("JSON:", json.dumps(data, ensure_ascii=False, indent=2))
except Exception:
    print("TEXT:", resp.text[:2000])  # in tối đa 2000 ký tự cho dễ xem

