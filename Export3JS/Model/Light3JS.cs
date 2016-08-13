using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public struct LightType {
        public static string DirectionalLight = "DirectionalLight";
        public static string PointLight = "PointLight";
        public static string SpotLight = "SpotLight";
        public static string AmbientLight = "AmbientLight";
    }

    public class Light3JS : Object3JS {

        public int color;
        public float intensity;

        public Light3JS() : base() {

        }
    }

    public class AmbientLight3JS : Light3JS {

        public AmbientLight3JS() : base() {
            type = LightType.AmbientLight;
        }
    }

    public class DirectionalLight3JS : Light3JS {

        public bool castShadow;

        public DirectionalLight3JS() : base() {

        }

        public DirectionalLight3JS(Light3JS light) {
            uuid = light.uuid;
            name = light.name;
            matrix = light.matrix;
            color = light.color;
            intensity = light.intensity;
            type = LightType.DirectionalLight;
        }
    }

    public class PointLight3JS : Light3JS {

        public float distance;
        public float decay;

        public PointLight3JS() : base() {

        }

        public PointLight3JS(Light3JS light) {
            uuid = light.uuid;
            name = light.name;
            matrix = light.matrix;
            color = light.color;
            intensity = light.intensity;
            type = LightType.PointLight;
        }
    }

    public class SpotLight3JS : Light3JS {

        public bool castShadow;
        public float distance;
        public float angle;
        public float penumbra;
        public float decay;

        public SpotLight3JS() : base() {

        }

        public SpotLight3JS(Light3JS light) {
            uuid = light.uuid;
            name = light.name;
            matrix = light.matrix;
            color = light.color;
            intensity = light.intensity;
            type = LightType.SpotLight;
        }
    }
}
