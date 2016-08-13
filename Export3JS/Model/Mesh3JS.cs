using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Mesh3JS : Object3JS {

        public string geometry;
        public string material;
        public bool receiveShadow;
        public bool castShadow;

        public Mesh3JS() : base() {
            type = ObjectType.Mesh;
            receiveShadow = false;
            castShadow = false;
        }
    }
}
