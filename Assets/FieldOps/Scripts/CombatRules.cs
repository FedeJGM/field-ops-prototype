using System;
namespace FieldOps {
 public sealed class TargetDamage {
  public int Hits { get; private set; }
  public int TotalDamage { get; private set; }
  public void Apply(int amount) { if(amount<=0)return; Hits++; TotalDamage+=amount; }
  public void Reset() { Hits=0; TotalDamage=0; }
 }
 public sealed class ShotGate {
  readonly double interval; double nextShot=double.NegativeInfinity;
  public ShotGate(double interval) { if(interval<=0)throw new ArgumentOutOfRangeException(nameof(interval)); this.interval=interval; }
  public void Reset() { nextShot=double.NegativeInfinity; }
  public bool TryFire(double now) { if(double.IsNaN(now)||double.IsInfinity(now)||now<nextShot)return false; nextShot=now+interval; return true; }
 }
 public static class GameText {
  public static string Get(string key,bool spanish) {
   switch(key) {
    case "aim": return spanish?"APUNTAR":"AIM";
    case "settings": return spanish?"AJUSTES":"SETTINGS";
    case "aimMode": return spanish?"MODO DE APUNTADO":"ADS MODE";
    case "hold": return spanish?"MANTENER":"HOLD";
    case "toggle": return spanish?"ALTERNAR":"TOGGLE";
    case "keepCross": return spanish?"MIRA DE CRUZ AL APUNTAR":"CROSS WHILE AIMING";
    case "on": return spanish?"SÍ":"ON";
    case "off": return spanish?"NO":"OFF";
    case "close": return spanish?"CERRAR":"CLOSE";
    case "reload": return spanish?"RECARGAR":"RELOAD";
    case "reloading": return spanish?"RECARGANDO":"RELOADING";
    case "ammo": return spanish?"MUNICIÓN":"AMMO";
    case "roundTitle": return spanish?"RETO DE ENTRENAMIENTO":"TRAINING CHALLENGE";
    case "duration": return spanish?"DURACIÓN EN SEGUNDOS (1–300)":"DURATION IN SECONDS (1–300)";
    case "start": return spanish?"INICIAR PARTIDA":"START ROUND";
    case "durationError": return spanish?"Introduce un número entero entre 1 y 300 segundos.":"Enter a whole number from 1 to 300 seconds.";
    case "scoreRule": return spanish?"Cada impacto en el blanco suma 1 punto.":"Each target hit earns 1 point.";
    case "time": return spanish?"TIEMPO":"TIME";
    case "score": return spanish?"PUNTOS":"SCORE";
    case "finished": return spanish?"PARTIDA TERMINADA":"ROUND COMPLETE";
    case "replay": return spanish?"VOLVER A JUGAR":"PLAY AGAIN";
    case "configure": return spanish?"CAMBIAR DURACIÓN":"CHANGE DURATION";
    case "paused": return spanish?"PARTIDA EN PAUSA":"ROUND PAUSED";
    case "fire": return spanish?"DISPARAR":"FIRE";
    case "sprint": return spanish?"ESPRINTAR":"SPRINT";
    case "move": return spanish?"MOVER":"MOVE";
    case "look": return spanish?"DESLIZA PARA MIRAR":"SWIPE TO LOOK";
    case "hits": return spanish?"IMPACTOS":"HITS";
    case "damage": return spanish?"DAÑO":"DAMAGE";
    case "reset": return spanish?"REINICIAR":"RESET";
    case "status": return spanish?"CAMPO DE ENTRENAMIENTO · BLANCO INDESTRUCTIBLE":"TRAINING RANGE · INDESTRUCTIBLE TARGET";
    case "help": return spanish?"WASD: mover · Shift: esprintar · Clic: capturar ratón · Esc: liberar":"WASD: move · Shift: sprint · Click: capture mouse · Esc: release";
    default: throw new ArgumentException("Unknown text key: "+key,nameof(key));
   }
  }
 }
}
