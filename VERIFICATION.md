# Timed-training verification

The local source now provides configurable **1–300-second** training rounds, a visible countdown, one point per successful target hit, frozen final results, replay and duration changes. The new flow was built into a fresh APK, installed on a physical Motorola edge 60 fusion (Android 16) and reported working by the user (see the device section below). The original free-practice evidence is historical.

## Current source checks

| Evidence | Result | Scope |
|---|---|---|
| Strict behavioral RED: `Logs/training-round-red.xml` | 11 failed, 8 passed | Compilable rule scaffold; valid duration/start/countdown/score/pause/replay contracts failed before implementation |
| Strict UI RED: `Logs/training-ui-red.xml` | 7 failed, 0 passed | Missing saved numeric-duration menu/actions and missing bilingual round labels |
| Final EditMode: `Logs/training-editmode-final.xml` | **39 passed, 0 failed, 0 skipped** | Existing combat/reticle/shader regressions plus duration parsing, boundaries, expiry, replay, suspension and saved UI |
| Final PlayMode: `Logs/training-playmode-final.xml` | **9 passed, 0 failed, 0 skipped** | Actual saved scene, queued touch input/UI, movement/collision, shooting, marks/idle, round flow and bilingual HUD |
| Editor render previews | Visually checked | Configuration, active countdown and results views; ignored local `Logs/training-*-preview.bmp` |
| Re-run EditMode / PlayMode on 2026-10-11 | **39/39** and **9/9** passed | Independent re-run of the same suites before committing (`Logs/review-editmode.xml`, `Logs/review-playmode.xml`) |
| Android build and device install, 2026-10-11 | Build succeeded; installed in place and launched | New timed-round APK on the Motorola; see the device section below |

All launches used Unity **6000.0.70f1**. No packages, editor version or rendering pipeline were upgraded. Logs/XML/previews remain private ignored outputs, not repository deliverables.

## Rules and runtime architecture

- `TrainingRound` is a pure three-phase state machine: Configuration -> Active -> Results. Input is decimal digits representing an integer in **1..300**; empty, fractions, negatives, signs, overflow and out-of-range values fail. Default duration is 60 seconds. The UI requests a numeric mobile keyboard but retains pasted/raw text so validation can reject it instead of turning `1000` into `100` or `1.5` into `15`.
- `TrainingSession` owns the active-play elapsed clock. It updates before input and synchronizes again at the shot/hit boundary, so expiry cannot add an extra scored hit. `TrainingTarget` rejects hits outside an active, unsuspended round. Score is one point per accepted hit; 25 cumulative damage remains a separate counter, not health.
- Start/replay clears points, damage, impact marks, held controls and shot cooldown. Results preserve final score. The old RESET control is hidden and has no gameplay-reset listener, so it cannot restart or clear an active round.
- Configuration/results block touch movement/look/fire/sprint and desktop mouse capture. Finger roles are cleared; a held pointer cannot leak a shot into a new round. Focus loss and app pause are separate suspension causes: both must clear before active time resumes, and background time is excluded.
- `PrototypeBuilder` saves real menu/input/button objects and references in the editable scene. Every serialized UGUI graphic retains `UiDefault.mat`; the existing UI/Default inclusion/prebuild validator remains in effect. Runtime hides the centered reticle in menus.

## What the PlayMode tests establish

1. The saved scene opens at configuration with default 60. Numeric-field/start/replay/configure wiring exists, and the three rendered UI states have readable layout.
2. Queued InputSystem touchscreen events focus the real InputField and click the real start button. Text is then set **programmatically** for invalid/300-second cases; this does not prove physical keyboard typing.
3. Existing separate movement/sprint/look touches still move/rotate the player, release cleanly and respect the west boundary. A fire touch performs the actual rifle raycast, increases score/damage and creates visible surface impacts.
4. A real one-second elapsed round ends, stops scoring/firing, unlocks the cursor and freezes results. Replay reuses the selected duration and clears counters/marks; held fire does not leak into replay. Results can return to duration configuration.
5. Menu touches cannot drive gameplay. Explicit pause/focus callbacks stop active elapsed time, prevent hits and clear held controls; returning focus while another suspension remains does not resume early.
6. Directly applying 65 valid target hits retains 60 markers. The hidden old reset action does not erase the active score; replay resets them. This is **not** 65 player-fired shots.
7. Real language button actions localize configuration, gameplay timer and results in English/Spanish. They are automated button actions, not physical phone taps.

Intermediate checks found two harness assumptions, not bypassed gameplay failures: setting NumberPad changes UGUI `contentType` to Custom, so tests check keyboard/validation behavior rather than the preset enum; simulated joystick coordinates must scale with the Canvas. Final tests use raw-text validation and Canvas-aware coordinates.

## Reproduce

Use Unity 6000.0.70f1 and substitute PROJECT/RESULTS/LOG with local paths:

```text
Unity.exe -batchmode -nographics -projectPath PROJECT -runTests -testPlatform EditMode -testResults RESULTS -logFile LOG
Unity.exe -batchmode -projectPath PROJECT -runTests -testPlatform PlayMode -testResults RESULTS -logFile LOG
Unity.exe -batchmode -nographics -quit -projectPath PROJECT -executeMethod FieldOps.Editor.UiBuildValidation.Validate -logFile LOG
Unity.exe -batchmode -nographics -quit -projectPath PROJECT -buildTarget Android -executeMethod FieldOps.Editor.PrototypeBuilder.BuildAndroid -logFile LOG
```

