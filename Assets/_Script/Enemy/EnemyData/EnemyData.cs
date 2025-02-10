// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 47

using System;
using UnityEngine;

namespace _Script.Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Data/EnemyStats")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public string enemyID;
        public int health;
        public int damage;
        public int moveSpeed;
        public int attackFrequency;
        public int attackRange;
        public MonsterType monsterType;
        public Drop.DropTable.DropTable dropTable;
        public Sprite sprite;
    }
    
    [Serializable]
    public class EnemyAttribute
    {
        public int Health;
        public int Damage;
        public int MoveSpeed;
        public int AttackFrequency;
        public int AttackRange;
        public MonsterType MonsterType;
        public Drop.DropTable.DropTable DropTable;
        public Sprite Sprite;
        
        public EnemyAttribute(int health, int damage, int moveSpeed, int attackFrequency, int attackRange, 
            MonsterType monsterType, Drop.DropTable.DropTable dropTable, Sprite sprite)
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
}