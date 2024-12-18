using UnityEngine;

namespace _Script.Map.WorldMap
{
    public interface INodeHandle
    {
        Vector3Int GetPosition();
        void Highlight(bool state);
        
        /**
         * Show visual that the node is completed and cannot be explored again
         */
        void SetNodeComplete();
        
        /**
         * Show visual that the player is exploring the node
         */
        void SetNodeExploring();
    }
}