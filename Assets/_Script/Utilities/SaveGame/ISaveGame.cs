namespace _Script.Utilities.ServiceLocator
{
    public interface ISaveGame : IGameService
    {
        string SaveKey { get; }
        /// <summary>
        /// Called by a more superior class to save data
        /// </summary>
        /// <returns></returns>
        object OnSaveData();
        /// <summary>
        /// Implemented class Actively load saved data from SaveSystem
        /// </summary>
        void OnLoadData(object data);
        /// <summary>
        /// act as fallback when no data is found
        /// </summary>
        void LoadDefaultData();
    }
    
    public interface ISaveTileMap : ISaveGame
    {
        
    }

    //include 
    public interface ISaveGameManager : ISaveGame
    {
        
    }
}