// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 47

using System;
using UnityEngine;
using _Script.Character.PlayerRank;
using Sirenix.OdinInspector;
using UnityEditor;

namespace _Script.Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly, ShowInInspector]
        public string enemyID;
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (enemyID == "")
            {
                enemyID = GUID.Generate().ToString();
                EditorUtility.SetDirty(this);
            }
#endif
        }
        public string enemyName;
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
        public EnemyAttribute CreateScaledAttribute(NiRank rank)
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
        public static float GetHealthModifier(NiRank rank)
        {
            switch (rank)
            {
                case NiRank.G:
                    return 1f;
                case NiRank.F:
                    return 1.5f;
                case NiRank.E:
                    return 2f;
                case NiRank.D:
                    return 2.5f;
                // Example: C, B, A, S, Ss, Sss all share the same multiplier
                case NiRank.C:
                    return 3f;
                case NiRank.B:
                    return 4f;
                case NiRank.A:
                    return 5f;
                case NiRank.S:
                case NiRank.Ss:
                case NiRank.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Fallback to 3x health.");
                    return 3f;
            }
        }

        public static float GetDamageModifier(NiRank rank)
        {
            switch (rank)
            {
                case NiRank.G:
                    return 1f;
                case NiRank.F:
                    return 1.5f;
                case NiRank.E:
                    return 2f;
                case NiRank.D:
                    return 2.5f;
                case NiRank.C:
                case NiRank.B:
                case NiRank.A:
                case NiRank.S:
                case NiRank.Ss:
                case NiRank.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Fallback to 3x damage.");
                    return 3f;
            }
        }

        public static float GetSpeedModifier(NiRank rank)
        {
            switch (rank)
            {
                case NiRank.G:
                    return 1f;
                case NiRank.F:
                    return 1.1f;
                case NiRank.E:
                    return 1.3f;
                case NiRank.D:
                    return 1.5f;
                case NiRank.C:
                    return 1.5f;
                case NiRank.B:
                    return 1.6f;
                case NiRank.A:
                    return 1.7f;
                case NiRank.S:
                    return 2f;
                case NiRank.Ss:
                    return 2f;
                case NiRank.Sss:
                    return 3f;
                default:
                    Debug.LogError($"Invalid rank {rank}. Extermination speed modifier.");
                    return 3f;
            }
        }
    }
}