from common import call_api, pretty_print

payload = {
    "query": "anything",
    "max_calories": 1,   # intentionally tiny to likely force empty results
    "avoid_allergens": []
}
res = call_api(payload)
pretty_print("TEST: no results (very small max_calories)", res)
