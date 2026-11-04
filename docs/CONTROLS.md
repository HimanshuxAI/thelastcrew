# TheLastCrew — Control Spec

Version 0.1 · phone-first, headphones required, playable without sight.

---

## 1. Layout

The player cannot look at the screen, so every control must live somewhere a
hand can find without seeing it. That means **corners and edges only**. Anything
floating in the middle of the screen is unfindable and therefore unusable.

```
┌──────────────────────────────────────────┐
│  TURN LEFT                    TURN RIGHT │
│                                          │
│                                          │
│                 ┌────────┐               │
│                 │  MENU  │               │
│                 └────────┘               │
│                                          │
│         walk: drag anywhere here         │
│                                          │
│  HAND                              EAR   │
└──────────────────────────────────────────┘
```

| Zone | Position | Role |
|---|---|---|
| Turn left | top-left corner | rotate facing left while held |
| Turn right | top-right corner | rotate facing right while held |
| Menu | screen centre | settings / options, double-tap to select |
| Walk | everything else | floating joystick, appears under the thumb |
| Hand | bottom-left corner | physical actions |
| Ear | bottom-right corner | listening actions |

The walk area is not a fixed widget. Wherever the thumb lands becomes the
centre of the stick, so the player never has to find it. That is the only
pattern that works when you cannot see where you are putting your finger.

---

## 2. The two thumbs

The bottom buttons are the gameplay. They are deliberately split by *kind of
verb*, not by *importance*, so the player only has to remember one sentence:

> **Left thumb is your body. Right thumb is your ears.**

That sentence is the entire control scheme, and it can be spoken in one breath.

### 2.1 Ear — bottom right

The listening hand. This is the voice of the game and the source of its
signature tension: **everything here makes noise, and noise is what finds you.**

| Gesture | Action | What it does | Cost |
|---|---|---|---|
| Tap | **Ping** | short echo pulse, roughly 8 m, reveals walls and openings | quiet, 2 s cooldown |
| Hold | **Deep scan** | wide, precise map of the space and anything moving in it | loud, 10 s cooldown, attracts |
| Hold + drag toward a sound | **Aim** | points a directional mic at one suspicious sound and analyses it | safe, but you must stand still |

*Aim* is the investigation verb. It is how the player answers *"is that voice
real?"* without walking into the trap. It turns the mimic from a coin flip into
a skill: you point at it, you listen to what comes back, you decide.

### 2.2 Hand — bottom left

The physical hand. Slow, close-range, safe, and never loud unless you choose it.

| Gesture | Action | What it does | Cost |
|---|---|---|---|
| Tap | **Take** | pick up evidence, tools, crewmate's belongings | none |
| Hold | **Brace** | crouch, stop breathing, kill your footsteps | you cannot move |
| Hold + drag toward a direction | **Throw** | hurl a stone or a flare that way | very loud, and it lands where the noise is |

*Throw* is the escape valve. Every creature in the maze investigates sound, so
the player can always buy a route by spending noise somewhere else. That single
verb gives you stealth, lure, and panic-button gameplay from one gesture.

### 2.3 Why both hands cost something

| | Hand (body) | Ear (sound) |
|---|---|---|
| Range | touching distance | the whole room |
| Speed | immediate | has a cooldown |
| Risk | almost none | reveals your position |

Neither hand is strictly better. The ear sees further; the hand is safer. The
player chooses which risk to take, which is what a decision is.

---

## 3. Menu

Tap once: opens the menu and speaks the first option.
Tap again: moves to the next option.
Double tap: selects.

This mirrors VoiceOver and TalkBack, so anyone who has used a phone accessibly
already knows the rhythm. Option changes must be spoken **immediately on
change**, not on release, or the menu becomes a memory test.

---

## 4. Audio grammar

Every zone is announced on touch-down, never on release. A player pressing a
button and hearing nothing has no idea whether the press registered.

| Event | Sound |
|---|---|
| Finger lands on a zone | short earcon + spoken name |
| Tap confirms | click, then the result spoken |
| Action refused (cooldown) | dull thud, never silence |
| Menu option changes | spoken option, no earcon |
| Menu confirm | rising two-tone chime |

Rules that keep this fair:

1. **Silence must mean something.** Reserve true silence for danger. A dead
   cooldown or a blocked action gets a sound.
2. **Never two important sounds at once.** Environmental cues duck under
   speech. Speech ducks under nothing.
3. **Direction first, then meaning.** A sound should be locatable before it is
   identifiable, or the player learns the map by guessing.

---

## 5. Self-voicing, not screen readers

Unity does not expose its UI to VoiceOver or TalkBack. Building on the OS
screen reader will not work.

The game must speak for itself: pre-recorded lines, or a small text-to-speech
bridge. This is better anyway — the voice can become part of the fiction, and it
works identically on both platforms and on PC.

---

## 6. Open decisions

- Does a creature hear the *ping* from across the maze, or only within range?
- Is *Aim* always available, or does it drain a battery that *Ping* also uses?
- Can the player throw while walking, or must they stop first?
- Turn speed: fixed, or faster when the turn button is double-tapped?
