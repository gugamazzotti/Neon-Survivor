# Neon Survivor

The ship does not stop. Neither does the swarm.

Neon Survivor is a portrait survival game in the spirit of *Vampire Survivors*. Hold the pointer to steer, the ship fires on its own at the nearest enemy, and every kill leaves experience in the void. The run keeps growing until the hull gives out. Between runs, coins become new ships, best scores open denser maps, and achievements stay in the hangar.

Play in the browser: **[gugamazzotti.github.io/Neon-Survivor](https://gugamazzotti.github.io/Neon-Survivor/)**

The game follows the system language. Portuguese and English both cover the menu, HUD, maps, ships, and achievements.

## How to fly

The screen is vertical. The camera stays close so you can read what is coming.

- Hold the mouse or a finger and the ship slides toward the pointer.
- Let go and it keeps drifting forward, slowly. It never sits still.
- Fire is automatic and aims at the nearest enemy.
- Leveling up pauses the run and offers three shuffled upgrades. Pick one.
- The run score is the experience you collect. Each orb is worth 10.

Colors so the firefight stays readable:

| What you see | What it means |
| --- | --- |
| Red outline | Enemy |
| Yellow outline | Pickup |
| Green-yellow bolt | Your shot |
| Red bolt | Enemy shot |

## What falls in space

| Item | Effect |
| --- | --- |
| XP orb | Stays where the enemy died. It only comes to you up close, or while a magnet is active. |
| Magnet | Pulls orbs from any distance for a while. Bosses and mini-bosses drop one more often. |
| Coin | Worth 1. Spawns around the arena and goes straight to the hangar. |
| Green cross | Restores all HP. Shows up from time to time, one on the map at most. If the hull is already full, it waits. Bosses always drop one; mini-bosses sometimes do. |

## Three maps

Each map remembers the best score on the one before it.

| Map | To enter | Mood |
| --- | --- | --- |
| Drift | Open | The neon outskirts. The swarm still leaves room to breathe. |
| Pulse | 400 points on Drift | Spawns come faster and enemies move harder. |
| Core | 900 points on Pulse | The magenta heart. Short intervals and high speed. |

## Hangar

The starter ship is Vertex. The others are bought with coins found in a run. The choice is saved and used on the next flight.

| Ship | Price | Look |
| --- | --- | --- |
| Vertex | Free | The patrol fighter. |
| Blade | 24 | A long dart nose. |
| Bastion | 60 | A wide armored hull. |
| Comet | 110 | Swept crescent wings. |

## Upgrades

On each level, three cards. You can pick the same one again and evolve it. Taking the first three in a single run completes the Arsenal achievement.

- **Fire rate.** Less time between shots.
- **Multishot.** One more projectile in an arc.
- **Fast projectile.** Shots travel faster.
- **Shield.** Blocks a hit, then recharges. Later ranks take more hits and come back sooner.
- **Pulse.** A damage wave expands from the ship. Later ranks reach farther and fire more often.
- **Orbs.** Two orbs spin and hurt on touch. Later ranks add orbs and damage.
- **Missile.** The impact leaves a damaging area. Later ranks fire more missiles over a larger area.

## Who is chasing you

Eight silhouettes, all with a red outline. Some run straight at you, some are slow and tough, some shoot from a distance, some weave, some charge, and two are larger: a mini-boss in orbit and a boss that fires a burst.

Touching them hurts. Some enemy shots do too. HP sits in the top bar of the HUD, inside the neon frame.

## Achievements

They live in the menu and pop a notice during the run.

- First contact: destroy an enemy.
- Hunter: destroy 25 enemies in one run.
- Exterminator: destroy 100 enemies in total.
- Ascent: reach level 5.
- Pilot: score 400 points in one run.
- Veteran: score 900 points in one run.
- Explorer: unlock Pulse.
- Cartographer: unlock Core.
- Arsenal: pick fire rate, multishot, and fast projectile in the same run.
- Endurance: survive for 90 seconds.

## Open the project

Unity **2022.3**. Open the repository folder, let the `MainMenu` scene load, and press Play. The menu builds its interface at runtime. Play drops you into the arena.

Combat does not use the engine physics system. Movement, shots, and pickups are distance and direction. The look is neon over a starfield, with bloom.

The browser build lives in `docs/`, and that folder is what GitHub Pages publishes. In the editor, the shortcut is **Neon Survivor → Build WebGL Pages**.
