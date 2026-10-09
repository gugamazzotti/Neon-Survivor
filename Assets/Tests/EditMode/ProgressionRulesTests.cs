using NUnit.Framework;
using UnityEngine;

namespace NeonSurvivor.Tests
{
    public class ProgressionRulesTests
    {
        [Test]
        public void Language_UnknownAndUnsupported_UseEnglish()
        {
            Assert.AreEqual(SystemLanguage.English, Loc.Resolve(SystemLanguage.Unknown));
            Assert.AreEqual(SystemLanguage.English, Loc.Resolve(SystemLanguage.Spanish));
            Assert.AreEqual(SystemLanguage.English, Loc.Resolve(SystemLanguage.English));
            Assert.AreEqual(SystemLanguage.Portuguese, Loc.Resolve(SystemLanguage.Portuguese));
        }

        [Test]
        public void Language_TablesFollowTheResolvedLanguage()
        {
            Loc.Use(SystemLanguage.English);
            Assert.AreEqual("Play", Loc.Get("menu.play"));
            Loc.Use(SystemLanguage.Portuguese);
            Assert.AreEqual("Jogar", Loc.Get("menu.play"));
            Loc.Use(SystemLanguage.Japanese);
            Assert.AreEqual("Play", Loc.Get("menu.play"));
            Loc.Use(Application.systemLanguage);
        }

        [Test]
        public void MapUnlock_RequiresScoreOnThePreviousMap()
        {
            Assert.IsTrue(MapCatalog.IsUnlocked(0, 0));
            Assert.IsFalse(MapCatalog.IsUnlocked(1, MapCatalog.Maps[1].RequiredScore - 1));
            Assert.IsTrue(MapCatalog.IsUnlocked(1, MapCatalog.Maps[1].RequiredScore));
            Assert.IsFalse(MapCatalog.IsUnlocked(2, MapCatalog.Maps[2].RequiredScore - 1));
            Assert.IsTrue(MapCatalog.IsUnlocked(2, MapCatalog.Maps[2].RequiredScore));
            Assert.IsFalse(MapCatalog.IsUnlocked(3, 99999));
        }

        [Test]
        public void Achievements_UnlockOnExactGoals()
        {
            Assert.AreEqual(MapCatalog.Maps[1].RequiredScore, AchievementCatalog.Goal("score_400"));
            Assert.AreEqual(MapCatalog.Maps[2].RequiredScore, AchievementCatalog.Goal("score_900"));

            AchievementProgress progress = new AchievementProgress();
            Assert.IsFalse(AchievementCatalog.IsMet("first_kill", progress));
            progress.LifetimeKills = 1;
            Assert.IsTrue(AchievementCatalog.IsMet("first_kill", progress));
            Assert.IsFalse(AchievementCatalog.IsMet("life_100", progress));
            progress.LifetimeKills = 100;
            Assert.IsTrue(AchievementCatalog.IsMet("life_100", progress));

            progress.BestRunKills = 24;
            Assert.IsFalse(AchievementCatalog.IsMet("run_25", progress));
            progress.BestRunKills = 25;
            Assert.IsTrue(AchievementCatalog.IsMet("run_25", progress));

            progress.BestLevel = 5;
            Assert.IsTrue(AchievementCatalog.IsMet("level_5", progress));
            progress.BestScore = 400;
            Assert.IsTrue(AchievementCatalog.IsMet("score_400", progress));
            Assert.IsFalse(AchievementCatalog.IsMet("score_900", progress));
            progress.BestScore = 900;
            Assert.IsTrue(AchievementCatalog.IsMet("score_900", progress));
            progress.BestSeconds = 90;
            Assert.IsTrue(AchievementCatalog.IsMet("survive_90", progress));
            progress.Map2 = true;
            Assert.IsTrue(AchievementCatalog.IsMet("map_2", progress));
            progress.Map3 = true;
            Assert.IsTrue(AchievementCatalog.IsMet("map_3", progress));
            progress.AllUpgrades = true;
            Assert.IsTrue(AchievementCatalog.IsMet("arsenal", progress));
        }
    }
}
