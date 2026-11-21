# Sources

[Back to the README](../README.md)

## Start point

Unity's Universal 3D project template (`com.unity.template.urp-blank` 17.0.14), bundled with Unity
6000.3.28f1. I added the AR rig with AR Foundation's "XR Origin (Mobile AR)" and "AR Session" menu items.

## Course

- [Unity Essentials pathway](https://learn.unity.com/pathway/unity-essentials), Unity Learn.

## Documentation

- Unity manual:
  - [XR Origin](https://docs.unity.com/en-us/engine/6000.0/manual/xr/configuring-project-for/scene-setup/origin)
    (the 6000.0 page)
  - for 6000.3, [Create an XR project, World Space UI, Assemblies, ScriptableObject](https://docs.unity3d.com/6000.3/Documentation/Manual/).
- [AR Foundation 6.3 manual](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/), including
  XR Simulation, and its source bundled with the Editor.
- [Inference Engine 2.6 manual](https://docs.unity3d.com/Packages/com.unity.ai.inference@2.6/manual/)
  (`com.unity.ai.inference`).
- The manuals for Input System 1.20, Unity UI and TextMesh Pro, and Test Framework 1.6, bundled with the Editor.
- Unity scripting reference, [AsyncGPUReadbackRequest](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/rendering/asyncgpureadbackrequest):
  a finished readback can be read for one frame only.
- Unity blog, [Get started developing for Quest 3 with Unity](https://unity.com/blog/engine-platform/get-started-developing-for-quest-3-with-unity),
  read for the overall workflow only.

## Tutorial videos

| Video | Channel |
|---|---|
| [Tap to Place Objects in Unity's AR Foundation](https://www.youtube.com/watch?v=lYDfV-GaKQA) | samyam |
| [Setup a Unity 6 AR foundation project in 3 minutes](https://www.youtube.com/watch?v=hbBqu_LgAis) | Alive Studios |
| [How to Configure XR Simulation in Unity for Augmented Reality (2024)](https://www.youtube.com/watch?v=NCIPFP_dTyc) | Joed |
| [Unity 6: Place Objects in AR with Plane Detection](https://www.youtube.com/watch?v=e6WB3GVcF3E) | Alive Studios |
| [Unity Input System in Unity 6 (1/7): Input Action Editor](https://www.youtube.com/watch?v=TiTKAseu17A) | Unity |
| [Place UI Elements in your World!](https://www.youtube.com/watch?v=Zwgj3mwOVlg) | Code Monkey |
| [Billboarding in Unity](https://www.youtube.com/watch?v=eiGvVgwtJ8k) | Firnox |
| [How to convert the mouse position to world space in Unity](https://www.youtube.com/watch?v=5NTmxDSKj-Q) | Game Dev Beginner |
| [Screen/World Point in Unity](https://www.youtube.com/watch?v=CRbd2A8u4Uo) | { j }code |
| [Lerp smoothing is broken](https://www.youtube.com/watch?v=LSNQuFEDOyQ) | Freya Holmér |
| [Unity 6 automated tests – Edit Mode and Play mode](https://www.youtube.com/watch?v=5G1pPvEf15k) | Raz Men |
| [Unity Profiler Walkthrough & Tutorial](https://www.youtube.com/watch?v=xjsqv8nj0cw) | Unity |
| [Unity Sentis and Meta Quest Camera Access](https://www.youtube.com/watch?v=V14cYfLa8Rc) | Valem Tutorials |

## Repositories and samples read as reference

- [Unity-Technologies/arfoundation-samples](https://github.com/Unity-Technologies/arfoundation-samples/tree/6.3),
  branch 6.3: placing on planes, reading the camera picture.
- [Unity's YOLO page and `RunYOLO.cs`](https://huggingface.co/unity/inference-engine-yolo): a YOLO model in the
  Inference Engine. Not used in the app.
- Meta's Multi Object Detection sample, in
  [Unity-PassthroughCameraApiSamples](https://github.com/oculus-samples/Unity-PassthroughCameraApiSamples)
  ([overview](https://developers.meta.com/horizon/documentation/unity/unity-sample-camera-object-detection)).
  The idea in its `DetectionManager.cs`: a found object whose box holds a kept marker of its class is the same
  object. Used as an idea in `LabelKeeper.cs`, credited there.
- [Megvii-BaseDetection/YOLOX](https://github.com/Megvii-BaseDetection/YOLOX): source of the class list text.
- [MultimediaTechLab/YOLO](https://github.com/MultimediaTechLab/YOLO) and
  [LibreYOLO](https://huggingface.co/LibreYOLO/LibreYOLO9t): source of the model.
- [D-FINE](https://github.com/Peterande/D-FINE) and [DEIM](https://github.com/ShihuaHuang95/DEIM): two larger
  detectors tried and not used.
- [github/gitignore](https://github.com/github/gitignore): the Unity ignore file (CC0).

## Papers

- Davari, Bowman. Towards Context-Aware Adaptation in XR: A Design Space for XR Interfaces and an Adaptive
  Placement Strategy. [arXiv 2411.02607](https://arxiv.org/abs/2411.02607)
- Davari, Stover, Giovannelli, Ilo, Bowman. Towards Intelligent AR (iAR). [arXiv 2411.02684](https://arxiv.org/abs/2411.02684)
- Davari, Lu, Bowman. Validating the Benefits of Glanceable and Context-Aware Augmented Reality for Everyday
  Information Access Tasks. IEEE VR 2022. [doi:10.1109/VR51125.2022.00063](https://doi.org/10.1109/VR51125.2022.00063)
- Davari, Ajikumar, Moghaddam. Context-Aware Augmented Reality for Human-Robot Collaboration. ISMAR-Adjunct
  2025. [doi:10.1109/ISMAR-Adjunct68609.2025.00087](https://doi.org/10.1109/ISMAR-Adjunct68609.2025.00087)
- Davari, Lu, Bowman. Occlusion Management Techniques for Everyday Glanceable AR Interfaces. IEEE VRW 2020.
  [doi:10.1109/VRW50115.2020.00072](https://doi.org/10.1109/VRW50115.2020.00072)
- Davari-Najafabadi. Intelligent Augmented Reality (iAR). PhD dissertation, Virginia Tech, 2024.
  [hdl:10919/121123](https://hdl.handle.net/10919/121123)
- Bell, Feiner, Höllerer. View Management for Virtual and Augmented Reality. UIST 2001.
  [PDF](https://sites.cs.ucsb.edu/~holl/pubs/bell-2001-uist.pdf)
- Azuma, Furmanski. Evaluating Label Placement for Augmented Reality View Management. ISMAR 2003.
  [doi:10.1109/ISMAR.2003.1240689](https://doi.org/10.1109/ISMAR.2003.1240689)
- Gabbard, Swan, Hix. The Effects of Text Drawing Styles, Background Textures, and Natural Lighting on Text
  Legibility in Outdoor Augmented Reality. Presence 2006. [hdl:10919/25504](https://hdl.handle.net/10919/25504)
- Madsen et al. Temporal Coherence Strategies for Augmented Reality Labeling. TVCG 2016 (abstract only).
  [doi:10.1109/TVCG.2016.2518318](https://doi.org/10.1109/TVCG.2016.2518318)

## Design guides

- [Apple Human Interface Guidelines, augmented reality](https://developer.apple.com/design/human-interface-guidelines/augmented-reality)
- [Google ARCore design guidelines](https://developers.google.com/ar/design)
- [Microsoft Mixed Reality design guidance, typography](https://learn.microsoft.com/en-us/windows/mixed-reality/design/typography)
- [Meta mixed reality design guidelines](https://developers.meta.com/horizon/design/mr-design-guideline/)

## Pretrained model

YOLOv9-T trained on COCO, from MultimediaTechLab/YOLO (release v1.0-alpha), as the ONNX file published by
LibreYOLO, MIT licence. Details: [MODEL-CHOICE.md](MODEL-CHOICE.md) and `THIRD-PARTY-NOTICES.md`.

## 3D models, textures and the font

| Item | Author | Source | Licence |
|---|---|---|---|
| Potted Plant 01 | Rico Cilliers | [Poly Haven](https://polyhaven.com/a/potted_plant_01) | CC0 |
| Ceramic Vase 01 | James Ray Cock | [Poly Haven](https://polyhaven.com/a/ceramic_vase_01) | CC0 |
| Dining Chair 02 | James Ray Cock | [Poly Haven](https://polyhaven.com/a/dining_chair_02) | CC0 |
| Classic Laptop | Arrangemonk | [Poly Haven](https://polyhaven.com/a/classic_laptop) | CC0 |
| Stationery Supplies | Mateusz Sadek | [Poly Haven](https://polyhaven.com/a/stationery_supplies) | CC0 |
| Wall Clock | PierreB3D | [Poly Haven](https://polyhaven.com/a/wall_clock) | CC0 |
| Multi Cleaner Bottle | James Ray Cock | [Poly Haven](https://polyhaven.com/a/multi_cleaner_bottle) | CC0 |
| School Chair 01 | Ethan Place | [Poly Haven](https://polyhaven.com/a/SchoolChair_01) | CC0 |
| Laminate Floor 02 (texture) | Dario Barresi, Charlotte Baglioni | [Poly Haven](https://polyhaven.com/a/laminate_floor_02) | CC0 |
| White Plaster 02 (texture) | Rob Tuytel | [Poly Haven](https://polyhaven.com/a/white_plaster_02) | CC0 |
| Small Empty Room 1 (reflections) | Sergej Majboroda | [Poly Haven](https://polyhaven.com/a/small_empty_room_1) | CC0 |
| Monitor Dell P2314H | GregorJan | [Sketchfab](https://sketchfab.com/3d-models/monitor-dell-p2314h-c69f09bb812e46cdbd5bf6c62499b848) | CC BY 4.0 |
| Mouse and Keyboard | gorzi | [Sketchfab](https://sketchfab.com/3d-models/mouse-and-keyboard-c63ee81b71ff45b68a778101b4df3d46) | CC BY 4.0 |
| Liberation Sans | — | TextMesh Pro Essential Resources (`com.unity.ugui` 2.0.0) | SIL OFL 1.1 |

The full licence texts are in `THIRD-PARTY-NOTICES.md`.

## What I reused, and what I made

**Reused unchanged:**
- TextMesh Pro's font, shaders, style sheet and line-breaking lists
- the Poly Haven models and textures
- the YOLOv9-T file
- the COCO class list text

Unity's packages are not in the repository.

**Reused and changed:**
- Unity's template settings: AR Foundation's background renderer feature added to both renderers, and Unity's
  own upgrades of the render pipeline settings.
- TextMesh Pro's settings: the default sprite asset set to none, after its unused emoji sheet was removed.
- The `.gitignore`, from GitHub's `Unity.gitignore`, with two lines added to keep out Unity's sample rooms.
- The monitor model: converted to FBX, its parts joined into one mesh, scaled to real size, with a new screen
  picture.
- The keyboard and mouse model: converted to FBX, split into two files, materials merged, scaled to real
  size, fewer faces.
- The plant's leaf texture: its alpha joined into one file.

**Made in this project:**
- all code in `Assets/Scripts` and the tests in `Assets/Tests` (the 2D-to-3D placement, the label rules, the
  cube controls, the three detection sources and the model runner)
- the scene, prefabs and settings asset
- the test room (layout, furniture built from Unity shapes, materials, placement of the props)
- the simulator controls file
- the display names
- the monitor's screen picture
- the pictures, charts and recording in `docs/images`, and the video linked from the README

**Made by Unity:** `Assets/XR/`, the XR Plug-in Management and XR Simulation settings.

[Back to the README](../README.md)
