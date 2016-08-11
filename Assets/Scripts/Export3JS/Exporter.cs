using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Export3JS.Model;

namespace Export3JS {

    public struct ExporterOptions {
        public string dir;
        public bool exportCameras;
        public bool exportLights;
        public bool exportMeshes;
        public bool exportDisabled;
        public bool castShadows;
    }

    public class Exporter {

        private ExporterOptions options;
        private int objectTotal;
        private int objectsParsed;
        private Format4 content;
        private Dictionary<string, Material> materials;
        private Dictionary<string, Material[]> multiMaterials;
        private Dictionary<string, Mesh> geometries;

        public Exporter(ExporterOptions options) {
            this.options = options;
            materials = new Dictionary<string, Material>();
            multiMaterials = new Dictionary<string, Material[]>();
            geometries = new Dictionary<string, Mesh>();
        }

        public void Export() {
            objectTotal = UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
            objectsParsed = 0;
            parseScene();
            string json = JsonConvert.SerializeObject(content, Formatting.Indented);
            string filename = SceneManager.GetActiveScene().name + ".json";
            System.IO.File.WriteAllText(options.dir + filename, json);
            Debug.Log("Three.JS Exporter completed, " + DateTime.Now.ToLongTimeString());
            ExporterWindow.ClearProgress();
        }

        private void updateProgress() {
            objectsParsed++;
            float value = objectsParsed / (float)objectTotal;
            ExporterWindow.ReportProgress(value);
        }

        private void parseScene() {
            content = new Format4();
            // Create base scene
            Scene3JS scene = new Scene3JS();
            scene.name = SceneManager.GetActiveScene().name;
            scene.matrix = Utils.getMatrixAsArray(Matrix4x4.identity);
            // Checking if we have fog
            if (RenderSettings.fog) {
                Fog3JS fog = new Fog3JS();
                fog.color = Utils.getIntColor(RenderSettings.fogColor);
                switch (RenderSettings.fogMode) {
                    case FogMode.Linear:
                        LinearFog3JS linearFog = new LinearFog3JS(fog);
                        linearFog.near = RenderSettings.fogStartDistance;
                        linearFog.far = RenderSettings.fogEndDistance;
                        scene.fog = linearFog;
                        break;
                    case FogMode.Exponential:
                    case FogMode.ExponentialSquared:
                        ExpFog3JS expFog = new ExpFog3JS(fog);
                        expFog.density = RenderSettings.fogDensity;
                        scene.fog = expFog;
                        break;
                } 
            }
            scene.children.Add(createAmbientLight());
            content.@object = scene;
            // Enumerate through all the objects
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject gameObject in rootObjects) {
                Object3JS obj = parseGameObject(gameObject);
                scene.children.Add(obj);
            }
        }

        private Object3JS parseGameObject(GameObject gameObject) {
            if (gameObject.activeInHierarchy || options.exportDisabled) {
                Object3JS obj;
                if (gameObject.GetComponent<Renderer>() && options.exportMeshes) {
                    obj = createMesh(gameObject);
                }
                else if (gameObject.GetComponent<Light>() && options.exportLights) {
                    obj = createLight(gameObject);
                }
                else if (gameObject.GetComponent<Camera>() && options.exportCameras) {
                    obj = createCamera(gameObject);
                }
                else {
                    obj = createGroup(gameObject);
                }
                updateProgress();
                return obj;
            }
            else {
                updateProgress();
                return null;
            }
        }

