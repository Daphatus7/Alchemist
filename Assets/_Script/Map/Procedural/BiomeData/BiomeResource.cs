using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.Procedural.BiomeData
{
    [CreateAssetMenu(fileName = "NewBiomeResource", menuName = "Map/BiomeResource")]
    public class BiomeResource : ScriptableObject
    {
        [System.Serializable]
        public class BiomeResourceData
        {
            [HorizontalGroup("Item")]
            [LabelText("Item")]
            public GameObject resourcePrefab;

            public int weight;
        }

        [TableList(AlwaysExpanded = true)]
        public BiomeResourceData[] resources; // 可掉落的资源列表

        public IEnumerable<BiomeResourceData> GetResources()
        {
            return resources;
        }
        private AliasTable _aliasTable; // 缓存别名表

        public class AliasTable
        {
            public float[] prob;   // 槽位的修正概率
            public int[] alias;    // 槽位的别名索引
            public int count;      // 资源种类数 (N)
        }
        
        public AliasTable BuildAliasTable()
        {
            // 1) 收集所有资源的 weight，计算总和
            var resList = new List<BiomeResourceData>(resources);
            int n = resList.Count;
            if (n == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            foreach (var r in resList)
                totalWeight += r.weight;

            // 2) 创建临时数组存放各资源的 概率(标准化后) 
            //    以及两个队列 small 和 large，用来区分 < 1 和 > 1 的概率
            float[] p = new float[n];
            for (int i = 0; i < n; i++)
            {
                p[i] = (float)resList[i].weight / totalWeight * n; 
                // 注意 *n: 后面Alias法需要与1进行比较，所以这里乘以 n
            }

            // 3) 分别用 List<int> 来存放 "small 列表" 和 "large 列表"
            List<int> small = new List<int>();
            List<int> large = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (p[i] < 1f) small.Add(i);
                else           large.Add(i);
            }

            // 4) 为 prob[] 和 alias[] 分配空间
            var aliasTable = new AliasTable();
            aliasTable.prob  = new float[n];
            aliasTable.alias = new int[n];
            aliasTable.count = n;

            // 5) 开始构建
            while (small.Count > 0 && large.Count > 0)
            {
                int less = small[small.Count - 1];
                small.RemoveAt(small.Count - 1);

                int more = large[large.Count - 1];
                large.RemoveAt(large.Count - 1);

                aliasTable.prob[less] = p[less];
                aliasTable.alias[less] = more;

                // 将 p[more] 多余的概率部分移动到别名里
                p[more] = p[more] + p[less] - 1f;

                if (p[more] < 1f)
                {
                    small.Add(more);
                }
                else
                {
                    large.Add(more);
                }
            }

            // 6) 剩下的槽位，若在 small 里，就把概率设置为 1，别名随意
            while (small.Count > 0)
            {
                int idx = small[small.Count - 1];
                small.RemoveAt(small.Count - 1);
                aliasTable.prob[idx] = 1f;
                aliasTable.alias[idx] = idx;
            }
            // 7) 同理，对 large 里剩下的槽位也置为 1
            while (large.Count > 0)
            {
                int idx = large[large.Count - 1];
                large.RemoveAt(large.Count - 1);
                aliasTable.prob[idx] = 1f;
                aliasTable.alias[idx] = idx;
            }

            return aliasTable;
        }
        // 给外部使用的采样方法
        public GameObject GetRandomResourcePrefab()
        {
            // 如果还没构建过，就先构建
            if (_aliasTable == null)
            {
                _aliasTable = BuildAliasTable();
            }
            if (_aliasTable == null || _aliasTable.count == 0) 
                return null;

            int idx = SampleIndexFromAlias(_aliasTable);
            return resources[idx].resourcePrefab;
        }
        private int SampleIndexFromAlias(AliasTable aliasTable)
        {
            int column = Random.Range(0, aliasTable.count);
            float pTest = Random.value;
            if (pTest < aliasTable.prob[column])
                return column;
            else
                return aliasTable.alias[column];
        }
    }
}