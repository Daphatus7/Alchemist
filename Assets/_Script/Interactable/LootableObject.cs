// /**
//  *
//  *   Author: daphatus
//  *   File: ${File.Name}
//  *   Date: $[InvalidReference]
//  */

namespace _Script.Interactable
{
    public class LootableObject : AutoTrigger
    {
        
        protected override bool CanInteract()
        {
            return true;
        }
        
        protected override void OnInteract()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInteractCanceled()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInteractCompleted()
        {
            throw new System.NotImplementedException();
        }
    }
}