# Manual test (about 10 minutes, in the Editor)

Open `Assets/Scenes/Main.unity` and press Play. Tick each line that behaves as written. Note any line that
does not, with what you saw.

## Start (1 min)
- [ ] The Console shows no red errors. One information line about `InputActionReference(Unlock)` is expected.
- [ ] The controls list is at the top left; the hint line is at the bottom.
- [ ] The Detection button reads "Detection: model".

## Moving and looking (1 min)
- [ ] W A S D moves, Q and E go down and up, Shift moves faster.
- [ ] Holding the right mouse button and moving the mouse turns the view.
- [ ] After the first move the controls fold to "H: controls". H opens them again and folds them.
- [ ] After a few seconds of walking, faint surface outlines appear on the floor and the work table.

## The cube (3 min)
- [ ] Click a found surface on the floor: a cube appears there, with a "Cube" label above it, and the hint
      changes.
- [ ] While the cube is dragged, the surface outlines are drawn in full; after letting go they are faint again.
- [ ] Drag the cube: it slides along the surface under the mouse.
- [ ] Click the cube: it is picked up. Move the view with the right mouse button; the cube follows the mouse.
- [ ] Click the table or the floor: the cube is put down there.
- [ ] Click a wall: the cube is not placed, and the hint asks for a flat surface.
- [ ] Click a spot with no found surface yet (for example, before walking): the hint says no surface was found
      there yet.
- [ ] Press R: the cube is removed. Place it again, then click the Reset button: it is removed.
- [ ] A click on the Detection button changes the source and places no cube.

## Labels (4 min)
- [ ] Model: walk towards the work table. "Laptop" appears above the laptop within about a second, then
      "Chair", "Potted plant" and others as they come into view. At most six labels at once.
- [ ] Walk around the table: each label stays above its object and keeps facing you; the text stays the
      same size on screen as you walk closer and further.
- [ ] Turn quickly away and back: labels of objects out of view come back at the same place, faded until
      found again.
- [ ] Click Detection: "off". All labels go away.
- [ ] Click Detection: "fixed box (laptop only)". One "Laptop" label sits above the laptop and stays there as
      you move.
- [ ] Click Detection: "true box". Labels appear on the objects, including "Pen" on the pens when few bigger
      objects are in view.
- [ ] In any mode, no label of a found object sits under the controls panel, the hint line or the buttons,
      and no label of an object in the middle of the picture is cut by the picture's edge. (A faded label of
      an object not found now stays at its place in the room and may sit under a panel.)
- [ ] Click Detection again: back to "model", with no labels left over from true box mode.

## Stop (under 1 min)
- [ ] Stop Play. The Console has no red errors from the run.

Known limits that this test may show (listed in the README): the model's "Chair" at the table's edge,
"Keyboard" on screens, crowding at the pen cup, a label away from its object with no line when no near
place is free, and the cube on the chair's seat after a click behind the school chair.
