using System;
using System.Collections.Generic;
using _Script.Character;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting; // Import Odin

namespace _Script.Items.AbstractItemTypes
{
    namespace _Script.Items
    {
        [System.Serializable]
        public abstract class ItemData : ScriptableObject
        {
            [Title("Basic Info")] [SerializeField, Tooltip("Name of the item")]
            public string itemName;

            [SerializeField, Tooltip("Unique ID of the item")]
            public string itemID;

            [SerializeField, TextArea, Tooltip("Detailed description of the item")]
            public string itemDescription;

            [Title("Visuals")] [SerializeField, Tooltip("Icon representing the item"), PreviewField(75)]
            public Sprite itemIcon;

            [Title("Stacking & Rarity")] [SerializeField, Tooltip("Maximum stack size for this item")]
            public int maxStackSize = 1;

            public int MaxStackSize
            {
                get => maxStackSize;
                set => maxStackSize = value;
            }

            public ItemShapeType itemShapeType = ItemShapeType.Square11;
            private ItemShape _itemShape;

            public ItemShape ItemShape
            {
                get => _itemShape ??= new ItemShape(itemShapeType);
                set => _itemShape = value;
            }

            [SerializeField, Tooltip("Rarity of the item")]
            public Rarity rarity;


            [SerializeField] private int _value = 1;

            public int Value
            {
                get => _value;
                set => _value = value;
            }

            public int GetPivotIndex(bool isRotated)
            {
                return ItemShape.GetShapePivotIndex(itemShapeType, isRotated);
            }
            
            /// <summary>
            /// Offset for rendering display of the item.
            /// Hardcoded solution
            /// </summary>
            /// <param name="isRotated"></param>
            /// <returns></returns>
            public Vector3 GetRenderingOffset(bool isRotated)
            {
                return ItemShape.GetShapeRenderingOffset(itemShapeType, isRotated);
            }
            
            [Title("Read-Only Debug Info"), ReadOnly, ShowInInspector]
            public Sprite ItemSprite => itemIcon;

            [ReadOnly, ShowInInspector]
            public string ItemName
            {
                get => itemName;
                set => itemName = value;
            }

            [ReadOnly, ShowInInspector]
            public string ItemID
            {
                get => itemID;
                set => itemID = value;
            }

            [ReadOnly, ShowInInspector]
            public string ItemDescription
            {
                get => itemDescription;
                set => itemDescription = value;
            }

            [ReadOnly, ShowInInspector] public abstract ItemType ItemType { get; }
            public abstract string ItemTypeString { get; }

            /// <summary>
            /// Use the item. Applies effects to the player (e.g., equip, consume).
            /// </summary>
            /// <param name="playerCharacter">The player character to apply effects to.</param>
            /// <returns>True if the item was used successfully; false otherwise.</returns>
            public abstract bool Use(PlayerCharacter playerCharacter);

            public bool Equals(ItemData other)
            {
                return other != null && itemName == other.itemName;
            }
        }

        public enum ItemType
        {
            Equipment,
            Consumable,
            Material,
            Seed,
            Fruit,
            Torch,
            Container
        }

        public enum Rarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }


        public enum ItemShapeType
        {
            Square11,
            Square22,
            Rectangle12,
            Rectangle23,

            /**
             * Circle with radius 1
             */
            Circle1,

            /**
             * L shape with 3 blocks
             */
            LShape2,

            /**
             * L shape with 5 blocks
             */
            LShape3,
            Stick13,
        }

        public class ItemShape
        {
            private List<Vector2Int> _positions;
            private readonly ItemShapeType _shapeType;
            
            //comparing the two shapes
            public bool CompareShapes(ItemShape other)
            {
                return other._shapeType == _shapeType;
            }


