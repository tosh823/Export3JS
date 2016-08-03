using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Export3JS.Model;

namespace Export3JS {

    public class Exporter {

        private string dir;
        private Format4 content;
        private Dictionary<string, Material> materials;
        private Dictionary<string, Mesh> geometries;

        public Exporter(string dir) {
            this.dir = dir;
            materials = new Dictionary<string, Material>();
            geometries = new Dictionary<string, Mesh>();
        }

        public void Export() {
            parseScene();
            string json = JsonConvert.SerializeObject(content, Formatting.Indented);
            string filename = SceneManager.GetActiveScene().name + ".json";
            System.IO.File.WriteAllText(dir + filename, json);
            Debug.Log("Three.JS Exporter completed, " + DateTime.Now.ToLongTimeString());
        }

        private void parseScene() {
            content = new Format4();
            // Create base scene
            Object3JSScene scene = new Object3JSScene();
            scene.name = SceneManager.GetActiveScene().name;
            content.@object = scene;
            // Enumerate through all the objects
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject gameObject in rootObjects) {
                Object3JS obj = parseGameObject(gameObject);
                scene.children.Add(obj);
            }
        }

        private Object3JS parseGameObject(GameObject gameObject) {
            Object3JS obj;
            if (!gameObject.activeInHierarchy) {
                obj = null;
            }
            else if (gameObject.GetComponent<Renderer>()) {
                obj = createMesh(gameObject);
            }
            else if (gameObject.GetComponent<Light>()) {
                obj = createLight(gameObject);
            }
            else if (gameObject.GetComponent<Camera>()) {
                obj = createCamera(gameObject);
            }
            else {
                obj = createGroup(gameObject);
            }
            return obj;
        }

