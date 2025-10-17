from common import call_api, pretty_print

payload = {
    "query": "grilled chicken"
    # missing max_calories on purpose (422 nếu server yêu cầu trường này)
}
res = call_api(payload)
pretty_print("TEST: invalid schema (missing max_calories)", res)
