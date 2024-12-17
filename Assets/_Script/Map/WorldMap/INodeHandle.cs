using UnityEngine;

namespace _Script.Map.WorldMap
{
    public interface INodeHandle
    {
        Vector3Int GetPosition();
        void Highlight(bool state);
        void SetNodeComplete();
        void SetNodeExploring();
    }
}