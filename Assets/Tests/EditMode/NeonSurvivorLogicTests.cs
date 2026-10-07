using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class NeonSurvivorLogicTests
    {
        readonly List<GameObject> created = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            Enemy.ClearRegistry();
            XPOrb.ClearRegistry();
            if (GameManager.Instance != null)
                GameManager.Instance.ResetRun();
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

            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                if (Enemy.All[i] != null)
                    Object.DestroyImmediate(Enemy.All[i].gameObject);
            }
            Enemy.ClearRegistry();

            for (int i = XPOrb.All.Count - 1; i >= 0; i--)
            {
                if (XPOrb.All[i] != null)
                    Object.DestroyImmediate(XPOrb.All[i].gameObject);
            }
            XPOrb.ClearRegistry();

            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.ResetRun();
            PlayerController.ClearStatics();
        }

        [Test]
        public void Aim_CardinalDirections_PointTransformUp()
        {
            AssertUp(Vector2.right, Vector2.right);
            AssertUp(Vector2.up, Vector2.up);
            AssertUp(Vector2.left, Vector2.left);
            AssertUp(Vector2.down, Vector2.down);
        }

        [Test]
        public void Multishot_IsCenteredOnAim()
        {
            const float baseAngle = 10f;
            Assert.AreEqual(baseAngle, PlayerCombat.AngleForShot(baseAngle, 0, 1), 0.001f);
            Assert.AreEqual(baseAngle - 12f, PlayerCombat.AngleForShot(baseAngle, 0, 3), 0.001f);
            Assert.AreEqual(baseAngle, PlayerCombat.AngleForShot(baseAngle, 1, 3), 0.001f);
            Assert.AreEqual(baseAngle + 12f, PlayerCombat.AngleForShot(baseAngle, 2, 3), 0.001f);
        }

        [Test]
        public void NearestIndex_PicksClosest()
        {
            int index = PlayerCombat.FindNearestIndex(Vector2.zero, new[]
            {
                new Vector2(4f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-3f, 2f)
            });
            Assert.AreEqual(1, index);
        }

        [Test]
        public void FindNearest_ReturnsClosestEnemy()
        {
            CreateEnemy(new Vector3(3f, 0f, 0f));
            Enemy close = CreateEnemy(new Vector3(1f, 0.2f, 0f));
            Assert.AreEqual(close, PlayerCombat.FindNearest(Vector2.zero));
        }

        [Test]
        public void HitRadius_ExcludesExactBoundary()
        {
            Assert.IsFalse(DistanceRules.IsHit(Vector2.zero, new Vector2(0.5f, 0f)));
            Assert.IsTrue(DistanceRules.IsHit(Vector2.zero, new Vector2(0.49f, 0f)));
        }

        [Test]
        public void Contact_InsideRadius_DamagesWithoutXp()
        {
            PlayerController player = CreatePlayer(Vector3.zero);
            Enemy enemy = CreateEnemy(new Vector3(0.49f, 0f, 0f));
            Assert.IsTrue(enemy.ResolveContact(player));
            Assert.AreEqual(90, player.Health);
            Assert.AreEqual(0, XPOrb.All.Count);
        }

        [Test]
        public void Contact_AtExactRadius_DoesNothing()
        {
            PlayerController player = CreatePlayer(Vector3.zero);
            Enemy enemy = CreateEnemy(new Vector3(0.5f, 0f, 0f));
            Assert.IsFalse(enemy.ResolveContact(player));
            Assert.AreEqual(PlayerController.MaxHealth, player.Health);
            Assert.IsFalse(enemy.IsDying);
        }

        [Test]
        public void Damage_SecondHitIsIgnoredWhileInvulnerable()
        {
            PlayerController player = CreatePlayer(Vector3.zero);
            player.TakeDamage(10);
            player.TakeDamage(10);
            Assert.AreEqual(90, player.Health);
            Assert.IsTrue(player.IsInvulnerable);
        }

        [Test]
        public void Enemy_MovesTowardPlayerWithoutTunnelingPast()
        {
            PlayerController player = CreatePlayer(Vector3.zero);
            Enemy enemy = CreateEnemy(new Vector3(5f, 0f, 0f));
            enemy.Tick(0.1f, player);
            Assert.AreEqual(5f - Enemy.DefaultSpeed * 0.1f, enemy.transform.position.x, 0.001f);
            Assert.AreEqual(PlayerController.MaxHealth, player.Health);
        }

        [Test]
        public void Projectile_HitsOnlyInsideRadius()
        {
            Enemy enemy = CreateEnemy(Vector3.zero);
            Projectile miss = CreateProjectile(new Vector3(0.5f, 0f, 0f));
            Assert.IsFalse(miss.TryHit());
            Assert.AreEqual(Enemy.DefaultHp, enemy.Hp);

            Projectile hit = CreateProjectile(new Vector3(0.49f, 0f, 0f));
            Assert.IsTrue(hit.TryHit());
            Assert.AreEqual(10, enemy.Hp);
        }

        [Test]
        public void Projectile_TravelsAlongAim_AndTwoHitsDropOneOrb()
        {
            CreatePlayer(new Vector3(0f, -6f, 0f));
            Enemy enemy = CreateEnemy(new Vector3(1.2f, 0f, 0f));
            Projectile first = CreateProjectile(Vector3.zero);
            first.Launch(30f);
            first.transform.rotation = Quaternion.Euler(0f, 0f, PlayerCombat.AimAngleDegrees(Vector2.right));
            first.Tick(0.2f);
            Assert.IsFalse(enemy == null);
            Assert.AreEqual(10, enemy.Hp);

            Projectile second = CreateProjectile(Vector3.zero);
            second.Launch(30f);
            second.transform.rotation = Quaternion.Euler(0f, 0f, PlayerCombat.AimAngleDegrees(Vector2.right));
            second.Tick(0.2f);
            Assert.IsTrue(enemy == null);
            Assert.AreEqual(1, XPOrb.All.Count);
            Assert.AreEqual(XPOrb.XpValue, XPOrb.All[0].Value);
        }

        [Test]
        public void Orb_DriftsThenMagnetizesThenCollects()
        {
            GameManager gm = EnsureManager();
            PlayerController player = CreatePlayer(Vector3.zero);

            XPOrb far = CreateOrb(new Vector3(5f, 0f, 0f));
            far.Tick(0.1f, player);
            Assert.AreEqual(5f - XPOrb.DriftSpeed * 0.1f, far.transform.position.x, 0.001f);
            Assert.AreEqual(0, gm.CurrentXp);

            XPOrb near = CreateOrb(new Vector3(2f, 0f, 0f));
            near.Tick(0.05f, player);
            Assert.AreEqual(2f - XPOrb.MagnetSpeed * 0.05f, near.transform.position.x, 0.001f);

            XPOrb close = CreateOrb(new Vector3(0.4f, 0f, 0f));
            close.Tick(0.01f, player);
            Assert.AreEqual(XPOrb.XpValue, gm.CurrentXp);
            Assert.IsFalse(gm.IsChoosingUpgrade);
        }

        [Test]
        public void LevelUp_ZeroesXp_DoublesGoal_AndIgnoresSecondApply()
        {
            GameManager gm = EnsureManager();
            PlayerCombat combat = CreatePlayer(Vector3.zero).gameObject.AddComponent<PlayerCombat>();
            UpgradeManager upgrades = EnsureUpgrades();

            gm.AddXP(250);
            Assert.IsTrue(gm.IsChoosingUpgrade);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(250, gm.CurrentXp);
            Assert.AreEqual(3, gm.PendingChoices.Length);

            gm.AddXP(50);
            Assert.AreEqual(250, gm.CurrentXp);

            float fireRate = combat.FireRate;
            upgrades.UpgradeFireRate();
            Assert.AreEqual(fireRate * 0.9f, combat.FireRate, 0.0001f);
            Assert.AreEqual(0, gm.CurrentXp);
            Assert.AreEqual(200, gm.XpToLevel);
            Assert.AreEqual(2, gm.Level);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(gm.IsChoosingUpgrade);

            upgrades.UpgradeFireRate();
            Assert.AreEqual(fireRate * 0.9f, combat.FireRate, 0.0001f);
            Assert.AreEqual(2, gm.Level);
            Assert.AreEqual(200, gm.XpToLevel);
        }

        [Test]
        public void MultishotAndBulletSpeed_ApplyOnce()
        {
            GameManager gm = EnsureManager();
            PlayerCombat combat = CreatePlayer(Vector3.zero).gameObject.AddComponent<PlayerCombat>();
            UpgradeManager upgrades = EnsureUpgrades();

            gm.AddXP(100);
            upgrades.UpgradeMultishot();
            Assert.AreEqual(2, combat.BulletCount);
            Assert.AreEqual(1f, Time.timeScale);

            gm.AddXP(200);
            upgrades.UpgradeBulletSpeed();
            Assert.AreEqual(PlayerCombat.DefaultBulletSpeed * 1.2f, combat.BulletSpeed, 0.001f);
            Assert.AreEqual(3, gm.Level);
            Assert.AreEqual(400, gm.XpToLevel);
        }

        [Test]
        public void FireRateAndMultishot_RespectCaps()
        {
            PlayerCombat combat = CreatePlayer(Vector3.zero).gameObject.AddComponent<PlayerCombat>();
            for (int i = 0; i < 40; i++)
                combat.ReduceFireRate();
            for (int i = 0; i < 10; i++)
                combat.AddMultishot();

            Assert.AreEqual(PlayerCombat.MinFireRate, combat.FireRate, 0.0001f);
            Assert.AreEqual(PlayerCombat.MaxBullets, combat.BulletCount);
        }

        [Test]
        public void Death_BlocksLevelUp()
        {
            GameManager gm = EnsureManager();
            PlayerController player = CreatePlayer(Vector3.zero);
            player.TakeDamage(PlayerController.MaxHealth);
            Assert.IsTrue(player.IsDead);
            Assert.IsTrue(gm.IsGameOver);
            Assert.AreEqual(0f, Time.timeScale);

            gm.AddXP(100);
            Assert.IsFalse(gm.IsChoosingUpgrade);
            Assert.AreEqual(0, gm.CurrentXp);
            Assert.IsNull(gm.PendingChoices);
        }

        static void AssertUp(Vector2 direction, Vector2 expected)
        {
            float angle = PlayerCombat.AimAngleDegrees(direction);
            Vector3 up = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            Assert.AreEqual(expected.x, up.x, 0.001f);
            Assert.AreEqual(expected.y, up.y, 0.001f);
        }

        GameObject Track(GameObject go)
        {
            created.Add(go);
            return go;
        }

        PlayerController CreatePlayer(Vector3 position)
        {
            GameObject go = Track(new GameObject("Player"));
            go.transform.position = position;
            return go.AddComponent<PlayerController>();
        }

        Enemy CreateEnemy(Vector3 position)
        {
            GameObject go = Track(new GameObject("Enemy"));
            go.transform.position = position;
            Enemy enemy = go.AddComponent<Enemy>();
            enemy.Register();
            return enemy;
        }

        Projectile CreateProjectile(Vector3 position)
        {
            GameObject go = Track(new GameObject("Projectile"));
            go.transform.position = position;
            return go.AddComponent<Projectile>();
        }

        XPOrb CreateOrb(Vector3 position)
        {
            GameObject go = Track(new GameObject("XP"));
            go.transform.position = position;
            XPOrb orb = go.AddComponent<XPOrb>();
            orb.Register();
            orb.Initialize(XPOrb.XpValue);
            return orb;
        }

        GameManager EnsureManager()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
                gm = Track(new GameObject("GameManager")).AddComponent<GameManager>();
            gm.ResetRun();
            return gm;
        }

        UpgradeManager EnsureUpgrades()
        {
            UpgradeManager upgrades = UpgradeManager.Instance;
            if (upgrades == null)
                upgrades = Track(new GameObject("UpgradeManager")).AddComponent<UpgradeManager>();
            return upgrades;
        }
    }
}
