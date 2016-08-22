using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Export3JS.Model {
    public class Format4 {

        public Metadata3JS metadata;
        public List<Geometry3JS> geometries;
        public List<Material3JS> materials;
        public List<Texture3JS> textures;
        public List<Image3JS> images;
        public Object3JS @object;

        public Format4() {
            metadata = new Metadata3JS();
            geometries = new List<Geometry3JS>();
            materials = new List<Material3JS>();
            textures = new List<Texture3JS>();
            images = new List<Image3JS>();
            @object = new Object3JS();
        }
    }

    public class TagsFormat {
        public Metadata3JS metadata;
        public Dictionary<string, List<string>> tags;

        public TagsFormat() {
            metadata = new Metadata3JS();
            tags = new Dictionary<string, List<string>>();
        }
    }

    public class LightsConfig {
        public Metadata3JS metadata;
        public Dictionary<string, float[]> spotlights;

        public LightsConfig() {
            metadata = new Metadata3JS();
            spotlights = new Dictionary<string, float[]>();
        }

        public bool isEmpty() {
            if (spotlights.Count > 0) return false;
            else return true;
        }
    }
}
