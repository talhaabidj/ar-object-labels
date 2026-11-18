using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class TrackMatchTests
    {
        const float SameObject = 0.5f;
        static readonly List<string> earlierNames = new List<string> { "Laptop", "Chair", "Chair" };
        static readonly List<Vector3> earlierPlaces = new List<Vector3> { new Vector3(1.7f, 0.9f, 3.7f), new Vector3(0.9f, 0.5f, 2.0f), new Vector3(2.4f, 0.5f, 3.0f) };

        static List<int> Match(List<string> names, List<Vector3> places)
        {
            var into = new List<int>();
            TrackMatch.Match(earlierNames, earlierPlaces, names, places, SameObject, into);
            return into;
        }

        [Test]
        public void SameObjectsInAnotherOrder_AreFoundAgain()
        {
            var names = new List<string> { "Chair", "Laptop" };
            var places = new List<Vector3> { new Vector3(2.45f, 0.5f, 3.02f), new Vector3(1.72f, 0.9f, 3.69f) };

            CollectionAssert.AreEqual(new[] { 2, 0 }, Match(names, places));
        }

        [Test]
        public void NewObject_ContinuesNothing()
        {
            var names = new List<string> { "Vase" };
            var places = new List<Vector3> { new Vector3(1.7f, 0.9f, 3.7f) };

            CollectionAssert.AreEqual(new[] { -1 }, Match(names, places));
        }

        [Test]
        public void SameNameButFarAway_IsANewObject()
        {
            var names = new List<string> { "Laptop" };
            var places = new List<Vector3> { new Vector3(0.2f, 0.9f, 0.5f) };

            CollectionAssert.AreEqual(new[] { -1 }, Match(names, places));
        }

        [Test]
        public void TwoObjectsNearOneEarlierObject_OnlyTheNearerContinuesIt()
        {
            var names = new List<string> { "Chair", "Chair" };
            var places = new List<Vector3> { new Vector3(1.2f, 0.5f, 2.0f), new Vector3(0.95f, 0.5f, 2.0f) };

            CollectionAssert.AreEqual(new[] { -1, 1 }, Match(names, places));
        }

        [Test]
        public void NothingFoundNow_GivesAnEmptyList()
        {
            CollectionAssert.IsEmpty(Match(new List<string>(), new List<Vector3>()));
        }
    }
}
