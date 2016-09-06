using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Export3JS {

    [Flags]
    public enum FaceMask {
        TRIANGLE = 0,
        FACE_MATERIAL = 2,
        FACE_VERTEX_UV = 8,
        VERTEX_NORMAL = 32
    }

    public static class Utils {

        // ThreeJS parses in column-major format, as well as Unity
        public static float[] getMatrixAsArray(Matrix4x4 input) {
            float[] output = new float[16];
            for (int row = 0; row < 4; row++) {
                for (int column = 0; column < 4; column++) {
                    output[row + column * 4] = input[row + column * 4];
                }   
            }
            // ThreeJS uses right-handed coordinate system, 
            // while Unity left-handed system, apply convertion
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

        public static bool arraryContainsValue<T>(T[] array, T value) {
            bool contains = false;
            foreach (T element in array) {
                if (element.Equals(value)) {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        public static string capitalizeFirstSymbol(string str) {
            if (!string.IsNullOrEmpty(str)) {
                str = str.ToLower();
                return (char.ToUpper(str[0]) + str.Substring(1));
            }
            else return string.Empty;
        }

        public static int getIntColor(Color inputColor) {
            Color32 color = inputColor;
            int output = (color.r << 16) | (color.g << 8) | (color.b);
            return output;
        }

        public static bool isFormatSupported(string assetPath) {
            Regex pattern = new Regex(@"^*\.(?:png|jpg|gif|dds)$", RegexOptions.IgnoreCase);
            string filename = Path.GetFileName(assetPath);
            Match check = pattern.Match(filename);
            if (check.Success) {
                Debug.Log(filename + " matches pattern");
                return true;
            }
            else {
                Debug.Log(filename + " doesn't match pattern");
                return false;
            }
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

		public static string writeTextureAsPNG(Texture sourceTexture, string assetPath, string destination) {
			string texturesDir = destination + "textures";
            // The original file extension is not removed in order to prevent conflicts if there are textures with the
            // same name but different file extension.
			string filename = Path.GetFileName(assetPath) + ".png";
			Directory.CreateDirectory(texturesDir);
			string url = "textures/" + filename;

			if (!File.Exists(destination + url)) {
				Texture2D texture = (Texture2D) sourceTexture;
				Color[] colors;
				try {
					colors = texture.GetPixels();
				} catch( UnityException e ) {
					Debug.LogError("Source texture is not readable. You have to mark the texture as readable from the" +
						" texture import settings: " + e.ToString());
					return null;
				}
				Texture2D convertedTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
				convertedTexture.SetPixels(colors);
				byte[] bytes = convertedTexture.EncodeToPNG();
				FileStream f = File.Create(destination + url);
				try {
					f.Write(bytes, 0, bytes.Length);
					f.Close();
				} catch(IOException exception) {
					Debug.Log("Error while writing PNG texture: " + exception.Message);
					url = "";
				}
			}
			return url;
		}
    }
}