            public static int GetShapePivotIndex(ItemShapeType shape, bool isRotated)
            {
                if (!isRotated)
                {
                    return shape switch
                    {
                        ItemShapeType.Square11 => 0,
                        ItemShapeType.Square22 => 0,
                        ItemShapeType.Rectangle12 => 0,
                        ItemShapeType.Rectangle23 => 0,
                        ItemShapeType.Circle1 => 0,
                        ItemShapeType.LShape2 => 0,
                        ItemShapeType.LShape3 => 0,
                        ItemShapeType.Stick13 => 0,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                else
                {
                    return shape switch
                    {
                        ItemShapeType.Square11 => 0,
                        ItemShapeType.Square22 => 0,
                        ItemShapeType.Rectangle12 => 0,
                        ItemShapeType.Rectangle23 => 0,
                        ItemShapeType.Circle1 => 0,
                        ItemShapeType.LShape2 => 0,
                        ItemShapeType.LShape3 => 0,
                        ItemShapeType.Stick13 => 1,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            public static Vector3 GetShapeRenderingOffset(ItemShapeType shape, bool isRotated)
            {
                if (!isRotated)
                {
                    return shape switch
                    {
                        ItemShapeType.Square11 => new Vector3(0, 0, 0),
                        ItemShapeType.Square22 => new Vector3(0, 0, 0),
                        ItemShapeType.Rectangle12 => new Vector3(0, 0, 0),
                        ItemShapeType.Rectangle23 => new Vector3(0, 0, 0),
                        ItemShapeType.Circle1 => new Vector3(0, 0, 0),
                        ItemShapeType.LShape2 => new Vector3(0, 0, 0),
                        ItemShapeType.LShape3 => new Vector3(0, 0, 0),
                        ItemShapeType.Stick13 => new Vector3(0, 0, 0),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                
                //Default case, no offset
                return shape switch
                {
                    ItemShapeType.Square11 => new Vector3(0, 0, 0),
                    ItemShapeType.Square22 => new Vector3(0, 50, 0),
                    ItemShapeType.Rectangle12 => new Vector3(25, 25, 0),
                    ItemShapeType.Rectangle23 => new Vector3(0, 0, 0),
                    ItemShapeType.Circle1 => new Vector3(0, 0, 0),
                    ItemShapeType.LShape2 => new Vector3(0, 0, 0),
                    ItemShapeType.LShape3 => new Vector3(0, 0, 0),
                    ItemShapeType.Stick13 => new Vector3(0, 50, 0),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            
            
            public Vector2 IconScale
            {
                get
                {
                    return _shapeType switch
                    {
                        ItemShapeType.Square11 => new Vector2(1, 1),
                        ItemShapeType.Square22 => new Vector2(2, 2),
                        ItemShapeType.Rectangle12 => new Vector2(1, 2),
                        ItemShapeType.Rectangle23 => new Vector2(2, 3),
                        ItemShapeType.Circle1 => new Vector2(3, 3),
                        ItemShapeType.LShape2 => new Vector2(2, 2),
                        ItemShapeType.LShape3 => new Vector2(3, 3),
                        ItemShapeType.Stick13 => new Vector2(1, 3),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            /// <summary>
            /// The current list of offsets that define this shape.
            /// </summary>
            public List<Vector2Int> Positions => _positions;

            /// <summary>
            /// Constructor that initializes shape based on a predefined type.
            /// </summary>
            public ItemShape(ItemShapeType shapeType)
            {
                _shapeType = shapeType;
                _positions = shapeType switch
                {
                    ItemShapeType.Square11 => GetSquareShape(1),
                    ItemShapeType.Square22 => GetSquareShape(2),
                    ItemShapeType.Rectangle12 => GetRectangleShape(1, 2),
                    ItemShapeType.Rectangle23 => GetRectangleShape(2, 3),
                    ItemShapeType.Circle1 =>
                        // For "Circle2", radius=1 => yields a rough 3x3 disk shape
                        GetCircleShape(1),
                    ItemShapeType.LShape2 => GetLShape(2),
                    ItemShapeType.LShape3 => GetLShape(3),
                    ItemShapeType.Stick13 => GetRectangleShape(1, 3),
                    _ => throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, null)
                };
            }

            public ItemShape(ItemShape itemShape)
            {
                _shapeType = itemShape._shapeType;
                _positions = new List<Vector2Int>(itemShape._positions);
            }

            /// <summary>
            /// Generates a square shape of side length 'size'.
            /// e.g. size=2 => offsets: (0,0), (1,0), (0,1), (1,1)
            /// </summary>
            private List<Vector2Int> GetSquareShape(int size)
            {
                return GetRectangleShape(size, size);
            }

            /// <summary>
            /// Generates a rectangle shape of 'width' x 'height'.
            /// Offsets start from (0,0) in top-left corner,
            /// increasing x to the right, y downward.
            /// </summary>
            private List<Vector2Int> GetRectangleShape(int width, int height)
            {
                if (width < 1 || height < 1)
                    throw new ArgumentException("Width and height must be greater than 0.");

                List<Vector2Int> rectangle = new List<Vector2Int>();
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        rectangle.Add(new Vector2Int(x, y));
                    }
                }

                return rectangle;
            }

            /// <summary>
            /// Generates a "circle" shape by radius in grid offsets.
            /// For radius=1 => yields positions forming a 3x3 diamond-like shape.
            /// </summary>
            private List<Vector2Int> GetCircleShape(int radius)
            {
                if (radius < 0)
                    throw new ArgumentException("Radius must be non-negative.");

                List<Vector2Int> circle = new List<Vector2Int>();
                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        if (i * i + j * j <= radius * radius)
                        {
                            circle.Add(new Vector2Int(i, j));
                        }
                    }
                }

                return circle;
            }

            /// <summary>
            /// Generates a cross shape (plus sign). For size=1 => center plus 4 directions.
            /// </summary>
            private List<Vector2Int> GetCrossShape(int size)
            {
                if (size < 1)
                    throw new ArgumentException("Size must be greater than 0.");

                List<Vector2Int> cross = new List<Vector2Int>();
                // The center
                cross.Add(Vector2Int.zero);

                // Extending in four directions
                for (int i = 1; i <= size; i++)
                {
                    cross.Add(new Vector2Int(i, 0)); // right
                    cross.Add(new Vector2Int(-i, 0)); // left
                    cross.Add(new Vector2Int(0, i)); // up/down, depending on coordinate system
                    cross.Add(new Vector2Int(0, -i));
                }

                return cross;
            }

            /// <summary>
            /// Generates an L-shape of size 'size'. e.g. size=3 => offsets:
            /// (0,0), (1,0), (2,0), (0,1), (0,2)
            /// </summary>
            private List<Vector2Int> GetLShape(int size)
            {
                if (size < 1)
                    throw new ArgumentException("L-shape size must be greater than 0.");

                // If size=2 => (0,0), (1,0), (0,1)
                List<Vector2Int> lShape = new List<Vector2Int>();
                // Horizontal part
                for (int x = 0; x < size; x++)
                {
                    lShape.Add(new Vector2Int(x, 0));
                }

                // Vertical part
                for (int y = 1; y < size; y++)
                {
                    lShape.Add(new Vector2Int(0, y));
                }

                return lShape;
            }

            public int GetSelectedSlotIndex(Vector2Int relativePosition)
            {
                for (int i = 0; i < _positions.Count; i++)
                {
                    if (_positions[i] == relativePosition)
                    {
                        return i;
                    }
                }

                return -1;
            }

            public List<Vector2Int> ProjectedPositions(Vector2Int pivot)
            {
                List<Vector2Int> projected = new List<Vector2Int>();
                foreach (Vector2Int pos in _positions)
                {
                    projected.Add(pivot + pos);
                }

                return projected;
            }
        }
    }
}