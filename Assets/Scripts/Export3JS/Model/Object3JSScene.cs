using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Object3JSScene : Object3JS {

        public Object3JSScene() : base() {
            type = ObjectType.Scene;
        }
    }

    public class Object3JSGroup : Object3JS {
        public Object3JSGroup() : base() {
            type = ObjectType.Group;
        }
    }
}
