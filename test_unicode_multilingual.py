from common import call_api, pretty_print

payload_vi = {
    "query": "món gà ít dầu",
    "max_calories": 500,
    "avoid_allergens": []
}
res1 = call_api(payload_vi)
pretty_print("TEST: multilingual (Vietnamese)", res1)

payload_en = {
    "query": "low-fat chicken",
    "max_calories": 500,
    "avoid_allergens": []
}
res2 = call_api(payload_en)
pretty_print("TEST: multilingual (English)", res2)
