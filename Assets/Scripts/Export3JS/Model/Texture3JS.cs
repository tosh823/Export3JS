using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public struct WrapType {
        public static int RepeatWrapping = 1000;
        public static int ClampToEdgeWrapping = 1001;
        public static int MirroredRepeatWrapping = 1002;
    }

    public class Texture3JS {

        public string uuid;
        public string name;
        public string image;
        public int[] wrap;
        public float[] repeat;

        public Texture3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
        }
    }
}
