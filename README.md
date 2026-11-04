# Web build

A single HTML file. No build step, no dependencies, no install.

    cd ..                 # project root
    ./tools/serve.sh      # serves on :8000 and prints the phone URL

Open `http://<your-computer-ip>:8000/web/index.html`.

---

## Publishing

    ./tools/publish.sh "what changed"

That is the whole process. It does three things:

1. bumps `CACHE` in `sw.js` (skip it and phones keep the old build),
2. commits and pushes the source to `main`,
3. force-pushes the contents of `web/` to the `gh-pages` branch.

GitHub Pages serves the `gh-pages` branch directly — **there is no build step on
GitHub's side and no Actions workflow**, so publishing cannot get stuck in a
runner queue.

Live at: https://himanshuxai.github.io/thelastcrew/

After publishing, fully close the app on the phone and reopen it. A PWA does not
reload itself.

---

## Movement modes

The start screen offers two ways to walk:

| Mode | How it works | Good for |
|---|---|---|
| **Drag** | put a thumb anywhere in the middle and slide; the stick appears under your finger | sitting, lying down, any posture, one hand |
| **Tilt** | lean the phone forward to walk, back to reverse | standing, holding the phone up |

Tilt needs motion permission (iOS asks once) and **only works over HTTPS** — the
browser refuses motion events on a plain `http://` LAN address. Neutral angle is
captured when you start; **double-tap the centre** to recalibrate if you change
posture.

There is no strafing on tilt: sideways movement in a maze you cannot see is how
you walk into walls. Turning is always the two top corners.

---

## Installing it on a phone

The game is a Progressive Web App: it can live on the home screen, open
full-screen, and run with no network.

**But installation requires HTTPS.** Over a plain `http://192.168.x.x` address
the browser silently refuses the service worker, so:

| Served from | Plays | Installs | Offline | Wake lock |
|---|---|---|---|---|
| `http://192.168.x.x:8000` (LAN) | yes | no | no | no |
| `https://…` (any host) | yes | yes | yes | yes |
| `http://localhost` (this machine) | yes | dev only | yes | yes |

So LAN is fine for testing; use HTTPS when you want the real thing.

### Once it is on an HTTPS address

**Android / Chrome**
1. Open the URL.
2. Menu (⋮) → **Add to Home screen** / **Install app**.
3. Launch from the home screen — no browser bars.

**iPhone / Safari**
1. Open the URL in **Safari** (not Chrome — only Safari can install).
2. Share button → **Add to Home Screen**.
3. Launch from the home screen.

---

## What the PWA layer adds

| File | Purpose |
|---|---|
| `manifest.webmanifest` | name, icon, fullscreen, theme colour |
| `sw.js` | caches the game so it opens with no network |
| `icons/` | 192, 512, maskable, and the iOS 180 touch icon |

Regenerate icons with:

    python3 tools/generate_icons.py

**When you change `index.html`, bump `CACHE` in `sw.js`** (`tlc-v3` → `tlc-v4`).
Otherwise the phone keeps serving the old copy from the cache, and you will
spend an hour debugging a fix that already works.

---

## Known platform limits

- **iOS** suspends web audio when the screen locks or the app goes to the
  background. No web API prevents this. The wake lock below keeps the screen on
  while playing, which is the workaround.
- **iOS** has no `navigator.vibrate`. Android does.
- **iOS** ignores `orientation` in the manifest.
- **Silent switch** on iPhone mutes web audio. This is the most common
  "the game is broken" report.
