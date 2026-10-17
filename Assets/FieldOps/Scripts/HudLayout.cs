using System;
using System.Collections.Generic;
using UnityEngine;
namespace FieldOps {
 // Pure HUD layout rules: saved offsets for the movable touch buttons, clamping inside the safe area and overlap tests.
 public sealed class HudLayout {
  public const int Version=1;
  public const float MaxOffset=10000f;
  public static readonly string[] Ids={"joystick","fire","reload","aim","sprint","jump","slide","crouch","language","fps"};
  [Serializable] public sealed class Saved { public int version; public Entry[] entries; }
  [Serializable] public struct Entry { public string id; public float x; public float y; }
  readonly Dictionary<string,Vector2> positions=new Dictionary<string,Vector2>();
  public bool IsEmpty => positions.Count==0;
  static bool Finite(float value) { return !float.IsNaN(value)&&!float.IsInfinity(value)&&Math.Abs(value)<=MaxOffset; }
  public bool TryGet(string id,out Vector2 position) { position=default;return id!=null&&positions.TryGetValue(id,out position); }
  public bool Set(string id,Vector2 position) {
   if(id==null||Array.IndexOf(Ids,id)<0||!Finite(position.x)||!Finite(position.y))return false;
   positions[id]=position;return true;
  }
  public void Clear() { positions.Clear(); }
  public string ToJson() {
   if(IsEmpty)return "";
   var entries=new List<Entry>();
   foreach(var id in Ids)if(positions.TryGetValue(id,out var p))entries.Add(new Entry{id=id,x=p.x,y=p.y});
   return JsonUtility.ToJson(new Saved{version=Version,entries=entries.ToArray()});
  }
  // Any corrupt, unsupported or ambiguous data yields an empty layout, which means the default places.
  public static HudLayout FromJson(string json) {
   var layout=new HudLayout();
   if(string.IsNullOrWhiteSpace(json))return layout;
   Saved saved;
   try { saved=JsonUtility.FromJson<Saved>(json); } catch(Exception) { return layout; }
   if(saved==null||saved.version!=Version||saved.entries==null)return layout;
   var seen=new HashSet<string>();
   foreach(var entry in saved.entries) {
    if(!seen.Add(entry.id??"")||!layout.Set(entry.id,new Vector2(entry.x,entry.y)))return new HudLayout();
   }
   return layout;
  }
  // Keeps the whole element inside the area. Positions are offsets from the element's anchor corner, in canvas units.
  public static Vector2 ClampInside(Vector2 anchoredPosition,Vector2 anchor,Vector2 size,Vector2 area) {
   return new Vector2(ClampAxis(anchoredPosition.x,anchor.x,size.x,area.x),ClampAxis(anchoredPosition.y,anchor.y,size.y,area.y));
  }
  static float ClampAxis(float position,float anchor,float size,float area) {
   float half=size*.5f,center=anchor*area+position;
   center=half>area-half?area*.5f:Mathf.Clamp(center,half,area-half);
   return center-anchor*area;
  }
  // Strict overlap: elements that only touch along an edge do not overlap.
  public static bool Overlaps(Vector2 centerA,Vector2 sizeA,Vector2 centerB,Vector2 sizeB) {
   return Math.Abs(centerA.x-centerB.x)<(sizeA.x+sizeB.x)*.5f&&Math.Abs(centerA.y-centerB.y)<(sizeA.y+sizeB.y)*.5f;
  }
 }
}
