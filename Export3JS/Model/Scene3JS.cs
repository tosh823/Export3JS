using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public class Scene3JS : Object3JS {

        public Fog3JS fog;

        public Scene3JS() : base() {
            type = ObjectType.Scene;
        }
    }

    public class Group3JS : Object3JS {
        public Group3JS() : base() {
            type = ObjectType.Group;
        }
    }
}
