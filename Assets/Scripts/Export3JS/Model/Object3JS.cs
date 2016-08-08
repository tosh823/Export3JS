using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Export3JS.Model {

    public struct ObjectType {
        public static string Scene = "Scene";
        public static string Mesh = "Mesh";
        public static string Group = "Group";
        public static string Light = "DirectionalLight";
        public static string Camera = "PerspectiveCamera";
    }

    public class Object3JS {

        public string uuid;
        public string name;
        public string type;
        //public float[] matrix;
        public float[] position;
        public float[] rotation;
        public float[] quaternion;
        public float[] scale;
        public List<Object3JS> children;

        public Object3JS() {
            uuid = System.Guid.NewGuid().ToString().ToUpper();
            //matrix = new float[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            children = new List<Object3JS>();
        }
    }
}
