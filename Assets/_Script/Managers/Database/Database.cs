using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

// 若要使用 EditorUtility、AssetDatabase 等编辑器相关功能，需要在 UNITY_EDITOR 环境下
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyGame.Managers.Database
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
    public class ItemDatabase : SerializedScriptableObject
    {
        //=========================
        // 数据区
        //=========================
        [BoxGroup("当前数据库内容"), LabelText("Items in Database")]
        [TableList(AlwaysExpanded = true)]
        [InfoBox("这是当前数据库中所有的物品数据。", InfoMessageType.Info)]
        public List<ItemData> Items = new List<ItemData>();

        [BoxGroup("当前数据库内容"), LabelText("临时待导入列表")]
        [InfoBox("在这里一次性选择(多选)要添加的 ItemData，然后点击下面的按钮“将选中的物品导入数据库”。", InfoMessageType.None)]
        public List<ItemData> ItemsToAdd = new List<ItemData>();

        //=========================
        // “尚未添加”的物品列表
        //=========================
        [FoldoutGroup("未添加的物品"), LabelText("未添加到数据库的 ItemData"), PropertyOrder(10)]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        [InfoBox("下面列表中的物品在整个项目中已存在，但尚未被添加到本数据库里。点击“刷新未添加列表”可重新扫描项目。", InfoMessageType.Info)]
        public List<ItemData> NotInDatabase = new List<ItemData>();


        //=========================
        // 批量添加
        //=========================
        [FoldoutGroup("添加/移除"), LabelText("批量添加"), PropertyOrder(0)]
        [InfoBox("在这里输入要一次性创建多少个新的 ItemData，并自动生成递增的 ID。", InfoMessageType.None)]
        [Button("批量添加空白 ItemData"), GUIColor(0.4f, 0.8f, 1f)]
        public void AddItemsInBulk(int count)
        {
            if (count <= 0)
            {
                Debug.LogWarning("批量添加数量必须大于 0");
                return;
            }

            // 寻找现有 ItemData 中最大的可解析数值ID
            int currentMaxId = 0;
            foreach (var item in Items)
            {
                if (int.TryParse(item.ItemID, out int parsed))
                {
                    if (parsed > currentMaxId)
                        currentMaxId = parsed;
                }
            }

            // 按照 currentMaxId + 1...N，依次创建空白数据
            for (int i = 1; i <= count; i++)
            {
                var newItem = ScriptableObject.CreateInstance<ItemData>();
                int newId = currentMaxId + i;
                newItem.ItemID = newId.ToString();
                newItem.ItemName = $"NewItem_{newId}";
                newItem.name = newItem.ItemName;

                Items.Add(newItem);

#if UNITY_EDITOR
                // 将新 ItemData 作为子资源存储在当前 database 中
                AssetDatabase.AddObjectToAsset(newItem, this);
#endif
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] 已批量添加 {count} 个新 ItemData，ID 从 {currentMaxId + 1} 到 {currentMaxId + count}。");
        }

        [FoldoutGroup("添加/移除"), LabelText("批量将已存在的物品导入数据库"), PropertyOrder(1)]
        [Button("将选中的物品导入数据库"), GUIColor(0.4f, 1f, 0.4f)]
        [InfoBox("会将上面“临时待导入列表”中的所有物品一次性添加到 Items 列表。", InfoMessageType.None)]
        public void AddSelectedItemsToDatabase()
        {
            int addedCount = 0;
            foreach (var item in ItemsToAdd)
            {
                if (item != null && !Items.Contains(item))
                {
                    Items.Add(item);
                    addedCount++;
                }
            }
            ItemsToAdd.Clear();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] 批量添加了 {addedCount} 个 ItemData 到数据库。");
        }

        //=========================
        // 一键把“未添加”列表的内容也加进来
        //=========================
        [FoldoutGroup("未添加的物品"), PropertyOrder(11)]
        [Button("将上述所有“未添加”物品加入数据库"), GUIColor(0.4f, 1f, 0.4f)]
        public void AddAllNotInDatabase()
        {
            if (NotInDatabase == null || NotInDatabase.Count == 0)
            {
                Debug.LogWarning("[ItemDatabase] “未添加”列表为空，先点“刷新未添加列表”看看。");
                return;
            }

            int addedCount = 0;
            foreach (var item in NotInDatabase)
            {
                if (item != null && !Items.Contains(item))
                {
                    Items.Add(item);
                    addedCount++;
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] 已从“未添加”列表中导入 {addedCount} 个物品。");
        }

        //=========================
        // 刷新“未添加”列表
        //=========================
        [FoldoutGroup("未添加的物品"), PropertyOrder(10)]
        [Button("刷新未添加列表"), GUIColor(1f, 0.9f, 0.4f)]
        public void RefreshNotInDatabase()
        {
#if UNITY_EDITOR
            NotInDatabase.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            Debug.Log($"找到 {guids.Length} 个匹配 't:ItemData' 的资源GUID。");

            int totalFound = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);

                if (itemData == null)
                {
                    Debug.LogWarning($"GUID={guid} 对应的资源无法转换为 ItemData 对象，路径：{path}");
                    continue;
                }

                totalFound++;
                if (!Items.Contains(itemData))
                {
                    NotInDatabase.Add(itemData);
                    Debug.Log($"发现未添加的 ItemData：{itemData.name}，路径：{path}");
                }
            }

            Debug.Log($"[ItemDatabase] 扫描到项目中共有 {totalFound} 个 ItemData，其中 {NotInDatabase.Count} 个尚未添加进数据库。");
