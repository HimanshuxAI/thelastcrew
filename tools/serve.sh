#!/bin/sh
# Serves the web build so a phone on the same Wi-Fi can open it.
#
#   ./tools/serve.sh          -> port 8000
#   ./tools/serve.sh 8080     -> port 8080
#
# Ctrl+C to stop. Closing the terminal stops the game.

cd "$(dirname "$0")/.." || exit 1

PORT="${1:-8000}"
IP="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null)"

echo ""
echo "  TheLastCrew"
echo "  ------------------------------------------------"
echo "  this computer : http://localhost:$PORT/web/index.html"
if [ -n "$IP" ]; then
  echo "  your phone    : http://$IP:$PORT/web/index.html"
else
  echo "  your phone    : no Wi-Fi address found (not connected?)"
fi
echo "  ------------------------------------------------"
echo "  phone must be on the same Wi-Fi. Ctrl+C to stop."
echo ""

exec python3 -m http.server "$PORT" --bind 0.0.0.0
