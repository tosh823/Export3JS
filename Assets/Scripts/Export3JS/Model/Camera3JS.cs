using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public struct CameraType {
        public static string PerspectiveCamera = "PerspectiveCamera";
        public static string OrthographicCamera = "OrthographicCamera";
    }

    public class Camera3JS : Object3JS {

        public float near;
        public float far;

        public Camera3JS() : base() {
            
        }
    }

    public class PerspectiveCamera3JS : Camera3JS {

        public float fov;
        public float aspect;

        public PerspectiveCamera3JS() : base() {
            type = CameraType.PerspectiveCamera;
        }

        public PerspectiveCamera3JS(Camera3JS camera) {
            uuid = camera.uuid;
            name = camera.name;
            type = CameraType.PerspectiveCamera;
            matrix = camera.matrix;
            near = camera.near;
            far = camera.far;
        }
    }

    public class OrthographicCamera3JS : Camera3JS {

        public float left;
        public float right;
        public float top;
        public float bottom;

        public OrthographicCamera3JS() : base() {
            type = CameraType.OrthographicCamera;
        }

        public OrthographicCamera3JS(Camera3JS camera) {
            uuid = camera.uuid;
            name = camera.name;
            type = CameraType.OrthographicCamera;
            matrix = camera.matrix;
            near = camera.near;
            far = camera.far;
        }

    }
}