#else
            Debug.LogWarning("只能在 Unity Editor 环境下执行此操作。");
#endif
        }


        //=========================
        // 批量“移除”改为软删除
        //=========================
        [FoldoutGroup("添加/移除"), LabelText("批量移除(根据ID)"), PropertyOrder(2)]
        [Button("批量移除(根据ID)"), GUIColor(1f, 0.5f, 0.5f)]
        [InfoBox("用逗号分隔多个 ItemID，如：101,102,103。仅从数据库列表中移除，不会从工程删除。", InfoMessageType.None)]
        public void RemoveItemsByIDs(string commaSeparatedIDs)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedIDs))
            {
                Debug.LogWarning("请输入要移除的 ItemID 列表（逗号分隔）！");
                return;
            }

            string[] idArray = commaSeparatedIDs
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();

            int removeCount = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (idArray.Contains(item.ItemID))
                {
                    removeCount++;
                    // 这里仅从列表中移除，不执行物理删除
                    Items.RemoveAt(i);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] 批量移除完成，共移除了 {removeCount} 个匹配 ID 的 ItemData（未物理删除）。");
        }

        [FoldoutGroup("添加/移除"), LabelText("批量移除(根据Name)"), PropertyOrder(3)]
        [Button("批量移除(根据Name)"), GUIColor(1f, 0.5f, 0.5f)]
        [InfoBox("用逗号分隔多个物品Name，如：Apple,Axe。仅从数据库列表中移除，不会从工程删除。", InfoMessageType.None)]
        public void RemoveItemsByNames(string commaSeparatedNames)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedNames))
            {
                Debug.LogWarning("请输入要移除的 Name 列表（逗号分隔）！");
                return;
            }

            string[] nameArray = commaSeparatedNames
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();

            int removeCount = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (nameArray.Contains(item.ItemName))
                {
                    removeCount++;
                    // 这里仅从列表中移除，不执行物理删除
                    Items.RemoveAt(i);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] 批量移除完成，共移除了 {removeCount} 个匹配 Name 的 ItemData（未物理删除）。");
        }


        //=========================
        // 搜索相关
        //=========================
        [FoldoutGroup("搜索/查看"), LabelText("搜索物品(根据ID)"), PropertyOrder(4)]
        [Button("搜索物品(根据ID)"), GUIColor(0.7f, 0.7f, 1f)]
        public ItemData SearchItemByID(string itemID)
        {
            var found = Items.FirstOrDefault(i => i.ItemID == itemID);
            if (found == null)
            {
                Debug.LogWarning($"[ItemDatabase] 未找到 ItemID = {itemID} 的物品。");
            }
            else
            {
                Debug.Log($"[ItemDatabase] 找到物品：{found.ItemName} (ID = {found.ItemID})。");
            }
            return found;
        }

        [FoldoutGroup("搜索/查看"), LabelText("搜索物品(根据Name关键字)"), PropertyOrder(5)]
        [Button("搜索物品(根据Name关键字)"), GUIColor(0.7f, 0.7f, 1f)]
        public List<ItemData> SearchItemsByNameKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                Debug.LogWarning("[ItemDatabase] 关键字不能为空。");
                return null;
            }

            var matchedItems = Items
                .Where(i => !string.IsNullOrEmpty(i.ItemName) 
                            && i.ItemName.ToLower().Contains(keyword.ToLower()))
                .ToList();

            if (matchedItems.Count == 0)
            {
                Debug.LogWarning($"[ItemDatabase] 未找到名称包含 '{keyword}' 的物品。");
            }
            else
            {
                Debug.Log($"[ItemDatabase] 找到 {matchedItems.Count} 个名称包含 '{keyword}' 的物品：\n"
                          + string.Join(", ", matchedItems.Select(i => i.ItemName)));
            }
            return matchedItems;
        }

        //=========================
        // 自定义拖拽区 (可选)
        //=========================
        [FoldoutGroup("拖拽添加"), PropertyOrder(999)]
        [OnInspectorGUI, LabelText("Drag & Drop Area")]
        [InfoBox("可以一次性从Project多选多个ItemData，拖到下面的区域自动加入数据库中。", InfoMessageType.None)]
        private void DrawDragAndDropArea()
        {
            var dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "将多个 ItemData 拖到这里批量添加");

            var evt = Event.current;
            if (!dropArea.Contains(evt.mousePosition)) return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    bool canDrop = DragAndDrop.objectReferences.Any(o => o is ItemData);
                    if (canDrop)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            int addedCount = 0;
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (obj is ItemData item && !Items.Contains(item))
                                {
                                    Items.Add(item);
                                    addedCount++;
                                }
                            }
#if UNITY_EDITOR
                            EditorUtility.SetDirty(this);
                            AssetDatabase.SaveAssets();
#endif
                            Debug.Log($"[ItemDatabase] 拖拽成功，一次性添加了 {addedCount} 个 ItemData。");
                        }
                        evt.Use();
                    }
                    break;
            }
        }
    }
}
