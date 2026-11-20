# AR Object Labels

A small Unity AR app that runs in the Editor with XR Simulation. You place a cube on a found surface and
move it, and objects in a simulated lab room get a short name label ("Laptop", "Chair", "Pen") that stays
at the object in the room as you walk and look around. The labels come from one of four detection sources,
the last one being a small pretrained detector run inside Unity.

Everything here runs in the Unity Editor on a laptop. No headset, phone or paid software is needed.

## Open and run

1. Install Unity **6000.3.28f1** with Unity Hub.
2. In Unity Hub, choose **Add project from disk** and pick this folder.
3. Open `Assets/Scenes/Main.unity`.
4. Press Play. XR Simulation is already turned on for the desktop platform, and the test room
   (`Assets/Environments/LabRoom.prefab`) is already chosen as the simulation room.

The package versions are pinned in `Packages/manifest.json`. The main ones:

| Package | Version |
|---|---|
| AR Foundation (`com.unity.xr.arfoundation`) | 6.3.5 |
| Inference Engine (`com.unity.ai.inference`) | 2.6.1 |
| Input System | 1.20.1 |
| Universal Render Pipeline | 17.3.0 |
| Test Framework | 1.6.0 |

One information line in the Console at Play start is expected:
`InputActionReference(Unlock) not provided. Navigation Actions will be active by default.`

## Controls

| Action | How |
|---|---|
| Move | W A S D; Q and E for down and up; hold Shift to go faster |
| Look | hold the right mouse button and move the mouse |
| Place the cube | click a found surface on the table or the floor |
| Move the cube | drag it, or click it to pick it up, move the view, and click a surface to put it down |
| Remove the cube | R, or the Reset button |
| Change the detection source | click the Detection button |
| Show or hide the controls | H |

The simulator only finds surfaces while the camera moves, so walk and look around for a few seconds first.
Found surfaces are drawn faint, and in full while the cube is moved or carried. A hint line at the bottom
says what to do next.

## The four detection sources

The Detection button goes through them in this order. The app starts with the model.

