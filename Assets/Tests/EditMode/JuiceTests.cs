using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class JuiceTests
    {
        [Test]
        public void Effects_StayDormantOutsidePlayMode()
        {
            int before = Object.FindObjectsOfType<SpaceFx>().Length;
            SpaceFx.Burst(Vector3.zero, Color.white, 8, 4f, 0.2f);
            SpaceFx.Ring(Vector3.zero, Color.cyan, 2f, 0.3f);
            SpaceFx.LevelUp(Vector3.zero);
            SpaceFx.EnemyDown(Vector3.zero, Color.red, 2);
            SpaceFx.Pickup(Vector3.zero, 10);
            Assert.AreEqual(before, Object.FindObjectsOfType<SpaceFx>().Length);
            Assert.AreEqual(0, SpaceFx.ActiveCount);
        }
    }
}
