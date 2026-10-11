# Timed-training verification

The local source now provides configurable **1–300-second** training rounds, a visible countdown, one point per successful target hit, frozen final results, replay and duration changes. The original Android free-practice evidence is historical and does **not** establish that the new flow works on a physical phone.

## Current source checks

| Evidence | Result | Scope |
|---|---|---|
| Strict behavioral RED: `Logs/training-round-red.xml` | 11 failed, 8 passed | Compilable rule scaffold; valid duration/start/countdown/score/pause/replay contracts failed before implementation |
| Strict UI RED: `Logs/training-ui-red.xml` | 7 failed, 0 passed | Missing saved numeric-duration menu/actions and missing bilingual round labels |
| Final EditMode: `Logs/training-editmode-final.xml` | **39 passed, 0 failed, 0 skipped** | Existing combat/reticle/shader regressions plus duration parsing, boundaries, expiry, replay, suspension and saved UI |
| Final PlayMode: `Logs/training-playmode-final.xml` | **9 passed, 0 failed, 0 skipped** | Actual saved scene, queued touch input/UI, movement/collision, shooting, marks/idle, round flow and bilingual HUD |
| Editor render previews | Visually checked | Configuration, active countdown and results views; ignored local `Logs/training-*-preview.bmp` |
| Updated Android build/device verification | Pending at source handoff | Must use a newly built APK, not the previous free-practice APK |

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

## Physical-device acceptance checklist for the new APK

- [ ] Numeric keyboard opens; entering 1 and 300 starts valid rounds. 0/301/empty and invalid pasted text show a clear message.
- [ ] A short round counts down to 00:00 and shows frozen final score; further touches cannot score.
- [ ] Repeat uses the selected duration and starts with zero counters/marks; change duration returns to configuration.
- [ ] Configuration/results touches and typing do not move/look/fire; active multi-touch still works.
- [ ] Background/lock/focus loss while holding controls pauses active time, clears controls and resumes fairly.
- [ ] English/Spanish labels, safe-area layout and native keyboard behavior are usable on the Motorola phone.

No updated device result, FPS/thermal benchmark, prolonged endurance, broad Android compatibility or production/release signing is claimed here.

## Historical first-delivery evidence (before timed rounds)

The earlier free-practice prototype passed **11 EditMode / 5 PlayMode** tests and built/installed/launched on a Motorola edge 60 fusion running Android 16. The user reported physical move/look/sprint/fire/marks/reset/language checks passing. The public screenshot is from that earlier delivery.

Historical APK: 29,636,840 bytes; SHA-256 `51A4740AA6C8B5C817851F7BB9195E270B6F0461FCB29CF369EB6C9D5EFBBB69`. Historical scene SHA-256 `578D8234BAB875013A905EA2E2A54EAC243A2A03C8778AF893193903FB44C8DC`. These hashes are not the updated feature artifacts.

The original strict combat RED had 7 failed/1 passed. The first Android APK showed magenta UGUI; explicitly including UI/Default and saving UiDefault.mat on the 21 original graphics fixed it. Shader RED was 2 failed; actual corrected phone captures confirmed the visual fix. The new menu graphics preserve that dependency and the validator counts them dynamically.

An initial sandbox-only license IPC failure was harness unavailability, not behavioral RED or proof of missing activation. Approved launches ran actual tests. No licensing data is published.

## Work-unit and rollback boundary

This uncommitted feature is one cohesive unit: new TrainingRound/TrainingSession scripts and metas; combat/input/rifle/target/HUD integration; saved scene generation and scene; new rule/UI tests and updated runtime tests; README/this record. Rollback removes the new files and restores only those modified tracked files to the pre-feature prototype, without touching pinned packages or unrelated work. No commit, staging, push, release or GitHub mutation was performed for this implementation.

Sound, attacking/dead enemies, ADS, ammunition, reload, multiplayer and campaign missions remain out of scope.
