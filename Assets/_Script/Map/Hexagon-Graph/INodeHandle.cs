using UnityEngine;

namespace _Script.Hexagon_Graph
{
    public interface INodeHandle
    {
        void Highlight(bool isHighlighted);
        Vector3Int GetPosition();
    }
}