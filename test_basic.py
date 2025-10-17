from common import call_api, pretty_print

payload = {
    "query": "món gà healthy dưới 400 kcal",
    "max_calories": 400,
    "avoid_allergens": ["đậu phộng"]
}
res = call_api(payload)
pretty_print("TEST: basic success (VN query + allergen avoid + 400 kcal)", res)
