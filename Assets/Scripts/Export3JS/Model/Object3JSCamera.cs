using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    public class Object3JSCamera : Object3JS {

        public float fov;
        public float aspect;
        public float near;
        public float far;

        public Object3JSCamera() : base() {
            type = ObjectType.Camera;
        }
    }
}
