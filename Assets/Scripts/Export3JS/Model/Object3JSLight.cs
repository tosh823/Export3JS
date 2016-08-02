using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Object3JSLight : Object3JS {

        public int color;
        public float intensity;
        public float distance;

        public Object3JSLight() : base() {
            type = ObjectType.Light;
        }
    }
}
