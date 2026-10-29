using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class ThreatMarkerTests
    {
        ThreatMarkers.ThreatSample[] incoming;
        ThreatMarkers.ThreatMark[] scratch;
        ThreatMarkers.ThreatMark[] output;

        [SetUp]
        public void SetUp()
        {
            incoming = new ThreatMarkers.ThreatSample[8];
            scratch = new ThreatMarkers.ThreatMark[8];
            output = new ThreatMarkers.ThreatMark[ThreatMarkers.MaxMarks];
        }

        [Test]
        public void InsideTheView_DrawsNothing()
        {
            incoming[0].Position = new Vector2(1f, 1f);
            incoming[0].Weight = 0;
            int count = Collect(1);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void EnemyOnTheRight_SitsOnTheRightEdge_PointingRight()
        {
            incoming[0].Position = new Vector2(30f, 0f);
            int count = Collect(1);
            Assert.AreEqual(1, count);
            Assert.AreEqual(9f, output[0].Position.x, 0.001f);
            Assert.AreEqual(0f, output[0].Position.y, 0.001f);
            Assert.AreEqual(-90f, output[0].AngleDegrees, 0.001f);
        }

        [Test]
        public void EnemyAbove_SitsOnTheTopEdge_PointingUp()
        {
            incoming[0].Position = new Vector2(0f, 40f);
            int count = Collect(1);
            Assert.AreEqual(1, count);
            Assert.AreEqual(0f, output[0].Position.x, 0.001f);
            Assert.AreEqual(5f, output[0].Position.y, 0.001f);
            Assert.AreEqual(0f, output[0].AngleDegrees, 0.001f);
        }

        [Test]
        public void Diagonal_HitsTheNearerEdge()
        {
            incoming[0].Position = new Vector2(30f, 10f);
            int count = ThreatMarkers.Collect(
                Vector2.zero,
                10f,
                10f,
                0f,
                incoming,
                1,
                scratch,
                output);
            Assert.AreEqual(1, count);
            Assert.AreEqual(10f, output[0].Position.x, 0.001f);
            Assert.AreEqual(3.333f, output[0].Position.y, 0.01f);
        }

        [Test]
        public void Cap_KeepsTheBossAheadOfACloserTrash()
        {
            incoming[0].Position = new Vector2(12f, 0f);
            incoming[0].Weight = 0;
            incoming[1].Position = new Vector2(-80f, 0f);
            incoming[1].Weight = 2;
            ThreatMarkers.ThreatMark[] one = new ThreatMarkers.ThreatMark[1];
            int count = ThreatMarkers.Collect(Vector2.zero, 10f, 6f, 1f, incoming, 2, scratch, one);
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, one[0].Weight);
            Assert.Less(one[0].Position.x, 0f);
        }

        [Test]
        public void Weight_BossOutranksMiniBossOutranksTrash()
        {
            Assert.AreEqual(2, ThreatMarkers.WeightFor(EnemyArchetype.Boss));
            Assert.AreEqual(1, ThreatMarkers.WeightFor(EnemyArchetype.MiniBoss));
            Assert.AreEqual(0, ThreatMarkers.WeightFor(EnemyArchetype.Chaser));
            Assert.AreEqual(0, ThreatMarkers.WeightFor(EnemyArchetype.Gunner));
        }

        int Collect(int count)
        {
            return ThreatMarkers.Collect(Vector2.zero, 10f, 6f, 1f, incoming, count, scratch, output);
        }
    }
}
