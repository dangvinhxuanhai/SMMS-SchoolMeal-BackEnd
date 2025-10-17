from common import call_api, pretty_print

payload = {
    "query": "fresh vegetable soup",
    "max_calories": 350,
    "avoid_allergens": []
}
res = call_api(payload)
pretty_print("TEST: LLM summary / fallback", res)

data = res.get("data", {})
summary = data.get("ai_summary") if isinstance(data, dict) else None
if summary:
    print("\nSUMMARY:", str(summary)[:300])
else:
    print("\nNo ai_summary field in response.")
