
namespace _Script.Alchemy.PotionInstance
{
  
    
    /// <summary>
    /// 单个药剂实例，包含药剂的基本信息
    /// </summary>
    public class PotionInstance
    {
        public string PotionID { get; }
        public PotionType PotionType { get; }
        public int EffectValue { get; }
        /// <summary>
        /// 单位：秒
        /// </summary>
        public int Duration { get; set; } 

        public PotionInstance(Potion potion)
        {
            PotionID = potion.itemID;
            PotionType = potion.potionEffect.potionType;
            EffectValue = potion.potionEffect.effectValue;
            Duration = potion.potionEffect.duration;
        }
        
        public PotionInstance(string potionID, PotionType potionType, int effectValue, int duration)
        {
            PotionID = potionID;
            PotionType = potionType;
            EffectValue = effectValue;
            Duration = duration;
        }
    }
    
}
