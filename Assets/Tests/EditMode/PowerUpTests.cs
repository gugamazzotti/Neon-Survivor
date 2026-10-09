using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class PowerUpTests
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
            DestroyAll<PulseWave>();
            DestroyAll<DamageZone>();
            DestroyAll<EnergyMissile>();
            DestroyAll<PlayerPowers>();
            Enemy.ClearRegistry();
            PlayerController.ClearStatics();
        }

        [Test]
        public void LevelChoices_AreThreeDistinctPowers()
        {
            UpgradeChoice[] choices = UpgradeManager.RollThree();
            Assert.AreEqual(3, choices.Length);
            Assert.AreNotEqual(choices[0].Type, choices[1].Type);
            Assert.AreNotEqual(choices[0].Type, choices[2].Type);
            Assert.AreNotEqual(choices[1].Type, choices[2].Type);
        }

        [Test]
        public void MaxedShield_LeavesTheLevelPool()
        {
            PlayerPowers powers = Track(new GameObject("Player")).AddComponent<PlayerController>().gameObject.AddComponent<PlayerPowers>();
            for (int i = 0; i < PlayerPowers.MaxRank; i++)
                powers.RankUp(UpgradeType.Shield);

            UpgradeChoice[] choices = UpgradeManager.RollThree();
            for (int i = 0; i < choices.Length; i++)
                Assert.AreNotEqual(UpgradeType.Shield, choices[i].Type);
        }

        [Test]
        public void Shield_BreaksOnHit_ThenRecharges()
        {
            PlayerPowers powers = Track(new GameObject("Player")).AddComponent<PlayerPowers>();
            powers.RankUp(UpgradeType.Shield);
            Assert.IsTrue(powers.ShieldUp);

            Assert.IsTrue(powers.TryAbsorbHit());
            Assert.IsFalse(powers.ShieldUp);
            powers.Tick(7.9f);
            Assert.IsFalse(powers.ShieldUp);
            powers.Tick(0.2f);
            Assert.IsTrue(powers.ShieldUp);
            Assert.AreEqual(1, powers.ShieldCharges);
        }

        [Test]
        public void Shield_OnThePlayer_AbsorbsDamage()
        {
            PlayerController player = Track(new GameObject("Player")).AddComponent<PlayerController>();
            PlayerPowers powers = player.gameObject.AddComponent<PlayerPowers>();
            powers.RankUp(UpgradeType.Shield);

            player.TakeDamage(40);
            Assert.AreEqual(PlayerController.MaxHealth, player.Health);
            Assert.IsFalse(powers.ShieldUp);
        }

        [Test]
        public void Orbs_StartAtTwo_AndGrowOnLaterRanks()
        {
            PlayerPowers powers = Track(new GameObject("Player")).AddComponent<PlayerPowers>();
            powers.RankUp(UpgradeType.Orbit);
            Assert.AreEqual(2, powers.OrbCount);
            powers.RankUp(UpgradeType.Orbit);
            Assert.AreEqual(2, powers.OrbCount);
            powers.RankUp(UpgradeType.Orbit);
            Assert.AreEqual(3, powers.OrbCount);

            Vector2 point = PlayerPowers.OrbitPoint(Vector2.zero, 0f, 2f, 0, 2);
            Assert.AreEqual(2f, point.x, 0.001f);
            Assert.AreEqual(0f, point.y, 0.001f);
        }

        [Test]
        public void Pulse_DamagesOnlyEnemiesInsideTheWave()
        {
            Enemy near = CreateEnemy(new Vector3(1f, 0f, 0f));
            Enemy far = CreateEnemy(new Vector3(4f, 0f, 0f));
            PulseWave wave = PulseWave.Spawn(Vector3.zero, null, 3f, 10, 8f);
            wave.Tick(0.2f);

            Assert.Less(near.Hp, Enemy.DefaultHp);
            Assert.AreEqual(Enemy.DefaultHp, far.Hp);
        }

        [Test]
        public void Zone_DamagesOverTimeInsideItsRadius()
        {
            Enemy inside = CreateEnemy(new Vector3(0.4f, 0f, 0f));
            Enemy outside = CreateEnemy(new Vector3(3f, 0f, 0f));
            DamageZone zone = DamageZone.Spawn(Vector3.zero, 1.2f, 2f, 5);
            zone.Tick(0.05f);

            Assert.AreEqual(Enemy.DefaultHp - 5, inside.Hp);
            Assert.AreEqual(Enemy.DefaultHp, outside.Hp);
        }

        [Test]
        public void Missile_OnHit_LeavesADamageZone()
        {
            Enemy enemy = CreateEnemy(Vector3.zero);
            EnergyMissile missile = EnergyMissile.Spawn(Vector3.zero, Vector2.right, 15f, 1.4f, 2f, 6);
            Assert.IsTrue(missile.TryHit());
            Assert.AreEqual(Enemy.DefaultHp - EnergyMissile.ImpactDamage, enemy.Hp);
            Assert.Greater(Object.FindObjectsOfType<DamageZone>().Length, 0);
        }

        Enemy CreateEnemy(Vector3 position)
        {
            Enemy enemy = Track(new GameObject("Enemy")).AddComponent<Enemy>();
            enemy.transform.position = position;
            enemy.Register();
            return enemy;
        }

        GameObject Track(GameObject go)
        {
            created.Add(go);
            return go;
        }

        static void DestroyAll<T>() where T : Component
        {
            T[] found = Object.FindObjectsOfType<T>();
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null)
                    Object.DestroyImmediate(found[i].gameObject);
            }
        }
    }
}
