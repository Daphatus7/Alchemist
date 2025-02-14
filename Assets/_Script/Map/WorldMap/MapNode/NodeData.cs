// Author : Peiyu Wang @ Daphatus
// 08 12 2024 12 48

using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace _Script.Map.WorldMap.MapNode
{
    /// <summary>
    /// Base class representing data for a particular node on the world map. 
    /// Subclasses should define the NodeType and any additional behavior or data required.
    /// </summary>
    [InfoBox("This ScriptableObject represents node data used to generate maps in the game.")]
    public abstract class NodeData : ScriptableObject
    {
        /// <summary>
        /// The type of node this data represents. Must be defined by subclasses.
        /// </summary>
        public abstract NodeType NodeType { get; }

        [ShowInInspector, ReadOnly, LabelText("Map Name"), Tooltip("Automatically generated map name based on the node type.")]
        public string MapName => "E_" + NodeType + "Map";
        public virtual string Description => "This is a boss node";
        public int NodeID { get; set; }
    }
    

}