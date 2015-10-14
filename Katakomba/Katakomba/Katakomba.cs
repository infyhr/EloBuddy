using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Katakomba {
    class Katakomba {
        private static AIHeroClient myHero;
        private static Spell.Targeted Q;
        private static Spell.Active W;
        private static Spell.Targeted E;
        private static Spell.Active R;
        private static SpellSlot IgniteSlot; // ignite
        private static Menu menu, ComboMenu, HarassMenu, KillStealMenu, EtcMenu; // menus
        private static bool _isChanneling; // channeling the ultimate
        private static AIHeroClient target; // enemy target
        private static Vector3 mousePos { get { return Game.CursorPos; } }

        public static void Init() {
            Bootstrap.Init(null);
            Loading.OnLoadingComplete += OnLoad;
        }

        private static void initMenu() {
            menu = MainMenu.AddMenu("Katakomba", "Katakomba");

            // Some main crap
            menu.AddGroupLabel("Katakomba");
            menu.AddLabel("Version: 1.1.2.0");
            menu.AddSeparator();
            menu.AddLabel("infy"); // Author

            // Combo
            ComboMenu = menu.AddSubMenu("Combo", "KatakombaCombo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("combokey", new KeyBind("Combo Key", false, KeyBind.BindTypes.HoldActive, 'C'));
            ComboMenu.AddLabel("Remember to unbind orbwalker combo!");

            // Harrasment
            HarassMenu = menu.AddSubMenu("Harass", "KatakombaHarass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("autoharass", new CheckBox("Auto Harass with Q W?", true));

            // Killsteal
            KillStealMenu = menu.AddSubMenu("KillSteal", "KatakombaKillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("killsteal", new CheckBox("Do Killsteal?", true));
            KillStealMenu.Add("fleeks", new CheckBox("Flee KS", true));
            KillStealMenu.Add("useignite", new CheckBox("Use ignite?", true));

            // Etc menu
            EtcMenu = menu.AddSubMenu("Et cetera", "KatakombaEtc");
            EtcMenu.AddGroupLabel("Flee Settings");
            EtcMenu.Add("wardjump", new KeyBind("Ward Jump", false, KeyBind.BindTypes.HoldActive, 'Z'));
        }

        private static void OnLoad(EventArgs args) {
            // Register champion
            myHero = ObjectManager.Player;
            if(myHero.Hero != Champion.Katarina) return;

            // Register spell slots
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 375);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 550);

            // Register Ignite slot if any
            IgniteSlot = Player.Spells.FirstOrDefault(o => o.SData.Name.ToLower().Contains("summonerdot")).Slot;

            // Initialize the menu
            initMenu();

            //GameObject.OnCreate += GameObject_OnCreate1;
            Game.OnTick += OnTick;
            Chat.Print("Katakomba Loaded successfully.");
            Orbwalker.OnPreAttack     += Orbwalker_OnPreAttack;
            Player.OnProcessSpellCast += Player_OnProcessSpellCast;
        }

        /// <summary>
        /// Gets hit when a casted spell is being processed.
        /// </summary>
        /// <param name="sender">Who is casting</param>
        /// <param name="args">args</param>
        private static void Player_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if(!sender.IsMe || args.SData.Name != "KatarinaR" || !myHero.HasBuff("katarinarsound")) return;

            _isChanneling = true;
            Chat.Print("Channeling ultimate . . .");
            Orbwalker.DisableMovement.Equals(true);
            Orbwalker.DisableAttacking.Equals(true);
            // Zhonya logic.
            if(myHero.HealthPercent <= 15) {
                var zhonyaslot = myHero.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Zhonyas_Hourglass);
                if(zhonyaslot != null && Player.GetSpell(zhonyaslot.SpellSlot).IsReady) {
                    Player.CastSpell(zhonyaslot.SpellSlot);
                    _isChanneling = false;
                    return;
                }
            }
            _isChanneling = false;
        }

        /// <summary>
        /// Function that hits before an attack has been made. Antistop spin.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="args"></param>
        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args) {
            if(myHero.IsMe) args.Process = !myHero.HasBuff("KatarinaR");
        }

        private static void OnTick(EventArgs args) {
            // Figure if ult is being channeled, if so return.
            if(myHero.HasBuff("KatarinaR") || Player.Instance.Spellbook.IsChanneling || myHero.HasBuff("katarinarsound")) {
                Orbwalker.DisableMovement.Equals(true);
                Orbwalker.DisableAttacking.Equals(true);
            }

            // Process wardjump
            if(EtcMenu["wardjump"].Cast<KeyBind>().CurrentValue) wardjump();

            // Process combo
            if(ComboMenu["combokey"].Cast<KeyBind>().CurrentValue) combo();

            if(!myHero.HasBuff("KatarinaR") && !Player.Instance.Spellbook.IsChanneling && !myHero.HasBuff("katarinarsound")) {
                // Harass
                if(HarassMenu["autoharass"].Cast<CheckBox>().CurrentValue) autoHarass();

                // Killsteal
                if(KillStealMenu["killsteal"].Cast<CheckBox>().CurrentValue) killSteal();
            }

            // Orbwalker
            switch(Orbwalker.ActiveModesFlags) {
                // OrbWalker combomode
                case Orbwalker.ActiveModes.Combo:
                    // This must not be bound.
                break;

                // OrbWalker harass, we don't care about.
                case Orbwalker.ActiveModes.Harass:
                    // We don't care about this.
                break;

                // OrbWalker LaneClear mode.
                case Orbwalker.ActiveModes.LaneClear:
                    laneClear();
                break;

                // OrbWalker LastHit mode
                case Orbwalker.ActiveModes.LastHit:
                    // Not implemented... yet.
                break;

                // OrbWalker flee
                case Orbwalker.ActiveModes.Flee:
                    flee();
                break;
            }
        }

        private static double markDamage(Obj_AI_Base target) {
            return target.HasBuff("katarinaqmark") ? myHero.GetSpellDamage(target, SpellSlot.Q) : 0;
        }

        private static void handleIgnite(Obj_AI_Base target) {
            if(KillStealMenu["useignite"].Cast<CheckBox>().CurrentValue) {
                var ignite = new Spell.Targeted(IgniteSlot, 600);
                if(ignite.IsReady() && ignite.IsInRange(target)) {
                    ignite.Cast(target);
                }
            }
        }

        private static void killSteal() {
            if(_isChanneling) { Chat.Print("Channeling therefore KS disabled!"); return; } // + add in every for a check
            target = TargetSelector.GetTarget(1375, DamageType.Magical);
            if(target == null) return;

            // QEW
            if(Q.IsInRange(target.ServerPosition) && E.IsInRange(target.ServerPosition) && E.IsReady() && Q.IsReady() && W.IsReady() &&
              (myHero.GetSpellDamage(target, SpellSlot.Q) + myHero.GetSpellDamage(target, SpellSlot.W) + myHero.GetSpellDamage(target, SpellSlot.E) + markDamage(target))> target.Health) {
                Chat.Print("KS: QEW");
                Q.Cast(target);
                E.Cast(target);
                W.Cast();
                handleIgnite(target);
                return;
            }

            // EQ(W)
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && Q.IsReady() &&
              (myHero.GetSpellDamage(target, SpellSlot.E) + myHero.GetSpellDamage(target, SpellSlot.Q) + markDamage(target)) > target.Health) {
                Chat.Print("KS: EQ");
                E.Cast(target);
                Q.Cast(target);
                W.Cast();
                handleIgnite(target);
                return;
            }


            // EW
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && W.IsReady() &&
               (myHero.GetSpellDamage(target, SpellSlot.W) + myHero.GetSpellDamage(target, SpellSlot.E)) > target.Health) {
                Chat.Print("KS: EW");
                E.Cast(target);
                W.Cast();
                handleIgnite(target);
                return;
            }

            //E
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && (myHero.GetSpellDamage(target, SpellSlot.E)) > target.Health) {
                Chat.Print("KS: E");
                //Chat.Print(curr_dmg);
                E.Cast(target);
                handleIgnite(target);
                return;
            }

            // Q -> jump with ward... someday maybe.
            if(KillStealMenu["fleeks"].Cast<CheckBox>().CurrentValue) {
                if((myHero.GetSpellDamage(target, SpellSlot.Q) + markDamage(target)) > target.Health && !Q.IsInRange(target.ServerPosition) && Q.IsReady()) {
                    Chat.Print("calling flee!");
                    flee();
                    if(Q.IsInRange(target)) Q.Cast(target);
                    handleIgnite(target);
                    Chat.Print("ward ks!");
                    return;
                }
            }
        }

        private static void combo() {
            target = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            // If we can Q them, why not
            if(Q.IsReady() && myHero.Distance(target.Position) <= Q.Range) Q.Cast(target);

            // E
            if(E.IsReady() && myHero.Distance(target.Position) <= E.Range) {
                // If ultimate is not ready and there are >= 2 enemies, don't jump.
                //if(myHero.CountEnemiesInRange(500) >= 2 && (!R.IsReady() || !(R.State == SpellState.Surpressed && R.Level > 0))) return;

                // Disable orbwalking otherwise.
                Orbwalker.DisableAttacking.Equals(true);
                Orbwalker.DisableMovement.Equals(true);

                // Jump on 'em
                E.Cast(target);
            }

            // W
            if(W.IsReady() && myHero.Distance(target.Position) <= W.Range && Q.IsOnCooldown) W.Cast();

            // R
            if(R.IsReady() && myHero.CountEnemiesInRange(R.Range) > 0) {
                if(!Q.IsReady() && !E.IsReady()) {
                    Orbwalker.DisableAttacking.Equals(true);
                    Orbwalker.DisableMovement.Equals(true);
                    R.Cast();
                }
            }
        }

        public static void autoHarass() {
            target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if(target.IsValidTarget()) {
                if(Q.IsInRange(target)) Q.Cast(target);
                if(W.IsInRange(target)) W.Cast();
            }
        }

        public static void laneClear() {
            // Soon (tm)
        }

        public static void flee() {
            // Try to cast a wardjump first
            if(wardjump()) return;

            // Try to sniff for _any_ wards
            if(E.IsReady()) {
                foreach(var ward in ObjectManager.Get<Obj_Ward>().Where(ward => ward.Distance(myHero.Position) <= E.Range)) {
                    Chat.Print("Found ward and should E to it: " + ward.Position + ward.Name);
                    E.Cast(ward);
                    return;
                }
            }

            // Try to sniff for any hero
            foreach(Obj_AI_Base hero in ObjectManager.Get<Obj_AI_Base>().Where(hero => hero.Distance(myHero.Position) <= E.Range && !hero.IsDead)) {
                if(E.IsReady() && hero.Name != myHero.Name && !hero.Name.Contains("Turret")) {
                    Chat.Print("Found hero and should E to it: " + hero.Position + hero.Name);
                    E.Cast(hero);
                    return;
                }
            }

            // Try to sniff for any minion
            foreach(Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.Distance(myHero.Position) <= E.Range)) {
                if(E.IsReady()) {
                    Chat.Print("Found minion and should E to it: " + minion.Position + minion.Name);
                    E.Cast(minion);
                    return;
                }
            }
        }

        public static bool wardjump() {
            var totemward = myHero.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Warding_Totem_Trinket);
            if(totemward == null || !Player.GetSpell(totemward.SpellSlot).IsReady) return false;
            
            var cursorPos = Game.CursorPos;
            var myPos     = myHero.ServerPosition;
            var delta     = cursorPos - myPos;
            delta.Normalize();

            var wardposition = myPos + delta * (600 - 5);
            totemward.Cast(wardposition);
            if(E.IsReady()) { E.Cast(wardposition); Chat.Print("Casted E @ wardposition."); }
            // E.Cast();
            return true;
        }

        /*private static void GameObject_OnCreate1(GameObject sender, EventArgs args) {
            //Chat.Print("onCreate");
            foreach(var ward in ObjectManager.Get<Obj_Ward>()) {
                if(EtcMenu["wardjump"].Cast<KeyBind>().CurrentValue && ward.Name.ToLower().Contains("ward") && E.IsReady()) {
                    E.Cast(ward);
                }
            }
        }*/
    }
}