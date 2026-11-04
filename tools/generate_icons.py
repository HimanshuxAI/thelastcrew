#!/usr/bin/env python3
"""Generates the home-screen icons for TheLastCrew.

Pure stdlib PNG writer, no Pillow required. The mark is a set of concentric
rings: a sound pulse leaving the centre of a dark planet.

Usage:  python3 tools/generate_icons.py
Output: web/icons/*.png
"""

import math
import os
import struct
import zlib

OUT = os.path.join(os.path.dirname(__file__), "..", "web", "icons")

BG = (5, 7, 10)
FG = (79, 195, 247)
FG_DIM = (32, 96, 128)


def write_png(path, size, pixel):
    raw = bytearray()
    for y in range(size):
        raw.append(0)  # filter: none
        for x in range(size):
            raw.extend(pixel(x, y))

    def chunk(tag, data):
        return (struct.pack(">I", len(data)) + tag + data
                + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF))

    ihdr = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(chunk(b"IHDR", ihdr))
        f.write(chunk(b"IDAT", zlib.compress(bytes(raw), 9)))
        f.write(chunk(b"IEND", b""))


def make_mark(scale=1.0):
    """Returns a pixel function drawing the pulse mark."""

    def pixel(x, y, size):
        # -1 .. 1 coordinates
        nx = (x + 0.5 - size / 2) / (size / 2)
        ny = (y + 0.5 - size / 2) / (size / 2)
        r = math.hypot(nx, ny) / scale

        # soft background vignette
        bg = 1.0 - min(1.0, r * 0.45)
        base = tuple(int(c * (0.35 + 0.65 * bg)) for c in BG)

        # centre dot
        if r < 0.075:
            k = 1.0 - (r / 0.075)
            col = mix(base, FG, 0.95 * k + 0.05)
            return col + (255,)

        # rings
        best = 0.0
        for i, radius in enumerate((0.26, 0.43, 0.60, 0.77)):
            d = abs(r - radius)
            thickness = 0.030
            if d < thickness:
                k = 1.0 - d / thickness
                k = k * k * (1.0 - i * 0.16)
                best = max(best, k)

        if best > 0:
            target = FG if r < 0.5 else FG_DIM
            col = mix(base, target, min(1.0, best * 1.15))
            return col + (255,)

        return base + (255,)

    return pixel


def mix(a, b, t):
    t = max(0.0, min(1.0, t))
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def render(path, size, scale=1.0):
    fn = make_mark(scale)
    write_png(path, size, lambda x, y: fn(x, y, size))
    print(f"{path}  {size}x{size}")


if __name__ == "__main__":
    render(os.path.join(OUT, "icon-192.png"), 192)
    render(os.path.join(OUT, "icon-512.png"), 512)
    render(os.path.join(OUT, "apple-touch-icon.png"), 180)
    # maskable: content pulled in to survive the platform's circle crop
    render(os.path.join(OUT, "maskable-512.png"), 512, scale=0.72)
    render(os.path.join(OUT, "favicon-32.png"), 32)
