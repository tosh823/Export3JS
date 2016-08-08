using UnityEngine;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

namespace Export3JS {

    public class ExporterWindow : EditorWindow {

        [MenuItem("File/Three.JS/Export %#e")]
        static void Init() {
            GetWindow(typeof(ExporterWindow), false, "Exporter", true);
        }

        void OnEnable() {
            Debug.Log("Three.JS Exporter started, " + DateTime.Now.ToLongTimeString());
        }

        void OnGUI() {
            GUILayout.BeginVertical();
            GUILayout.Label("Three.JS Exporter", EditorStyles.boldLabel);
            if (GUILayout.Button("Export", GUILayout.ExpandWidth(false))) {
                string dir = EditorUtility.OpenFolderPanel("Choose destination folder", "", "");
                //Debug.Log("Export pressed " + dir);
                Exporter exporter = new Exporter(dir + "/");
                exporter.Export();
            }
            GUILayout.EndVertical();
        }
    }
}

#endif
