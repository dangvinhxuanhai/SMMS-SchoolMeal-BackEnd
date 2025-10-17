import requests, json, sys, os, time

DEFAULT_URL = os.environ.get("API_URL", "http://127.0.0.1:8000/recommend")

def call_api(payload, url=None, timeout=60):
    url = url or DEFAULT_URL
    try:
        resp = requests.post(url, json=payload, timeout=timeout)
        ct = resp.headers.get("content-type", "")
        try:
            data = resp.json()
        except Exception:
            data = {"_raw_text": resp.text}
        return {
            "ok": resp.ok,
            "status": resp.status_code,
            "content_type": ct,
            "data": data,
            "url": url,
        }
    except requests.exceptions.RequestException as e:
        return {"ok": False, "status": None, "error": str(e), "url": url}

def pretty_print(title, result):
    print("="*80)
    print(title)
    print("-"*80)
    if "error" in result and result["error"]:
        print("ERROR:", result["error"])
        print("URL:", result.get("url"))
        return
    print("STATUS:", result["status"], "| CONTENT-TYPE:", result["content_type"])
    print("URL:", result.get("url"))
    data = result.get("data")
    try:
        print("JSON:", json.dumps(data, ensure_ascii=False, indent=2))
    except Exception:
        print("DATA:", str(data)[:2000])
