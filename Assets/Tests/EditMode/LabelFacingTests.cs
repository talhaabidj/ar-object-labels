using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelFacingTests
    {
        static readonly Vector3 label = new Vector3(0f, 1f, 0f);

        // The camera's turn when it stands at this place and looks straight at the label.
        static Quaternion LookingAtTheLabelFrom(Vector3 cameraPosition, Vector3 up)
        {
            return Quaternion.LookRotation(label - cameraPosition, up);
        }

        // A label's text is read looking along its forward direction, so "facing the camera" means its forward
        // points the way the camera looks.
        static void AssertFacesCamera(Quaternion rotation, Quaternion camera)
        {
            Assert.Greater(Vector3.Dot(rotation * Vector3.forward, camera * Vector3.forward), 0.999f);
        }

        [Test]
        public void FromTheFront_FacesTheCamera_Upright()
        {
            Quaternion camera = LookingAtTheLabelFrom(new Vector3(0f, 1f, -2f), Vector3.up);
            Quaternion rotation = LabelFacing.Rotation(camera * Vector3.forward, camera * Vector3.up);

            AssertFacesCamera(rotation, camera);
            Assert.Greater(Vector3.Dot(rotation * Vector3.up, Vector3.up), 0.999f);
        }

        [Test]
        public void FromTheSide_TurnsTowardsTheCamera()
        {
            Quaternion camera = LookingAtTheLabelFrom(new Vector3(2f, 1f, 0f), Vector3.up);
            Quaternion rotation = LabelFacing.Rotation(camera * Vector3.forward, camera * Vector3.up);

            AssertFacesCamera(rotation, camera);
            Assert.Greater(Vector3.Dot(rotation * Vector3.forward, Vector3.left), 0.999f);
            Assert.Greater(Vector3.Dot(rotation * Vector3.up, Vector3.up), 0.999f);
        }

        [Test]
        public void FromStraightAbove_LiesFlatFacingUp_TextTopAtThePicturesTop()
        {
            Vector3 topOfPicture = Vector3.forward;
            Quaternion camera = LookingAtTheLabelFrom(new Vector3(0f, 3f, 0f), topOfPicture);
            Quaternion rotation = LabelFacing.Rotation(camera * Vector3.forward, camera * Vector3.up);

            Assert.Greater(Vector3.Dot(rotation * Vector3.forward, Vector3.down), 0.999f);
            Assert.Greater(Vector3.Dot(rotation * Vector3.up, topOfPicture), 0.999f);
        }

        [Test]
        public void FromFarAndHigher_TiltsTowardsTheCamera()
        {
            Quaternion camera = LookingAtTheLabelFrom(new Vector3(0f, 3f, -10f), Vector3.up);
            Quaternion rotation = LabelFacing.Rotation(camera * Vector3.forward, camera * Vector3.up);

            AssertFacesCamera(rotation, camera);
            Assert.Less((rotation * Vector3.forward).y, 0f, "its face leans back, towards the higher camera");
        }

        [Test]
        public void LabelAtTheEdgeOfThePicture_StaysParallelToTheScreen()
        {
            // The camera looks straight ahead; the label is far off to one side of what it sees.
            Quaternion camera = Quaternion.Euler(10f, 40f, 0f);
            Quaternion rotation = LabelFacing.Rotation(camera * Vector3.forward, camera * Vector3.up);

            Assert.Greater(Vector3.Dot(rotation * Vector3.forward, camera * Vector3.forward), 0.9999f);
            Assert.Greater(Vector3.Dot(rotation * Vector3.right, camera * Vector3.right), 0.9999f);
            Assert.Greater(Vector3.Dot(rotation * Vector3.up, camera * Vector3.up), 0.9999f);
        }

        [Test]
        public void FromAnyDirection_TextIsNeverMirrored()
        {
            for (int yaw = 0; yaw < 360; yaw += 30)
            {
                for (int pitch = -80; pitch <= 80; pitch += 20)
                {
                    Quaternion view = Quaternion.Euler(pitch, yaw, 0f);
                    Quaternion rotation = LabelFacing.Rotation(view * Vector3.forward, view * Vector3.up);

                    // Not mirrored: the label's right is the viewer's right. Not upside down: its up is the viewer's up.
                    Assert.Greater(Vector3.Dot(rotation * Vector3.right, view * Vector3.right), 0.999f, $"yaw {yaw}, pitch {pitch}");
                    Assert.Greater(Vector3.Dot(rotation * Vector3.up, view * Vector3.up), 0.999f, $"yaw {yaw}, pitch {pitch}");
                }
            }
        }

        [Test]
        public void NoViewDirection_GivesAUsableRotation()
        {
            Assert.AreEqual(Quaternion.identity, LabelFacing.Rotation(Vector3.zero, Vector3.up));
        }

        [Test]
        public void ViewersUpAlongTheLineOfSight_StillGivesAUsableRotation()
        {
            Quaternion rotation = LabelFacing.Rotation(Vector3.down, Vector3.down);

            Assert.Greater(Vector3.Dot(rotation * Vector3.forward, Vector3.down), 0.999f);
            Assert.IsFalse(float.IsNaN(rotation.x));
        }
    }
}
