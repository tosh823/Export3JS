using UnityEngine;
using System.Collections.Generic;

namespace Export3JS.Model {

    public struct MaterialType {
        public static string MeshBasicMaterial = "MeshBasicMaterial";
        public static string MeshPhongMaterial = "MeshPhongMaterial";
        public static string MultiMaterial = "MultiMaterial";
    }

    public struct MaterialSide {
        public static int FrontSide = 0;
        public static int BackSide = 1;
        public static int DoubleSide = 2;
    }

    public class Material3JS {

        public string uuid;
        public string name;
        public string type;

        public int side;
        public float opacity; // Material.color.a
        public bool transparent; // ?
        public bool wireframe; // ? false

        public Material3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
            type = MaterialType.MeshPhongMaterial;
            side = MaterialSide.BackSide;
        }
    }

    public class MeshBasicMaterial3JS : Material3JS {

        public string map;
        public int color; // _Color

        public MeshBasicMaterial3JS() : base() {
            type = MaterialType.MeshBasicMaterial;
        }
    }

    public class MeshPhongMaterial3JS : Material3JS {

        public string map; // _MainTex 
        public string normalMap; // _BumpMap
        public string emissiveMap; // _EmissionMap
        public string specularMap; // _SpecGlossMap
        public int color; // _Color
        public int specular; // _SpecColor
        public float shininess; // _Shininess
        public int emissive; // _EmissionColor
        public float emissiveIntensity; // _Emission

        public MeshPhongMaterial3JS() : base() {
            type = MaterialType.MeshPhongMaterial;
        }

    }

    public class MultiMaterial3JS : Material3JS {

        public List<Material3JS> materials;

        public MultiMaterial3JS() : base() {
            type = MaterialType.MultiMaterial;
            materials = new List<Material3JS>();
        }
    }
}
