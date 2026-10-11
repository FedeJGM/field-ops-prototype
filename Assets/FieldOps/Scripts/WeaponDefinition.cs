using UnityEngine;
namespace FieldOps {
 // Data for one selectable weapon. The scene generator creates these assets and the Rifle controller reads them.
 public sealed class WeaponDefinition : ScriptableObject {
  public string nameKey="weaponRifle";
  public GameObject modelPrefab;
  public float modelLength;
  public Vector3 muzzleLocal;
  public Vector3 aimOffset=new Vector3(-.28f,-.03f,-.04f);
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
