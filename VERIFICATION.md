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

## Historical first-delivery evidence (before timed rounds)

The earlier free-practice prototype passed **11 EditMode / 5 PlayMode** tests and built/installed/launched on a Motorola edge 60 fusion running Android 16. The user reported physical move/look/sprint/fire/marks/reset/language checks passing. The public screenshot is from that earlier delivery.

Historical APK: 29,636,840 bytes; SHA-256 `51A4740AA6C8B5C817851F7BB9195E270B6F0461FCB29CF369EB6C9D5EFBBB69`. Historical scene SHA-256 `578D8234BAB875013A905EA2E2A54EAC243A2A03C8778AF893193903FB44C8DC`. These hashes are not the updated feature artifacts.

The original strict combat RED had 7 failed/1 passed. The first Android APK showed magenta UGUI; explicitly including UI/Default and saving UiDefault.mat on the 21 original graphics fixed it. Shader RED was 2 failed; actual corrected phone captures confirmed the visual fix. The new menu graphics preserve that dependency and the validator counts them dynamically.

An initial sandbox-only license IPC failure was harness unavailability, not behavioral RED or proof of missing activation. Approved launches ran actual tests. No licensing data is published.

## Work-unit and rollback boundary

The timed-round feature (commit `913cea2`) is one cohesive unit: new TrainingRound/TrainingSession scripts and metas; combat/input/rifle/target/HUD integration; saved scene generation and scene; new rule/UI tests and updated runtime tests; README/this record. Rollback removes the new files and restores only those modified tracked files to the pre-feature prototype, without touching pinned packages or unrelated work. No release or APK upload was created.

Sound, attacking/dead enemies, ADS, ammunition, reload, multiplayer and campaign missions remain out of scope.
