using _Script.Character;
using UnityEngine;

namespace _Script.Items
{
    namespace _Script.Items
    {
        [System.Serializable]
        public abstract class ItemData : ScriptableObject
        {
            public string itemName;
            public int itemID;
            [TextArea]
            public string itemDescription;
            public Sprite itemIcon;
            public bool isStackable;
            public int maxStackSize = 1;

            public abstract void Use(PlayerCharacter playerCharacter);
        }
    }


}