# SHIFT

A tiny puzzle-platformer built for **Game Off 2026** (theme: **SHIFT**).

The world exists in two states — **A** (cyan, deep indigo) and **B** (orange, dark plum). Blocks, platforms, and spikes exist in one state or the other. Press **SHIFT** and the whole world flips: solid becomes ghost, ghost becomes solid, hazards swap, and the palette inverts — while you keep your exact position and momentum. Every level is a single screen, and every level is only passable by flipping at the right moment: mid-run, mid-fall, mid-jump.

## Controls

| Action | Keys |
|---|---|
| Move | A / D or Left / Right arrows |
| Jump | Space, W, or Up arrow (coyote time + jump buffer included) |
| **Flip the world** | **Left or Right SHIFT** |
| Restart level | R |
| Back to title | Esc |

## How the flip works

- Objects belonging to the inactive state stay visible as faint **ghosts** with no collision, so you can plan your next flip.
- A flip is **denied** (short red flash) if a block would materialize inside you — you can never be crushed or soft-locked.
- Flipping has a short cooldown (0.22s) so it can't be spammed.
- Touching spikes that are active in the current state respawns you at the level start, back in State A.
- Reach the green goal to clear the level. Clearing unlocks the next level in the Level Select screen (in memory only — there is deliberately no save system).

## The 8 levels

Each level teaches exactly one flip idea:

1. **First Flip** — flip to make a bridge appear
2. **Now You See It** — flip to remove a wall (and flip back for the spikes)
3. **Leap of Faith** — flip mid-jump to land on a B platform
4. **Crossfire** — flip between alternating spike patches
5. **The Shaft** — flip while falling through a floor
6. **Double Shift** — chained double-flip across a gap
7. **Both Sides Now** — low ceiling, flips only, spikes in both states
8. **Finale** — everything at once

Levels are plain ASCII maps in `Assets/Levels/level1.txt` … `level8.txt`, parsed at runtime — edit the text files to remix them. Legend: `#` solid always · `A` solid in State A · `B` solid in State B · `^` spikes (State A) · `v` spikes (State B) · `P` player start · `G` goal · `.` empty.

## Tech

- Unity **2022.3.48f1** (LTS), WebGL build for itch.io — plays in the browser, no download.
- Zero art assets: every sprite is generated procedurally at runtime and tinted from the two palettes.
- Everything (player, levels, UI) is built in code from a single `Bootstrap` object — see `SETUP.md` for the 5-minute editor setup.

## Jam info

- **Game Off 2026**, theme: SHIFT. Submission deadline: **December 1, 2026, 4:37 PM ET** (target submit: November 30). Source must be in a public GitHub repo; the game is submitted on itch.io.
- Solo project by Quantez Wilson (QuantuumTech).

## AI-use note

Honest disclosure for the jam submission form: the code and project scaffolding for this game were written with substantial AI assistance (Muse). The game concept, level design direction, scope decisions, playtesting, and final calls are the author's. This will be stated on the submission form under the jam's AI-involvement question.
