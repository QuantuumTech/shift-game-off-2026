# SETUP — from opening the project to uploading to itch.io

Target editor: **Unity 2022.3.48f1** (the version this project declares in
`ProjectSettings/ProjectVersion.txt`).

## 1. Open the project

1. Open **Unity Hub**.
2. Click **Add** (or "Open" dropdown → "Add project from disk") and select this
   folder (`game-off-2026-shift`).
3. Open it with **2022.3.48f1**. If Hub offers a different 2022.3.x LTS, that's
   fine too — the code only uses long-stable 2022.3 APIs.
4. Wait for the first import to finish (Unity generates `Library/` and the
   rest of `ProjectSettings/` automatically — both are intentionally absent
   from the repo).

## 2. One-click scene setup (~1 minute)

Scenes are **not** hand-authored in this project. Instead:

1. In the Unity editor menu bar, click **SHIFT → Setup Project (Scene + Bootstrap + WebGL)**.
   Do this before editing anything else, since it starts from a fresh scene
   (Unity may ask to save changes to the default sample scene — you can discard it).
2. The menu does everything:
   - creates `Assets/Scenes/Main.unity` with a Main Camera (orthographic, with CameraFit),
   - adds one **Bootstrap** GameObject and assigns all 8 level files from `Assets/Levels/`,
   - registers the scene in Build Settings,
   - sets the WebGL player settings (company QuantuumTech, product SHIFT, 960×540, Brotli),
   - switches the active build target to WebGL.
3. **Press Play.** You should see the title screen. Click **Start** and you're in Level 1.

That's the whole manual setup. There is nothing else to wire: the player,
levels, and all UI are created in code at runtime.

## 3. Quick playtest checklist

- Title → Start → Level 1 ("First Flip").
- Run with A/D, jump with Space, press **SHIFT** — the cyan bridge should
  ghost out / materialize and the background color should swap.
- Walk into spikes → respawn at the start. Press **R** → level restarts.
- Finish Level 1 → "Level Clear!" → Level 2 loads. **Esc** → title.
- Level Select shows Level 2 unlocked.

## 4. Build for WebGL

1. **File → Build Settings…** — platform should already be **WebGL**
   (the setup menu switched it; if not, select WebGL and click "Switch Platform").
2. Confirm `Scenes/Main` is listed and checked.
3. Click **Build** (not "Build and Run") and choose a folder named **Build**
   at the project root (it's git-ignored).
4. Wait for the build to finish — first WebGL build can take several minutes.

## 5. Upload to itch.io

1. Zip the **contents** of the `Build` folder (the `index.html` must be at the
   top level of the zip, not inside a subfolder).
2. On the itch.io project page (draft): set **Kind of project = HTML**.
3. Upload the zip and check **"This file will be played in the browser."**
4. Embed options: viewport **960 × 540**, enable the **fullscreen button**.
5. Copy for the page body (title, tagline, description, controls, AI
   disclosure) is drafted in `docs/itch-page-copy.md`.
6. Save as a draft first, play it once in the browser yourself, then publish /
   submit to the jam before the deadline.

## Troubleshooting

- **Title screen says "No levels assigned"** → run the SHIFT setup menu
  (step 2). If you already did, check the Bootstrap object's `levelFiles`
  array in the Inspector: it should hold `level1`…`level8`.
- **Menu "SHIFT" doesn't appear** → a script failed to compile; check the
  Console for red errors and that the editor finished importing.
- **Text doesn't render** → the UI uses Unity's built-in font
  (`LegacyRuntime.ttf` / `Arial.ttf` fallback, both ship with 2022.3).
- **Flip does nothing** → make sure the Game view has keyboard focus (click
  inside it once), and that you're not holding Shift from before Play started.
- **Blocks overlap the player after a flip** → by design this can't happen:
  flips that would materialize a block inside the player are denied with a
  red flash. If a level feels unfair, its map is a text file — edit
  `Assets/Levels/levelN.txt` and press Play again.
