#!/bin/sh
# Publishes the web build.
#
#   ./tools/publish.sh "what changed"
#
# Three things happen:
#   1. the service worker cache version is bumped (skip this and your phone
#      keeps running the old build),
#   2. the source is committed and pushed to origin/main,
#   3. web/ is deployed to Cloudflare Pages.
#
# Cloudflare Pages is used instead of GitHub Pages because it deploys in a few
# seconds with no build queue, and it keeps working when the repo is private
# (GitHub Pages needs a paid plan for that).

set -e
cd "$(dirname "$0")/.."

MSG="${1:-Update}"
PROJECT="thelastcrew"

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

# 1. Source
git add -A
if ! git diff --cached --quiet; then
    git commit -q -m "$MSG"
    git push -q origin main
    echo "source pushed"
else
    echo "source: nothing to commit"
fi

# 2. Site
echo "deploying to Cloudflare Pages..."
npx --yes wrangler pages deploy web \
    --project-name "$PROJECT" \
    --branch main \
    --commit-dirty=true 2>&1 | grep -E "Success|Deployment complete|error|Error" || true

echo ""
echo "live: https://$PROJECT.pages.dev"
echo ""
echo "On the phone: close the app fully and reopen it to pick up the new build."
