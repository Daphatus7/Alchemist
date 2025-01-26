// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 30

using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Managers.Database
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
    public class ItemDatabase : SerializedScriptableObject
    {
        [TableList]
        public List<ItemData> Items = new List<ItemData>();

        /// <summary>
        /// 利用 Odin 的按钮功能，在编辑器里一次性批量创建若干 Item。
        /// </summary>
        /// <param name="count">要创建的数量</param>
        [Button("批量添加 Item")]
        public void AddItemsInBulk(int count)
        {
            if (count <= 0)
            {
                Debug.LogWarning("批量添加数量必须大于0");
                return;
            }

            // 1. 获取当前列表中最大 ItemID
            int currentMaxId = 0;
            foreach (var item in Items)
            {
                if (item.ItemID > currentMaxId)
                {
                    currentMaxId = item.ItemID;
                }
            }

            // 2. 批量创建并添加到列表和数据库资产中
            for (int i = 1; i <= count; i++)
            {
                // 创建新的 ItemData 实例
                var newItem = ScriptableObject.CreateInstance<ItemData>();

                // 自动生成 ID（从 currentMaxId + 1 开始）
                newItem.ItemID = currentMaxId + i;
                // 这里给 ScriptableObject 命名，方便在资产中区分
                newItem.name = $"NewItem_{newItem.ItemID}";

                // 添加到列表
                Items.Add(newItem);

                // 将新建的 ItemData 作为子资源存储到当前数据库中
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.AddObjectToAsset(newItem, this);
#endif
            }

            // 3. 标记并保存改动
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            Debug.Log($"批量创建了 {count} 个新 Item，ID 从 {currentMaxId + 1} 到 {currentMaxId + count}。");
        }
    }
}