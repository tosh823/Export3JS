using UnityEngine;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

namespace Export3JS {

    public class ExporterWindow : EditorWindow {

        private ExporterOptions options;
        private bool exportAll;

        [MenuItem("ThreeJS/Export %#e")]
        static void Init() {
            ExporterWindow window = (ExporterWindow)GetWindow(typeof(ExporterWindow));
            window.titleContent = new GUIContent("ThreeJS");
            window.Show();
        }

        void OnEnable() {
            Debug.Log("Three.JS Exporter started, " + DateTime.Now.ToLongTimeString());
            options = new ExporterOptions();
            options.dir = string.Empty;
            options.exportLights = true;
            options.exportMeshes = true;
            options.exportCameras = true;
            options.exportDisabled = true;
            options.castShadows = false;
        }

        void OnGUI() {

            // Toggle options
            exportAll = options.exportLights && options.exportMeshes && options.exportCameras && options.exportDisabled;

            GUILayout.BeginVertical();
            GUILayout.Label("Options", EditorStyles.boldLabel);
            GUILayout.Label("Choose what to export:", EditorStyles.boldLabel);
            if (EditorGUILayout.Toggle("All", exportAll)) {
                options.exportCameras = true;
                options.exportLights = true;
                options.exportMeshes = true;
                options.exportDisabled = true;
            }
            options.exportMeshes = EditorGUILayout.Toggle("Meshes", options.exportMeshes);
            options.exportCameras = EditorGUILayout.Toggle("Cameras", options.exportCameras);
            options.exportLights = EditorGUILayout.Toggle("Lights", options.exportLights);
            options.exportDisabled = EditorGUILayout.Toggle("Disabled GameObjects", options.exportDisabled);
            EditorGUILayout.Space();
            GUILayout.Label("Shadows", EditorStyles.boldLabel);
            options.castShadows = EditorGUILayout.Toggle("Cast shadows", options.castShadows);
            EditorGUILayout.Space();
            GUILayout.Label("Specify output location:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            options.dir = GUILayout.TextField(options.dir);
            if (GUILayout.Button("...", GUILayout.ExpandWidth(false))) {
                string dir = EditorUtility.OpenFolderPanel("Choose destination folder", "", "");
                options.dir = dir + "/";
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Export", GUILayout.ExpandWidth(false))) {
                Exporter exporter = new Exporter(options);
                exporter.Export();
            }
            GUILayout.EndVertical();
        }

        public static void ReportProgress(float value, string message = "") {
            EditorUtility.DisplayProgressBar("ThreeJS", message, value);
        }

        public static void ClearProgress() {
            EditorUtility.ClearProgressBar();
        }
    }
}

#endif
