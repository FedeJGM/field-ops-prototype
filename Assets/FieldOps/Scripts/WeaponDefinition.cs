using UnityEngine;
namespace FieldOps {
 // Data for one selectable weapon. The scene generator creates these assets and the Rifle controller reads them.
 public sealed class WeaponDefinition : ScriptableObject {
  public string nameKey="weaponRifle";
  public GameObject modelPrefab;
  public AudioClip shotClip;
  public AudioClip reloadClip;
  public float modelLength;
  public Vector3 muzzleLocal;
  public Vector3 aimOffset=new Vector3(-.28f,-.03f,-.04f);
  // Hip-pose, holder-local points of the sight line: rear aperture (or scope eyepiece) and front post tip (or scope objective).
  public Vector3 sightRear,sightFront;
  public bool scopeView;   // aiming shows the scope view (dark surround, circular opening) instead of the gun
  // Hand-worked action after each shot (sniper bolt, shotgun pump): the weapon moves during the fire cooldown.
  public bool manualAction;
  public float cycleRoll,cyclePitch,cyclePull,cycleSoundStart;
  public AudioClip cycleClip;
  public int magazineSize=30;
  public float fireInterval=.12f;
  public int damage=25;
  public int pellets=1;
  public float spreadDegrees;
  public float range=80f;
  public float reloadSeconds=1.5f;
  public float kick=.045f;
  public float aimFov=40f;
 }
}
