# First-delivery verification

The corrected prototype builds, installs and renders on Android 16. Automated editor checks passed, and the user reported that the requested physical-phone interactions passed. This record separates those evidence sources and does not claim performance measurements.

## Final checks

| Evidence source | Check | Result |
|---|---|---|
| Automated EditMode | Combat rules, bilingual labels, reticle and UI shader dependencies | 11 passed, 0 failed, 0 skipped |
| Independent PlayMode | Scene, simulated touch input, movement/boundary, raycast impacts, idle target, HUD reset/language | 5 passed, 0 failed, 0 skipped |
| Independent build validator | Required UI shader and saved scene materials | Passed |
| Android build | Unity 6000.0.70f1, ARM64 IL2CPP, OpenGL ES 3 development APK | Build Success; editor exit 0 |
| Actual device observation | Motorola edge 60 fusion, Android 16: corrected APK install/launch, world/HUD/reticle rendering and language tap | Passed; actual Spanish phone capture in `docs/images/android-preview.png` |
| User-reported physical test | Movement while looking, sprint, fire with increasing impact/damage counters and visible marks, reset and language buttons | All requested steps reported passed |

Corrected APK: **29,636,840 bytes**, SHA-256 `51A4740AA6C8B5C817851F7BB9195E270B6F0461FCB29CF369EB6C9D5EFBBB69`.
Saved corrected scene SHA-256: `578D8234BAB875013A905EA2E2A54EAC243A2A03C8778AF893193903FB44C8DC`.

Logs and test XML are retained locally, not published: they can contain machine-specific information. The APK is also excluded from source control.

## What the automated runtime tests establish

- The scene loads its player, CharacterController, camera, target and bilingual HUD without unexpected runtime exceptions.
- Queued InputSystem events exercise separate move, sprint and look fingers through `PrototypeInput.Update`; the player moves and rotates, released controls clear, and the CharacterController respects the west boundary.
- A separate fire touch executes the Rifle raycast, increments target damage and creates visible surface markers.
- The target changes its idle rig position. Sixty-five directly applied hits leave sixty impact renderers after deferred destruction; the HUD reset action clears marks and counters.
- Real HUD button actions change FIRE to DISPARAR and back. Four centered white reticle Images do not intercept touch.

Editor touch events are simulated, not physical-phone input. The mark-cap check does not claim sixty-five player-fired rounds. Automated language/reset checks invoke button actions, not UI finger taps. Physical checks are separately attributed above.

## Regression history

The initial strict-TDD combat/label suite produced **7 failed, 1 passed** before implementation. A reticle regression was reproduced before correction.

The first installed Android APK rendered magenta UGUI elements. The build lacked explicit `UI/Default` inclusion and the saved graphics had no material references. The correction explicitly includes that shader and assigns saved `UiDefault.mat` references to all 21 HUD graphics. Focused shader RED: **2 failed**; full EditMode GREEN: **11 passed**. `UiBuildValidation.Validate` checks these dependencies and also runs before Android builds. Corrected build evidence records compiled UI shader variants, and the actual corrected phone capture shows the magenta issue resolved.

A first sandboxed test launch could not access licensing IPC; it was harness unavailability, not behavioral RED. Approved launches ran the real tests. No machine licensing data is included here.

## Reproduce

Use Unity 6000.0.70f1, substituting PROJECT for your project folder and RESULTS/LOG for ignored local output paths:

```text
Unity.exe -batchmode -nographics -projectPath PROJECT -runTests -testPlatform EditMode -testResults RESULTS -logFile LOG
Unity.exe -batchmode -projectPath PROJECT -runTests -testPlatform PlayMode -testResults RESULTS -logFile LOG
Unity.exe -batchmode -nographics -quit -projectPath PROJECT -executeMethod FieldOps.Editor.UiBuildValidation.Validate -logFile LOG
Unity.exe -batchmode -nographics -quit -projectPath PROJECT -buildTarget Android -executeMethod FieldOps.Editor.PrototypeBuilder.BuildAndroid -logFile LOG
```

Do not add `-quit` to test-runner commands: the runner owns completion. Install through Unity's Android **Build And Run**, or install the generated development APK on an authorized USB-debugging phone.

In hidden batch testing, test-only focus/input settings and Canvas-aware coordinates are necessary. Idle motion is sampled across a time window to avoid a false failure at a sine turning point. Editor preview captures are not Android evidence; the image in this repository is an actual phone capture.

## Limits and next checks

No FPS benchmark, thermal measurement, long-session endurance, broad device compatibility or Google Play readiness is claimed. Further device checks should cover pause/resume, focus loss, safe-area behavior and performance under prolonged use. Sound, enemy combat/death, ADS, ammunition, reload and multiplayer remain outside this first delivery.

## Work-unit and rollback boundary

This first source publication is one standalone functional prototype: scene, gameplay, tests, editor tooling, pinned dependencies and documentation travel together. Removing that project is the rollback boundary; no unrelated project is modified. Scene regeneration replaces generated assets, so keep manual scene variants separately.

Repository preparation changed passive documentation, a screenshot and Git metadata only. Gameplay, editor scripts, tests, materials, scene and Unity settings were SHA-256 compared before and after preparation; existing runtime evidence therefore remains applicable without another build.
