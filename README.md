# TheLastCrew

An audio-first survival game for phones. You land on an alien planet, the planet
is a maze, and your crew is somewhere inside it. You navigate by sound, collect
evidence, and work out which voices you can trust.

**Design rules**

- Phone first, PC second, same code.
- Headphones required. The screen is optional.
- Written for players who cannot see it, and better for players who can.

---

## Opening the project

1. Open Unity Hub → **Add** → **Add project from disk**.
2. Select this folder: `Documents/game`.
3. Open it with **6000.3.23f1** (or let Unity upgrade it).
4. First open takes a few minutes — Unity regenerates `Library/` and fills in
   the missing `ProjectSettings` assets.

If Unity Hub refuses the folder, create a new empty project in Hub, then copy
`Assets/`, `Packages/` and `ProjectSettings/ProjectVersion.txt` into it.

---

## Current state

| System | Status |
|---|---|
| Folder structure | done |
| Player movement (touch + keyboard) | written, untested on device |
| Touch zone layout | written, untested on device |
| Footstep audio | written |
| Wall-proximity audio | written |
| Fixed maze (seeded generator) | written |
| One-click test scene | written |
| Placeholder sound pack | generated |
| Echo scanner, creatures, mimic | not started |

### Scripts

| File | What it does |
|---|---|
| `Assets/Scripts/Core/IPlayerInput.cs` | the contract between input and movement |
| `Assets/Scripts/Player/TouchInputProvider.cs` | six touch zones, gestures, self-voicing events |
| `Assets/Scripts/Player/PlayerMotor.cs` | walks and turns the CharacterController, raises `Footstep` |
| `Assets/Scripts/Audio/FootstepAudio.cs` | plays the footsteps, quieter when crouching |
| `Assets/Scripts/Audio/WallProximityAudio.cs` | probe ring that places rustles on nearby walls |
| `Assets/Scripts/Audio/SpatialAudioGuard.cs` | catches duplicate Audio Listeners |
| `Assets/Scripts/Maze/MazeGrid.cs` | seeded maze data, always solvable, with loops |
| `Assets/Scripts/Maze/MazeBuilder.cs` | turns that data into walls, floor and landmarks |
| `Assets/Scripts/Editor/PrototypeSceneBuilder.cs` | builds the whole test scene from a menu item |

Read `docs/CONTROLS.md` before changing the layout — the corner placement is a
functional requirement, not a style choice.

### Docs

| Doc | Purpose |
|---|---|
| `docs/CONTROLS.md` | the full control and audio-grammar spec |
| `docs/SETUP-NAV-PROTOTYPE.md` | **start here** — scene setup, test checklist, phone build |

### Placeholder audio

`Assets/Audio/Placeholder/` holds five synthetic sounds (3 footsteps, a wall
rustle loop, a landmark hum). Regenerate with:

    python3 tools/generate_placeholder_audio.py

---

## Two builds

The design is platform-independent; the implementation is not. There are now two
front ends to the same game:

| Build | Folder | Status |
|---|---|---|
| Unity | `Assets/` | written, waiting on editor install |
| Browser / PWA | `web/` | **live**, installable, offline-capable |

**Play it: https://himanshuxai.github.io/thelastcrew/**

Any push to `main` redeploys the web build automatically
(`.github/workflows/deploy-pages.yml`). Only `web/` is published.

The browser build exists because Web Audio has real HRTF spatialisation and
built-in speech synthesis, both of which Unity needs plugins for. Try it first:

    ./tools/serve.sh

Then open `http://<your-lan-ip>:8000/web/index.html` on the phone.
See `web/README.md` for installing it to the home screen (needs HTTPS).

---

## Next step

1. Open the project in Unity.
2. Menu **`TheLastCrew → Build Playable Test Scene`**.
3. Press Play. Headphones on. Walk the maze with your eyes closed.
4. `File → Build Profiles → Android → Build And Run`.

Do not build the scanner or the mimic until walking by sound works on the
phone.
