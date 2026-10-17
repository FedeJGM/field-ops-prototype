using NUnit.Framework;
namespace FieldOps.Tests {
 public class CombatRulesTests {
  [Test] public void PositiveDamageAccumulatesWithoutRemovingTarget() { var target=new TargetDamage(); target.Apply(25); target.Apply(25); Assert.That(target.Hits,Is.EqualTo(2)); Assert.That(target.TotalDamage,Is.EqualTo(50)); }
  [Test] public void NonPositiveDamageDoesNotCount() { var target=new TargetDamage(); target.Apply(0); target.Apply(-4); Assert.That(target.Hits,Is.Zero); }
  [Test] public void AutomaticFireRespectsCooldown() { var gate=new ShotGate(0.12); Assert.That(gate.TryFire(0),Is.True); Assert.That(gate.TryFire(0.05),Is.False); Assert.That(gate.TryFire(0.12),Is.True); }
  [TestCase("fire","FIRE","DISPARAR")][TestCase("sprint","SPRINT","ESPRINTAR")][TestCase("jump","JUMP","SALTAR")][TestCase("sprintMode","SPRINT MODE","MODO DE CARRERA")][TestCase("sprintSens","SPRINT SENSITIVITY","SENSIBILIDAD DE CARRERA")][TestCase("sensHigh","HIGH","ALTA")][TestCase("sensMedium","MEDIUM","MEDIA")][TestCase("sensLow","LOW","BAJA")][TestCase("sensNote","JOYSTICK MODE ONLY","SOLO MODO JOYSTICK")][TestCase("stick","JOYSTICK","JOYSTICK")][TestCase("button","BUTTON","BOTÓN")][TestCase("slide","SLIDE","DESLIZAR")][TestCase("crouch","CROUCH","AGACHARSE")][TestCase("move","MOVE","MOVER")][TestCase("look","SWIPE TO LOOK","DESLIZA PARA MIRAR")][TestCase("hits","HITS","IMPACTOS")]
  public void RequiredLabelsExistInBothLanguages(string key,string en,string es) { Assert.That(GameText.Get(key,false),Is.EqualTo(en)); Assert.That(GameText.Get(key,true),Is.EqualTo(es)); }
 }
}
