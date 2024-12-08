using UnityEngine;

namespace _Script.Map.Hexagon_Graph
{
    public interface INodeHandle
    {
        void Highlight(bool isHighlighted);
        Vector3Int GetPosition();
        void SetNodeComplete();
    }
}