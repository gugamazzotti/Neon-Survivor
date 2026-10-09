using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public static class Loc
    {
        static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "hud.level", "LV {0}" },
            { "hud.xp", "XP {0}/{1}" },
            { "hud.score", "SCORE {0}" },
            { "levelup.title", "Level reached" },
            { "levelup.sub", "Choose an upgrade" },
            { "gameover.title", "Signal lost" },
            { "gameover.sub", "The ship was destroyed" },
            { "gameover.score", "Score {0}" },
            { "gameover.retry", "Play again" },
            { "gameover.menu", "Main menu" },
            { "upgrade.fire.title", "Fire rate" },
            { "upgrade.fire.desc", "Time between shots -10%" },
            { "upgrade.multi.title", "Multishot" },
            { "upgrade.multi.desc", "+1 projectile in an arc" },
            { "upgrade.speed.title", "Fast projectile" },
            { "upgrade.speed.desc", "Bullet speed +20%" },
            { "upgrade.level", "Lv {0}" },
            { "upgrade.shield.title", "Shield" },
            { "upgrade.shield.desc", "Blocks a hit, then recharges" },
            { "upgrade.shield.up", "More hits and a faster recharge" },
            { "upgrade.pulse.title", "Pulse" },
            { "upgrade.pulse.desc", "A damage wave expands from the ship" },
            { "upgrade.pulse.up", "Wider wave, fired more often" },
            { "upgrade.orb.title", "Orbs" },
            { "upgrade.orb.desc", "Two orbs spin and hurt on touch" },
            { "upgrade.orb.up", "More orbs and more damage" },
            { "upgrade.missile.title", "Missile" },
            { "upgrade.missile.desc", "Impact leaves a damaging area" },
            { "upgrade.missile.up", "More missiles and a larger area" },
            { "upgrade.placeholder", "Upgrade" },
            { "magnet.on", "Magnet" },
            { "menu.loading", "Loading" },
            { "menu.play", "Play" },
            { "menu.achievements", "Achievements" },
            { "menu.back", "Back" },
            { "menu.maps", "Select a map" },
            { "menu.locked", "Locked" },
            { "menu.best", "Best {0}" },
            { "menu.need", "Score {0} on {1}" },
            { "menu.hint", "Hold the mouse button to fly" },
            { "spawn.mini", "Mini-boss" },
            { "spawn.boss", "Boss incoming" },
            { "ach.title", "Achievements" },
            { "ach.done", "Unlocked" },
            { "ach.progress", "{0}/{1}" },
            { "ach.toast", "Achievement unlocked: {0}" },
            { "map.0.name", "Drift" },
            { "map.0.blurb", "The neon outskirts" },
            { "map.1.name", "Pulse" },
            { "map.1.blurb", "Faster, denser swarms" },
            { "map.2.name", "Core" },
            { "map.2.blurb", "The magenta heart" },
            { "ach.first_kill.desc", "Destroy an enemy" },
            { "ach.run_25.desc", "Destroy {0} enemies in one run" },
            { "ach.life_100.desc", "Destroy {0} enemies" },
            { "ach.level_5.desc", "Reach level {0}" },
            { "ach.score_400.desc", "Score {0} points in one run" },
            { "ach.score_900.desc", "Score {0} points in one run" },
            { "ach.map_2.desc", "Unlock the second map" },
            { "ach.map_3.desc", "Unlock the third map" },
            { "ach.arsenal.desc", "Pick all three upgrades in one run" },
            { "ach.survive_90.desc", "Survive for {0} seconds" },
            { "ach.first_kill.title", "First contact" },
            { "ach.run_25.title", "Hunter" },
            { "ach.life_100.title", "Exterminator" },
            { "ach.level_5.title", "Ascent" },
            { "ach.score_400.title", "Pilot" },
            { "ach.score_900.title", "Veteran" },
            { "ach.map_2.title", "Explorer" },
            { "ach.map_3.title", "Cartographer" },
            { "ach.arsenal.title", "Arsenal" },
            { "ach.survive_90.title", "Endurance" }
        };

        static readonly Dictionary<string, string> Portuguese = new Dictionary<string, string>
        {
            { "hud.level", "NV {0}" },
            { "hud.xp", "XP {0}/{1}" },
            { "hud.score", "PONTOS {0}" },
            { "levelup.title", "Nível alcançado" },
            { "levelup.sub", "Escolha uma melhoria" },
            { "gameover.title", "Sinal perdido" },
            { "gameover.sub", "A nave foi destruída" },
            { "gameover.score", "Pontuação {0}" },
            { "gameover.retry", "Jogar de novo" },
            { "gameover.menu", "Menu inicial" },
            { "upgrade.fire.title", "Cadência" },
            { "upgrade.fire.desc", "Intervalo entre tiros -10%" },
            { "upgrade.multi.title", "Tiro múltiplo" },
            { "upgrade.multi.desc", "+1 projétil em arco" },
            { "upgrade.speed.title", "Projétil rápido" },
            { "upgrade.speed.desc", "Velocidade do tiro +20%" },
            { "upgrade.level", "Nv {0}" },
            { "upgrade.shield.title", "Escudo" },
            { "upgrade.shield.desc", "Bloqueia um golpe e recarrega" },
            { "upgrade.shield.up", "Aguenta mais golpes e volta mais rápido" },
            { "upgrade.pulse.title", "Pulso" },
            { "upgrade.pulse.desc", "Onda de dano que cresce a partir da nave" },
            { "upgrade.pulse.up", "Onda maior e mais frequente" },
            { "upgrade.orb.title", "Orbes" },
            { "upgrade.orb.desc", "Dois orbes giram e ferem ao tocar" },
            { "upgrade.orb.up", "Mais orbes e mais dano" },
            { "upgrade.missile.title", "Míssil" },
            { "upgrade.missile.desc", "Ao acertar, deixa uma área de dano" },
            { "upgrade.missile.up", "Mais mísseis e área maior" },
            { "upgrade.placeholder", "Melhoria" },
            { "magnet.on", "Ímã" },
            { "menu.loading", "Carregando" },
            { "menu.play", "Jogar" },
            { "menu.achievements", "Conquistas" },
            { "menu.back", "Voltar" },
            { "menu.maps", "Escolha um mapa" },
            { "menu.locked", "Bloqueado" },
            { "menu.best", "Recorde {0}" },
            { "menu.need", "Faça {0} pontos em {1}" },
            { "menu.hint", "Segure o mouse para voar" },
            { "spawn.mini", "Mini-chefe" },
            { "spawn.boss", "Chefe à vista" },
            { "ach.title", "Conquistas" },
            { "ach.done", "Conquistada" },
            { "ach.progress", "{0}/{1}" },
            { "ach.toast", "Conquista: {0}" },
            { "map.0.name", "Deriva" },
            { "map.0.blurb", "Os arredores neon" },
            { "map.1.name", "Pulso" },
            { "map.1.blurb", "Enxames mais rápidos" },
            { "map.2.name", "Núcleo" },
            { "map.2.blurb", "O coração magenta" },
            { "ach.first_kill.desc", "Destrua um inimigo" },
            { "ach.run_25.desc", "Destrua {0} inimigos em uma partida" },
            { "ach.life_100.desc", "Destrua {0} inimigos" },
            { "ach.level_5.desc", "Alcance o nível {0}" },
            { "ach.score_400.desc", "Faça {0} pontos em uma partida" },
            { "ach.score_900.desc", "Faça {0} pontos em uma partida" },
            { "ach.map_2.desc", "Desbloqueie o segundo mapa" },
            { "ach.map_3.desc", "Desbloqueie o terceiro mapa" },
            { "ach.arsenal.desc", "Escolha as três melhorias na mesma partida" },
            { "ach.survive_90.desc", "Sobreviva por {0} segundos" },
            { "ach.first_kill.title", "Primeiro contato" },
            { "ach.run_25.title", "Caçador" },
            { "ach.life_100.title", "Exterminador" },
            { "ach.level_5.title", "Ascensão" },
            { "ach.score_400.title", "Piloto" },
            { "ach.score_900.title", "Veterano" },
            { "ach.map_2.title", "Explorador" },
            { "ach.map_3.title", "Cartógrafo" },
            { "ach.arsenal.title", "Arsenal" },
            { "ach.survive_90.title", "Resistência" }
        };

        static bool ready;

        public static SystemLanguage Language { get; private set; } = SystemLanguage.English;

        public static SystemLanguage Resolve(SystemLanguage system)
        {
            if (system == SystemLanguage.Portuguese)
                return SystemLanguage.Portuguese;
            return SystemLanguage.English;
        }

        public static void Use(SystemLanguage system)
        {
            Language = Resolve(system);
            ready = true;
        }

        public static void Ensure()
        {
            if (ready)
                return;
            Use(Application.systemLanguage);
        }

        public static string Get(string key)
        {
            Ensure();
            string value;
            Dictionary<string, string> table = Language == SystemLanguage.Portuguese ? Portuguese : English;
            if (table.TryGetValue(key, out value))
                return value;
            if (English.TryGetValue(key, out value))
                return value;
            return key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
