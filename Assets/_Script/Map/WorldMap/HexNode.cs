using System;
using _Script.Map.WorldMap.MapNode;
using UnityEngine;

namespace _Script.Map.WorldMap
{
    public enum NodeType
    {
        Empty,
        Obstacle,
        Resource,
        Enemy,
        Boss,
        Bonfire,
    }

    public enum NodeExplorationState
    {
        Unrevealed, // Not visible at all
        Revealed,   // Visible on the map but not yet explored
        Exploring,  // Currently being explored (player entered this node's map)
        Explored    // Exploration finished, cannot be re-explored
    }

    public class HexNode
    {
        // Cube coordinates
        private Vector3Int _position; 
        public Vector3Int Position => _position;

        public HexNode Parent;

        public NodeType NodeType { get; set; }

        private NodeData _nodeData;

        public NodeData NodeData
        {
            get => _nodeData;
            set => _nodeData = value;
        }
        
        public float InterpolatedValue { get; set; }
        
        public int gCost; // Cost from the start node
        public int hCost; // Cost to the goal node
        public int fCost => gCost + hCost; // Total cost

        public int NodeLevel => (Mathf.Abs(_position.x) + Mathf.Abs(_position.y) + Mathf.Abs(_position.z)) / 2;

        public bool IsBlocked { get; private set; }

        private NodeExplorationState _explorationState = NodeExplorationState.Unrevealed;
        public NodeExplorationState ExplorationState => _explorationState;

        // Directions to get the neighbors of a HexNode
        public static readonly (int x, int y, int z)[] directions = new (int, int, int)[]
        {
            (1, -1, 0),
            (1, 0, -1),
            (0, 1, -1),
            (-1, 1, 0),
            (-1, 0, 1),
            (0, -1, 1)
        };

        // Constructor
        public HexNode(Vector3Int position, NodeType nodeType)
        {
            _position = position;
            if (_position.x + _position.y + _position.z != 0)
                throw new ArgumentException("Invalid cube coordinates");
            NodeType = nodeType;
            if (nodeType == NodeType.Obstacle)
                IsBlocked = true;
        }

        // Update exploration state
        public void SetExplorationState(NodeExplorationState state)
        {
            _explorationState = state;
        }

        // Calculate the distance between two HexNodes
        public int Distance(HexNode other)
        {
            return (Mathf.Abs(_position.x - other.Position.x) 
                  + Mathf.Abs(_position.y - other.Position.y) 
                  + Mathf.Abs(_position.z - other.Position.z)) / 2;
        }

        // Override Equals and GetHashCode
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            HexNode other = (HexNode)obj;
            return _position.x == other.Position.x && _position.y == other.Position.y && _position.z == other.Position.z;
        }

        public override int GetHashCode()
        {
            unchecked // Prevent overflow
            {
                int hash = 17;
                hash = hash * 23 + _position.x.GetHashCode();
                hash = hash * 23 + _position.y.GetHashCode();
                hash = hash * 23 + _position.z.GetHashCode();
                return hash;
            }
        }

        // Convert cube coordinates to world position (2D)
        public Vector2 CubeToWorldPosition2D(float hexSize)
        {
            float xPosition = hexSize * (Mathf.Sqrt(3f) * (_position.x + _position.z / 2f));
            float yPosition = hexSize * (3f / 2f * _position.z);
            return new Vector2(xPosition, yPosition);
        }
    }
}