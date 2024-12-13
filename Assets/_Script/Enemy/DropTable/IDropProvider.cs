// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 32

using System.Collections.Generic;

namespace _Script.Enemy.DropTable
{
    public interface IDropProvider
    {
        IEnumerable<DropTable.DropItem> GetDrops();
    }
}