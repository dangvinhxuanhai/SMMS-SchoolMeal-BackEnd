import subprocess, sys, os, glob

# Allow override API URL via env var API_URL
api_url = os.environ.get("API_URL", "http://127.0.0.1:8000/recommend")
print("Using API_URL =", api_url)

tests = sorted([p for p in glob.glob("test_*.py") if not p.endswith("_skip.py")])

print("\nFound tests:")
for t in tests:
    print(" -", t)

print("\nRunning...\n")
failed = 0

for t in tests:
    print("\n" + "#"*90)
    print("RUN", t)
    print("#"*90 + "\n")
    try:
        rc = subprocess.call([sys.executable, t])
        if rc != 0:
            failed += 1
            print(f"[FAIL] {t} exited with code {rc}")
        else:
            print(f"[OK] {t}")
    except Exception as e:
        failed += 1
        print(f"[EXCEPTION] {t}: {e}")

print("\n" + "="*90)
if failed:
    print(f"Completed with {failed} failing script(s).")
    sys.exit(1)
else:
    print("All tests completed successfully.")
