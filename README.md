# About
You want export your static scene from Unity to Three.JS? Well, here you go, this package could export some stuff.

# Supported types
* Transforms hierarchy
* Mesh geometries
* Lights
  * Ambient Light
  * Directional Lights
  * Point Lights
  * Spot Lights
* Materials
  * Basic materials
  * Multimaterials
  * Textures, normal maps, specualar maps, emmissive maps
  * Opacity
* Cameras
  * Perspective Camera
  * Orthographic Camera

!Warning: not everything is tested, but should work. Development is in progress.

# Installation
Just place _Export3JS_ folder in your Unity project assets folder, and corresponding menu item must appear in Unity's menu panel.

# Usage
In the exportation window select options you need and choose the folder for output file. After job is done, load JSON file with ObjectLoader:
```javascript
var loader = new THREE.ObjectLoader();
loader.load('assets/output.json',
  function onLoad(object) {
    scene.add(object);
    console.log('Scene has loaded, yeah!');
  }
);
```

