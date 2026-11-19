using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class SourceSwitchTests
    {
        GameObject holder;
        MonoBehaviour[] sources;

        [SetUp]
        public void MakeSources()
        {
            holder = new GameObject("sources");
            sources = new MonoBehaviour[]
            {
                holder.AddComponent<FixedBoxSource>(), holder.AddComponent<TrueBoxSource>(), holder.AddComponent<ModelSource>()
            };
        }

        [TearDown]
        public void RemoveSources()
        {
            Object.DestroyImmediate(holder);
        }

        [Test]
        public void StartsWithTheModel_WhenTheModelIsSetToStartWith()
        {
            Assert.AreEqual(2, SourceSwitch.StartNumber(sources, sources[2]));
        }

        [Test]
        public void StartsOff_WhenNoSourceIsSetToStartWith()
        {
            Assert.AreEqual(sources.Length, SourceSwitch.StartNumber(sources, null));
        }

        [Test]
        public void StartsOff_WhenTheSourceToStartWithIsNotOnTheButton()
        {
            MonoBehaviour other = holder.AddComponent<FixedBoxSource>();

            Assert.AreEqual(sources.Length, SourceSwitch.StartNumber(sources, other));
        }
    }
}
