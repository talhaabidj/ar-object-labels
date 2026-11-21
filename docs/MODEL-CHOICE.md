# Choosing the detector

[Back to the README](../README.md)

## How I compared them (18 Nov 2026)

- **Pictures.** 45 pictures of my lab room. Each was also made dark, overexposed, blurred, small and far,
  half-covered and grainy. That gives 315 pictures with 1,404 visible objects the models can name: laptop,
  monitor, keyboard, mouse, cup, bottle, vase, plant, clock, chairs, tables.
- **Judging.** A label is right when an object it names lies under it: an overlap of 0.3, or 80 % of the
  label's box inside the object. A name that fits a part counts as right, so "keyboard" on the laptop counts
  for the laptop. Anything else is a wrong label. Objects smaller than 600 square pixels in a picture, such as
  slivers cut by the edge, are not counted as present.
- **Decoding.** The test keeps one class per candidate box, the one with the highest score, and checks single
  pictures. The app keeps every class above its score limit and waits for 3 runs in a row, so the test ranks the
  models against each other but does not reproduce the app's labels.
- **Fair accuracy.** Every label down to a score of 0.05 is kept, then the cut-off is moved from 0.05 to 0.95.
  Models are compared by how many objects they find while making no more than a set number of wrong labels.
  No model gains from one shared cut-off.
- **Fair speed.** YOLOX-Tiny was also run at 640, and YOLOv9-T also at 416. Every model was timed in Unity's
  Inference Engine 2.6.1 on this Mac (Apple M1 Pro), on the graphics card (GPUCompute) over five interleaved
  rounds, and on the CPU separately. The first Unity run of each was thrown away, because it pays one-time
  set-up costs.

## Results

| Model and picture size | Unity GPU ms | Unity CPU ms | Found, at most 25 wrong | Found, at most 50 wrong | File | Verdict |
|---|---|---|---|---|---|---|
| YOLOX-Nano 416 | about 6 | 12 | 329 | 348 | 4 MB | Fastest and smallest, finds the fewest |
| YOLOX-Tiny 416 | 14.6 | 27 | 358 | 424 | 20 MB | A small picture costs a quarter of the finds |
| YOLOv9-T 416 | 18.4 | about 31 | 366 | 419 | 8 MB | Same, for YOLOv9-T |
| YOLOX-Tiny 640 | 24.1 | 55 | 483 | 543 | 20 MB | About equal to YOLOv9-T 640. Faster on the GPU, slower on the CPU, 2.5 times the file |
| PicoDet-L 416 | about 26 | 46 | 480 | 518 | 23 MB | Similar finds, bigger file, slower on the CPU |
| **YOLOv9-T 640 (picked)** | **26.8** | **about 40** | **505** | **533** | **8 MB** | **Most finds of the models near it in speed, a small file, MIT, and boxes come out ready** |
| YOLOX-S 640 | 32.3 | 90 | 577 | 624 | 36 MB | The most accurate, but much slower on the CPU and four and a half times the file |

Found is out of 1,404. GPU times varied by about 1 ms between rounds.

What this shows:
1. **Picture size matters more than the model.** Going from 416 to 640 finds about a third more for both
   YOLOX-Tiny and YOLOv9-T.
2. **At the same picture size, YOLOv9-T and YOLOX-Tiny are about equal.** At 640: 505 against 483 found at 25
   wrong, 533 against 543 at 50 wrong.
3. **My first test overstated YOLOv9-T's lead.** Its "3 wrong labels against 24" came mostly from the shared
   0.5 cut-off. YOLOv9-T gives lower scores, so it shows fewer labels at 0.5. YOLOX-Tiny looked twice as fast
   mostly because it ran on smaller pictures.
4. **YOLOX-S is the only one clearly more accurate,** about 14 % more found, at 5.5 ms more on the GPU, more
   than twice the CPU time and four and a half times the file size.

At a score of 0.45, YOLOv9-T found 533 objects with 32 wrong labels in the 315 pictures (0.50 gives 505 and
19). Most of the wrong labels came from the dark, blurred and grainy versions. Since 19 Nov the app shows a
label after 3 runs in a row at 0.40 or more, which on test walks gave as few wrong labels as 3 runs at 0.45.

