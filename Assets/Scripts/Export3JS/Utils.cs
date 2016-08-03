using UnityEngine;
using System.Collections;
using System.IO;

namespace Export3JS {

    public static class Utils {
        
        public static float[] getMatrixAsArray(Matrix4x4 input) {
            // ThreeJS parses in column-major format
            float[] output = new float[16];
            for (int column = 0; column < 4; column++) {
                for (int row = 0; row < 4; row++) {
                    output[column * 4 + row] = input[row, column];
                }   
            }
            return output;
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
