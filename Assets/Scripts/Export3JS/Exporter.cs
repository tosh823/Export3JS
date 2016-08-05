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
        private Dictionary<string, Material[]> multiMaterials;
        private Dictionary<string, Mesh> geometries;

        public Exporter(string dir) {
            this.dir = dir;
            materials = new Dictionary<string, Material>();
            multiMaterials = new Dictionary<string, Material[]>();
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
            if (gameObject.GetComponent<Renderer>() != null) {
                mesh.material = parseMaterials(gameObject);
            }
            if (gameObject.GetComponent<MeshFilter>() != null) mesh.geometry = parseGeometries(gameObject);
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
            Debug.Log(meshFilter.gameObject.name);
            Debug.Log("Vertices count " + mesh.vertexCount);
            Debug.Log("Triangles count " + mesh.triangles.Length);
            Debug.Log("UV count " + mesh.uv.Length);
            Debug.Log("Normal count " + mesh.normals.Length);
            FaceMask code = FaceMask.TRIANGLE;
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
            if (normals.Length > 0) code = code | FaceMask.VERTEX_NORMAL;
            for (int i = 0; i < normals.Length; i++) {
                Vector3 normal = normals[i];
                geometry.data.normals[i * 3] = normal.x;
                geometry.data.normals[i * 3 + 1] = normal.y;
                geometry.data.normals[i * 3 + 2] = normal.z;
            }
            // UV
            geometry.data.uvs = new float[1, mesh.uv.Length * 2];
            Vector2[] uvs = mesh.uv;
            if (uvs.Length > 0) code = code | FaceMask.FACE_VERTEX_UV;
            for (int i = 0; i < uvs.Length; i++) {
                Vector2 uv = uvs[i];
                geometry.data.uvs[0, i * 2] = uv.x;
                geometry.data.uvs[0, i * 2 + 1] = uv.y;
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
            int subMeshCount = mesh.subMeshCount;
            if (subMeshCount > 1) code = code | FaceMask.FACE_MATERIAL;
            switch ((int)code) {
                case 0:
                    // 0, [vertex_index, vertex_index, vertex_index]
                    geometry.data.faces = createFaces(mesh);
                    break;
                case 2:
                    // 2, [vertex_index, vertex_index, vertex_index],
                    // [material_index]
                    geometry.data.faces = createFacesWithMaterials(mesh);
                    break;
                case 8:
                    // 8, [vertex_index, vertex_index, vertex_index],
                    // [vertex_uv, vertex_uv, vertex_uv]
                    geometry.data.faces = createFacesWithUV(mesh);
                    break;
                case 10:
                    // 10, [vertex_index, vertex_index, vertex_index],
                    // [material_index],
                    // [vertex_uv, vertex_uv, vertex_uv]
                    geometry.data.faces = createFacesWithMaterialsUV(mesh);
                    break;
                case 34:
                    // 34, [vertex_index, vertex_index, vertex_index],
                    // [material_index],
                    // [vertex_normal, vertex_normal, vertex_normal]
                    geometry.data.faces = createFacesWithMaterialsNormals(mesh);
                    break;
                case 40:
                    // 40, [vertex_index, vertex_index, vertex_index],
                    // [vertex_uv, vertex_uv, vertex_uv],
                    // [vertex_normal, vertex_normal, vertex_normal]
                    geometry.data.faces = createFacesWithUVNormals(mesh);
                    break;
                case 42:
                    // 42, [vertex_index, vertex_index, vertex_index],
                    // [material_index],
                    // [vertex_uv, vertex_uv, vertex_uv],
                    // [vertex_normal, vertex_normal, vertex_normal]
                    geometry.data.faces = createFacesWithMaterialsUVNormals(mesh);
                    break;

            }
            content.geometries.Add(geometry);
            geometries.Add(geometry.uuid, mesh);
            return geometry.uuid;
        }

        private string parseMaterials(GameObject gameObject) {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            string uuid = string.Empty;
            if (renderer.sharedMaterials.Length > 1) {
                Material[] objMaterials = renderer.sharedMaterials;
                // If cash contains the same array of materials, get their uuid
                // Else create a new one
                if (!Utils.dictContainsArray(out uuid, multiMaterials, objMaterials)) {
                    uuid = createMultiMaterial(objMaterials);
                }
            } 
            else {
                Material objMaterial = renderer.sharedMaterial;
                // If cash contains the same material, get its uuid
                // Else create a new one
                if (!Utils.dictContainsValue(out uuid, materials, objMaterial)) {
                    uuid = createMaterial(objMaterial);
                }
            }
            return uuid;
        }

        private string createMaterial(Material mat) {
            MeshPhongMaterial3JS matJS = new MeshPhongMaterial3JS();
            matJS.name = mat.name;
            // Colors
            matJS.color = Utils.getIntColor(mat.color);
            if (mat.HasProperty("_SpecColor")) {
                matJS.specular = Utils.getIntColor(mat.GetColor("_SpecColor"));
            }
            if (mat.HasProperty("_EmissionColor")) {
                matJS.emissive = Utils.getIntColor(mat.GetColor("_EmissionColor"));
            }
            if (mat.HasProperty("_AmbientColor")) {
                matJS.ambient = Utils.getIntColor(mat.GetColor("_AmbientColor"));
            }
            if (mat.HasProperty("_Shininess")) {
                matJS.shininess = mat.GetFloat("_Shininess");
            }
            // Textures
            if (mat.HasProperty("_MainTex")) {
                Texture mainTexture = mat.GetTexture("_MainTex");
                if (mainTexture != null) {
                    string uuid = createTexture(mainTexture, mat);
                    if (!string.IsNullOrEmpty(uuid)) matJS.map = uuid;
                }
            }
            // Opacity and wireframe
            matJS.opacity = mat.color.a;
            matJS.transparent = false;
            matJS.wireframe = false;

            content.materials.Add(matJS);
            materials.Add(matJS.uuid, mat);
            return matJS.uuid;
        }

        private string createMultiMaterial(Material[] mats) {
            MultiMaterial3JS multiMatJS = new MultiMaterial3JS();
            multiMatJS.name = "MultiMaterial";
            foreach (Material mat in mats) {
                string uuid = string.Empty;
                if (Utils.dictContainsValue(out uuid, materials, mat)) {
                    // If we already had the same material, find it
                    Material3JS existingMatJS = content.materials.Find(x => (x.uuid.Equals(uuid)));
                    multiMatJS.materials.Add(existingMatJS);
                }
                else {
                    // Else create one
                    // And, duhh, find it too
                    uuid = createMaterial(mat);
                    Material3JS existingMatJS = content.materials.Find(x => (x.uuid.Equals(uuid)));
                    multiMatJS.materials.Add(existingMatJS);
                    // Because we created new material for the purpose of it
                    // Remove it from other materials
                    content.materials.Remove(existingMatJS);
                }
            }

            content.materials.Add(multiMatJS);
            multiMaterials.Add(multiMatJS.uuid, mats);
            return multiMatJS.uuid;
        }

        private string createTexture(Texture tex, Material mat) {
            Texture3JS jsText = new Texture3JS();
            jsText.name = tex.name;
            Image3JS jsImg = new Image3JS();
            // Copying the texture file
            string relativePath = AssetDatabase.GetAssetPath(tex);
            string url = Utils.copyTexture(relativePath, dir);
            if (!string.IsNullOrEmpty(url)) {
                jsImg.url = url;
                jsText.image = jsImg.uuid;
                // Wrap mode
                switch (tex.wrapMode) {
                    case TextureWrapMode.Repeat:
                        jsText.wrap = new int[2] { WrapType.RepeatWrapping, WrapType.RepeatWrapping };
                        break;
                    case TextureWrapMode.Clamp:
                        jsText.wrap = new int[2] { WrapType.ClampToEdgeWrapping, WrapType.ClampToEdgeWrapping };
                        break;
                }
                jsText.repeat = new float[2] { mat.mainTextureScale.x, mat.mainTextureScale.y };
                // Add to content
                content.images.Add(jsImg);
                content.textures.Add(jsText);
                return jsText.uuid;
            }
            else return null;
        }

        // 0, [vertex_index, vertex_index, vertex_index]
        private int[] createFaces(Mesh mesh, int code = 0) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int[] faces = new int[mesh.triangles.Length + totalTrianglesCount];
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 1];
                faces[pos + 3] = mesh.triangles[vertex + 2];
            }
            return faces;
        }

        // 2, [vertex_index, vertex_index, vertex_index],
        // [material_index]
        private int[] createFacesWithMaterials(Mesh mesh, int code = 2) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[mesh.triangles.Length + 2 * totalTrianglesCount];
            int shift = 0;
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++) {
                int[] subMeshTriangles = mesh.GetTriangles(subMesh);
                int trianglesCount = subMeshTriangles.Length / 3;
                for (int i = 0; i < trianglesCount; i++) {
                    int vertex = i * 3;
                    int pos = shift + i + vertex + i;
                    faces[pos] = code;
                    faces[pos + 1] = subMeshTriangles[vertex];
                    faces[pos + 2] = subMeshTriangles[vertex + 1];
                    faces[pos + 3] = subMeshTriangles[vertex + 2];
                    faces[pos + 4] = subMesh;
                }
                shift += (subMeshTriangles.Length + 2 * trianglesCount);
            }
            return faces;
        }

        // 8, [vertex_index, vertex_index, vertex_index],
        // [vertex_uv, vertex_uv, vertex_uv]
        private int[] createFacesWithUV(Mesh mesh, int code = 8) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int[] faces = new int[2 * mesh.triangles.Length + totalTrianglesCount];
            Vector2[] uvs = mesh.uv;
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex * 2;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 1];
                faces[pos + 3] = mesh.triangles[vertex + 2];
                faces[pos + 4] = mesh.triangles[vertex];
                faces[pos + 5] = mesh.triangles[vertex + 1];
                faces[pos + 6] = mesh.triangles[vertex + 2];
            }
            return faces;
        }

        // 10, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_uv, vertex_uv, vertex_uv]
        private int[] createFacesWithMaterialsUV(Mesh mesh, int code = 10) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[0];
            return faces;
        }

        // 40, [vertex_index, vertex_index, vertex_index],
        // [vertex_uv, vertex_uv, vertex_uv],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithUVNormals(Mesh mesh, int code = 40) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int[] faces = new int[3 * mesh.triangles.Length + totalTrianglesCount];
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex * 3;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 1];
                faces[pos + 3] = mesh.triangles[vertex + 2];
                faces[pos + 4] = mesh.triangles[vertex];
                faces[pos + 5] = mesh.triangles[vertex + 1];
                faces[pos + 6] = mesh.triangles[vertex + 2];
                faces[pos + 7] = mesh.triangles[vertex];
                faces[pos + 8] = mesh.triangles[vertex + 1];
                faces[pos + 9] = mesh.triangles[vertex + 2];
            }
            return faces;
        }

        // 34, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithMaterialsNormals(Mesh mesh, int code = 34) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[0];
            return faces;
        }

        // 42, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_uv, vertex_uv, vertex_uv],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithMaterialsUVNormals(Mesh mesh, int code = 42) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[0];
            return faces;
        }
    }
}
