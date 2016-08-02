using UnityEngine;
using System.Collections;

namespace Export3JS.Model {

    public class Image3JS {

        public string uuid;
        public string url;

        public Image3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
        }
    }
}
