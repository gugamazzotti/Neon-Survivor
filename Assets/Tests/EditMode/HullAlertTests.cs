using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class HullAlertTests
    {
        [Test]
        public void FullHull_IsNotCritical()
        {
            Assert.IsFalse(HullAlert.IsCritical(PlayerController.MaxHealth, PlayerController.MaxHealth, false, false));
        }

        [Test]
        public void ThirtyPercent_IsCritical_OnePointAboveIsNot()
        {
            int line = PlayerController.MaxHealth * HullAlert.CriticalPercent / 100;
            Assert.IsTrue(HullAlert.IsCritical(line, PlayerController.MaxHealth, false, false));
            Assert.IsFalse(HullAlert.IsCritical(line + 1, PlayerController.MaxHealth, false, false));
        }

        [Test]
        public void DeadOrCoveredOrEmpty_StaysQuiet()
        {
            int line = PlayerController.MaxHealth * HullAlert.CriticalPercent / 100;
            Assert.IsFalse(HullAlert.IsCritical(line, PlayerController.MaxHealth, true, false));
            Assert.IsFalse(HullAlert.IsCritical(line, PlayerController.MaxHealth, false, true));
            Assert.IsFalse(HullAlert.IsCritical(0, PlayerController.MaxHealth, false, false));
            Assert.IsFalse(HullAlert.IsCritical(line, 0, false, false));
        }

        [Test]
        public void TookAHit_OnlyWhenHullDroppedAndStillFlying()
        {
            Assert.IsFalse(HullAlert.TookAHit(-1, PlayerController.MaxHealth));
            Assert.IsFalse(HullAlert.TookAHit(PlayerController.MaxHealth, PlayerController.MaxHealth));
            Assert.IsTrue(HullAlert.TookAHit(PlayerController.MaxHealth, PlayerController.MaxHealth - 10));
            Assert.IsFalse(HullAlert.TookAHit(20, 0));
            Assert.IsFalse(HullAlert.TookAHit(20, PlayerController.MaxHealth));
        }

        [Test]
        public void Pulse_RisesToTheMiddle_ThenFalls()
        {
            Assert.AreEqual(0f, HullAlert.Pulse01(0f, HullAlert.PulsePeriod), 0.001f);
            Assert.AreEqual(0.5f, HullAlert.Pulse01(HullAlert.PulsePeriod * 0.25f, HullAlert.PulsePeriod), 0.001f);
            Assert.AreEqual(1f, HullAlert.Pulse01(HullAlert.PulsePeriod * 0.5f, HullAlert.PulsePeriod), 0.001f);
            Assert.AreEqual(0f, HullAlert.Pulse01(HullAlert.PulsePeriod, HullAlert.PulsePeriod), 0.001f);
            Assert.AreEqual(0f, HullAlert.Pulse01(1f, 0f), 0.001f);
        }

        [Test]
        public void Edge_IsSilentUntilAHitOrACriticalHull()
        {
            Assert.AreEqual(0f, HullAlert.EdgeAlpha(false, 1f, 0f), 0.001f);
            Assert.AreEqual(HullAlert.FlashPeak, HullAlert.EdgeAlpha(false, 0f, 1f), 0.001f);
            Assert.AreEqual(HullAlert.BreathMax, HullAlert.EdgeAlpha(true, 1f, 0f), 0.001f);
            Assert.AreEqual(HullAlert.FlashPeak, HullAlert.EdgeAlpha(true, 1f, 1f), 0.001f);
        }

        [Test]
        public void Bar_StaysCalmUntilTheHullIsCritical()
        {
            Color calm = new Color(1f, 0.28f, 0.48f, 1f);
            Color hot = new Color(1f, 0.72f, 0.8f, 1f);
            Assert.AreEqual(calm, HullAlert.BarColor(calm, hot, false, 1f));
            Assert.AreEqual(hot, HullAlert.BarColor(calm, hot, true, 1f));
            Assert.AreEqual(calm, HullAlert.BarColor(calm, hot, true, 0f));
        }
    }
}
