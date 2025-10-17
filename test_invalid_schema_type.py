from common import call_api, pretty_print

payload = {
    "query": "món cá",
    "max_calories": "bốn trăm",  # wrong type, expect 422 if server validates strictly
    "avoid_allergens": []
}
res = call_api(payload)
pretty_print("TEST: invalid schema (max_calories as string)", res)
