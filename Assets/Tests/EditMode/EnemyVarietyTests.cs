using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class EnemyVarietyTests
    {
        readonly List<GameObject> created = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            Enemy.ClearRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
            {
                if (created[i] != null)
                    Object.DestroyImmediate(created[i]);
            }

            created.Clear();
            HostileShot[] shots = Object.FindObjectsOfType<HostileShot>();
            for (int i = 0; i < shots.Length; i++)
            {
                if (shots[i] != null)
                    Object.DestroyImmediate(shots[i].gameObject);
            }

            Enemy.ClearRegistry();
            PlayerController.ClearStatics();
        }

        [Test]
        public void KeepDistance_RetreatsWhenTooClose_AndClosesWhenTooFar()
        {
            Vector2 close = EnemySteering.KeepDistance(new Vector2(1f, 0f), Vector2.zero, 4f, 2f, 0.5f, 0f);
            Assert.Greater(close.x, 1f);

            Vector2 far = EnemySteering.KeepDistance(new Vector2(10f, 0f), Vector2.zero, 4f, 2f, 0.5f, 0f);
            Assert.Less(far.x, 10f);
            Assert.Greater(far.x, 4f);
        }

        [Test]
        public void Weave_LeavesTheStraightLine()
        {
            float time = (Mathf.PI * 0.5f) / 5.5f;
            Vector2 next = EnemySteering.Weave(new Vector2(5f, 0f), Vector2.zero, 2f, 0.25f, time);
            Assert.Less(next.x, 5f);
            Assert.AreNotEqual(0f, next.y);
        }

        [Test]
        public void Along_FollowsTheLockedDirection()
        {
            Vector2 next = EnemySteering.Along(Vector2.zero, Vector2.up, 4f, 0.5f);
            Assert.AreEqual(0f, next.x, 0.001f);
            Assert.AreEqual(2f, next.y, 0.001f);
        }

        [Test]
        public void Boss_ContactHurtsWithoutDestroyingIt()
        {
            PlayerController player = Track(new GameObject("Player")).AddComponent<PlayerController>();
            Enemy boss = Track(new GameObject("Boss")).AddComponent<Enemy>();
            boss.Register();
            boss.ApplyArchetype(EnemyArchetype.Boss, 1f);
            boss.transform.position = new Vector3(0.4f, 0f, 0f);

            int orbs = XPOrb.All.Count;
            Assert.IsTrue(boss.ResolveContact(player));
            Assert.AreEqual(PlayerController.MaxHealth - EnemyArchetypes.Get(EnemyArchetype.Boss).ContactDamage, player.Health);
            Assert.IsFalse(boss.IsDying);
            Assert.AreEqual(orbs, XPOrb.All.Count);
            Assert.IsFalse(boss.ResolveContact(player));
        }

        [Test]
        public void HostileShot_HitsOnlyInsidePlayerRadius()
        {
            PlayerController player = Track(new GameObject("Player")).AddComponent<PlayerController>();
            player.transform.position = Vector3.zero;

            HostileShot miss = HostileShot.Spawn(new Vector3(0.5f, 0f, 0f), Vector2.left, 8f);
            Assert.IsFalse(miss.TryHit());
            Assert.AreEqual(PlayerController.MaxHealth, player.Health);

            HostileShot hit = HostileShot.Spawn(new Vector3(0.49f, 0f, 0f), Vector2.left, 8f);
            Assert.IsTrue(hit.TryHit());
            Assert.AreEqual(PlayerController.MaxHealth - HostileShot.Damage, player.Health);
        }

        GameObject Track(GameObject go)
        {
            created.Add(go);
            return go;
        }
    }
}
