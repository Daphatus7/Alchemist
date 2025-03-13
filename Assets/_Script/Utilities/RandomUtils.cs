// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 43

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _Script.Utilities
{

    public static class RandomUtils
    {
        /// <summary>
        /// Returns a list containing a random selection of unique items.
        /// If the source contains fewer items than the requested count,
        /// the full list is returned.
        /// </summary>
        public static List<T> GetRandomUniqueItems<T>(IEnumerable<T> items, int count)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var list = items.ToList();
            if (list.Count <= count)
                return new List<T>(list);

            // Shuffle the list using Unity's Random.value and take the first 'count' items.
            return list.OrderBy(item => Random.value)
                .Take(count)
                .ToList();
        }
    }

}