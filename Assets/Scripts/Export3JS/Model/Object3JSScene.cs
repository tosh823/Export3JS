using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Object3JSScene : Object3JS {

        public float[] matrix;

        public Object3JSScene() : base() {
            type = ObjectType.Scene;
            matrix = new float[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }
    }

    public class Object3JSGroup : Object3JS {
        public Object3JSGroup() : base() {
            type = ObjectType.Group;
        }
    }
}
