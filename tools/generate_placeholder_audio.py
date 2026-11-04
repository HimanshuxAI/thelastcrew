#!/usr/bin/env python3
"""Generates placeholder audio for TheLastCrew.

These are deliberately crude synthetic sounds: enough to test navigation
tonight, without waiting on real assets. Replace them later; keep the file
names so nothing in the scenes breaks.

Usage:  python3 tools/generate_placeholder_audio.py
Output: Assets/Audio/Placeholder/*.wav
"""

import math
import os
import random
import struct
import wave

SR = 44100
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "Placeholder")


# ---------------------------------------------------------------- helpers

def lowpass(samples, cutoff):
    a = 1.0 - math.exp(-2.0 * math.pi * cutoff / SR)
    y = 0.0
    out = []
    for s in samples:
        y += a * (s - y)
        out.append(y)
    return out


def highpass(samples, cutoff):
    lp = lowpass(samples, cutoff)
    return [s - l for s, l in zip(samples, lp)]


def normalise(samples, peak=0.85):
    m = max(abs(s) for s in samples) or 1.0
    return [s / m * peak for s in samples]


def fade(samples, ms=6):
    n = max(1, int(SR * ms / 1000))
    n = min(n, len(samples) // 2)
    for i in range(n):
        g = i / n
        samples[i] *= g
        samples[-1 - i] *= g
    return samples


def write_wav(name, samples):
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, name)
    frames = bytearray()
    for s in samples:
        v = max(-1.0, min(1.0, s))
        frames += struct.pack("<h", int(v * 32767))
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(bytes(frames))
    print(f"{path}  {len(samples) / SR:.2f}s")


# ---------------------------------------------------------------- sounds

def footstep(seed):
    """A soft thump with a dry scrape: weight landing on alien ground."""
    random.seed(seed)
    n = int(SR * 0.28)
    scrape = [random.uniform(-1, 1) for _ in range(n)]
    scrape = highpass(scrape, 400)
    scrape = lowpass(scrape, 4200)

    out = []
    for i in range(n):
        t = i / SR
        click = scrape[i] * math.exp(-t / 0.028) * 0.55
        thump = math.sin(2 * math.pi * 68 * t) * math.exp(-t / 0.055) * 0.9
        grit = math.sin(2 * math.pi * 190 * t) * math.exp(-t / 0.02) * 0.15
        out.append(click + thump + grit)

    return fade(normalise(out, 0.8), 4)


def rustle_loop(seconds=3.0):
    """Wind moving through dry alien leaves, band-limited and breathing."""
    random.seed(7)
    fade_len = int(SR * 0.25)
    n = int(SR * seconds)
    raw = [random.uniform(-1, 1) for _ in range(n + fade_len)]

    raw = lowpass(raw, 5200)
    raw = highpass(raw, 750)

    # Gust envelope: periods divide the loop length, so it stays seamless.
    for i in range(len(raw)):
        t = i / SR
        env = (0.55
               + 0.22 * math.sin(2 * math.pi * (1.0 / seconds) * t)
               + 0.13 * math.sin(2 * math.pi * (3.0 / seconds) * t + 1.1)
               + 0.08 * math.sin(2 * math.pi * (7.0 / seconds) * t + 2.3))
        raw[i] *= env

    loop = raw[:n]
    for i in range(fade_len):
        g = i / fade_len
        loop[i] = raw[i] * g + raw[n + i] * (1 - g)

    return fade(normalise(loop, 0.6), 8)


def hum_loop(seconds=4.0):
    """A dying electrical structure: two low tones and a whisper of noise."""
    random.seed(11)
    fade_len = int(SR * 0.3)
    n = int(SR * seconds)
    raw = [0.0] * (n + fade_len)

    for i in range(len(raw)):
        t = i / SR
        s = (math.sin(2 * math.pi * 55 * t) * 0.5
             + math.sin(2 * math.pi * 82.5 * t) * 0.22
             + math.sin(2 * math.pi * 110 * t) * 0.12
             + math.sin(2 * math.pi * 165 * t) * 0.05)
        tremolo = 0.75 + 0.25 * math.sin(2 * math.pi * (1.0 / seconds) * t)
        raw[i] = s * tremolo

    noise = [random.uniform(-1, 1) for _ in range(len(raw))]
    noise = highpass(lowpass(noise, 5500), 2200)
    for i in range(len(raw)):
        raw[i] += noise[i] * 0.05

    loop = raw[:n]
    for i in range(fade_len):
        g = i / fade_len
        loop[i] = raw[i] * g + raw[n + i] * (1 - g)

    return fade(normalise(loop, 0.7), 10)


if __name__ == "__main__":
    for i in range(1, 4):
        write_wav(f"footstep_{i:02d}.wav", footstep(seed=i * 101))
    write_wav("wall_rustle_loop.wav", rustle_loop())
    write_wav("landmark_hum_loop.wav", hum_loop())
