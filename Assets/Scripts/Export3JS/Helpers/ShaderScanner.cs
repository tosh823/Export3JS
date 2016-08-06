using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace Export3JS.Helpers {

    [ExecuteInEditMode]
    public class ShaderScanner : MonoBehaviour {

        void OnEnable() {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null) return;
            foreach (Material sharedMaterial in renderer.sharedMaterials) {
                Shader shader = sharedMaterial.shader;
                Debug.Log("Scanning " + sharedMaterial.name + " : " + shader.name);
                List<string> props = new List<string>();
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++) {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, i);
                    props.Add(propertyName);
                    Debug.Log(propertyName + " of type " + propertyType.ToString());
                }
                //Debug.Log(string.Join("\n", props.ToArray()));
            }
        }
    }
}

#endif