Published COCO scores, as the authors report them (YOLOv9-T about 38 AP at 640, YOLOX-Tiny about 33 AP at
416), agree with the ranking at each model's own size.

## My first test, kept as a record (17 Nov)

Five small detectors on 434 hard cases made from 18 test-room pictures, and on 18 real photos, then timed in
Unity. This is the test I redid above, because one shared cut-off and different picture sizes made it unfair.

| Model | Found (of 434) | Wrong labels | Real photos | Unity GPU | Unity CPU | File |
|---|---|---|---|---|---|---|
| YOLOX-Nano | 209 | 14 | 11/18 | 9.3 ms | 12.4 ms | 3.7 MB |
| YOLOX-Tiny | 260 | 24 | 14/18 | 14.8 ms | 29.5 ms | 20.2 MB |
| YOLOv9-T | 259 | 3 | 13/18 | 27.5 ms | 40.4 ms | 8.5 MB |
| PicoDet-L 416 | 298 | 19 | 15/18 | 25.6 ms | 46.8 ms | 23.2 MB |
| YOLOX-S | 289 | 13 | 14/18 | 32.4 ms | 86.9 ms | 35.9 MB |

## Two larger detectors, tried and not used (17 Nov)

D-FINE-S, trained on Objects365 (365 classes, with pen and cup), and DEIM-D-FINE-N, trained on COCO. Both
imported into the Inference Engine with no unsupported layer and gave the same five best results as
onnxruntime on five photos.

| | YOLOX-Tiny (then my model) | DEIM-D-FINE-N | D-FINE-S |
|---|---|---|---|
| One run, GPU | 15.8 ms | 100.0 ms | 163.5 ms |
| One run, CPU | 27.6 ms | 619 ms | about 2,000 ms |
| Frame rate while detecting (360 with no model) | not measured then | 64, with a 70 ms frame at each run | 38, with a 130 ms frame at each run |
| Found the cup / the pens (18 pictures at 1 m) | no / no | no / no | 2 of 3 / 3 of 3 |
| Wrong labels in the 18 pictures | 2 | 4 | 20 |

Why not:
- **D-FINE-S** froze the picture at each run, has no usable CPU mode and gave 20 wrong labels. Its training
  data, Objects365, says it is for academic purposes only.
- **DEIM-D-FINE-N** found nothing in this room that the small models missed, and took 100 ms a run.

Neither model file is in the repository.

## Do detectors trained on photos see rendered objects? (15 Nov, outside Unity)

Before building the room I ran three small detectors (onnxruntime on this Mac) on:
- 18 real photos, as a control
- 18 Poly Haven models rendered with their textures
- the same 18 models without textures
- 5 crops of Unity's sample rooms

The table counts how often the expected class was found, at a score of 0.50.

| | Real photos | Textured renders | Untextured renders | Unity sample rooms |
|---|---|---|---|---|
| YOLOX-Nano | 11/18 | 12/18 | 9/18 | 0/5 |
| YOLOX-Tiny | 14/18 | 12/18 | 10/18 | 0/5 |
| YOLO11n | 14/18 | 12/18 | 5/18 | 0/5 |
| YOLOv9-T (added 17 Nov) | 13/18 | 13/18 | 5/18 | 0/5 |

Textured, photo-scanned models were found about as often as real photos. Without textures they were found
less, and Unity's untextured sample rooms not at all at 0.50. That is why my room uses textured Poly Haven
models. Plain, modern-looking objects worked, but ornate antiques did not. YOLO11n was only used in this test: its
AGPL licence would have covered the whole repository.

## The model file

`Assets/Models/yolov9_t.onnx`:
- YOLOv9-T trained on COCO, MIT, Copyright (c) 2024 Kin-Yiu Wong and Hao-Tang Tsui.
- Weights from MultimediaTechLab/YOLO, release v1.0-alpha (`v9-t.pt`).
- Converted to ONNX by LibreYOLO (`LibreYOLO9t.onnx`), unchanged, 8,451,972 bytes.
- The first YOLOv9 code, `WongKinYiu/yolov9`, is GPL-3.0 and is not used.

In Unity its results matched onnxruntime on three test-room pictures: the same objects from 0.45 up, boxes
within 2 px, scores within 0.08. The class names come from `Assets/Models/coco_classes.txt`, and the names on
the labels from `coco_display_names.txt`.

[Back to the README](../README.md)
