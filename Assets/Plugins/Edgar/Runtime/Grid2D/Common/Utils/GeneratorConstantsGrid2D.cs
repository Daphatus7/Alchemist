namespace Edgar.Unity
{
    /// <remarks>
    /// Constants that are used in the generator.
    /// </remarks>
    public static class GeneratorConstantsGrid2D
    {
        /// <summary>
        /// Name of the game object that holds tilemaps layers. This name is used both in room templates and in generated levels.
        /// </summary>
        private static string tilemapsRootName = "Tilemaps"; public static string TilemapsRootName => tilemapsRootName;
        
        
        private static string tilemapRootAlternativeName = "Grid"; public static string TilemapRootAlternativeName => tilemapRootAlternativeName;

        
   
        private static string roomsRootName = "Rooms";
        /// <summary>
        /// Name of the game object that holds instance of all the room templates that are used in a generated level.
        /// </summary>
        public static string RoomsRootName => roomsRootName;
     

        private static string outlineOverrideLayerName = "Outline Override";
        /// <summary>
        /// Name of the Outline Override tilemap layer.
        /// </summary>
        public static string OutlineOverrideLayerName => outlineOverrideLayerName;
    }
}