using UnityEngine;
using System.Collections;

namespace Export3JS.Model {
    
    public struct FogType {
        public static string FogExp2 = "FogExp2";
        public static string Linear = "Fog";
    }

    public class Fog3JS {

        public string type;
        public int color;

        public Fog3JS() {

        }
    }

    public class ExpFog3JS : Fog3JS {

        public float density;

        public ExpFog3JS() : base() {
            type = FogType.FogExp2;
        }

        public ExpFog3JS(Fog3JS fog) : this() {
            color = fog.color;
        }
    }

    public class LinearFog3JS : Fog3JS {

        public float near;
        public float far;

        public LinearFog3JS() : base() {
            type = FogType.Linear;
        }

        public LinearFog3JS(Fog3JS fog) : this() {
            color = fog.color;
        }
    }
}
