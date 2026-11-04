#!/bin/sh
# Serves the web build over a public HTTPS URL, for testing on a real phone.
#
#   ./tools/tunnel.sh
#
# Why this exists: phones only deliver motion (tilt) events to HTTPS pages, and
# only install a PWA from HTTPS. A LAN address cannot do either.
#
# The URL is random and dies when this script stops, so it is for testing, not
# for keeping. It does not need an account, a domain, or any configuration.
#
# Ctrl-C to stop.

set -e
cd "$(dirname "$0")/.."

PORT=8001

# Serve web/ as the site root, so the PWA scope is / and the URL has no path.
python3 -m http.server "$PORT" --bind 127.0.0.1 --directory web >/dev/null 2>&1 &
SERVER=$!
TUNNEL=

cleanup() {
  echo ""
  echo "shutting down"
  [ -n "$TUNNEL" ] && kill "$TUNNEL" 2>/dev/null || true
  kill "$SERVER" 2>/dev/null || true
}
trap cleanup INT TERM EXIT

# Keep this machine awake: if it sleeps, the tunnel dies and the phone loses
# the game mid-walk.
caffeinate -dims -w $$ &

echo "serving web/ on 127.0.0.1:$PORT"
echo "opening tunnel..."
echo ""

npx --yes cloudflared tunnel --url "http://127.0.0.1:$PORT" --no-autoupdate 2>&1 |
  tee /tmp/tlc-tunnel.log &

TUNNEL=$!

# Wait for the URL to appear in the log, then print it clearly.
i=0
while [ $i -lt 40 ]; do
  URL=$(grep -Eo 'https://[a-z0-9-]+\.trycloudflare\.com' /tmp/tlc-tunnel.log 2>/dev/null | head -1)
  if [ -n "$URL" ]; then
    echo ""
    echo "=========================================================="
    echo "  Open this on the phone:"
    echo ""
    echo "  $URL"
    echo ""
    echo "  HTTPS, so tilt works and it can be installed."
    echo "=========================================================="
    echo ""
    break
  fi
  i=$((i + 1))
  sleep 1
done

wait $TUNNEL
