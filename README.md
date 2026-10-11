# Field Ops Prototype

A functional, small Android first-person shooter prototype for an academic progress demonstration, built with **Unity 6000.0.70f1**. Walk, look, sprint and fire at an animated training target in a simplified semi-open military field. Choose a training round from **1 to 300 seconds**, earn one point per target hit, review the final score and play again. The interface supports English and Spanish.

![Previous training-range Android phone preview with Spanish controls](docs/images/android-preview.png)

The phone screenshot above is from the previous free-practice delivery, before the timed-round menus. It is not evidence of the new round flow on Android.

This is an original training-range prototype, not an affiliated Call of Duty product. No Call of Duty assets, logos or copied game content are included.

## Open and play

1. Download or clone this repository into a local folder.
2. In Unity Hub, choose **Add > Add project from disk**, select that folder, and open it with **6000.0.70f1**. Install that editor's **Android Build Support**, including SDK, NDK and OpenJDK, through Hub if needed.
3. Wait for the first import, then open `Assets/FieldOps/Scenes/TrainingRange.unity`.
4. Press **Play**. Enter whole seconds (**1–300**, default **60**) and select **START ROUND / INICIAR PARTIDA**. In the active round, click the Game view to capture the mouse; press Escape to release it for the language button.
5. At zero, firing and scoring stop. Read your score, then select **PLAY AGAIN / VOLVER A JUGAR** to replay the same duration, or **CHANGE DURATION / CAMBIAR DURACIÓN**.

The saved scene is editable: expand the field, target, player and HUD in the Hierarchy. Keep the editor version and pinned packages unchanged for this first demonstration.

## Controls

| Action | Android | Editor |
|---|---|---|
| Move | Left joystick | WASD |
| Look | Swipe right side | Captured mouse |
| Sprint | Hold SPRINT / ESPRINTAR while moving | Hold left Shift |
| Fire | Hold FIRE / DISPARAR | Hold left mouse button |
| Switch language | ESPAÑOL / ENGLISH button | Escape, then click the button |
| Start / repeat a round | Enter seconds and tap the menu buttons | Enter seconds and click the menu buttons |

The visible countdown uses **MM:SS**. Empty, fractional, nonnumeric and out-of-range durations are rejected with a bilingual message, rather than silently clamped or shortened.

The target breathes and sways in place. Each hit adds 25 cumulative damage and a visible mark; it deliberately never dies. Damage is a practice counter, not a health bar. At most 60 marks are retained. English is the initial language, and the chosen language is stored locally. Separate fingers can operate movement, look, fire and sprint. Each successful target hit earns **1 point**; misses earn none. Starting/replaying clears points, damage, impact marks and firing cooldown. There is no mid-round reset that can bypass the timer.

Configuration/results menus block movement, looking, firing and mouse capture. Held controls must be released and pressed again across a round transition. Losing app focus or going into the background pauses active-play time and clears held controls; resuming does not spend the background interval.

## Build for Android

1. Unlock the phone, connect USB, enable USB debugging and accept the computer authorization prompt if requested.
2. In Unity, select **Field Ops > Build Android APK**. It creates a development APK at `Builds/FieldOpsPrototype.apk`: ARM64, IL2CPP, OpenGL ES 3 and landscape orientation.
3. Install that APK, or use Unity's Android build profile with this scene and **Build And Run** after selecting the phone.
4. On the phone, check numeric keyboard entry, invalid durations, a short round ending at zero, replay/change-duration, language switching, move/look/sprint/fire together, and background/resume while holding controls.

The first import and IL2CPP build can take several minutes. A compiled APK exists locally for the first demonstration; no GitHub release or APK download is provided by this source repository yet. Google Play publication and release signing are not included. Never commit signing keys or credentials.

## What is included

- First-person rifle, walking, looking, sprinting and touchscreen controls.
- Raycast firing, target damage counters and capped persistent impact marks.
- Animated idle target and original simplified training-field geometry.
- Configurable 1–300-second training rounds, countdown, hit score, results and replay.
- English and Spanish buttons and messages.

**Out of scope for this delivery:** sound, attacking enemies, death, aim-down-sights, ammunition, reload, multiplayer, campaign missions and production-quality animation.

## Verification

Current timed-round source checks passed: **39/39 EditMode** and **9/9 PlayMode** tests. These include duration boundaries, countdown/score rules, queued touch interactions with the actual menu/InputField, expiry/replay, held-input suppression and pause/focus behavior. Configuration, active-play and results views were rendered and visually checked in the editor.

The previously built free-practice APK was physically checked on a Motorola edge 60 fusion running Android 16. **That older APK and its phone screenshot do not contain or prove this new timed-round feature.** The new flow still requires its own Android build/device verification; editor automation sets text programmatically after touch focus, not through a physical Android keyboard.

No FPS, thermal or long-session endurance benchmark is claimed. See [VERIFICATION.md](VERIFICATION.md) for evidence and reproduction commands.

## Project layout and editing

| Path | Purpose |
|---|---|
| `Assets/FieldOps/Scenes` | Editable training scene |
| `Assets/FieldOps/Scripts` | Movement, input, rifle, target, combat rules and HUD |
| `Assets/FieldOps/Editor` | Scene generation, Android build and UI build validation |
| `Assets/FieldOps/Tests` | EditMode and PlayMode tests |
| `Packages` | Pinned manifest and dependency lockfile |
| `ProjectSettings` | Portable Unity project configuration |
| `docs/images` | Actual Android demonstration capture |

**Field Ops > Create or Rebuild Training Range** replaces the generated scene and materials. Save custom scene variants under another name first. Run tests through **Window > General > Test Runner** in the EditMode and PlayMode tabs. The project uses Unity's built-in render pipeline.

Source includes Unity `.meta` files to preserve asset references. `.gitignore` excludes caches, local settings, logs, builds, IDE files and credentials. Do not upload those files when extending the project.

## Licensing

Original project code and original generated art use the included [MIT license](LICENSE). Preserve its copyright and license notice when redistributing source; no in-game attribution screen is required by that license.

Unity engine/editor and Unity packages have their own terms and licenses. This project does not replace those terms or promise legal clearance for future resources. Check both use and source-redistribution permissions before adding assets to a public repository.
