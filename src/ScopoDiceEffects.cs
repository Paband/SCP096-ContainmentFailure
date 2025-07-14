using MysteryDice;
using MysteryDice.Effects;
using UnityEngine;

namespace Scopophobia.Dice
{
    internal class ShyGuySpawn : IEffect
    {
        public string Name => "ShyGuySpawn";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Oh no, shy guy spawning";

        public void Use()
        {
            int ShyGuySpawnChance = Random.Range(5, 10);
            if (MysteryDice.Patches.GetEnemies.allEnemies.Find(x => x.enemyName == "Shy guy") == null) return;
                MysteryDice.Misc.SpawnEnemyForced(StartOfRound.Instance.currentLevel.Enemies.Find(x => x.enemyType.enemyName == "Shy guy"), 1, false, false);
            }
        }
    }
