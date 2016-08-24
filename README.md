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
Place _Export3JS_ folder in your Unity project assets folder, and corresponding menu item must appear in Unity's menu panel.

# Usage
In the exportation window select options you need and choose the folder for output file. 

In **Tags** submenu you can choose tags you wanted to trace. Then exporter will also create _{SceneName}Tags.json_ file in the output folder with following format:
```javascript
tags: {
 ExampleTag: [
  "THREE Object UUID"
 ],
 ...
}
```
In your Three.JS code then read this file, find objects by their uuids and do whatever you need.

If scene contains spotlights, exporter will create another .json file in output directory, named _{SceneName}LightsConfig.json_. Three.JS spotlights doesn't use their rotation and need a target object to be able to shine in specific direction. The format of this helper file is following:
```javascript
spotlights: {
 "Light Object UUID": [
  x,
  y,
  z
 ],
 ...
}
```
For each spotlight in scene there are coordinates of abstract target object, that you need to create manually in your Three.JS code.

After job is done, load JSON file with ObjectLoader:
```javascript
var loader = new THREE.ObjectLoader();
loader.load('assets/output.json',
  function onLoad(object) {
    scene.add(object);
    // Or even like this, because exporter exports into ready for use THREE.Scene
    // scene = object;
    console.log('Scene has loaded, yeah!');
  }
);
```

