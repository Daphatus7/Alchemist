// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 47

using System;
using UnityEngine;
using _Script.Character.PlayerRank;

namespace _Script.Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public string enemyID;
        
        // Base stats (unscaled)
        public int health;
        public int damage;
        public float moveSpeed;
        public float attackFrequency;
        public float attackRange;
        
        public MonsterType monsterType;
        public Drop.DropTable.DropTable dropTable;
        public Sprite sprite;

        /// <summary>
        /// Creates a new EnemyAttribute with stats
        /// scaled by the player's rank.
        /// </summary>
        public EnemyAttribute CreateScaledAttribute(PlayerRankEnum rank)
        {
            float healthMod = EnemyScalable.GetHealthModifier(rank);
            float damageMod = EnemyScalable.GetDamageModifier(rank);
            float speedMod  = EnemyScalable.GetSpeedModifier(rank);

            // Apply multipliers to base stats
            int finalHealth = Mathf.RoundToInt(health * healthMod);
            int finalDamage = Mathf.RoundToInt(damage * damageMod);
            float finalSpeed  = moveSpeed * speedMod;
            return new EnemyAttribute(
                finalHealth,
                finalDamage,
                finalSpeed,
                attackFrequency,
                attackRange,
                monsterType,
                dropTable,
                sprite
            );
        }
    }

    [Serializable]
    public class EnemyAttribute
    {
        public int Health;
        public int Damage;
        public float MoveSpeed;
        public float AttackFrequency;
        public float AttackRange;
        public MonsterType MonsterType;
        public Drop.DropTable.DropTable DropTable;
        public Sprite Sprite;

        public EnemyAttribute(
            int health,
            int damage,
            float moveSpeed,
            float attackFrequency,
            float attackRange,
            MonsterType monsterType,
            Drop.DropTable.DropTable dropTable,
            Sprite sprite)
        {
            Health = health;
            Damage = damage;
            MoveSpeed = moveSpeed;
            AttackFrequency = attackFrequency;
            AttackRange = attackRange;
            MonsterType = monsterType;
            DropTable = dropTable;
            Sprite = sprite;
        }
    }

    public enum MonsterType
    {
        Melee,
        Ranged,
        Caster,
        Minion
    }

    /// <summary>
    /// Utility class that returns scaling multipliers
    /// for health, damage, and movement speed
    /// based on the player's rank.
    /// </summary>
    public static class EnemyScalable
    {
        public static float GetHealthModifier(PlayerRankEnum rank)
        {
            switch (rank)
            {
                case PlayerRankEnum.G:
                    return 1f;
                case PlayerRankEnum.F:
                    return 1.5f;
                case PlayerRankEnum.E:
                    return 2f;
                case PlayerRankEnum.D:
                    return 2.5f;
                // Example: C, B, A, S, Ss, Sss all share the same multiplier
                case PlayerRankEnum.C:
                case PlayerRankEnum.B:
                case PlayerRankEnum.A:
                case PlayerRankEnum.S:
                case PlayerRankEnum.Ss:
                case PlayerRankEnum.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Fallback to 3x health.");
                    return 3f;
            }
        }

        public static float GetDamageModifier(PlayerRankEnum rank)
        {
            switch (rank)
            {
                case PlayerRankEnum.G:
                    return 1f;
                case PlayerRankEnum.F:
                    return 1.5f;
                case PlayerRankEnum.E:
                    return 2f;
                case PlayerRankEnum.D:
                    return 2.5f;
                case PlayerRankEnum.C:
                case PlayerRankEnum.B:
                case PlayerRankEnum.A:
                case PlayerRankEnum.S:
                case PlayerRankEnum.Ss:
                case PlayerRankEnum.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Fallback to 3x damage.");
                    return 3f;
            }
        }

        public static float GetSpeedModifier(PlayerRankEnum rank)
        {
            switch (rank)
            {
                case PlayerRankEnum.G:
                    return 1f;
                case PlayerRankEnum.F:
                    return 1.1f;
                case PlayerRankEnum.E:
                    return 1.3f;
                case PlayerRankEnum.D:
                    return 1.5f;
                case PlayerRankEnum.C:
                    return 1.5f;
                case PlayerRankEnum.B:
                    return 1.6f;
                case PlayerRankEnum.A:
                    return 1.7f;
                case PlayerRankEnum.S:
                    return 2f;
                case PlayerRankEnum.Ss:
                    return 2f;
                case PlayerRankEnum.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Extermination speed modifier.");
                    return 3f;
            }
        }
    }
}