Do not append `-quit` to test-runner commands. The runner owns completion. A hidden batch Game view needs test-only input/focus settings; physical-device behavior must be checked separately. Scene regeneration overwrites the generated scene/materials: save custom variants under another name before rebuilding it.

## Physical-device acceptance for the new APK (2026-10-11)

Build: ARM64 IL2CPP OpenGL ES 3 development APK from commit `913cea2`, Unity 6000.0.70f1, build result Success. APK: 41,493,278 bytes; SHA-256 `70430E131DA739D63176035B36C822AB33B603A3AD42AB4A3A1854CF0B4C306C`. Saved scene on disk: SHA-256 `73810718D6F0B01463FDAA161C7C01E96290953A1A57DEAA42C17519D3A9A441`.

Observed by the assistant: `adb install -r` succeeded without uninstalling or clearing data, and `UnityPlayerGameActivity` was the top resumed activity after launch. The only fatal-exception lines in logcat belonged to an Android system chooser activity from the previous day, not to this app.

Reported by the user after manual testing on the phone: **all checks passed as expected.**

- [x] Numeric keyboard opens; entering 1 and 300 starts valid rounds. 0/301/empty and invalid input show a clear message.
- [x] A short round counts down to 00:00 and shows frozen final score; further touches cannot score.
- [x] Repeat uses the selected duration and starts with zero counters/marks; change duration returns to configuration.
- [x] Configuration/results touches do not move/look/fire; active multi-touch still works.
- [x] Backgrounding the app during a round pauses active time and resumes fairly.
- [x] English/Spanish labels and native keyboard behavior are usable on the Motorola phone.

These are user-reported results, not recorded measurements: no screenshots of the new flow were captured, and lock-screen and focus-loss-while-holding-controls cases were not separately itemized. No FPS/thermal benchmark, prolonged endurance, broad Android compatibility or production/release signing is claimed.

## Ammunition and manual reload (2026-10-11)

Rules: 30-round magazine, infinite reserve, reload only by button (or R in the editor), 1.5 s reload that blocks firing, no auto-reload on an empty magazine. Reload time advances only while the round is actively playing, so pause/focus loss freezes it. Every round start/replay restores a full magazine and cancels any reload. The HUD shows `AMMO n / ∞`, or `RELOADING` while reloading; the RELOAD button turns red when the magazine is empty. Reload is a one-frame edge read in `PrototypeInput` (touch on the button or R key); the button is hidden and inert in menus.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/ammo-red.xml` | 10 failed, 39 passed | `Magazine` rule scaffold throwing `NotImplementedException` |
| Strict RED, `Logs/ammo2-red-edit.xml` / `ammo2-red-play.xml` | EditMode 4 failed / PlayMode 5 failed | Saved reload UI, bilingual labels and rifle/HUD integration missing |
| Final EditMode, `Logs/ammo2-green-edit.xml` | **53 passed, 0 failed** | Adds 10 `Magazine` rule tests, saved-scene reload wiring and bilingual labels |
| Final PlayMode, `Logs/ammo2-green-play.xml` | **14 passed, 0 failed** | Adds: firing consumes ammo and an empty magazine stops scoring; reload button refills after 1.5 s and blocks firing; R key; button inert in menus; new round restores ammo; pause freezes reload |
| Editor render preview | Visually checked | `AMMO 30 / ∞` and RELOAD button above FIRE; the infinity glyph renders |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route, ARM64 IL2CPP, GLES3 |

APK: 41,493,278 bytes; SHA-256 `D395F1A87B9DCCFA9EB6DB566910F8B7682734CB21CBF53EB5945BA579BDE6D6`. Saved scene on disk: SHA-256 `C5FB0454B54D7268335419F72DD0ACD8FD5B903CDE26C7C24B107EBFC079DCEA`.

Observed by the assistant: the APK hash differs from the previous build, the packaged scene contains the reload button, `adb install -r` succeeded without uninstalling, and the activity was the top resumed activity. Logcat showed no `NullReference`; the "Exception" lines belonged to the system window manager and a vendor service looking up the unpublished package on the Play Store.

Reported by the user after manual testing on the Motorola edge 60 fusion: **all checks passed as expected** (counter, consumption, reload timing and blocked firing, empty magazine, button position, round restart, background during reload). These are user-reported results; no screenshots of this build were captured, and no FPS/thermal benchmark is claimed.

Tests establish rule behavior and scene wiring. Queued touches stand in for physical fingers, and reload timing uses real elapsed time of about 1.5 s.

## Aim down sights and settings (2026-10-11)

Rules (`AimControl`, pure): hold mode aims while the AIM button is held; toggle mode flips on each new press. Sprinting or any suspension cancels aiming; reloading does not. Blend takes 0.15 s; field of view goes from 68 to 40 degrees and look sensitivity is multiplied by 0.5 at full blend. Aiming shows a red center dot, hides the cross reticle unless the "cross while aiming" option is on, and moves the rifle toward the center with 70% less bob/recoil. `GameSettings` stores the hold/toggle and keep-cross choices in PlayerPrefs (defaults: hold, cross off). The SETTINGS button is available in menus and during play; opening it suspends the round clock through `TrainingSession.SetSettingsOpen`. The editor right mouse button also aims. The AIM button is inert in menus.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/ads-red.xml` | 12 failed, 53 passed | `AimControl` and `GameSettings` scaffolds throwing `NotImplementedException` |
| Strict RED, `Logs/ads-red2-edit.xml` / `ads-red2-play.xml` | EditMode 10 failed / PlayMode 6 failed | Saved aim/settings UI, bilingual labels and integration missing |
| Final EditMode, `Logs/ads-final-edit.xml` | **75 passed, 0 failed** | Adds 10 aim-rule tests, 2 settings-persistence tests, saved-scene wiring and 9 bilingual label cases |
| Final PlayMode, `Logs/ads-preview2-play.xml` | **20 passed, 0 failed** | Adds: hold-aim zoom/sensitivity/dot/weapon pose; toggle mode from settings; keep-cross option; settings pause the round and work in menus; sprint cancels aiming while reloading does not; round end resets aiming |
| Editor render previews | Visually checked | Aiming view and settings panel (`Logs/ads-aim-preview.bmp`, `Logs/ads-settings-preview.bmp`). The first aiming capture showed the rifle stock covering the view and the panel showing 2% of the menu behind it; both were fixed and re-checked |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route, ARM64 IL2CPP, GLES3 |

