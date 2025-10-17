from common import call_api, pretty_print

payload_no_avoid = {
    "query": "chicken salad low fat",
    "max_calories": 600,
    "avoid_allergens": []
}
res1 = call_api(payload_no_avoid)
pretty_print("TEST: allergens toggle (no avoid)", res1)

payload_avoid = {
    "query": "chicken salad low fat",
    "max_calories": 600,
    "avoid_allergens": ["peanut", "đậu phộng"]
}
res2 = call_api(payload_avoid)
pretty_print("TEST: allergens toggle (avoid peanut)", res2)