        private Mesh3JS createMesh(GameObject gameObject) {
            Mesh3JS mesh = new Mesh3JS();
            mesh.name = gameObject.name;
            mesh.matrix = getMatrix(gameObject);

            if (gameObject.GetComponent<Renderer>() != null) {
                string uuid = parseMaterials(gameObject);
                if (!string.IsNullOrEmpty(uuid)) {
                    mesh.material = uuid;
                    mesh.receiveShadow = gameObject.GetComponent<Renderer>().receiveShadows;
                    mesh.castShadow = options.castShadows;
                }
            }
            if (gameObject.GetComponent<MeshFilter>() != null) {
                string uuid = parseGeometries(gameObject);
                if (!string.IsNullOrEmpty(uuid)) mesh.geometry = uuid;
            }
            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) mesh.children.Add(childObj);
                }
            }
            return mesh;
        }

        private AmbientLight3JS createAmbientLight() {
            AmbientLight3JS ambientLight = new AmbientLight3JS();
            ambientLight.name = "AmbientLight";
            ambientLight.matrix = Utils.getMatrixAsArray(Matrix4x4.identity);
            ambientLight.color = Utils.getIntColor(RenderSettings.ambientLight);
            ambientLight.intensity = RenderSettings.ambientIntensity / 8.0f;
            return ambientLight;
        }

        private Light3JS createLight(GameObject gameObject) {
            Light3JS light = new Light3JS();
            Light lightComponent = gameObject.GetComponent<Light>();
            light.name = gameObject.name;
            light.matrix = getMatrix(gameObject);
            light.color = Utils.getIntColor(lightComponent.color);
            light.intensity = lightComponent.intensity / 8.0f;

            // Create light of the type
            switch (lightComponent.type) {
                case UnityEngine.LightType.Directional:
                    light = new DirectionalLight3JS(light);
                    (light as DirectionalLight3JS).castShadow = ((lightComponent.shadows != LightShadows.None) && options.castShadows);
                    break;
                case UnityEngine.LightType.Point:
                    light = new PointLight3JS(light);
                    (light as PointLight3JS).distance = lightComponent.range;
                    (light as PointLight3JS).decay = 2f;
                    break;
                case UnityEngine.LightType.Spot:
                    light = new SpotLight3JS(light);
                    (light as SpotLight3JS).distance = lightComponent.range;
                    (light as SpotLight3JS).decay = 2f;
                    (light as SpotLight3JS).angle = lightComponent.spotAngle * (Mathf.PI / 180);
                    (light as SpotLight3JS).penumbra = 0.5f;
                    (light as SpotLight3JS).castShadow = ((lightComponent.shadows != LightShadows.None) && options.castShadows);
                    break;
                default:
                    Debug.Log("Unsupported light type");
                    break;
            }

            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) light.children.Add(childObj);
                }
            }
            return light;
        }

        private Group3JS createGroup(GameObject gameObject) {
            Group3JS group = new Group3JS();
            group.name = gameObject.name;
            group.matrix = getMatrix(gameObject);

            // Parse children
            if (gameObject.transform.childCount > 0) {
                foreach (Transform child in gameObject.transform) {
                    Object3JS childObj = parseGameObject(child.gameObject);
                    if (childObj != null) group.children.Add(childObj);
                }
            }
            return group;
        }

        private Camera3JS createCamera(GameObject gameObject) {
            Camera3JS camera = new Camera3JS();
            Camera cameraComponent = gameObject.GetComponent<Camera>();
            camera.name = gameObject.name;
            camera.matrix = getMatrix(gameObject);
            camera.near = cameraComponent.nearClipPlane;
            camera.far = cameraComponent.farClipPlane;

            // Create camera of desired type
            if (cameraComponent.orthographic) {
                // Orthographic
                camera = new OrthographicCamera3JS(camera);
                (camera as OrthographicCamera3JS).top = cameraComponent.rect.yMax;
                (camera as OrthographicCamera3JS).bottom = cameraComponent.rect.yMin;
                (camera as OrthographicCamera3JS).left = cameraComponent.rect.xMin;
                (camera as OrthographicCamera3JS).top = cameraComponent.rect.xMax;
            }
            else {
                // Perspective
                camera = new PerspectiveCamera3JS(camera);
                (camera as PerspectiveCamera3JS).fov = cameraComponent.fieldOfView;
                (camera as PerspectiveCamera3JS).aspect = cameraComponent.aspect;
            }
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
            geometry.metadata.vertices = mesh.vertexCount;
            geometry.metadata.normals = mesh.normals.Length;
            geometry.metadata.uvs = mesh.uv.Length;
            geometry.metadata.faces = mesh.triangles.Length;
            FaceMask code = FaceMask.TRIANGLE;
            // Vertices
            geometry.data.vertices = new float[mesh.vertexCount * 3];
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < mesh.vertexCount; i++) {
                Vector3 vertex = vertices[i];
                // (-1) * 
                geometry.data.vertices[i * 3] = vertex.x;
                geometry.data.vertices[i * 3 + 1] = vertex.y;
                geometry.data.vertices[i * 3 + 2] = (-1) * vertex.z;
            }
            // Normals
            geometry.data.normals = new float[mesh.normals.Length * 3];
            Vector3[] normals = mesh.normals;
            if (normals.Length > 0) code = code | FaceMask.VERTEX_NORMAL;
            for (int i = 0; i < normals.Length; i++) {
                Vector3 normal = normals[i];
                normal.Normalize();
                // (-1) * 
                geometry.data.normals[i * 3] = normal.x;
                geometry.data.normals[i * 3 + 1] = normal.y;
                geometry.data.normals[i * 3 + 2] = (-1) * normal.z;
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
                case 32:
                    // 32, [vertex_index, vertex_index, vertex_index],
                    // [vertex_normal, vertex_normal, vertex_normal]
                    geometry.data.faces = createFacesWithNormals(mesh);
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
                if (Utils.dictContainsValue(out uuid, materials, objMaterial)) {
                    Material3JS existingMatJS = content.materials.Find(x => (x.uuid.Equals(uuid)));
                    if (existingMatJS == null) {
                        // If we didn't find the material in main list, it has to be somewhere in multimaterials children
                        // Let's loop through them to get the desired
                        foreach (Material3JS material in content.materials) {
                            if (material is MultiMaterial3JS) {
                                existingMatJS = (material as MultiMaterial3JS).materials.Find(x => (x.uuid.Equals(uuid)));
                                if (existingMatJS != null) {
                                    // Copy the material to main list
                                    content.materials.Add(existingMatJS);
                                    break;
                                }
                            }
                        }
                    }
                }
                else {
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
                mat.EnableKeyword("_EMISSION");
                matJS.emissive = Utils.getIntColor(mat.GetColor("_EmissionColor"));
            }
            // Values
            if (mat.HasProperty("_Emission")) {
                // Standrad shader doesn't have this value in Unity 5 :(
                matJS.emissiveIntensity = mat.GetFloat("_Emission");
            }
            if (mat.HasProperty("_Shininess")) {
                matJS.shininess = mat.GetFloat("_Shininess");
            }
            // Maps
            // Main texture
            if (mat.HasProperty("_MainTex")) {
                Texture mainTexture = mat.GetTexture("_MainTex");
                if (mainTexture != null) {
                    string uuid = createTexture(mainTexture, mat);
                    if (!string.IsNullOrEmpty(uuid)) matJS.map = uuid;
                }
            }
            // Normal map
            if (mat.HasProperty("_BumpMap")) {
                Texture normalMap = mat.GetTexture("_BumpMap");
                if (normalMap != null) {
                    string uuid = createTexture(normalMap, mat);
                    if (!string.IsNullOrEmpty(uuid)) matJS.normalMap = uuid;
                }
            }
            // Emissive map
            if (mat.HasProperty("_EmissionMap")) {
                Texture emissionMap = mat.GetTexture("_EmissionMap");
                if (emissionMap != null) {
                    string uuid = createTexture(emissionMap, mat);
                    if (!string.IsNullOrEmpty(uuid)) matJS.emissiveMap = uuid;
                }
            }
            // Specualar map
            if (mat.HasProperty("_SpecGlossMap")) {
                Texture specularMap = mat.GetTexture("_SpecGlossMap");
                if (specularMap != null) {
                    string uuid = createTexture(specularMap, mat);
                    if (!string.IsNullOrEmpty(uuid)) matJS.specularMap = uuid;
                }
            }
            // Opacity and wireframe
            matJS.opacity = mat.color.a;
            matJS.transparent = (mat.color.a < 1f);
            matJS.wireframe = false;

            content.materials.Add(matJS);
            materials.Add(matJS.uuid, mat);
            return matJS.uuid;
        }

        private string createMultiMaterial(Material[] mats) {
            MultiMaterial3JS multiMatJS = new MultiMaterial3JS();
            string multName = string.Empty;
            foreach (Material mat in mats) {
                string uuid = string.Empty;
                multName += Utils.capitalizeFirstSymbol(mat.name.Substring(0, 5));
                if (Utils.dictContainsValue(out uuid, materials, mat)) {
                    // If we already had the same material, find it
                    Material3JS existingMatJS = content.materials.Find(x => (x.uuid.Equals(uuid)));
                    if (existingMatJS == null) {
                        // If we didn't find the material, it has to be somewhere in multimaterials children
                        // Let's loop through them to get the desired
                        foreach (Material3JS material in content.materials) {
                            if (material is MultiMaterial3JS) {
                                existingMatJS = (material as MultiMaterial3JS).materials.Find(x => (x.uuid.Equals(uuid)));
                                if (existingMatJS != null) break;
                            }
                        }
                    }
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
            multiMatJS.name = multName;
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
            string url = Utils.copyTexture(relativePath, options.dir);
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
                faces[pos + 2] = mesh.triangles[vertex + 2];
                faces[pos + 3] = mesh.triangles[vertex + 1];
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
                    faces[pos + 2] = subMeshTriangles[vertex + 2];
                    faces[pos + 3] = subMeshTriangles[vertex + 1];
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
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex * 2;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 2];
                faces[pos + 3] = mesh.triangles[vertex + 1];
                faces[pos + 4] = mesh.triangles[vertex];
                faces[pos + 5] = mesh.triangles[vertex + 2];
                faces[pos + 6] = mesh.triangles[vertex + 1];
            }
            return faces;
        }

        // 10, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_uv, vertex_uv, vertex_uv]
        private int[] createFacesWithMaterialsUV(Mesh mesh, int code = 10) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[2 * mesh.triangles.Length + 2 * totalTrianglesCount];
            int shift = 0;
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++) {
                int[] subMeshTriangles = mesh.GetTriangles(subMesh);
                int trianglesCount = subMeshTriangles.Length / 3;
                for (int i = 0; i < trianglesCount; i++) {
                    int vertex = i * 3;
                    int pos = shift + i + vertex + i + vertex;
                    faces[pos] = code;
                    faces[pos + 1] = subMeshTriangles[vertex];
                    faces[pos + 2] = subMeshTriangles[vertex + 2];
                    faces[pos + 3] = subMeshTriangles[vertex + 1];
                    faces[pos + 4] = subMesh;
                    faces[pos + 5] = subMeshTriangles[vertex];
                    faces[pos + 6] = subMeshTriangles[vertex + 2];
                    faces[pos + 7] = subMeshTriangles[vertex + 1];
                }
                shift += (2 * subMeshTriangles.Length + 2 * trianglesCount);
            }
            return faces;
        }

        // 32, [vertex_index, vertex_index, vertex_index],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithNormals(Mesh mesh, int code = 32) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int[] faces = new int[2 * mesh.triangles.Length + totalTrianglesCount];
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex * 2;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 2];
                faces[pos + 3] = mesh.triangles[vertex + 1];
                faces[pos + 4] = mesh.triangles[vertex];
                faces[pos + 5] = mesh.triangles[vertex + 1];
                faces[pos + 6] = mesh.triangles[vertex + 2];
            }
            return faces;
        }

        // 34, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithMaterialsNormals(Mesh mesh, int code = 34) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[2 * mesh.triangles.Length + 2 * totalTrianglesCount];
            int shift = 0;
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++) {
                int[] subMeshTriangles = mesh.GetTriangles(subMesh);
                int trianglesCount = subMeshTriangles.Length / 3;
                for (int i = 0; i < trianglesCount; i++) {
                    int vertex = i * 3;
                    int pos = shift + i + vertex + i + vertex;
                    faces[pos] = code;
                    faces[pos + 1] = subMeshTriangles[vertex];
                    faces[pos + 2] = subMeshTriangles[vertex + 2];
                    faces[pos + 3] = subMeshTriangles[vertex + 1];
                    faces[pos + 4] = subMesh;
                    faces[pos + 5] = subMeshTriangles[vertex];
                    faces[pos + 6] = subMeshTriangles[vertex + 1];
                    faces[pos + 7] = subMeshTriangles[vertex + 2];
                }
                shift += (2 * subMeshTriangles.Length + 2 * trianglesCount);
            }
            return faces;
        }

        // 40, [vertex_index, vertex_index, vertex_index],
        // [vertex_uv, vertex_uv, vertex_uv],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithUVNormals(Mesh mesh, int code = 40) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int[] faces = new int[3 * mesh.triangles.Length + totalTrianglesCount];
            for (int i = 0; i < totalTrianglesCount; i++) {
                int vertex = i * 3;
                int pos = i + vertex * 3;
                faces[pos] = code;
                faces[pos + 1] = mesh.triangles[vertex];
                faces[pos + 2] = mesh.triangles[vertex + 2];
                faces[pos + 3] = mesh.triangles[vertex + 1];
                faces[pos + 4] = mesh.triangles[vertex];
                faces[pos + 5] = mesh.triangles[vertex + 2];
                faces[pos + 6] = mesh.triangles[vertex + 1];
                faces[pos + 7] = mesh.triangles[vertex];
                faces[pos + 8] = mesh.triangles[vertex + 1];
                faces[pos + 9] = mesh.triangles[vertex + 2];
            }
            return faces;
        }

        // 42, [vertex_index, vertex_index, vertex_index],
        // [material_index],
        // [vertex_uv, vertex_uv, vertex_uv],
        // [vertex_normal, vertex_normal, vertex_normal]
        private int[] createFacesWithMaterialsUVNormals(Mesh mesh, int code = 42) {
            int totalTrianglesCount = mesh.triangles.Length / 3;
            int subMeshCount = mesh.subMeshCount;
            int[] faces = new int[3 * mesh.triangles.Length + 2 * totalTrianglesCount];
            int shift = 0;
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++) {
                int[] subMeshTriangles = mesh.GetTriangles(subMesh);
                int trianglesCount = subMeshTriangles.Length / 3;
                for (int i = 0; i < trianglesCount; i++) {
                    int vertex = i * 3;
                    int pos = shift + i + vertex + i + vertex;
                    faces[pos] = code;
                    faces[pos + 1] = subMeshTriangles[vertex];
                    faces[pos + 2] = subMeshTriangles[vertex + 2];
                    faces[pos + 3] = subMeshTriangles[vertex + 1];
                    faces[pos + 4] = subMesh;
                    faces[pos + 5] = subMeshTriangles[vertex];
                    faces[pos + 6] = subMeshTriangles[vertex + 2];
                    faces[pos + 7] = subMeshTriangles[vertex + 1];
                    faces[pos + 8] = subMeshTriangles[vertex];
                    faces[pos + 7] = subMeshTriangles[vertex + 2];
                    faces[pos + 9] = subMeshTriangles[vertex + 1];
                }
                shift += (3 * subMeshTriangles.Length + 2 * trianglesCount);
            }
            return faces;
        }

        private float[] getMatrix(GameObject gameObject) {
            Vector3 unityPosition = gameObject.transform.localPosition;
            Quaternion unityQuartenion = gameObject.transform.localRotation;
            Vector3 unityScale = gameObject.transform.lossyScale;
            Matrix4x4 unityMatrix = Matrix4x4.TRS(unityPosition, unityQuartenion, unityScale);
            return Utils.getMatrixAsArray(unityMatrix);
        }
    }
}
