using UnityEngine.Events;

namespace _Script.Character
{
    public interface IPlayerUIHandle
    {
        UnityEvent GetPlayerHealthUpdateEvent();
        float GetPlayerHealth();
        float GetPlayerMaxHealth();
    }

}