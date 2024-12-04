using UnityEngine.Events;

namespace _Script.Character
{
    public interface IPlayerUIHandle
    {
        UnityEvent GetPlayerHealthUpdateEvent();
        UnityEvent<int> PlayerGoldUpdateEvent();
        float GetPlayerHealth();
        float GetPlayerMaxHealth();
        int GetPlayerGold();
    }

}