namespace _Script.Map.Hexagon_Graph
{
    public class GridConfiguration
    {
        public float HexSize;
        public int GridRadius;
        public readonly int ObstacleWeight;
        public readonly int ResourceWeight;
        public readonly int EnemyWeight;
        public readonly int CampfireWeight;
        public readonly int BossWeight;
        
        public GridConfiguration(float hexSize, int gridRadius = 5, 
            int obstacleWeight = 5, 
            int resourceWeight = 3, 
            int enemyWeight = 5, 
            int campfireWeight = 2, 
            int bossWeight = 1)
        {
            HexSize = hexSize;
            GridRadius = gridRadius;
            ObstacleWeight = obstacleWeight;
            ResourceWeight = resourceWeight;
            EnemyWeight = enemyWeight;
            CampfireWeight = campfireWeight;
            BossWeight = bossWeight;
        }
    }
}