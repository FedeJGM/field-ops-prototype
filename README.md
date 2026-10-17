# Field Ops Prototype

**[English](#english) · [Español](#español)**

![Aiming through the assault rifle iron sights on the phone: soldier target with amber impact marks, score, damage, timer, ammo and FPS counter, Spanish interface / Apuntando por las miras de hierro del fusil de asalto en el teléfono: blanco soldado con marcas de impacto ámbar, puntaje, daño, tiempo, munición y contador de FPS, interfaz en español](docs/images/android-gameplay.jpg)

*EN: screenshot taken on the Motorola edge 60 fusion (development build, hence the "Development Build" label) while aiming through the assault rifle iron sights: 6 points, 150 damage, amber marks on the soldier, 60 FPS and the player's own button layout (AIM moved to the top-left).*
*ES: captura tomada en el Motorola edge 60 fusion (compilación de desarrollo, de ahí la etiqueta "Development Build") apuntando por las miras de hierro del fusil de asalto: 6 puntos, 150 de daño, marcas ámbar en el soldado, 60 FPS y la disposición de botones propia del jugador (APUNTAR movido arriba a la izquierda).*

---

## English

A functional, small Android first-person shooter prototype for an academic demonstration, built with **Unity 6000.0.70f1**. Walk, look, sprint, jump, slide, aim and fire one of five weapons at an animated soldier target in a simplified military training base. Choose a training round from **1 to 300 seconds**, earn one point per target hit, review the final score and play again. The interface supports English and Spanish.

This is an original training-range prototype, not an affiliated Call of Duty product. No Call of Duty assets, logos or copied game content are included.

### Open and play

1. Download or clone this repository into a local folder.
2. In Unity Hub, choose **Add > Add project from disk**, select that folder, and open it with **6000.0.70f1**. Install that editor's **Android Build Support**, including SDK, NDK and OpenJDK, through Hub if needed.
3. Wait for the first import, then open `Assets/FieldOps/Scenes/TrainingRange.unity`.
4. Press **Play**. Enter whole seconds (**1–300**, default **60**) and select **START ROUND / INICIAR PARTIDA**. In the active round, click the Game view to capture the mouse; press Escape to release it for the language and settings buttons.
5. At zero, firing and scoring stop. Read your score, then select **PLAY AGAIN / VOLVER A JUGAR** to replay the same duration, or **CHANGE DURATION / CAMBIAR DURACIÓN**.

The saved scene is editable: expand the field, target, player and HUD in the Hierarchy. Keep the editor version and pinned packages unchanged.

### Controls

| Action | Android | Editor |
|---|---|---|
| Move | Left joystick | WASD |
| Look | Swipe right side | Captured mouse |
| Sprint | Push the joystick to its edge, straight forward (default), or hold SPRINT / ESPRINTAR when the sprint mode is set to button | Hold left Shift |
| Jump | Tap or hold JUMP / SALTAR (holding chains hops) | Space |
| Slide | Tap SLIDE / DESLIZAR while running at sprint speed; jumping cancels the slide | Left Ctrl or C |
| Fire | Hold FIRE / DISPARAR | Hold left mouse button |
| Reload | Tap RELOAD / RECARGAR (above the fire button) | R key |
| Aim down sights | Hold or tap AIM / APUNTAR (above sprint); mode set in settings | Right mouse button |
| Settings | SETTINGS / AJUSTES button, bottom-right | Escape, then click the button |
| Move the touch buttons, the language button and the FPS counter | SETTINGS > EDIT LAYOUT / EDITAR BOTONES, then drag and SAVE / GUARDAR | Same, dragging with the mouse |
| Choose a weapon | `<` / `>` in the pre-round menu, on the results screen or in SETTINGS (swapping mid-round restarts the score) | Same |
| Switch language | ESPAÑOL / ENGLISH button | Escape, then click the button |
| Start / repeat a round | Enter seconds and tap the menu buttons | Enter seconds and click the menu buttons |

The visible countdown uses **MM:SS**. Empty, fractional, nonnumeric and out-of-range durations are rejected with a bilingual message, rather than silently clamped or shortened.

The target breathes and sways in place. Each hit adds the weapon's damage (for example 25 for the assault rifle) to a cumulative counter and leaves a visible mark; it deliberately never dies. Damage is a practice counter, not a health bar. At most 60 marks are retained. English is the initial language, and the chosen language is stored locally. Separate fingers can operate movement, look, fire and sprint. Each successful target hit earns **1 point**; misses earn none. Starting or replaying a round clears points, damage, impact marks, ammunition and firing cooldown. There is no mid-round reset that can bypass the timer.

Configuration and results menus block movement, looking, firing and mouse capture. Held controls must be released and pressed again across a round transition. Losing app focus, going into the background or opening the settings menu pauses active-play time and clears held controls; resuming does not spend the paused interval.

### Build for Android

1. Unlock the phone, connect USB, enable USB debugging and accept the computer authorization prompt if requested.
2. In Unity, select **Field Ops > Build Android APK**. It creates a development APK at `Builds/FieldOpsPrototype.apk`: ARM64, IL2CPP, OpenGL ES 3 and landscape orientation.
3. Install that APK, or use Unity's Android build profile with this scene and **Build And Run** after selecting the phone.
4. On the phone, check numeric keyboard entry, invalid durations, a short round ending at zero, replay and change-duration, language switching, move/look/sprint/fire together, reload and ammunition, aiming in both modes, the settings menu, weapon selection and per-weapon sound, button layout editing, the FPS counter, shots at every part of the soldier from all sides, and background/resume while holding controls.

The first import and IL2CPP build can take several minutes. A compiled APK exists locally for the demonstration; no GitHub release or APK download is provided by this source repository. Google Play publication and release signing are not included. Never commit signing keys or credentials.

### What is included

- First-person weapons (five, listed below), walking, looking, sprinting and touchscreen controls.
- Raycast firing, target damage counters and capped persistent impact marks.
- Animated soldier target and original simplified training-base geometry.
- Configurable 1–300-second training rounds, countdown, hit score, results and replay.
- Per-weapon magazine with infinite reserve and manual reload (about 1.4 to 2.1 s depending on the weapon, the length of its reload sound; no auto-reload); ammo counter in the HUD. Ammo refills at every round start and reload time pauses with the round.
- Soldier target: the practice target is a low-poly tactical soldier (CC0 model by Quaternius, 3,106 triangles) that breathes with an idle animation and plays a short hit reaction when it is hit, then returns to idle. It is indestructible: it never dies or disappears. Hit zones cover the head, neck, torso, abdomen, hips, arms and legs, and the amber impact marks stay attached to the body while it moves. The model holds no weapon.
- Military training-base map: overcast open-field sky with a distant treeline, cracked asphalt firing lane, dry-dirt ground, shipping containers along both sides, sandbag walls, concrete barriers, a watchtower, an open hangar, oil drums, tires and supply crates. Everything is built from simple shapes with CC0 textures (Poly Haven), kept light for the phone, and the firing lane stays clear. It is an original base inspired by a military training ground, not a copy of any commercial game's map.
- On-screen FPS counter (top center, green from 50 FPS, yellow from 30, red below). It is on by default and can be moved like any button; the settings menu has a switch to hide or show it, and the choice is stored locally. In development builds the game also logs frame-time statistics every 10 seconds for `adb logcat`.
- Five selectable weapons with generic names: pistol, assault rifle, sub-machine gun, shotgun and sniper rifle. Each has its own magazine size, fire rate, damage, range, recoil and aiming zoom (the shotgun fires 8 pellets and scores one point per shot that hits). The choice is stored locally and can be changed in the pre-round menu, on the results screen and in the settings menu; swapping during a round restarts the score and target counters, gives a full magazine and keeps the clock. Models are low-poly CC0 assets (see [CREDITS.md](CREDITS.md)).
- Configurable HUD: from the settings menu, drag the joystick, the fire, reload, aim, sprint, jump and slide buttons, the language button and the FPS counter anywhere on screen. The layout is stored locally and restored on the next launch. Buttons stay inside the screen, saving is refused while elements overlap each other or the settings button (which stays fixed on purpose so a bad layout can never lock the player out), and RESET / CANCEL are available. While editing, the FPS counter is shown even if it is turned off, and the language button is inert so a drag never changes the language. The round is paused while editing.
- Aim down sights (ADS): per-weapon zoom (20 to 50 degrees) with smooth transition, half look sensitivity and real iron sights on the weapon that line up with the screen centre, where the shots land. A settings menu (it pauses the round) chooses hold or toggle mode and whether the cross reticle stays visible while aiming; both choices are stored locally. Sprinting cancels aiming; reloading does not.
- Per-weapon sound effects: every weapon has its own recorded shot and reload sound (processed single shots and reloads from CC BY 3.0 and CC0 recordings, credited in [CREDITS.md](CREDITS.md)), and a reload lasts as long as its sound. The sniper bolt and the shotgun pump sounds are short cuts of those same recordings. The empty-magazine click and the target-hit sound are generated by code. Sounds stop when a round is paused or ends, swapping weapons cuts the previous sound, and menus are silent.
- Movement: jump (about 1 m, with a short buffer and coyote time so a slightly early or late tap still jumps), slide (starts above sprint speed, lowers the view and body, fades out in about a second, cannot stand up under low cover) and slide-cancel by jumping, which keeps the slide speed. Landing and jumping again within a short window keeps momentum and adds a small, capped speed gain, so holding JUMP while sprinting chains hops (a bunny-hop-style chain, inspired by the genre; the real values of any commercial game are not public and nothing is copied). Jumping and sliding do not stop shooting, aiming or reloading.
- Sprint mode: pushing the joystick to its edge, mostly forward (within 40 degrees; a looser release threshold of 50 degrees avoids flicker), sprints, while sideways, diagonal or backward pushes only walk, so a quick strafe to either side never breaks the aim (pushing straight forward to the edge while aiming still sprints and leaves the aim). The player can also switch to the on-screen SPRINT button in the settings; the choice is stored locally and joystick mode is the default. In button mode, sliding needs SPRINT held with a third finger, which is why joystick mode is the default.
- Sprint sensitivity: a settings button cycles HIGH, MEDIUM and LOW, which sets how far the joystick must be pushed to sprint (80 %, 92 % and 97 % of its travel; MEDIUM is the default). It only applies to joystick mode.
- Sprint carry: while sprinting forward the weapon rises with the muzzle up and turned toward the middle (in about 0.2 s, back down in 0.12 s) with a stronger bob, in the spirit of a tactical sprint. Firing while sprinting drops the weapon to the firing pose and shoots, and aiming or sliding keep the normal pose.
- Iron sights: the pistol has a notch with three white dots, the assault rifle a front post with guards and an orange tip, the sub-machine gun a ring with a cyan accent and a front post, and the shotgun a gold bead; the assault rifle and the shotgun have no rear frame. They are simple original shapes built in code on the real surface of each model (not a third-party asset), and a test checks that no part floats, that the sight line passes through the screen centre when aiming and that no two parts share a face (which would flicker). The long guns sit further forward at the hip so their sights and the scope are in view.
- Sniper scope: the sniper rifle model carries its own scope. When aiming, the screen closes into a circular scope view with a thin reticle, the rifle is hidden, the zoom is 20 degrees and turning is slower. The circular mask is an image generated by the project itself.
- Hand-worked action: after each shot the sniper rifle (bolt) and the shotgun (pump) move during the fire cooldown, so the weapon is never still while waiting: it turns out, is pulled back and returns exactly when the next shot is possible, with a short bolt or pump sound. The game has no modelled hands, so what moves is the weapon. With the scope on, the player leaves the scope for the cycle and returns to it by itself if aiming is still held.
- Title tab: the FIELD OPS / PROTOTYPE header shows for the first 5 seconds of each round (paused time does not count) and also appears in the settings menu.
- English and Spanish buttons and messages.

**Out of scope for this delivery:** music, footsteps and menu sounds, attacking enemies, death, multiplayer, campaign missions and production-quality animation.

### Verification

Current source checks passed: **298/298 EditMode** and **101/101 PlayMode** tests. These include soldier-target, scenery, sky and FPS-counter checks, weapon audio, stats, selection and model-orientation checks, HUD layout rules and the layout editor, sound synthesis and playback rules, aim and settings rules, magazine and reload rules, duration boundaries, countdown and score rules, queued touch interactions with the actual menu and input field, expiry and replay, held-input suppression and pause/focus behavior. Configuration, active-play, aiming and settings views were rendered and visually checked in the editor.

The timed-round, ammo/reload, ADS/settings, sound, configurable-HUD, five-weapon, per-weapon-audio, military-base-map, FPS-counter and soldier-target builds were installed and manually tested on a Motorola edge 60 fusion running Android 16; the user reported that keyboard entry, validation, countdown, results, replay, language, background/resume, reload, aiming, settings, sound, layout-editor, weapon-selection, weapon-model, weapon-sound, map, FPS-counter and soldier-target checks all passed. A later fix to the soldier's hit zones and to the player's collider (which blocked downward shots) was confirmed on the phone through a development-build shot log: 61 of 64 shots at the legs hit the soldier and none was blocked by the player. Jump, slide, sprint-mode, title-tab and movable language/FPS builds were also tested on the phone (user-reported: all checks passed); the development-build movement log showed 18 jumps with up to 4 chained hops, a top speed of 6.4 m/s and slides starting at 6.9 m/s. Sprint sensitivity, the sprint weapon pose and forward-only sprint were also tested on the phone (user-reported: all checks passed). The iron sights, the sniper scope view and the bolt and pump cycle were tested on the phone as well; the first phone test found the assault rifle shooting lower than where it aimed (a placement bug of mine, fixed and verified by a test) and later a flickering orange tip (fixed). The two cycle sounds could not be listened to by the assistant; the user reported that the phone checks passed.

On the phone, a 4-minute development-build session with the soldier averaged 59.9 FPS (the display cap); no thermal, battery or long-session endurance benchmark is claimed. See [VERIFICATION.md](VERIFICATION.md) (in English) for evidence and reproduction commands.

### Project layout and editing

| Path | Purpose |
|---|---|
| `Assets/FieldOps/Scenes` | Editable training scene |
| `Assets/FieldOps/Scripts` | Movement (jump, slide, sprint), input, weapons, ammunition, aiming, scope view, weapon action cycle, sound, target, round rules, settings and HUD |
| `Assets/FieldOps/Weapons` | Generated weapon definitions (stats and model per weapon) |
| `Assets/FieldOps/ThirdParty` | Third-party assets with their notices (see [CREDITS.md](CREDITS.md)) |
| `Assets/FieldOps/Audio` | Processed weapon sound effects with their notice (see [CREDITS.md](CREDITS.md)) |
| `Assets/FieldOps/Animation` | Generated animator controller for the soldier target |
| `Assets/FieldOps/Materials` | Generated materials (environment, weapon sights, UI) |
| `Assets/FieldOps/Textures` | Generated scope mask image (original, no third-party asset) |
| `Assets/FieldOps/Editor` | Scene generation (map, soldier, weapon iron sights), Android build and UI build validation |
| `Assets/FieldOps/Tests` | EditMode and PlayMode tests |
| `Packages` | Pinned manifest and dependency lockfile |
| `ProjectSettings` | Portable Unity project configuration |
| `docs/images` | Android gameplay capture |

**Field Ops > Create or Rebuild Training Range** replaces the generated scene and materials. Save custom scene variants under another name first. Run tests through **Window > General > Test Runner** in the EditMode and PlayMode tabs. The project uses Unity's built-in render pipeline.

Source includes Unity `.meta` files to preserve asset references. `.gitignore` excludes caches, local settings, logs, builds, IDE files and credentials. Do not upload those files when extending the project.

### Licensing

Original project code and original generated art use the included [MIT license](LICENSE). Preserve its copyright and license notice when redistributing source; no in-game attribution screen is required by that license. The weapon models are third-party CC0 assets listed in [CREDITS.md](CREDITS.md); weapon names in the game are generic and no Call of Duty content is used. The weapon shot sounds are CC BY 3.0 (credit: Vincent Sevedge) and the reload sounds are CC0; credits and the changes made are in [CREDITS.md](CREDITS.md) and the notice next to the audio files. The environment textures and the sky are CC0 (Poly Haven), and the soldier model is CC0 (Quaternius); both are credited there.

Unity engine/editor and Unity packages have their own terms and licenses. This project does not replace those terms or promise legal clearance for future resources. Check both use and source-redistribution permissions before adding assets to a public repository.

[Back to top](#field-ops-prototype)

---

## Español

Prototipo funcional y pequeño de un juego de disparos en primera persona para Android, hecho para una demostración académica con **Unity 6000.0.70f1**. Camina, mira, corre, salta, desliza, apunta y dispara una de cinco armas a un blanco soldado animado en una base de entrenamiento militar simplificada. Elige una partida de entrenamiento de **1 a 300 segundos**, gana un punto por cada impacto en el blanco, revisa tu puntaje final y vuelve a jugar. La interfaz está en inglés y en español.

Es un prototipo original de campo de entrenamiento y no es un producto afiliado a Call of Duty. No incluye recursos, logotipos ni contenido copiado de Call of Duty.

### Abrir y jugar

1. Descarga o clona este repositorio en una carpeta local.
2. En Unity Hub, elige **Add > Add project from disk**, selecciona esa carpeta y ábrela con **6000.0.70f1**. Si hace falta, instala desde Hub el **Android Build Support** de ese editor, con SDK, NDK y OpenJDK.
3. Espera la primera importación y abre `Assets/FieldOps/Scenes/TrainingRange.unity`.
4. Pulsa **Play**. Escribe los segundos en números enteros (**1–300**, por defecto **60**) y elige **START ROUND / INICIAR PARTIDA**. Durante la partida, haz clic en la vista Game para capturar el ratón; pulsa Escape para liberarlo y usar los botones de idioma y ajustes.
5. Al llegar a cero, se detienen los disparos y el puntaje. Lee tu puntaje y elige **PLAY AGAIN / VOLVER A JUGAR** para repetir la misma duración, o **CHANGE DURATION / CAMBIAR DURACIÓN**.

La escena guardada es editable: expande el campo, el blanco, el jugador y la interfaz en la Hierarchy. Mantén sin cambios la versión del editor y los paquetes fijados.

### Controles

| Acción | Android | Editor |
|---|---|---|
| Moverse | Joystick izquierdo | WASD |
| Mirar | Deslizar el lado derecho | Ratón capturado |
| Correr | Empujar el joystick hasta el borde, recto hacia adelante (por defecto), o mantener SPRINT / ESPRINTAR cuando el modo de carrera está en botón | Mantener Shift izquierdo |
| Saltar | Tocar o mantener JUMP / SALTAR (mantener encadena saltos) | Barra espaciadora |
| Deslizar | Tocar SLIDE / DESLIZAR mientras corres a velocidad de carrera; saltar cancela el deslizamiento | Ctrl izquierdo o C |
| Disparar | Mantener FIRE / DISPARAR | Mantener el botón izquierdo del ratón |
| Recargar | Tocar RELOAD / RECARGAR (encima del botón de disparo) | Tecla R |
| Apuntar (ADS) | Mantener o tocar AIM / APUNTAR (encima de correr); el modo se elige en ajustes | Botón derecho del ratón |
| Ajustes | Botón SETTINGS / AJUSTES, abajo a la derecha | Escape y luego clic en el botón |
| Mover los botones táctiles, el botón de idioma y el contador de FPS | SETTINGS / AJUSTES > EDIT LAYOUT / EDITAR BOTONES, arrastrar y GUARDAR | Igual, arrastrando con el ratón |
| Elegir arma | `<` / `>` en el menú previo, en la pantalla de resultados o en AJUSTES (cambiar a mitad de partida reinicia el puntaje) | Igual |
| Cambiar idioma | Botón ESPAÑOL / ENGLISH | Escape y luego clic en el botón |
| Iniciar o repetir una partida | Escribir los segundos y tocar los botones del menú | Escribir los segundos y hacer clic en los botones del menú |

La cuenta regresiva visible usa el formato **MM:SS**. Las duraciones vacías, con decimales, no numéricas o fuera de rango se rechazan con un mensaje bilingüe, en lugar de ajustarse o recortarse en silencio.

El blanco respira y se balancea en su sitio. Cada impacto suma el daño del arma (por ejemplo 25 con el fusil de asalto) a un contador acumulado y deja una marca visible; a propósito, nunca muere. El daño es un contador de práctica, no una barra de vida. Se conservan como máximo 60 marcas. El idioma inicial es el inglés y el idioma elegido se guarda localmente. Distintos dedos pueden manejar a la vez el movimiento, la mirada, el disparo y la carrera. Cada impacto exitoso en el blanco da **1 punto**; los disparos fallidos no dan nada. Al iniciar o repetir una partida se reinician los puntos, el daño, las marcas de impacto, la munición y el tiempo de espera entre disparos. No hay un reinicio a mitad de partida que permita saltarse el cronómetro.

Los menús de configuración y de resultados bloquean el movimiento, la mirada, el disparo y la captura del ratón. Los controles mantenidos deben soltarse y volver a pulsarse al cambiar de fase. Perder el foco de la aplicación, enviarla a segundo plano o abrir el menú de ajustes pausa el tiempo de juego activo y suelta los controles mantenidos; al volver, no se gasta el tiempo en pausa.

### Compilar para Android

1. Desbloquea el teléfono, conéctalo por USB, activa la depuración USB y acepta la autorización del computador si se solicita.
2. En Unity, elige **Field Ops > Build Android APK**. Se crea un APK de desarrollo en `Builds/FieldOpsPrototype.apk`: ARM64, IL2CPP, OpenGL ES 3 y orientación horizontal.
3. Instala ese APK, o usa el perfil de compilación de Android de Unity con esta escena y **Build And Run** tras seleccionar el teléfono.
4. En el teléfono, comprueba el teclado numérico, las duraciones inválidas, una partida corta que termina en cero, repetir y cambiar la duración, el cambio de idioma, mover, mirar, correr y disparar a la vez, la recarga y la munición, apuntar en los dos modos, el menú de ajustes, la selección de arma y el sonido por arma, la edición de la posición de los botones, el contador de FPS, disparos a todas las partes del soldado desde todos los lados, y pasar a segundo plano y volver mientras se mantienen controles.

La primera importación y la compilación con IL2CPP pueden tardar varios minutos. Existe un APK compilado de forma local para la demostración; este repositorio de código fuente no ofrece una publicación (release) de GitHub ni una descarga del APK. No se incluyen la publicación en Google Play ni la firma de producción. Nunca subas claves de firma ni credenciales al repositorio.

### Qué incluye

- Armas en primera persona (cinco, listadas más abajo), caminar, mirar, correr y controles táctiles.
- Disparo por raycast, contadores de daño del blanco y marcas de impacto persistentes con límite.
- Blanco soldado animado y geometría original y simplificada de la base de entrenamiento.
- Partidas de entrenamiento configurables de 1 a 300 segundos, con cuenta regresiva, puntaje por impactos, resultados y repetición.
- Cargador por arma con reserva infinita y recarga manual (de 1,4 a 2,1 s según el arma, lo que dura su sonido de recarga; sin recarga automática); contador de munición en pantalla. La munición se restablece al iniciar cada partida y el tiempo de recarga se pausa junto con la partida.
- Blanco soldado: el blanco de práctica es un soldado táctico de pocos polígonos (modelo CC0 de Quaternius, 3.106 triángulos) que respira con una animación de reposo y hace una breve reacción cuando recibe un impacto, y luego vuelve a reposo. Es indestructible: nunca muere ni desaparece. Las zonas de impacto cubren cabeza, cuello, torso, abdomen, cadera, brazos y piernas, y las marcas de impacto ámbar se quedan pegadas al cuerpo mientras se mueve. El modelo no sostiene ningún arma.
- Mapa de base militar de entrenamiento: cielo nublado de campo abierto con una línea de árboles al fondo, carril de tiro de asfalto agrietado, suelo de tierra seca, contenedores a ambos lados, muros de sacos de arena, barreras de concreto, una torre de vigilancia, un hangar abierto, barriles, neumáticos y cajas de suministros. Todo está hecho con formas simples y texturas CC0 (Poly Haven), liviano para el teléfono, y el carril de tiro queda despejado. Es una base original inspirada en un campo de entrenamiento militar, no una copia del mapa de ningún juego comercial.
- Contador de FPS en pantalla (arriba al centro; verde desde 50 FPS, amarillo desde 30 y rojo por debajo). Viene activo por defecto y se puede mover como cualquier botón; el menú de ajustes tiene un interruptor para ocultarlo o mostrarlo y la elección se guarda localmente. En las compilaciones de desarrollo el juego también registra estadísticas de tiempo de cuadro cada 10 segundos para `adb logcat`.
- Cinco armas seleccionables con nombres genéricos: pistola, fusil de asalto, subfusil, escopeta y francotirador. Cada una tiene su propio tamaño de cargador, cadencia, daño, alcance, retroceso y zoom al apuntar (la escopeta dispara 8 perdigones y da un punto por disparo que acierta). La elección se guarda localmente y se puede cambiar en el menú previo, en la pantalla de resultados y en el menú de ajustes; cambiar a mitad de partida reinicia el puntaje y los contadores del blanco, da un cargador lleno y conserva el cronómetro. Los modelos son recursos CC0 de pocos polígonos (ver [CREDITS.md](CREDITS.md)).
- Interfaz configurable: desde el menú de ajustes, arrastra el joystick, los botones de disparar, recargar, apuntar, correr, saltar y deslizar, el botón de idioma y el contador de FPS a cualquier lugar de la pantalla. El diseño se guarda localmente y se restaura al abrir la aplicación. Los botones no salen de la pantalla, no se puede guardar mientras se superpongan entre sí o con el botón de ajustes (que queda fijo a propósito para que un diseño mal armado nunca deje al jugador sin acceso), y hay opciones para restablecer y cancelar. Al editar, el contador de FPS se muestra aunque esté apagado y el botón de idioma queda inerte para que arrastrarlo nunca cambie el idioma. La partida se pausa mientras editas.
- Apuntar con mira (ADS): zoom por arma (de 20 a 50 grados) con transición suave, mitad de sensibilidad al mirar y miras de hierro reales sobre el arma, alineadas con el centro de la pantalla, donde caen los disparos. Un menú de ajustes (que pausa la partida) permite elegir el modo mantener o alternar y si la mira de cruz sigue visible al apuntar; ambas opciones se guardan localmente. Correr cancela el apuntado; recargar no.
- Efectos de sonido por arma: cada arma tiene su propio sonido grabado de disparo y de recarga (disparos y recargas sueltos procesados a partir de grabaciones CC BY 3.0 y CC0, acreditados en [CREDITS.md](CREDITS.md)), y la recarga dura lo mismo que su sonido. Los sonidos del cerrojo del francotirador y del bombeo de la escopeta son recortes cortos de esas mismas grabaciones. El clic de cargador vacío y el sonido de impacto en el blanco se generan por código. Los sonidos se cortan al pausar o terminar una partida, cambiar de arma corta el sonido anterior, y los menús son silenciosos.
- Movimiento: salto (de cerca de 1 m, con un pequeño margen antes y después de tocar el suelo para que un toque algo temprano o tardío aún salte), deslizamiento (empieza por encima de la velocidad de carrera, baja la vista y el cuerpo, se apaga en cerca de un segundo y no deja ponerse de pie bajo una cobertura baja) y cancelar el deslizamiento saltando, que conserva su velocidad. Aterrizar y volver a saltar dentro de una ventana corta conserva el impulso y suma una ganancia pequeña y con tope, de modo que mantener JUMP mientras corres encadena saltos (una cadena al estilo del bunny hop, inspirada en el género; los valores reales de cualquier juego comercial no son públicos y no se copia nada). Saltar y deslizar no impiden disparar, apuntar ni recargar.
- Modo de carrera: empujar el joystick hasta el borde, sobre todo hacia adelante (dentro de 40 grados; un umbral de salida más amplio, de 50 grados, evita parpadeos), hace correr, mientras que los empujes hacia los lados, en diagonal o hacia atrás solo caminan, así que un movimiento lateral rápido a cualquier lado nunca rompe la mira (empujar recto hacia adelante hasta el borde mientras apuntas sí corre y sale de la mira). También se puede cambiar en los ajustes al botón SPRINT en pantalla; la elección se guarda localmente y el modo joystick es el predeterminado. En modo botón, deslizar exige mantener SPRINT con un tercer dedo, por eso el modo joystick es el predeterminado.
- Sensibilidad del sprint: un botón de ajustes alterna ALTA, MEDIA y BAJA, que fija cuánto hay que empujar el joystick para correr (80 %, 92 % y 97 % de su recorrido; MEDIA es la predeterminada). Solo se aplica en modo joystick.
- Arma levantada al correr: mientras corres hacia adelante el arma sube con el cañón hacia arriba y girado hacia el centro (en cerca de 0,2 s, y baja en 0,12 s) con un balanceo más fuerte, al estilo de un sprint táctico. Disparar corriendo baja el arma a la posición de disparo y dispara, y apuntar o deslizar mantienen la pose normal.
- Miras de hierro: la pistola tiene una muesca con tres puntos blancos, el fusil de asalto un punto de mira con protectores y una punta naranja, el subfusil un aro con acento celeste y un punto de mira, y la escopeta una perla dorada; el fusil de asalto y la escopeta no tienen marco trasero. Son formas simples originales construidas en código sobre la superficie real de cada modelo (no un recurso de terceros), y una prueba comprueba que ninguna pieza flote, que la línea de mira pase por el centro de la pantalla al apuntar y que dos piezas no compartan una cara (lo que parpadearía). Las armas largas están más adelantadas de cadera para que se vean sus miras y el scope.
- Scope del francotirador: el modelo del francotirador trae su propio scope. Al apuntar, la pantalla se cierra en una vista circular de scope con una retícula fina, el rifle se oculta, el zoom es de 20 grados y el giro es más lento. La máscara circular es una imagen generada por el propio proyecto.
- Acción manual: después de cada disparo el francotirador (cerrojo) y la escopeta (bombeo) se mueven durante la espera entre disparos, así que el arma nunca queda quieta esperando: se gira, retrocede y vuelve justo cuando se puede disparar de nuevo, con un sonido corto de cerrojo o bombeo. El juego no tiene manos modeladas, así que lo que se mueve es el arma. Con el scope activo, el jugador sale del scope durante el ciclo y vuelve solo si sigue manteniendo apuntar.
- Pestaña de título: el encabezado FIELD OPS / PROTOTYPE se muestra los primeros 5 segundos de cada partida (el tiempo en pausa no cuenta) y también aparece en el menú de ajustes.
- Botones y mensajes en inglés y en español.

**Fuera del alcance de esta entrega:** música, pasos y sonidos de menú, enemigos que atacan, muerte, multijugador, misiones de campaña y animación de calidad de producción.

### Verificación

Las comprobaciones actuales del código fuente pasaron: **298/298 pruebas EditMode** y **101/101 pruebas PlayMode**. Incluyen las comprobaciones del soldado, el escenario, el cielo y el contador de FPS, las de armas (audio, estadísticas, selección y orientación de los modelos), las reglas de diseño de la interfaz y su editor, las reglas de síntesis y reproducción de sonido, de apuntado y ajustes, de cargador y recarga, los límites de duración, la cuenta regresiva y el puntaje, toques en cola sobre el menú y el campo de texto reales, expiración y repetición, supresión de controles mantenidos y comportamiento de pausa y foco. Las vistas de configuración, juego activo, apuntado y ajustes se renderizaron y se revisaron visualmente en el editor.

Las compilaciones de partidas cronometradas, munición y recarga, ADS y ajustes, sonido, interfaz configurable, cinco armas, audio por arma, mapa de base militar, contador de FPS y soldado se instalaron y se probaron manualmente en un Motorola edge 60 fusion con Android 16; el usuario reportó que pasaron todas las comprobaciones de teclado, validación, cuenta regresiva, resultados, repetición, idioma, segundo plano y regreso, recarga, apuntado, ajustes, sonido, editor de diseño, selección de arma, modelos de armas, sonido por arma, mapa, contador de FPS y soldado. Una corrección posterior de las zonas de impacto del soldado y del collider del jugador (que bloqueaba los disparos hacia abajo) se confirmó en el teléfono con un registro de disparos de la compilación de desarrollo: 61 de 64 disparos a las piernas dieron en el soldado y ninguno fue bloqueado por el jugador. Las compilaciones con salto, deslizamiento, modo de carrera, pestaña de título e idioma y FPS movibles también se probaron en el teléfono (reportado por el usuario: todas las comprobaciones pasaron); el registro de movimiento de la compilación de desarrollo mostró 18 saltos con hasta 4 saltos encadenados, una velocidad máxima de 6,4 m/s y deslizamientos que empiezan a 6,9 m/s. La sensibilidad del sprint, el arma levantada al correr y el sprint solo hacia adelante también se probaron en el teléfono (reportado por el usuario: todas las comprobaciones pasaron). Las miras de hierro, la vista de scope del francotirador y el ciclo de cerrojo y bombeo también se probaron en el teléfono; la primera prueba encontró que el fusil de asalto disparaba más abajo de donde apuntaba (un error mío de colocación, corregido y verificado con una prueba) y después una punta naranja que parpadeaba (corregida). El asistente no pudo escuchar los dos sonidos del ciclo; el usuario reportó que las comprobaciones en el teléfono pasaron.

En el teléfono, una sesión de 4 minutos con la compilación de desarrollo y el soldado promedió 59,9 FPS (el tope de la pantalla); no se afirma ninguna medición de temperatura, batería ni resistencia en sesiones largas. Consulta [VERIFICATION.md](VERIFICATION.md) (en inglés) para ver la evidencia y los comandos de reproducción.

### Estructura del proyecto y edición

| Ruta | Propósito |
|---|---|
| `Assets/FieldOps/Scenes` | Escena de entrenamiento editable |
| `Assets/FieldOps/Scripts` | Movimiento (salto, deslizamiento, sprint), entrada, armas, munición, apuntado, vista de scope, ciclo de acción del arma, sonido, blanco, reglas de partida, ajustes e interfaz |
| `Assets/FieldOps/Weapons` | Definiciones de armas generadas (estadísticas y modelo de cada una) |
| `Assets/FieldOps/ThirdParty` | Recursos de terceros con sus avisos (ver [CREDITS.md](CREDITS.md)) |
| `Assets/FieldOps/Audio` | Efectos de sonido de armas procesados, con su aviso (ver [CREDITS.md](CREDITS.md)) |
| `Assets/FieldOps/Animation` | Controlador de animación generado para el blanco soldado |
| `Assets/FieldOps/Materials` | Materiales generados (entorno, miras de las armas, interfaz) |
| `Assets/FieldOps/Textures` | Imagen de la máscara del scope, generada (original, sin recursos de terceros) |
| `Assets/FieldOps/Editor` | Generación de la escena (mapa, soldado, miras de hierro de las armas), compilación para Android y validación de la interfaz |
| `Assets/FieldOps/Tests` | Pruebas EditMode y PlayMode |
| `Packages` | Manifiesto fijado y archivo de bloqueo de dependencias |
| `ProjectSettings` | Configuración portátil del proyecto de Unity |
| `docs/images` | Captura de juego en Android |

**Field Ops > Create or Rebuild Training Range** reemplaza la escena y los materiales generados. Guarda antes con otro nombre las variantes personalizadas de la escena. Ejecuta las pruebas desde **Window > General > Test Runner**, en las pestañas EditMode y PlayMode. El proyecto usa el pipeline de renderizado integrado de Unity.

El código fuente incluye los archivos `.meta` de Unity para conservar las referencias entre recursos. `.gitignore` excluye cachés, configuración local, registros, compilaciones, archivos de IDE y credenciales. No subas esos archivos al ampliar el proyecto.

### Licencias

El código original del proyecto y el arte original generado usan la [licencia MIT](LICENSE) incluida (el texto de la licencia está en inglés). Conserva su aviso de derechos de autor y de licencia al redistribuir el código; esa licencia no exige una pantalla de créditos dentro del juego. Los modelos de armas son recursos CC0 de terceros listados en [CREDITS.md](CREDITS.md); los nombres de las armas dentro del juego son genéricos y no se usa contenido de Call of Duty. Los sonidos de disparo de las armas son CC BY 3.0 (crédito: Vincent Sevedge) y los de recarga son CC0; los créditos y los cambios realizados están en [CREDITS.md](CREDITS.md) y en el aviso junto a los archivos de audio. Las texturas del entorno y el cielo son CC0 (Poly Haven) y el modelo del soldado es CC0 (Quaternius); ambos están acreditados allí.

El motor y el editor de Unity y sus paquetes tienen sus propios términos y licencias. Este proyecto no los reemplaza ni promete autorización legal para recursos futuros. Antes de agregar recursos a un repositorio público, verifica tanto el permiso de uso como el de redistribución del código fuente.

[Volver arriba](#field-ops-prototype)
