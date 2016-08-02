using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public struct MaterialType {
        public static string MeshPhongMaterial = "MeshPhongMaterial";
    }

    public class Material3JS {

        public string uuid;
        public string name;
        public string type;
        public int color; // Material.color
        public int ambient; // _AmbientColor
        public int emissive; // _EmissionColor
        public int specular; // _SpecColor
        public float shininess; // _Shininess
        public float opacity; // Material.color.a
        public bool transparent; // ?
        public bool wireframe; // ? false
        //public string map;

        public Material3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
            type = MaterialType.MeshPhongMaterial;
        }
    }
}
