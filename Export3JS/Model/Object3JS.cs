using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Export3JS.Model {

    public struct ObjectType {
        public static string Scene = "Scene";
        public static string Mesh = "Mesh";
        public static string Group = "Group";
    }

    public class Object3JS {

        public string uuid;
        public string name;
        public string type;
        public float[] matrix;
        public List<Object3JS> children;

        public Object3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
            children = new List<Object3JS>();
        }
    }
}
