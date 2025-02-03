using System;
using System.Collections.Generic;
using _Script.Utilities.ServiceLocator;

namespace _Script.Alchemy
{
    /// <summary>
    /// 玩家药剂效果管理器：统一管理所有激活中的药剂效果，
    /// 并通过 Observer Pattern 通知外部观察者在效果添加或移除时更新属性。
    /// 此实现采用“下一个到期”的更新策略：仅跟踪最近到期的药剂，
    /// 每次 UpdatePotion(deltaTime) 只更新该单一计时器，计时结束后移除对应药剂，再重新计算下一个到期时间。
    /// </summary>
    public sealed class PlayerPotionEffectManager : IPlayerPotionEffectHandler
    {
        // 存储所有激活的药剂效果
        private readonly List<PotionInstance.PotionInstance> _potionInstances = new List<PotionInstance.PotionInstance>();

        // 内部变量用于追踪当前最近到期的药剂及其剩余时间
        private float _timeUntilNextExpiry = float.MaxValue;
        private PotionInstance.PotionInstance _nextExpiringPotion = null;

        public event Action<PotionInstance.PotionInstance> onAddPotion;
        public event Action<PotionInstance.PotionInstance> onRemovePotion;

        /// <summary>
        /// 添加新的药剂效果（例如玩家喝下药剂后调用）。
        /// 当应用相同类型的药剂时，将先移除已有效果，再添加新的效果。
        /// </summary>
        public void ApplyPotionEffect(PotionInstance.PotionInstance potionInstance)
        {
            // 检查是否已有相同类型的药剂效果存在
            // 假设每个 potionInstance 拥有一个 PotionType 属性用以标识其类型
            var existingPotion = _potionInstances.Find(p => p.PotionType == potionInstance.PotionType);
            if (existingPotion != null)
            {
                RemovePotionEffect(existingPotion);
                OnOnRemovePotion(existingPotion);
            }
            
            // 添加新的药剂效果
            _potionInstances.Add(potionInstance);
            OnOnAddPotion(potionInstance);
            
            // 如果新药剂的持续时间小于当前剩余的下一到期时间，则更新内部记录
            if (potionInstance.Duration < _timeUntilNextExpiry)
            {
                _timeUntilNextExpiry = potionInstance.Duration;
                _nextExpiringPotion = potionInstance;
            }
        }

        /// <summary>
        /// 每帧或固定时间间隔调用，更新最近到期的药剂效果的计时器。
        /// 当计时器到零时，仅移除该药剂效果，并重新计算下一个到期效果。
        /// </summary>
        public void UpdatePotion(float deltaTime)
        {
            if (_potionInstances.Count == 0)
            {
                // 没有激活的药剂效果
                _timeUntilNextExpiry = float.MaxValue;
                _nextExpiringPotion = null;
                return;
            }

            // 只更新当前“最先到期”的计时器
            _timeUntilNextExpiry -= deltaTime;
            if (_timeUntilNextExpiry <= 0)
            {
                // 到期，移除该药剂效果
                RemovePotionEffect(_nextExpiringPotion);
                OnOnRemovePotion(_nextExpiringPotion);
                
                // 重新扫描剩余药剂效果，找到下一个到期的效果
                _timeUntilNextExpiry = float.MaxValue;
                _nextExpiringPotion = null;
                foreach (var potion in _potionInstances)
                {
                    if (potion.Duration < _timeUntilNextExpiry)
                    {
                        _timeUntilNextExpiry = potion.Duration;
                        _nextExpiringPotion = potion;
                    }
                }
            }
            else
            {
                // 如果还未到期，则更新该药剂的 Duration（模拟其剩余时间递减）
                // 注意：这里假设 Duration 表示剩余时间，实际项目中可能需要更精确的 float 类型。
                if (_nextExpiringPotion != null)
                {
                    _nextExpiringPotion.Duration = (int)_timeUntilNextExpiry;
                }
            }
        }

        /// <summary>
        /// 从列表中移除指定的药剂效果
        /// </summary>
        private void RemovePotionEffect(PotionInstance.PotionInstance potionInstance)
        {
            _potionInstances.Remove(potionInstance);
        }

        private void OnOnAddPotion(PotionInstance.PotionInstance obj)
        {
            onAddPotion?.Invoke(obj);
        }

        private void OnOnRemovePotion(PotionInstance.PotionInstance obj)
        {
            onRemovePotion?.Invoke(obj);
        }
    }

    public interface IPlayerPotionEffectHandler
    {
        void ApplyPotionEffect(PotionInstance.PotionInstance potionInstance);
        void UpdatePotion(float deltaTime);
    }
}