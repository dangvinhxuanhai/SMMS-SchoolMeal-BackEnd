# test_menu.py
import os, json, requests

URL = os.environ.get("API_URL", "http://127.0.0.1:8000/menu/recommend")

payload = {
  "query": "món gà tỏi",                 # giúp retriever nếu code ưu tiên query
  "main_ingredients": ["gà","tỏi"],      # nhưng lọc dùng ANY/ratio như trên
  "side_ingredients": ["xà lách","cà chua"],
  "avoid_allergens": ["đậu phộng"],
  "available_equipment": [],             # thử bỏ lọc thiết bị để xác nhận
  "diners_count": 80,
  "group_allergy_rates": {"đậu phộng": 0.2},
  "max_cal_main": 600,
  "max_cal_side": 350,
  "top_n_main": 5,
  "top_n_side": 5
}


resp = requests.post(URL, json=payload, timeout=60)
print("STATUS:", resp.status_code)
try:
    print(json.dumps(resp.json(), ensure_ascii=False, indent=2))
except Exception:
    print(resp.text)
