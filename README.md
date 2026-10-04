# Field Ops Prototype

A functional, small Android first-person shooter prototype for an academic progress demonstration, built with **Unity 6000.0.70f1**. Walk, look, sprint and fire at an animated training target in a simplified semi-open military field. The interface supports English and Spanish.

![Actual Android phone preview with Spanish controls](docs/images/android-preview.png)

This is an original training-range prototype, not an affiliated Call of Duty product. No Call of Duty assets, logos or copied game content are included.

## Open and play

1. Download or clone this repository into a local folder.
2. In Unity Hub, choose **Add > Add project from disk**, select that folder, and open it with **6000.0.70f1**. Install that editor's **Android Build Support**, including SDK, NDK and OpenJDK, through Hub if needed.
3. Wait for the first import, then open `Assets/FieldOps/Scenes/TrainingRange.unity`.
4. Press **Play**. Click the Game view to capture the mouse; press Escape to release it for the language and reset buttons.

The saved scene is editable: expand the field, target, player and HUD in the Hierarchy. Keep the editor version and pinned packages unchanged for this first demonstration.

## Controls

| Action | Android | Editor |
|---|---|---|
| Move | Left joystick | WASD |
| Look | Swipe right side | Captured mouse |
| Sprint | Hold SPRINT / ESPRINTAR while moving | Hold left Shift |
| Fire | Hold FIRE / DISPARAR | Hold left mouse button |
| Switch language | ESPAÑOL / ENGLISH button | Escape, then click the button |
| Clear impacts and counters | RESET / REINICIAR button | Escape, then click the button |

The target breathes and sways in place. Each hit adds 25 cumulative damage and a visible mark; it deliberately never dies. Damage is a practice counter, not a health bar. At most 60 marks are retained. English is the initial language, and the chosen language is stored locally. Separate fingers can operate movement, look, fire and sprint.

## Build for Android

1. Unlock the phone, connect USB, enable USB debugging and accept the computer authorization prompt if requested.
2. In Unity, select **Field Ops > Build Android APK**. It creates a development APK at `Builds/FieldOpsPrototype.apk`: ARM64, IL2CPP, OpenGL ES 3 and landscape orientation.
3. Install that APK, or use Unity's Android build profile with this scene and **Build And Run** after selecting the phone.
4. On the phone, check movement and looking together, sprint, firing and impact marks, reset and language switching.

The first import and IL2CPP build can take several minutes. A compiled APK exists locally for the first demonstration; no GitHub release or APK download is provided by this source repository yet. Google Play publication and release signing are not included. Never commit signing keys or credentials.

## What is included

- First-person rifle, walking, looking, sprinting and touchscreen controls.
- Raycast firing, target damage counters and capped persistent impact marks.
- Animated idle target and original simplified training-field geometry.
- English and Spanish buttons and messages.

**Out of scope for this delivery:** sound, attacking enemies, death, aim-down-sights, ammunition, reload, multiplayer, missions and production-quality animation.

## Verification

Automated checks passed: **11/11 EditMode** and **5/5 PlayMode** tests. The corrected APK was installed and launched on a Motorola edge 60 fusion running Android 16; actual phone captures showed the scene and healthy bilingual HUD. The user subsequently reported that physical movement/look, sprint, firing counters/marks, reset and language checks all passed.

These are distinct evidence sources, not a measured performance benchmark. FPS, heat and long-session pause/resume endurance have not been measured. See [VERIFICATION.md](VERIFICATION.md) for scope, regression evidence and reproduction commands.

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
