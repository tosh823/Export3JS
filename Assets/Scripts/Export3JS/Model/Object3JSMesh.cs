using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Object3JSMesh : Object3JS {

        public string geometry;
        public string material;

        public Object3JSMesh() : base() {
            type = ObjectType.Mesh;
        }
    }
}
