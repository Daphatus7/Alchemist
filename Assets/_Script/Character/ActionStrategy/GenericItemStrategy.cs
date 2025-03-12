using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    [DefaultExecutionOrder(50)]
    public sealed class GenericItemStrategy : BaseItemStrategy
    {
        protected override bool TryShowPreview()
        {
            throw new System.NotImplementedException("pending removal");
            return false;
        }
    }
}