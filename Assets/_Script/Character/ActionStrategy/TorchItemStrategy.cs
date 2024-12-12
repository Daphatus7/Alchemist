// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 37

using System.Collections;
using _Script.Inventory.PlayerInventory;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    public class TorchItemStrategy : BaseItemStrategy
    {
        private float burnDuration = 5f;
        private Coroutine burnCoroutine;

        // Store the context so we can call RemoveWeaponOrTorch later
        private ActionBarContext _context;

        protected override void OnItemChanged(ActionBarContext useItem)
        {
            _context = useItem;
            StartBurning();
        }

        protected override void OnItemRemoved()
        {
            StopBurning();
        }

        private void StartBurning()
        {
            if (burnCoroutine != null) StopCoroutine(burnCoroutine);
            burnCoroutine = StartCoroutine(BurnTimer());
        }

        private void StopBurning()
        {
            if (burnCoroutine != null)
            {
                StopCoroutine(burnCoroutine);
                burnCoroutine = null;
            }
        }

        private IEnumerator BurnTimer()
        {
            float timer = burnDuration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            // Torch is burnt out, now remove it
            // First remove the in-world representation
            RemoveItem(); 
            
            // Then inform the ActionBar to remove it from the inventory & unset the strategy
            _context.RemoveWeaponOrTorch();
        }
    }
}
