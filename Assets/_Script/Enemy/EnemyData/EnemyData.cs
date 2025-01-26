// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 47

using UnityEngine;

namespace _Script.Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Data/EnemyStats")]
    public class EnemyData : ScriptableObject
    {
        public string monsterName;
        public int health;
        public int damage;
        public int moveSpeed;
        public int attackFrequency;
        public int attackRange;
        public MonsterType monsterType;
        public Drop.DropTable.DropTable dropTable;
        public Sprite sprite;

        public EnemyData(string monsterName, int health, int damage, int moveSpeed, int attackFrequency, int attackRange, 
            MonsterType monsterType, Drop.DropTable.DropTable dropTable, Sprite sprite)
        {
            this.monsterName = monsterName;
            this.health = health;
            this.damage = damage;
            this.moveSpeed = moveSpeed;
            this.attackFrequency = attackFrequency;
            this.attackRange = attackRange;
            this.monsterType = monsterType;
            this.dropTable = dropTable;
            this.sprite = sprite;
        }
    }
    
    public class MonsterAttribute
    {
        public int health;
        public int damage;
        public int moveSpeed;
        public int attackFrequency;
        public int attackRange;
        public MonsterType monsterType;
        public Drop.DropTable.DropTable dropTable;
        public Sprite sprite;
        
        public MonsterAttribute(int health, int damage, int moveSpeed, int attackFrequency, int attackRange, 
            MonsterType monsterType, Drop.DropTable.DropTable dropTable, Sprite sprite)
        {
            this.health = health;
            this.damage = damage;
            this.moveSpeed = moveSpeed;
            this.attackFrequency = attackFrequency;
            this.attackRange = attackRange;
            this.monsterType = monsterType;
            this.dropTable = dropTable;
            this.sprite = sprite;
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