        private Object3JSMesh createMesh(GameObject gameObject) {
            Object3JSMesh mesh = new Object3JSMesh();
            mesh.name = gameObject.name;
            mesh.matrix = Utils.getMatrixAsArray(gameObject.transform.localToWorldMatrix);
            if (gameObject.GetComponent<MeshFilter>() != null) mesh.geometry = parseGeometries(gameObject);
            if (gameObject.GetComponent<Renderer>() != null) mesh.material = parseMaterials(gameObject);
            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) mesh.children.Add(childObj);
                }
            }
            return mesh;
        }

        private Object3JSLight createLight(GameObject gameObject) {
            Object3JSLight light = new Object3JSLight();
            Light lightComponent = gameObject.GetComponent<Light>();
            light.name = gameObject.name;
            light.matrix = Utils.getMatrixAsArray(gameObject.transform.localToWorldMatrix);
            light.color = Utils.getIntColor(lightComponent.color);
            light.intensity = lightComponent.intensity;
            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) light.children.Add(childObj);
                }
            }
            return light;
        }

        private Object3JSGroup createGroup(GameObject gameObject) {
            Object3JSGroup group = new Object3JSGroup();
            group.name = gameObject.name;
            group.matrix = Utils.getMatrixAsArray(gameObject.transform.localToWorldMatrix);
            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) group.children.Add(childObj);
                }
            }
            return group;
        }

        private Object3JSCamera createCamera(GameObject gameObject) {
            Object3JSCamera camera = new Object3JSCamera();
            Camera cameraComponent = gameObject.GetComponent<Camera>();
            camera.name = gameObject.name;
            camera.matrix = Utils.getMatrixAsArray(gameObject.transform.localToWorldMatrix);
            camera.fov = cameraComponent.fieldOfView;
            camera.aspect = cameraComponent.aspect;
            camera.near = cameraComponent.nearClipPlane;
            camera.far = cameraComponent.farClipPlane;
            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) camera.children.Add(childObj);
                }
            }
            return camera;
        }

        private string parseGeometries(GameObject gameObject) {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            bool contains = false;
            string uuid = "";
            foreach (KeyValuePair<string, Mesh> pair in geometries) {
                if (pair.Value.Equals(mesh)) {
                    contains = true;
                    uuid = pair.Key;
                    break;
                }
            }
            if (!contains) {
                uuid = createGeometry(meshFilter);
            }
            return uuid;
        }

        private string createGeometry(MeshFilter meshFilter) {
            Geometry3JS geometry = new Geometry3JS();
            geometry.name = meshFilter.name;
            Mesh mesh = meshFilter.sharedMesh;
            // Vertices
            geometry.data.vertices = new float[mesh.vertexCount * 3];
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < mesh.vertexCount; i++) {
                Vector3 vertex = vertices[i];
                geometry.data.vertices[i * 3] = vertex.x;
                geometry.data.vertices[i * 3 + 1] = vertex.y;
                geometry.data.vertices[i * 3 + 2] = vertex.z;
            }
            // Normals
            geometry.data.normals = new float[mesh.normals.Length * 3];
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) {
                Vector3 normal = normals[i];
                geometry.data.normals[i * 3] = normal.x;
                geometry.data.normals[i * 3 + 1] = normal.y;
                geometry.data.normals[i * 3 + 2] = normal.z;
            }
            // UV
            geometry.data.uvs = new float[mesh.uv.Length * 2];
            Vector2[] uvs = mesh.uv;
            for (int i = 0; i < uvs.Length; i++) {
                Vector2 uv = uvs[i];
                geometry.data.uvs[i * 2] = uv.x;
                geometry.data.uvs[i * 2 + 1] = uv.y;
            }
            // Colors
            geometry.data.colors = new float[mesh.colors.Length * 3];
            Color[] colors = mesh.colors;
            for (int i = 0; i < colors.Length; i++) {
                Color color = colors[i];
                geometry.data.colors[i * 3] = color.r;
                geometry.data.colors[i * 3 + 1] = color.b;
                geometry.data.colors[i * 3 + 2] = color.g;
            }
            // Faces
            int count = mesh.triangles.Length / 3;
            geometry.data.faces = new int[mesh.triangles.Length + count];
            for (int i = 0; i < count; i++) {
                int triangle = i * 3;
                int face = triangle + i;
                geometry.data.faces[face] = 0;
                geometry.data.faces[face + 1] = mesh.triangles[triangle];
                geometry.data.faces[face + 2] = mesh.triangles[triangle + 1];
                geometry.data.faces[face + 3] = mesh.triangles[triangle + 2];
            }

            content.geometries.Add(geometry);
            geometries.Add(geometry.uuid, mesh);
            return geometry.uuid;
        }

        private string parseMaterials(GameObject gameObject) {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material material = renderer.sharedMaterial;
            bool contains = false;
            string uuid = "";
            foreach (KeyValuePair<string, Material> pair in materials) {
                if (pair.Value.Equals(material)) {
                    contains = true;
                    uuid = pair.Key;
                    break;
                }
            }
            if (!contains) {
                uuid = createMaterial(material);
            }
            return uuid;
        }

        private string createMaterial(Material mat) {
            Material3JS jsMat = new Material3JS();
            jsMat.name = mat.name;
            // Colors
            jsMat.color = Utils.getIntColor(mat.color);
            if (mat.HasProperty("_SpecColor")) jsMat.specular = Utils.getIntColor(mat.GetColor("_SpecColor"));
            if (mat.HasProperty("_EmissionColor")) jsMat.emissive = Utils.getIntColor(mat.GetColor("_EmissionColor"));
            if (mat.HasProperty("_AmbientColor")) jsMat.ambient = Utils.getIntColor(mat.GetColor("_AmbientColor"));
            if (mat.HasProperty("_Shininess")) jsMat.shininess = mat.GetFloat("_Shininess");
            // Textures
            /*if (mat.HasProperty("_MainTex")) {
                Texture mainTexture = mat.GetTexture("_MainTex");
                if (mainTexture != null) {
                    string uuid = createTexture(mainTexture, mat);
                    if (string.IsNullOrEmpty(uuid)) jsMat.map = uuid;
                }
            }*/
            // Opacity and wireframe
            jsMat.opacity = mat.color.a;
            jsMat.transparent = false;
            jsMat.wireframe = false;
            content.materials.Add(jsMat);
            materials.Add(jsMat.uuid, mat);
            return jsMat.uuid;
        }

        private string createTexture(Texture tex, Material mat) {
            Texture3JS jsText = new Texture3JS();
            jsText.name = tex.name;
            Image3JS jsImg = new Image3JS();
            // Copying the texture file
            string relativePath = AssetDatabase.GetAssetPath(tex);
            string url = Utils.copyTexture(relativePath, dir);
            if (!string.IsNullOrEmpty(url)) {
                jsImg.url = '/' + url;
                jsText.image = jsImg.uuid;
                jsText.wrap = new float[2] { mat.mainTextureScale.x, mat.mainTextureScale.y };
                // Wrap mode
                /*switch (tex.wrapMode) {
                    case TextureWrapMode.Repeat:
                        jsText.wrap = new string[2] { "repeat", "repeat" };
                        break;
                    case TextureWrapMode.Clamp:
                        jsText.wrap = new string[2] { "clamp", "clamp" };
                        break;
                }*/
                jsText.repeat = new float[2] { mat.mainTextureScale.x, mat.mainTextureScale.y };
                // Add to content
                content.images.Add(jsImg);
                content.textures.Add(jsText);
                return jsText.uuid;
            }
            else return null;
        }
    }
}
