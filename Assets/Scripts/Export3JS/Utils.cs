using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace Export3JS {

    [Flags]
    public enum FaceMask {
        TRIANGLE = 0,
        FACE_MATERIAL = 2,
        FACE_VERTEX_UV = 8,
        VERTEX_NORMAL = 32
    }

    public static class Utils {
        
        public static float[] getMatrixAsArray(Matrix4x4 input) {
            // ThreeJS parses in column-major format
            float[] output = new float[16];
            for (int row = 0; row < 4; row++) {
                for (int column = 0; column < 4; column++) {
                    output[row + column * 4] = input[row + column * 4];
                }   
            }
            // ThreeJS uses right-handed coordinate system
            // Thus iinverting some values
            output[2] = -1 * output[2];
            output[6] = -1 * output[6];
            output[8] = -1 * output[8];
            output[9] = -1 * output[9];
            output[14] = -1 * output[14];
            return output;
        }

        public static bool dictContainsValue<T>(out string uuid, Dictionary<string, T> dict, T value) {
            uuid = string.Empty;
            foreach (KeyValuePair<string, T> pair in dict) {
                if (pair.Value.Equals(value)) {
                    uuid = pair.Key;
                    return true;
                }
            }
            return false;
        }

        public static bool dictContainsArray<T>(out string uuid, Dictionary<string, T[]> dict, T[] array) {
            uuid = string.Empty;
            foreach (KeyValuePair<string, T[]> pair in dict) {
                bool equals = false;
                if (pair.Value.Length != array.Length) continue;
                for (int i = 0; i < pair.Value.Length; i++) {
                    if (pair.Value[i].Equals(array[i])) {
                        equals = true;
                    }
                    else {
                        equals = false;
                        break;
                    }
                }
                if (equals) {
                    uuid = pair.Key;
                    return true;
                }
            }
            return false;
        }

        public static int getIntColor(Color inputColor) {
            Color32 color = inputColor;
            int output = (color.r << 16) | (color.g << 8) | (color.b);
            return output;
        }

        public static string copyTexture(string assetPath, string destination) {
            string projectPath = Directory.GetCurrentDirectory() + '/';
            string texturesDir = destination + "textures";
            string filename = Path.GetFileName(assetPath);
            Directory.CreateDirectory(texturesDir);
            string url = "textures/" + filename;
            if (!File.Exists(destination + url)) {
                try {
                    File.Copy(projectPath + assetPath, destination + url);
                }
                catch (IOException exception) {
                    Debug.Log("Error while copying texture: " + exception.Message);
                    url = "";
                }
            }
            return url;
        }
    }
}
