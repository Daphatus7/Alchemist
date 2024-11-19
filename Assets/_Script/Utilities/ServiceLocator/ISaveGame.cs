namespace _Script.Utilities.ServiceLocator
{
    public interface ISaveGame : IGameService
    {
        object OnSaveData();
        void OnLoadData(object data);
        void LoadDefaultData();
    }
}