using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Hexagon_Graph
{
    public class HexNode
    {
        // Cube coordinates
        private readonly Vector3Int position; public Vector3Int Position => position;

        public HexNode parent; // For retracing the path
        
        private readonly NodeType nodeType = NodeType.Empty; public NodeType NodeType => nodeType;
        
        public  int gCost; // Cost from the start node
        public int hCost; // Cost to the goal node
        public int fCost => gCost + hCost; // Total cost

        public bool isExplored = false;
        
        public bool isRevealed = false;
        
        public bool isBlocked = false;

        // Constructor
        public HexNode(Vector3Int position, NodeType nodeType)
        {
            this.position = position;
            if(this.position.x + this.position.y + this.position.z != 0)
                throw new ArgumentException("Invalid cube coordinates");
            this.nodeType = nodeType;
            if(nodeType == NodeType.Obstacle)
                isBlocked = true;
        }

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
        

        // Calculate the distance between two HexNodes
        public int Distance(HexNode other)
        {
            return (Mathf.Abs(position.x - other.Position.x) + Mathf.Abs(position.y - other.Position.y) + Mathf.Abs(position.z - other.Position.z)) / 2;
        }

        // Override Equals and GetHashCode
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            HexNode other = (HexNode)obj;
            return position.x == other.Position.x && position.y == other.Position.y && position.z == other.Position.z;
        }

        public override int GetHashCode()
        {
            unchecked // Prevent overflow
            {
                int hash = 17;
                hash = hash * 23 + position.x.GetHashCode();
                hash = hash * 23 + position.y.GetHashCode();
                hash = hash * 23 + position.z.GetHashCode();
                return hash;
            }
        }

        // Convert cube coordinates to world position (2D)
        public Vector2 CubeToWorldPosition2D(float hexSize)
        {
            float xPosition = hexSize * (Mathf.Sqrt(3f) * (position.x + position.z / 2f));
            float yPosition = hexSize * (3f / 2f * position.z);
            return new Vector2(xPosition, yPosition);
        }
    }
    public enum NodeType
    {
        Empty,
        Obstacle,
        Resource,
        Enemy,
        Boss,
        Campfire,
    }
}