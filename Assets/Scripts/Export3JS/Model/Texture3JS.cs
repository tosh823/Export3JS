using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public class Texture3JS {

        public string uuid;
        public string name;
        public string image;
        public float[] wrap;
        public float[] repeat;

        public Texture3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
        }
    }
}
