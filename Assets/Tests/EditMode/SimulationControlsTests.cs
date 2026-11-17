using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace ArObjectLabels.Tests
{
    /// <summary>Pins how the simulated camera is controlled, so a reset to Unity's defaults is noticed.</summary>
    public class SimulationControlsTests
    {
        const string ActionsPath = "Assets/Settings/SimulationControls.inputactions";
        const string PreferencesPath = "Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset";

        static InputActionAsset Actions()
        {
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        }

        static SerializedObject Preferences()
        {
            return new SerializedObject(AssetDatabase.LoadMainAssetAtPath(PreferencesPath));
        }

        static string[] Parts(InputAction action)
        {
            return action.bindings.Where(b => b.isPartOfComposite).Select(b => b.name + " " + b.path).ToArray();
        }

        [Test]
        public void OurActions_HaveNoUnlock_SoTheKeysNeedNoHeldButton()
        {
            Assert.IsNull(Actions().FindAction("Unlock"));
        }

        [Test]
        public void Look_WorksOnlyWhileTheRightMouseButtonIsHeld()
        {
            InputAction look = Actions().FindAction("Look", throwIfNotFound: true);

            Assert.AreEqual("OneModifier", look.bindings.Single(b => b.isComposite).path);
            CollectionAssert.AreEquivalent(
                new[] { "modifier <Mouse>/rightButton", "binding <Mouse>/delta" }, Parts(look));
        }

        [Test]
        public void Move_UsesSixKeys()
        {
            InputAction move = Actions().FindAction("Move", throwIfNotFound: true);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "forward <Keyboard>/w", "backward <Keyboard>/s", "left <Keyboard>/a", "right <Keyboard>/d",
                    "up <Keyboard>/e", "down <Keyboard>/q"
                },
                Parts(move));
        }

        [Test]
        public void Sprint_IsShift()
        {
            InputAction sprint = Actions().FindAction("Sprint", throwIfNotFound: true);

            Assert.AreEqual("<Keyboard>/shift", sprint.bindings.Single().path);
        }

        [TestCase("m_MoveInputActionReference", "Move")]
        [TestCase("m_LookInputActionReference", "Look")]
        [TestCase("m_SprintInputActionReference", "Sprint")]
        public void Simulator_UsesOurAction(string field, string actionName)
        {
            var reference = Preferences().FindProperty(field).objectReferenceValue as InputActionReference;

            Assert.IsNotNull(reference, field + " is empty");
            Assert.AreEqual(ActionsPath, AssetDatabase.GetAssetPath(reference));
            Assert.AreEqual(actionName, reference.action.name);
        }

        [Test]
        public void Simulator_HasNoUnlockAction()
        {
            Assert.IsNull(Preferences().FindProperty("m_UnlockInputActionReference").objectReferenceValue);
        }
    }
}