| Source | What it gives |
|---|---|
| model | A YOLOv9-T detector (COCO's 80 classes) runs on the camera picture inside Unity, 10 times a second. A label shows once an object is found in 3 runs in a row with a score of at least 0.40. |
| off | No labels. |
| fixed box (laptop only) | One 2D box typed by hand for the laptop in the starting view. This is the brief's first step, a hard-coded box. |
| true box | Boxes made from the simulator's own knowledge of where each marked object is, drawn as 2D boxes of the picture. This stands in for a perfect detector and is the only source that labels the pens. |

All four give the same thing to the rest of the app: 2D boxes in a camera picture, each with a class name,
a score and the camera pose when the picture was taken. So the 2D-to-3D step and the labels are the same
for all four.

## From a 2D box to a label in the room

1. A ray is cast from the camera pose of that picture through the middle of the box's lower edge
   (`DetectionPlacer.cs`).
2. The ray meets the found surface the object stands on. If it meets a small upright surface first, such as
   a table's edge, the table top just behind it is used (`SurfaceChoice.cs`).
3. The ray meets the object's front foot, so the point is moved back by half the object's depth to under its
   middle. The width and height come from the box size at that distance; the depth comes from a short table
   of usual depths per class, or the box width (`BoxPoint.cs`, `ObjectDepth.cs`).
4. The result is a box in the room. The label is a world-space object placed just above that box's top,
   turned to face the camera, with text the same height on screen at every distance (`WorldLabel.cs`).
5. Each label is kept at its room point (`LabelKeeper.cs`). When its object is not found it is drawn at half
   strength; after 10 s without being found it is dropped.
6. Once a frame the labels are laid out together (`LabelLayout.cs`, `LabelArrange.cs`): at most 6 are shown,
   biggest object first; a label that would sit on another label or object, under a panel, or off the
   picture is moved to a free place nearby. A line joins a label to its object when the two are apart, and
   is hidden when it would be longer than two label heights. While the camera moves, the labels keep their
   places.

Labels are world-space objects, not a 2D overlay drawn on the screen: they stay at their object in the room
as the view changes, and only the layout step moves them aside a little in the picture.

## Test results

All numbers are from the Unity Editor on an Apple M1 Pro. An Editor number is not a device number.

**Edit mode tests:** 394 tests, all pass (Test Framework, Edit mode). They cover the rules in
`Assets/Scripts/Core` (placing, the cube's press and drag rules, hints, label layout, label keeping, the
model's output) and sit in `Assets/Tests/EditMode`.

**Play check, 20 Nov 2026.** One scripted set of views (stills, walks, pans and side steps in all four
modes), Game view 993 x 543, taken before the last fixes (922 pictures) and after them (925 pictures), and
counted by the same script:

| Measure | before | now |
|---|---|---|
| Labels cut by the picture's edge | 82 | 43 |
| Found objects' labels wholly off the picture | 4 | 0 |
| Labels overlapping another label | 3 | 9 |
| Label jumps over 40 px in motion sets | 172 | 142 |
| Labels shown for objects not found at that moment | 272 | 203 |
| Same name twice, one not found | 9 | 22 |
| Labels at least a quarter under a panel | 3 | 2 |

The rise in "same name twice" was checked in the pictures: they are two real objects with one name (two
chairs, several pens), one of them faded, not old labels. The rise in overlaps is listed under the limits.

**Three screen sizes,** the same views (stills and pans in all four modes, panels not counted):

| Size | Pictures | Labels | Cut by the edge | Overlaps | Wholly off | Smallest letter height |
|---|---|---|---|---|---|---|
| 993 x 543 | 144 | 288 | 11 | 4 | 0 | 12.3 px |
| 1280 x 720 | 144 | 270 | 11 | 2 | 0 | 16.4 px |
| 800 x 1200 portrait | 143 | 172 | 16 | 4 | 0 | 27.3 px |

In portrait the panels and the hint fit and labels stay readable; fewer objects fit across, so more labels are
cut at the side edges in pans.

**Detector speed:** one YOLOv9-T run at 640 x 640 takes 26.8 ms on the graphics card and about 40 ms on the
CPU. The run is spread over frames (40 layers a frame), and the Editor kept 269 frames a second on average
while detecting, against 354 with detection off.

**Choosing the detector (18 Nov 2026).** Seven small detector set-ups on 315 pictures of the test room (45
pictures, each also made dark, overexposed, blurred, small and far, half-covered and grainy), with 1,404
visible objects the models can name. Times are one run in Unity's Inference Engine 2.6.1 on the same Mac.

| Model and picture size | Unity GPU ms | Unity CPU ms | Found, at most 25 false | Found, at most 50 false | File |
|---|---|---|---|---|---|
| YOLOX-Nano 416 | about 6 | 12 | 329 | 348 | 4 MB |
| YOLOX-Tiny 416 | 14.6 | 27 | 358 | 424 | 20 MB |
| YOLOv9-T 416 | 18.4 | about 31 | 366 | 419 | 8 MB |
| YOLOX-Tiny 640 | 24.1 | 55 | 483 | 543 | 20 MB |
| PicoDet-L 416 | about 26 | 46 | 480 | 518 | 23 MB |
| **YOLOv9-T 640 (chosen)** | **26.8** | **about 40** | **505** | **533** | **8 MB** |
| YOLOX-S 640 | 32.3 | 90 | 577 | 624 | 36 MB |

A first test was redone to be fair: every score is now kept and the models are compared at the same number of
false labels, YOLOX-Tiny and YOLOv9-T are each run at both picture sizes, and every model is timed in Unity.
YOLOv9-T at 640 found more objects than the two models closest to it in speed, with a file less than half
their size; only YOLOX-S found more, and it is larger and slower. It is also MIT-licensed.

A manual check you can do in about 10 minutes is in `MANUAL-TEST.md`.

## Known limits

- Everything runs in XR Simulation in the Editor. It does not replace tests on a phone: camera access,
  real camera pictures, the model's speed on a phone, tracking loss, screen notches and the picture's turn on
  a phone are not tested. No device build exists.
- An object's depth is not measured. It comes from a usual depth per class or the box width, so a room box
  can be too deep or too high.
- In true box mode a "Chair" label can sit by the laptop, about 0.7 m from the camera, while the school chair
  is about 1.1 m away. The cause is not found.
- In model mode the school chair's label sits at the work table's edge, not on the chair.
- When no near place is free, a label can sit away from its object with its line hidden. It stays inside the
  picture.
- "Keyboard" can overlap "Screen" at the desk, and labels crowd over the pen cup.
- A click on the table behind the school chair puts the cube on the chair's seat. A click on the table where
  no table surface has been found yet can put the cube on the floor under the table.
- The model calls the laptop's and the monitor's screens "Keyboard".
- The model labels no cup and no pen in the test room: COCO has no pen class, and the model missed the cup
  except from 0.5 m. True box mode shows the pen case.
- Several objects of one kind close together each get their own label ("Pen" three times); they are not
  merged.
- A double click places the cube and picks it up at once.
- With a mouse, dragging outside the Game view can move the cube out of sight.
- The cube and labels use no AR anchors. After a session reset or a tracking change on a phone they would
  likely be left in the wrong place.

## Privacy

The camera picture is used in memory for the detector and is never saved or sent. The app has no network
code, and Unity's analytics, crash reporting, ads and performance reporting are off.

## Attribution

**Start point.** Unity's Universal 3D project template (`com.unity.template.urp-blank` 17.0.14), bundled with
Unity 6000.3.28f1. The AR rig was added with AR Foundation's "XR Origin (Mobile AR)" and "AR Session" items.

**Documentation followed**
- Unity manual: XR Origin (the 6000.0 page,
  https://docs.unity.com/en-us/engine/6000.0/manual/xr/configuring-project-for/scene-setup/origin), and for
  6000.3 Create an XR project, World Space UI, Assemblies, ScriptableObject
  (https://docs.unity3d.com/6000.3/Documentation/Manual/).
- AR Foundation 6.3 manual, including XR Simulation
  (https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/).
- Inference Engine (`com.unity.ai.inference`) 2.6 manual
  (https://docs.unity3d.com/Packages/com.unity.ai.inference@2.6/manual/).
- The Input System 1.20, Unity UI and TextMesh Pro, and Test Framework 1.6 manuals bundled with the Editor.

**Tutorial videos used for learning**
- "Tap to Place Objects in Unity's AR Foundation", samyam (https://www.youtube.com/watch?v=lYDfV-GaKQA)
- "Setup a Unity 6 AR foundation project in 3 minutes", Alive Studios (https://www.youtube.com/watch?v=hbBqu_LgAis)
- "How to Configure XR Simulation in Unity for Augmented Reality (2024)", Joed (https://www.youtube.com/watch?v=NCIPFP_dTyc)
- "Unity 6: Place Objects in AR with Plane Detection", Alive Studios (https://www.youtube.com/watch?v=e6WB3GVcF3E)
- "Unity Input System in Unity 6 (1/7): Input Action Editor", Unity (https://www.youtube.com/watch?v=TiTKAseu17A)
- "Place UI Elements in your World!", Code Monkey (https://www.youtube.com/watch?v=Zwgj3mwOVlg)
- "Billboarding in Unity", Firnox (https://www.youtube.com/watch?v=eiGvVgwtJ8k)
- "How to convert the mouse position to world space in Unity", Game Dev Beginner (https://www.youtube.com/watch?v=5NTmxDSKj-Q)
- "Unity 6 automated tests – Edit Mode and Play mode", Raz Men (https://www.youtube.com/watch?v=5G1pPvEf15k)
- "Unity Profiler Walkthrough & Tutorial", Unity (https://www.youtube.com/watch?v=xjsqv8nj0cw)
- "Unity Sentis and Meta Quest Camera Access", Valem Tutorials (https://www.youtube.com/watch?v=V14cYfLa8Rc)

**Repositories read as reference**
- Unity-Technologies/arfoundation-samples, branch 6.3: placing on planes, reading the camera picture
  (https://github.com/Unity-Technologies/arfoundation-samples/tree/6.3).
- Unity's YOLO page and `RunYOLO.cs`, showing a YOLO model in the Inference Engine
  (https://huggingface.co/unity/inference-engine-yolo). Not used in the app.
- Megvii-BaseDetection/YOLOX (https://github.com/Megvii-BaseDetection/YOLOX): source of the class list text.
- MultimediaTechLab/YOLO (https://github.com/MultimediaTechLab/YOLO) and LibreYOLO
  (https://huggingface.co/LibreYOLO/LibreYOLO9t): source of the model.
- Peterande/D-FINE and ShihuaHuang95/DEIM: two larger detectors tried and not used.
- github/gitignore: the Unity ignore file (https://github.com/github/gitignore, CC0).

**Papers and design guides**
- Davari, Bowman. Towards Context-Aware Adaptation in XR: A Design Space for XR Interfaces and an Adaptive
  Placement Strategy. arXiv 2411.02607.
- Davari, Stover, Giovannelli, Ilo, Bowman. Towards Intelligent AR (iAR). arXiv 2411.02684.
- Davari, Lu, Bowman. Validating the Benefits of Glanceable and Context-Aware Augmented Reality for Everyday
  Information Access Tasks. IEEE VR 2022.
- Davari, Ajikumar, Moghaddam. Context-Aware Augmented Reality for Human-Robot Collaboration. ISMAR-Adjunct
  2025.
- Davari, Lu, Bowman. Occlusion Management Techniques for Everyday Glanceable AR Interfaces. IEEE VRW 2020.
- Davari-Najafabadi. Intelligent Augmented Reality (iAR). PhD dissertation, Virginia Tech, 2024.
- Bell, Feiner, Höllerer. View Management for Virtual and Augmented Reality. UIST 2001.
- Azuma, Furmanski. Evaluating Label Placement for Augmented Reality View Management. ISMAR 2003.
- Madsen et al. Temporal Coherence Strategies for Augmented Reality Labeling. TVCG 2016 (abstract).
- Gabbard, Swan, Hix. Text legibility in outdoor AR. Presence 2006 (abstract).
- Apple Human Interface Guidelines (augmented reality), Google ARCore design guidelines, Microsoft Mixed
  Reality design guidance, Meta mixed reality design guidelines.

**Pretrained model.** YOLOv9-T trained on COCO, from MultimediaTechLab/YOLO (release v1.0-alpha), as the ONNX
file published by LibreYOLO, MIT licence. Details in `THIRD-PARTY-NOTICES.md`.

**What was reused, and what was made in this project**
- Reused as they are: Unity's template settings files; TextMesh Pro's essential resources (font, shaders,
  settings); the Poly Haven models and textures; the YOLOv9-T file; the COCO class list text. The `.gitignore` is taken from
  GitHub's `Unity.gitignore`. Unity's packages are not in the repository.
- Modified: the two Sketchfab models (converted to FBX, joined, scaled to real size, fewer faces) and the
  plant's leaf texture (its alpha joined into one file). Details in `THIRD-PARTY-NOTICES.md`.
- Designed and implemented in this project: all code in `Assets/Scripts` and the tests in `Assets/Tests`
  (the 2D-to-3D placement, the label rules, the cube controls, the four detection sources and the model
  runner); the scene, prefabs and settings asset; the test room (layout, furniture built from Unity shapes,
  materials, placement of the props); the simulator controls file; the display names; the monitor's screen
  picture.
- Integrated: AR Foundation 6.3.5 with XR Simulation, and the Inference Engine 2.6.1 running YOLOv9-T on the AR
  camera picture.

## Licence

The code is under the MIT licence (`LICENSE`). The model, the font and the room's models and textures come
from others under their own licences, listed with full texts in `THIRD-PARTY-NOTICES.md`.
