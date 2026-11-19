using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class SurfaceOutlinesTests
    {
        [Test]
        public void CubeInHand_DrawsSurfacesAtFullStrength()
        {
            Assert.AreEqual(1f, OutlineRule.Strength(true, 0.15f));
        }

        [Test]
        public void CubeNotInHand_DrawsSurfacesFaint()
        {
            Assert.AreEqual(0.15f, OutlineRule.Strength(false, 0.15f));
        }

        [Test]
        public void FaintShare_IsKeptBetweenNothingAndFull()
        {
            Assert.AreEqual(0f, OutlineRule.Strength(false, -1f));
            Assert.AreEqual(1f, OutlineRule.Strength(false, 2f));
        }

        [Test]
        public void Tint_LowersTheRenderersOpacity_AndLeavesTheMaterialAlone()
        {
            var surface = new GameObject("surface");
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.8f));
            var renderer = surface.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            try
            {
                SurfaceOutlines.Tint(renderer, 0.25f, block);

                var drawn = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(drawn);
                Assert.AreEqual(0.2f, drawn.GetColor("_BaseColor").a, 0.0001f);
                Assert.AreEqual(0.8f, material.GetColor("_BaseColor").a, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(surface);
                Object.DestroyImmediate(material);
            }
        }
    }
}
