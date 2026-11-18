using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class DetectableObjectTests
    {
        [Test]
        public void MarkOnAnObjectFlaggedDontSave_IsStillFound()
        {
            var room = new GameObject("test room");
            try
            {
                var pen = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pen.transform.SetParent(room.transform);
                DetectableObject mark = pen.AddComponent<DetectableObject>();
                // The simulator does this to every mesh of the room when Play starts.
                pen.hideFlags = HideFlags.DontSave;

                var found = new List<DetectableObject>();
                DetectableObject.FindAll(found);

                CollectionAssert.Contains(found, mark);
            }
            finally
            {
                Object.DestroyImmediate(room);
            }
        }

        [Test]
        public void Corners_AreTheEightCornersOfEachPart()
        {
            var holder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                holder.transform.position = new Vector3(1f, 2f, 3f);
                holder.transform.localScale = new Vector3(2f, 1f, 1f);
                DetectableObject mark = holder.AddComponent<DetectableObject>();

                var corners = new List<Vector3>();
                mark.AddCorners(corners);

                Assert.AreEqual(8, corners.Count);
                CollectionAssert.Contains(corners, new Vector3(0f, 1.5f, 2.5f));
                CollectionAssert.Contains(corners, new Vector3(2f, 2.5f, 3.5f));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }
    }
}
