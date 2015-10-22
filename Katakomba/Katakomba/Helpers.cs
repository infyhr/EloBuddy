using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace Katakomba {
    class Helpers {
        public static InventorySlot zhonyaSlot; // where our zhonya lives

        /// <summary>
        /// Calculates our hero's damage.
        /// </summary>
        /// <param name="myHero">Reference to AIHeroClient -- ourselves</param>
        /// <param name="target">Reference to AIHeroClient -- the target</param>
        /// <param name="useQ">Consider Q damage</param>
        /// <param name="useW">Consider W damage</param>
        /// <param name="useE">Consider E damage</param>
        /// <param name="useMark">Consider mark damage (if any)</param>
        /// <returns>float of the damage</returns>
        public static float MyDamage(AIHeroClient myHero, AIHeroClient target, bool useQ, bool useW, bool useE, bool useMark = true) {
            float calculation = 0;

            if(useQ)    calculation += myHero.GetSpellDamage(target, SpellSlot.Q);
            if(useW)    calculation += myHero.GetSpellDamage(target, SpellSlot.W);
            if(useE)    calculation += myHero.GetSpellDamage(target, SpellSlot.E);
            if(useMark) calculation += target.HasBuff("katarinaqmark") ? myHero.GetSpellDamage(target, SpellSlot.Q) : 0;

            // Consider ignite damage
            var ignite = new Spell.Targeted(Katakomba.IgniteSlot, 600);
            calculation += ignite.IsReady() ? myHero.GetSpellDamage(target, Katakomba.IgniteSlot) : 0;

            return calculation;
        }

        /// <summary>
        /// Casts zhonya if Katarina is low on health.
        /// </summary>
        /// <param name="myHero">AIHeroClient reference -- ourselves</param>
        /// <returns>true if zhonya suceeded</returns>
        public static bool CastZhonya(AIHeroClient myHero) {
            if(myHero.HealthPercent <= 10) {
                zhonyaSlot = myHero.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Zhonyas_Hourglass);
                if(zhonyaSlot != null && Player.GetSpell(zhonyaSlot.SpellSlot).IsReady) {
                    Player.CastSpell(zhonyaSlot.SpellSlot);
                    return true;
                }else {
                    // Oh no! Zhonya was down and we are almost dead! Flee out!
                    Console.WriteLine("Zhonya down -> trying to flee out!");
                    Katakomba.flee();
                }
            }

            return false;
        }
    }
}