APK: 41,494,164 bytes; SHA-256 `55542095C3B45F853D205D9B123E71F40D3908CA9AB7CC42F9E358D0CD0F3720`. Saved scene on disk: SHA-256 `A4EBBCB4BB78E9C32C737484E786534CD0720D1E43C09A3E1FBFDD4B078F1A11`.

Observed by the assistant: the APK hash differs from the previous build, the packaged scene contains the aim button, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal or Unity exceptions.

Reported by the user after manual testing on the Motorola edge 60 fusion: **all checks passed as expected** (hold and toggle aiming, zoom and lower sensitivity, red dot and optional cross, settings pausing the clock, sprint cancelling aim, reload while aiming, persistence after closing the app, button layout and Spanish text fit). These are user-reported results; no screenshots of this build were captured and no FPS/thermal benchmark is claimed. The right-mouse-button aiming has no automated test.

## Sound effects (2026-10-11)

> Update: the code-generated shot and reload sounds described in this section were later replaced by recorded per-weapon clips (see "Per-weapon audio" below). Only the empty-magazine click and the target-hit sound are still generated this way.

Sounds are generated by code, so there are no audio assets or licenses (`SoundSynth`, pure and deterministic; `GameAudio` plays them). Four effects: shot (about 0.22 s, one per fired round), target hit (about 0.12 s, only for hits that count), empty-magazine click (about 0.06 s, replaces the shot at the weapon's fire rate and spends nothing) and reload (exactly the 1.5 s reload). A rejected reload is silent. Pausing, opening settings or ending a round cuts any playing sound; menus are silent. `GameAudio` keeps per-effect counters that work without an audio device, which is how batch-mode tests observe playback.

The reload sound was redesigned after the first device test: the first version was three similar metallic knocks and the user found it unrealistic. The final version is a sequence of physical events: release click (0.08 s), magazine sliding out (0.18 s), magazine dropping (0.34 s), new magazine sliding in with rising pitch (0.60 s), heavy metal seating (0.76 s), charging-handle scrape (0.98 s) and bolt slam (1.16 s). Metal impacts use inharmonic ringing partials plus a noise transient; scrapes are band-passed noise.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/sfx-red-edit.xml` / `sfx-red-play.xml` | EditMode 10 failed / PlayMode 4 failed | Synth scaffold and missing audio wiring |
| Strict RED, `Logs/reload-red.xml` | 2 failed, 86 passed | Realistic reload sequence and heavy-impact loudness hierarchy; the gap and brightness checks already held and remain as constraints |
| Final EditMode, `Logs/reload-green.xml` | **88 passed, 0 failed** | Clips audible, bounded and finite; lengths; decay; immediate shot attack; reload sequence, silent gaps and loudness hierarchy; determinism; sample-rate independence; invalid-rate rejection; saved-scene audio wiring and a single listener |
| Final PlayMode, `Logs/reload-green-play.xml` | **24 passed, 0 failed** | Shot and hit counts match fired rounds and accepted hits; misses play only the shot; empty magazine plays only the click; one reload sound per accepted reload, cut on pause; menus silent |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route, ARM64 IL2CPP, GLES3 |

APK: 41,550,643 bytes; SHA-256 `280C1B4804F7227207AE659B8DD5DEE614C38C6814266C13F41DF6791C437B7E`. Saved scene on disk: SHA-256 `979FEF6B9A4E6DE188F474D9BADBC94DB821DF7B7F9E29539CD24E1147F72548`.

Observed by the assistant: each APK hash differed from the previous build, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal or Unity exceptions. The only audio line was an informational system message that the output stream opened.

Reported by the user after listening on the Motorola edge 60 fusion: all sound checks passed as expected, and the redesigned reload sound was accepted. The assistant cannot listen to audio, so the tests establish structure, timing, relative loudness and wiring, not realism; realism is the user's judgment. Synthesized sound approximates a real recording and is not a replacement for one. No screenshots, loudness measurements or FPS/thermal benchmarks are claimed.

## Configurable HUD layout (2026-10-11)

First phase of the second round of features (configurable HUD, then multi-weapon assets and audio; see the project plan). The player can drag the joystick and the fire, reload, aim and sprint buttons from SETTINGS > EDIT LAYOUT and save the layout. The settings and language buttons are deliberately fixed so a bad layout can never lock the player out.

Rules: `HudLayout` (pure) stores per-button offsets from the button's own anchor corner, in canvas units, as versioned JSON in PlayerPrefs (`hudLayout`); only buttons that differ from their default place are stored, an all-default layout removes the key, and corrupt, unsupported, duplicate, unknown or out-of-range data yields the default places. Positions are clamped so the whole button stays inside the safe area (re-applied when the safe area changes). Dragging uses the same polled touch input as gameplay (`PrototypeInput` forwards pointers to `PrototypeHud.LayoutPointer` while editing; the mouse works in the editor). SAVE is refused while buttons overlap each other or the fixed settings/language buttons or the toolbar. The round is suspended while editing and resumes afterwards. The joystick label and the ammo counter are now children of their buttons so they move with them; the toolbar sits at the top center, where no default button is.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/hud-red-edit.xml` / `hud-red-play.xml` | EditMode 27 failed / PlayMode 8 failed | `HudLayout` and `GameSettings.HudLayout` scaffolds, missing editor UI and bilingual labels |
| Final EditMode, `Logs/hud-final-edit.xml` | **115 passed, 0 failed** | Adds 17 `HudLayout` rule tests (9 are corrupt-data cases), 3 persistence tests, saved-scene wiring and 6 bilingual label cases |
| Final PlayMode, `Logs/hud-green2-play.xml` | **33 passed, 0 failed** | Adds 9: drag moves a button and only its new spot fires; layout survives a scene reload; cancel; reset then save clears the stored layout; overlap blocks saving; buttons cannot leave the screen; corrupt stored layout falls back to defaults; editing pauses and resumes the round; the default layout is saveable |
| Editor render previews | Visually checked | Edit toolbar in English and Spanish (`Logs/hud-edit-preview.bmp`, `hud-edit-preview-es.bmp`); all labels fit |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route, ARM64 IL2CPP, GLES3 |

A first green run showed one real defect rather than a test problem: PlayMode runs at 640x480 (4:3), where the first toolbar position (bottom center) covered the default aim button and made the default layout unsaveable. The toolbar was moved to the top center, which no default button occupies at any aspect ratio.

APK: 41,551,798 bytes; SHA-256 `AFFA3AC90458CD78BD2A59E31E5249FC7CB763907F05A21CEF10A38C3CA487C2`. Saved scene on disk: SHA-256 `400B21739D80CA7D82ED362BB19BEB0930F6A97668F4328FB3891DA5073E321B`.

Observed by the assistant: the APK hash differs from the previous build, the packaged scene contains the layout toolbar, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal or Unity exceptions.

Reported by the user after manual testing on the Motorola edge 60 fusion: **all checks passed as expected** (entering the editor from the menu and during a round, dragging, screen limits, overlap blocking, save and play, persistence after fully closing the app, cancel, reset, round pause, Spanish text fit). These are user-reported results; no screenshots of this build were captured, positions only (no button scaling) are supported, and no FPS/thermal benchmark is claimed. The tests drive input with queued touches, not physical fingers.

## Five selectable weapons with imported models (2026-10-11)

Second phase of the second round of features. The single rifle was replaced by five weapons selected from the pre-round menu, the results screen and the settings menu. The player can swap weapons during a round; the score and the target's damage counters and impact marks restart, the new weapon starts with a full magazine, and the clock is not reset (user decision). Weapon names are generic. Models are five unmodified FBX files from the Quaternius "Ultimate Gun Pack" (CC0 according to its OpenGameArt page, downloaded 2026-10-11; the archive contained no separate license file), with `NOTICE.txt` next to them and an entry in `CREDITS.md`. The assistant checked license statements on the source pages; they were summarized by a tool, not read verbatim from legal text.

| Weapon (model) | Magazine | Fire interval | Damage | Aim zoom | Range |
|---|---|---|---|---|---|
| Pistol (Pistol_1) | 12 | 0.30 s | 20 | 50 deg | 60 |
| Assault rifle (AssaultRifle_2, default) | 30 | 0.12 s | 25 | 40 deg | 80 |
| Sub-machine gun (SubmachineGun_1) | 32 | 0.07 s | 15 | 45 deg | 60 |
| Shotgun (Shotgun_4) | 6 | 0.85 s | 15 x 8 pellets (4 deg ring) | 50 deg | 40 |
| Sniper rifle (SniperRifle_4) | 5 | 1.20 s | 100 | 20 deg | 120 |

All weapons keep the 1.5 s reload until per-weapon audio defines it. The shotgun pellet pattern is deterministic (`PelletPattern`); one point is scored per trigger pull that hits. The scene generator creates the `WeaponDefinition` assets, instantiates the models under the weapon holder (Ignore Raycast layer, no colliders, no shadows), fits each to a first-person length and computes the aiming pose from the model size.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/weapon-red-edit.xml` / `weapon-red-play.xml` | EditMode 19 failed / PlayMode 8 failed | Pellet pattern, per-weapon zoom, weapon settings, catalog, models and selector UI, stats, fire rate, shotgun scoring, persistence |
| Strict RED, `Logs/fix-red-edit.xml` / `fix-red-play.xml` | EditMode 2 failed / PlayMode 3 failed | Upright, forward-facing models; selectors on the results screen and in settings |
| Strict RED, `Logs/mid-red-edit.xml` / `mid-red-play.xml` | EditMode 1 failed / PlayMode 1 failed | Score reset rule and mid-round weapon change |
| Final EditMode, `Logs/mid-green-edit.xml` | **139 passed, 0 failed** | Adds weapon catalog and stats, model sizes, orientation, credits and notice, aim pose, pellet pattern, weapon settings, per-weapon zoom, score reset rule |
| Final PlayMode, `Logs/mid-green-play.xml` | **44 passed, 0 failed** | Adds: selector cycling and wrap-around; per-weapon stats in the menu and in play; fire rates; sniper damage; shotgun pellets and one point per shot; per-weapon zoom; persistence across a scene reload; weapon change on the results screen and in settings; mid-round change resets the score and keeps the clock |
| Editor render previews | Visually checked | Each weapon in hip and aiming views (`Logs/weapon-previews/`), configuration, results and settings menus |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route, ARM64 IL2CPP, GLES3 |

Defects found by the previews and the user's device test, all fixed with a failing test first where applicable:
- The generator overwrote the model's import scale (100) and import rotation (270 degrees about X), first making the guns 100 times too small and then laying them on their side. The generator now multiplies the scale and composes the rotation; a test checks that the gun's up axis points up and its muzzle points forward.
- Unity mirrors the X axis on FBX import, so the first +90/-90 choice pointed the muzzle at the camera; checked in the aiming previews.
- The first aiming pose left the gun glued to the lens or off-screen; the pose is now computed from each model (top edge just under the eye line, rear end 38 cm from the camera).
- The weapon selector initially existed only in the pre-round menu; it was added to the results screen and the settings menu after the first device test, and mid-round swapping was allowed after the user asked for it.
- A first showcase test tapped the AIM button using stale canvas coordinates right after a preview capture; the harness now refreshes the canvas first. This was a test problem, not a product defect.

APK: 41,551,894 bytes; SHA-256 `CA8026D9049B95A99943BBF31C0C61C908662D1D88A67CBD12C838267419F9C5`. Saved scene on disk: SHA-256 `B80F82FAA59A3F442EE4C487034C306DDD64260A8474373B0DCEDE901C89C090`.

Observed by the assistant: each APK hash differed from the previous build, the packaged scene contained the five weapon objects and three weapon selectors, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal or Unity exceptions.

Reported by the user after manual testing on the Motorola edge 60 fusion: **all checks passed as expected** on the final APK (upright, forward-facing weapons in all five cases, selectors in the pre-round menu, results screen and settings, mid-round swap with score reset, persistence, per-weapon feel). These are user-reported results; no screenshots of the final device build were captured and no FPS/thermal benchmark is claimed. The look is a stylized low-poly interpretation, not a reproduction of any commercial game, and the models have no iron sights. Sound is still shared by all weapons.

## Per-weapon audio (2026-10-11)

Third phase of the second round of features. Each weapon now plays its own recorded shot and reload clip (`WeaponDefinition.shotClip` / `reloadClip`, played by `GameAudio` for the equipped weapon), and **the reload lasts exactly as long as its clip**. The user asked for this so that, for example, a pistol does not sound like a sniper rifle. Clips are processed derivatives of third-party recordings, converted to mono 44.1 kHz and imported as decompress-on-load Vorbis for instant playback on the phone. Licenses and changes are recorded in `CREDITS.md` and `Assets/FieldOps/Audio/Weapons/NOTICE.txt`.

| Weapon | Shot clip | Reload clip (= reload time) | Origin |
|---|---|---|---|
| Pistol | 1.28 s | 1.58 s | Shot: single pistol shot cut from the CC BY recording; reload: CC0 pistol reload |
| Assault rifle | 1.57 s | 1.56 s | Shot: single rifle shot (CC BY); reload: CC0 rifle reload |
| Sub-machine gun | 0.55 s | 1.39 s | Shot: the rifle shot raised in pitch (x1.35), high-passed and shortened (user decision, no recorded SMG shot was found); reload: the rifle reload raised in pitch (x1.12) |
| Shotgun | 0.55 s | 2.05 s | Shot: single shotgun shot (CC BY); reload: three generated shell ticks plus the CC0 pump cock |
| Sniper rifle | 1.35 s | 1.97 s | Shot: bolt-rifle single shot (CC BY); reload: bolt-working sounds cut from the same recording, with silence added |

Licenses: the shots come from "Gunshot Sounds" (author Vincent Sevedge, uploaded to OpenGameArt by Tabasco). The OpenGameArt page labels it CC0, but the downloaded archive states **CC BY 3.0**, so the stricter terms are followed and attribution is given. The reloads come from "Gun reload sounds" by Joaken (CC0). If the APK is ever distributed publicly, CC BY 3.0 requires showing the credit where the sounds are used (for example a credits screen); the repository already complies.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/audio-red-edit.xml` | EditMode 8 failed | Per-weapon clips, distinct content, reload equals clip, mono/sample rate, SMG pitch, import settings, stats that referenced the old 1.5 s reload |
| Strict RED, `Logs/audio-red2-play.xml` | PlayMode 2 failed | Last-played shot/reload clips and reload timing per weapon. A first version of these two tests passed with no code (two null values compared equal); they were strengthened with not-null and clip-length assertions and then failed as expected |
| Final EditMode, `Logs/audio-green-edit.xml` | **141 passed, 0 failed** | Adds 7 weapon-audio tests; the code-generated shot/reload tests were removed together with that code |
| Final PlayMode, `Logs/audio-green-play.xml` | **46 passed, 0 failed** | Adds: each weapon plays its own shot and reload clip; reload takes as long as its clip for the pistol, shotgun and sniper |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route; all ten clips are in the APK (about 108 KB of compressed audio) |

APK: 41,551,978 bytes; SHA-256 `89E7F449890F7830F3C5143734952E8BF4D1FEFEEA31F0338D36FCD3D5064F6E`. Saved scene on disk: SHA-256 `628E63FBA22A8741A7CC51600504399D750DA3354799CFEBE0DDFDFF86359638`.

Observed by the assistant: the source recordings were inspected as waveforms to choose isolated shots and the bolt sounds; each processed clip was plotted and checked for clean attacks, fades and levels; the APK hash differed from the previous build; `adb install -r` succeeded without uninstalling; the activity was the top resumed activity; and logcat showed no `NullReference`, fatal, Unity or audio errors. The assistant cannot listen to audio, so realism and loudness balance are judged by the user.

Reported by the user after listening on the Motorola edge 60 fusion: all sound checks passed as expected on the final APK (distinct shots per weapon, reload lengths per weapon, quality, cutting the previous sound when swapping weapons). These are user-reported results; no loudness measurements, screenshots or FPS/thermal benchmarks are claimed. The empty-magazine click and the target-hit sound remain generated by code.

## Military training-base map, sky and FPS counter (2026-10-11)

Fourth step of the second round of features (the character step is still pending). The scenery was rebuilt as a military training base with a photographic overcast sky. It is an original layout made of simple shapes and CC0 textures from Poly Haven; the user rejected an earlier civilian city-kit idea as not military enough, and no Call of Duty map or asset is used.

Map: `EnvironmentBuilder` (editor generator) builds the ground (dry dirt), firing lane (cracked asphalt), concrete firing-line pad, backstop and field walls (concrete, normal-mapped), 10 shipping containers (3 tints, 2 stacked, container texture with normal map), 7 sandbag walls (hessian texture with normal map), concrete barriers, a watchtower and an open hangar (rusted green and corrugated metal), 7 oil drums, 3 tire stacks, painted-metal supply crates and a treeline beyond the walls. The firing corridor (|x| < 3 m from the player start to the target), the 44 x 56 m field walls at 22 m and the target position are unchanged, so gameplay tests still hold. Surface textures are 1K and compressed with ASTC on Android; the sky is a 2K panorama (the 1K version looked blurry on the phone). Android uses quality level "Medium" (hard shadows, low shadow resolution, no antialiasing, one pixel light). Sources and licenses are in `CREDITS.md` and `Assets/FieldOps/ThirdParty/PolyHaven/NOTICE.txt`; the HDR sky was converted to an 8-bit image (derivative).

FPS counter: `FpsMeter` (pure) counts frames over a 0.5 s window; the HUD shows it at the top center, green from 50 FPS, yellow from 30, red below. It is on by default and `GameSettings.ShowFps` (key `showFps`) stores the choice; the settings menu has a "FPS COUNTER: ON/OFF" switch (EN/ES). `FrameLogger` and `FrameTimeStats` (pure) log average FPS, 95th-percentile frame time and the worst frame every 10 s, only in development builds, for `adb logcat`.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/map-red-edit.xml` | EditMode 7 failed | Sky/fog/skybox camera, textured ground and lane, base props, normal maps, Android texture import, clear firing lane, batching budget |
| Strict RED, `Logs/fps-red-edit.xml` / `fps-red-play.xml` | EditMode 6 failed / PlayMode 1 failed | Frame statistics and the frame logger |
| Strict RED, `Logs/seam-red.xml` | PlayMode 1 failed | A vertical sky seam line, measured by rendering only the sky from 72 yaw angles |
| Strict RED, `Logs/fpsui-red-edit.xml` / `fpsui-red-play.xml` | EditMode 18 failed / PlayMode 2 failed | FPS meter, FPS setting, bilingual labels, counter and settings switch |
| Strict RED, `Logs/sky2k-red.xml` | EditMode 2 failed | 2K sky resolution and its import settings |
| Final EditMode, `Logs/sky2k-green-edit.xml` | **176 passed, 0 failed** | Adds map, frame statistics and FPS meter/setting tests |
| Final PlayMode, `Logs/sky2k-green-play.xml` | **51 passed, 0 failed** | Adds the sky-seam probe, FPS counter default and switch, frame logger report, and a multi-angle map capture |
| Editor render previews | Visually checked | Map from five angles, settings panel with the new switch (`Logs/weapon-previews/`) |
| Android build | Build succeeded | Same Unity 6000.0.70f1 route; the build report lists the 2K sky at about 914 KB and each 1K texture at about 612 KB |

Defects found and fixed: (1) the user saw a straight line in the sky. The panorama image itself is continuous (edge difference 4.0, normal neighbor difference 4.4), so the line was a rendering seam from texture mipmaps where the image wraps. A test renders only the sky from 72 angles and scores the strongest vertical step against the median: the worst angle scored 20.3 (normal about 2.5) and the test requires under 8. Disabling mipmaps on the sky texture fixed it. (2) The first sky was blurry on the phone because 1024 px was stretched about 12 times; a 2K version (6.5 MB download, md5 verified) replaced it, with the HDR original kept out of the repository. (3) A first version of the clear-lane test counted the player's own collider as scenery; the test now excludes it.

Device measurement (Motorola edge 60 fusion, development build, `adb logcat`): 4 ten-second windows were available in the log buffer. Steady state was **59 to 60 FPS** (the display cap) with the 95th-percentile frame time at 16.7 ms; there was one 2.1 s stall while the app loaded (once per launch, it also appears in the earlier menu-only measurement) and one 88 ms hitch. This is about 40 seconds of data, not a long-session benchmark; no thermal or battery measurement is claimed.

APK: 41,552,512 bytes; SHA-256 `1032190C178118D01687CD73720F08B4FD8FB22BA262818AA859B2D3A752690C`. Saved scene on disk: SHA-256 `5ABE9C7F2BDB2A9C2C4966A0B8152D4286C31BA0741929F58DE19DA79AFE2DE6`.

Observed by the assistant: the packaged scene contained the FPS counter label and its settings button, the build report listed the new textures and the 2K sky, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal, Unity, shader or texture errors.

Reported by the user after testing on the phone: all checks passed as expected on the final build (a sharper background, no visible sky line, the FPS counter and its switch, including persistence after fully closing the app). These are user-reported results; no screenshots of the final device build were captured.

## Soldier target (2026-10-11)

Fifth step of the second round of features. The primitive-shape humanoid target was replaced by the "Swat" soldier from Quaternius' Ultimate Modular Men Pack (CC0 according to the pack page; one unmodified FBX of about 8 MB, 3,106 triangles, 62 bones, flat-color materials). The user picked it from the pack preview as the closest free option to a tactical operator; no commercial game's character or operator is used, and the model holds no weapon because the pack provides none. The pack's own `License.txt` was not part of the downloaded FBX folder, so the CC0 statement comes from the pack page; this is recorded in `Assets/FieldOps/ThirdParty/QuaterniusMen/NOTICE.txt` and `CREDITS.md`.

Behavior: the soldier stands on the target platform facing the player (the model faces +Z, so it is turned 180 degrees), breathes with the `Idle` animation (looping) and plays `HitRecieve` when a scoring hit lands (at most once every 0.5 s), returning to idle afterwards. It is still indestructible. `CharacterBuilder` (editor generator) keeps only these two of the 24 imported clips, creates the animator controller (`Assets/FieldOps/Animation/SwatTarget.controller`, states Idle and Hit, trigger `Hit`) and attaches 14 capsule hit zones to the bones (hips, abdomen, torso, chest, neck, head, arms and legs) so the impact marks, which are parented to the collider that was hit, move with the animation. Impact marks are now bright amber because near-black marks vanished on the near-black suit (a contrast test checks this). The existing sway code and every gameplay test still apply.

Package change (approved by the user): the built-in Unity module `com.unity.modules.animation` was enabled, because the `Animator` type lives in it and the project previously listed only the modules it needed. This is one entry in `Packages/manifest.json` and six lines in `Packages/packages-lock.json`; nothing is downloaded and the editor version is unchanged.

| Evidence | Result | Scope |
|---|---|---|
| Strict RED, `Logs/char-red-edit.xml` / `char-red-play.xml` | EditMode 7 failed / PlayMode 2 failed | Imported soldier with animator, old primitives gone, looping idle and hit reaction, clip import settings, size and facing, bone colliders, credits; idle animation, hits on every body part and the hit reaction in play |
| Strict RED, `Logs/mark-red.xml` | EditMode 1 failed | Contrast between impact marks and the dark suit |
| Final EditMode, `Logs/mark-green-edit.xml` | **185 passed, 0 failed** | Adds 9 soldier tests |
| Final PlayMode, `Logs/char-final-play.xml` | **54 passed, 0 failed** | Adds: the soldier idles with real animation and every body part can be hit; a hit plays the reaction and returns to idle while the target stays indestructible; a multi-angle capture |
| Editor render previews | Visually checked | Soldier far, close, side and with impact marks (`Logs/weapon-previews/`) |
| Android build | Build succeeded | The build report lists the soldier model at about 1 MB and the controller at under 1 KB; the 2 kept clips total about 43 KB; the APK grew by about 0.4 MB |

Problems found while building it, all fixed: (1) the first test run could not compile because `Animator` is not in the project's module list; enabling the module needed the user's approval, which was requested and granted; (2) the model importer was not creating the rig avatar the `Animator` needs, so the generator failed until the import settings were set to create the avatar (a test now checks this); (3) a test assumed every bone is a child of the hips, but the legs hang from a different bone, so it now checks that the hit zones are inside the skeleton; (4) the number of planned hit zones was miscounted (13 instead of 14) and a neck zone was added to close a gap between torso and head; (5) the first impact marks were invisible on the black suit.

Device measurement (Motorola edge 60 fusion, development build, `adb logcat`, soldier animated): 24 ten-second windows, about 4 minutes of play. Weighted average **59.9 FPS** (the display cap), worst window 57.9 FPS, the 95th-percentile frame time stayed at 16.7 ms in every window, and the single worst frame was 367 ms (once). This is a short session, not a long-session benchmark; no thermal or battery measurement is claimed.

APK: 41,969,955 bytes; SHA-256 `EF579FCC8453C3C7433D14D5EC623885AD541AC354B1F83F5F891C2A157C05D5`. Saved scene on disk: SHA-256 `7A06987CC8584B5A5DC6B3AB4F675BF619F38090F1396C8ACEFC4AC8978761B2`.

Observed by the assistant: the build report listed the soldier assets, `adb install -r` succeeded without uninstalling, the activity was the top resumed activity, and logcat showed no `NullReference`, fatal, Unity, animator, avatar or shader errors.

Reported by the user after testing on the phone: all checks passed as expected on the final build (the soldier's look, idle breathing, hit reaction, hits on every body part with the marks staying attached, visibility of the amber marks, and unchanged weapon, settings and map behavior). These are user-reported results; no screenshots of the final device build were captured.

## Shot and hit-zone fixes after the soldier target (2026-10-11)

Two problems were reported by the user on the phone right after the soldier target: a red "Can't add component because class 'SphereCollider' doesn't exist" message in the development console on every hit, and shots at the legs not registering reliably.

**1. Console error.** The impact mark was created at runtime with `GameObject.CreatePrimitive(PrimitiveType.Sphere)`, which adds a `SphereCollider` that was then destroyed. The message points at the collider class missing from the Android build (probably removed by code stripping; this was not confirmed against the build itself). The mark is now an empty object with a serialized sphere mesh (`TrainingTarget.markMesh`, assigned by the scene generator) and a `MeshRenderer`; no collider is ever created. Reported by the user after installing the fixed build: the red text no longer appears.

**2. Legs not registering.** Two separate causes were found.
- *Hit zones too small (real, fixed).* Casting rays from the camera to the baked vertices of all four skinned meshes of the soldier showed that 1,120 of 6,259 visible points (about 18 %) were not covered by any hit zone, almost all between 0.8 and 1.0 m (hips and upper thighs). The capsules for hips, abdomen, thighs and calves were widened. Now 65 of 6,259 (about 1 %) are missed, only at the tips of the feet and part of the head. A later frame-by-frame check while the idle animation plays, from varying angles, measured 0 misses in 39,839 leg samples.
- *The player's own collider blocked the shots (the main cause).* Because the editor checks above showed the legs covered from every side, a development build logged every shot (`FIELD_OPS_SHOT`, dev builds only). On the phone, 30 of 32 shots hit the object `Player` at distance 0 and only 2 reached the soldier. The camera sits inside the player's capsule and a ray aimed steeply downward was blocked by it; this explains why legs worked "sometimes" from the front and not from the side or back, where the player stands closer and aims lower. The player is now on the `Ignore Raycast` layer (it still collides with the world when walking).

| Evidence | Result | Scope |
|---|---|---|
| Strict RED | Failed | A steep-pitch test (35 degrees and more) hit `Player`; a serialized-mark-mesh test failed |
| Final EditMode | **185 passed, 0 failed** | Unchanged count |
| Final PlayMode | **59 passed, 0 failed** | Adds 5: leg hit zones from the real eye position, all visible body vertices covered, frame-by-frame leg coverage while animating, impact marks without runtime primitives, steep-downward shots not blocked by the player |
| Phone log, diagnostic build | 30 of 32 shots hit `Player` | Observed by the assistant through `adb logcat` |

Phone check of the final build (APK SHA-256 `91cc53218144094ed64c10bad7336c1d79f5debffce9fb906aa0edec4d0454e5`, 41,970,215 bytes), observed by the assistant through `adb logcat` after the user shot the legs from the side and the back: 64 shots logged, 61 hit the soldier and all 61 were accepted (51 lower legs, 8 upper legs, 2 hips), 0 hit `Player`, and 3 hit the low target platform under the feet (the user's one missed shot, aimed at ground level). No `SphereCollider`, `NullReference` or other exception appeared. The user also reported the red console text gone. Limits: one short session on one phone. The development-only shot log (`FIELD_OPS_SHOT`) stays in the code and prints nothing in release builds.

## Historical first-delivery evidence (before timed rounds)

The earlier free-practice prototype passed **11 EditMode / 5 PlayMode** tests and built/installed/launched on a Motorola edge 60 fusion running Android 16. The user reported physical move/look/sprint/fire/marks/reset/language checks passing. The public screenshot is from that earlier delivery.

Historical APK: 29,636,840 bytes; SHA-256 `51A4740AA6C8B5C817851F7BB9195E270B6F0461FCB29CF369EB6C9D5EFBBB69`. Historical scene SHA-256 `578D8234BAB875013A905EA2E2A54EAC243A2A03C8778AF893193903FB44C8DC`. These hashes are not the updated feature artifacts.

The original strict combat RED had 7 failed/1 passed. The first Android APK showed magenta UGUI; explicitly including UI/Default and saving UiDefault.mat on the 21 original graphics fixed it. Shader RED was 2 failed; actual corrected phone captures confirmed the visual fix. The new menu graphics preserve that dependency and the validator counts them dynamically.

An initial sandbox-only license IPC failure was harness unavailability, not behavioral RED or proof of missing activation. Approved launches ran actual tests. No licensing data is published.

## Work-unit and rollback boundary

The timed-round feature (commit `913cea2`) is one cohesive unit: new TrainingRound/TrainingSession scripts and metas; combat/input/rifle/target/HUD integration; saved scene generation and scene; new rule/UI tests and updated runtime tests; README/this record. Rollback removes the new files and restores only those modified tracked files to the pre-feature prototype, without touching pinned packages or unrelated work. No release or APK upload was created.

Sound, attacking/dead enemies, ADS, ammunition, reload, multiplayer and campaign missions remain out of scope.
