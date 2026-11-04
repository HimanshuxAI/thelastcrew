#!/bin/sh
# Publishes the web build.
#
#   ./tools/publish.sh "what changed"
#
# Two things happen:
#   1. the service worker cache version is bumped (skip this and your phone
#      keeps running the old build),
#   2. the contents of web/ are pushed to the gh-pages branch, which is what
#      GitHub Pages serves.
#
# No GitHub Actions involved: it deploys straight from your machine in about
# ten seconds, and it cannot get stuck in a runner queue.

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

# 1. Source repository
git add -A
if ! git diff --cached --quiet; then
    git commit -q -m "$MSG"
    git push -q origin main
    echo "source pushed"
else
    echo "source: nothing to commit"
fi

# 2. Published site: web/ becomes the root of the gh-pages branch
REMOTE="$(git remote get-url origin)"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

cp -R web/. "$TMP"/

(
    cd "$TMP"
    git init -q
    git checkout -q -b gh-pages
    git add -A
    git -c commit.gpgsign=false commit -q -m "$MSG"
    git push -q --force "$REMOTE" gh-pages
)

echo "published: $MSG"
echo "live: https://himanshuxai.github.io/thelastcrew/"
echo ""
echo "On the phone: close the app fully and reopen it to pick up the new build."
