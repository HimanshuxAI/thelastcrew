#!/bin/sh
# Publishes the web build.
#
#   ./tools/publish.sh "what changed"
#
# Bumps the service worker cache, commits everything, pushes, and lets GitHub
# Actions deploy. Live in about 45 seconds.
#
# The cache bump is the whole point of this script: forget it once and your
# phone keeps running the old build, and you spend an hour debugging code that
# was already fixed.

set -e
cd "$(dirname "$0")/.."

MSG="${1:-Update}"

python3 - <<'PY'
import re
import pathlib

path = pathlib.Path("web/sw.js")
text = path.read_text()
match = re.search(r"const CACHE = 'tlc-v(\d+)';", text)

if match:
    version = int(match.group(1)) + 1
    path.write_text(text.replace(match.group(0), f"const CACHE = 'tlc-v{version}';"))
    print(f"service worker cache -> tlc-v{version}")
else:
    print("warning: could not find CACHE version in web/sw.js")
PY

git add -A

if git diff --cached --quiet; then
    echo "nothing to publish"
    exit 0
fi

git commit -q -m "$MSG"
git push -q origin main

echo "pushed: $MSG"
echo "live in ~45s: https://himanshuxai.github.io/thelastcrew/"
echo ""
echo "On the phone: close the app fully and reopen it to pick up the new build."
