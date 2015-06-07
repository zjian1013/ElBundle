using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElEasy.Plugins
{
    public class Katarina : Standards
    {
        private static float _rStart = 0;
        private static bool _isChanneling;
        private static long _lastECast;
        private static int _lastPlaced;
        private static Vector3 _lastWardPos;

        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 675) },
            { Spells.W, new Spell(SpellSlot.W, 375) },
            { Spells.E, new Spell(SpellSlot.E, 700) },
            { Spells.R, new Spell(SpellSlot.R, 550) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.R].SetCharged("KatarinaR", "KatarinaR", 550, 550, 1.0f);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            Orbwalking.BeforeAttack += BeforeAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            new AssassinManager();
        }

        #region HasRBuff

        private static bool HasRBuff()
        {
            return Player.HasBuff("KatarinaR") || Player.IsChannelingImportantSpell() ||
                   Player.HasBuff("katarinarsound");
        }

        #endregion

        #region BeforeAttack

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                args.Process = !Player.HasBuff("KatarinaR");
            }
        }

        #endregion

        #region Obj_AI_Hero_OnIssueOrder

        private static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount < _rStart + 300)
            {
                args.Process = false;
            }
        }

        #endregion

        #region Obj_AI_Base_OnProcessSpellCast

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.SData.Name != "KatarinaR" || !Player.HasBuff("katarinarsound"))
            {
                return;
            }

            _isChanneling = true;
            Orbwalker.SetMovement(false);
            Orbwalker.SetAttack(false);
            Utility.DelayAction.Add(1, () => _isChanneling = false);
        }

        #endregion

        #region Onupdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (HasRBuff())
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }
            else
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLasthit();
                    break;
            }

            KillSteal();

            var autoHarass = _menu.Item("ElEasy.Katarina.AutoHarass.Activated", true).GetValue<KeyBind>().Active;
            if (autoHarass)
            {
                OnAutoHarass();
            }

            var wardjump = _menu.Item("ElEasy.Katarina.Wardjump").GetValue<KeyBind>().Active;
            if (wardjump)
                DoWardJump();
        
            var autor = new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
            int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = new int[] { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[autor[i] - 1] = level[autor[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);

            }
         }
 
        }

        #endregion

        #region AutoHarass

        private static void OnAutoHarass()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                return;

            var useQ = _menu.Item("ElEasy.Katarina.AutoHarass.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.AutoHarass.W").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady() && target.IsValidTarget())
            {
                spells[Spells.Q].Cast(target);
            }
            ;

            if (useW && spells[Spells.W].IsReady() && target.IsValidTarget(spells[Spells.W].Range))
            {
                spells[Spells.W].Cast();
            }
        }

        #endregion

        #region WardSlot
        private static InventorySlot GetBestWardSlot()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            return slot;
        }
        #endregion

        #region Wardjump
        private static void DoWardJump()
        {
            if (Environment.TickCount <= _lastPlaced + 3000 || !spells[Spells.E].IsReady())
                return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = Player.ServerPosition;
            Vector3 delta = cursorPos - myPos;

            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);
            InventorySlot invSlot = GetBestWardSlot();

            if (invSlot == null)
                return;

            Items.UseItem((int)invSlot.Id, wardPosition);
            _lastWardPos = wardPosition;
            _lastPlaced = Environment.TickCount;

            spells[Spells.E].Cast();
        }

        #endregion

        #region GameObject_OnCreate
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!spells[Spells.E].IsReady() || !(sender is Obj_AI_Minion) || Environment.TickCount >= _lastPlaced + 300)
                return;

            if (Environment.TickCount >= _lastPlaced + 300) return;
            var ward = (Obj_AI_Minion)sender;

            if (ward.Name.ToLower().Contains("ward") && ward.Distance(_lastWardPos) < 500)
            {
                spells[Spells.E].Cast(ward);
            }
        }
        #endregion

        #region UseItems

        private static void UseItems(Obj_AI_Base target)
        {
            var useHextech = _menu.Item("ElEasy.Katarina.Items.hextech").GetValue<bool>();
            if (useHextech)
            {
                var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
                var hextech = ItemData.Hextech_Gunblade.GetItem();

                if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target))
                    cutlass.Cast(target);

                if (hextech.IsReady() && hextech.IsOwned(Player) && hextech.IsInRange(target))
                    hextech.Cast(target);
            }
        }

        #endregion

        #region E settings
        private static void CastE(Obj_AI_Base unit)
        {
            var playLegit = _menu.Item("ElEasy.Katarina.E.Legit").GetValue<bool>();
            var legitCastDelay = _menu.Item("ElEasy.Katarina.E.Delay").GetValue<Slider>().Value;

            if (playLegit)
            {
                if (Environment.TickCount > _lastECast + legitCastDelay)
                {
                    spells[Spells.E].CastOnUnit(unit);
                    _lastECast = Environment.TickCount;
                }
            }
            else
            {
                spells[Spells.E].CastOnUnit(unit);
                _lastECast = Environment.TickCount;
            }
        }

        #endregion

        #region OnJungleclear
        private static void OnJungleclear()
        {
            var useQ = _menu.Item("ElEasy.Katarina.JungleClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.JungleClear.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Katarina.JungleClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(minions[0]);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions[0]))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady())
            {
                CastE(minions[0]);
            }
        }

        #endregion

        #region OnLaneclear
        private static void OnLaneclear()
        {
            var useQ = _menu.Item("ElEasy.Katarina.LaneClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.LaneClear.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Katarina.LaneClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions[0]))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady())
            {
                CastE(minions[0]);
            }
        }

        #endregion

        #region OnLasthit
        private static void OnLasthit()
        {
            var useQ = _menu.Item("ElEasy.Katarina.Lasthit.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.Lasthit.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Katarina.Lasthit.E").GetValue<bool>();

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (spells[Spells.W].IsReady() && useW)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.W].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.W].Cast();
                            return;
                        }
                    }
                }
            }

            if (spells[Spells.E].IsReady() && useE)
            {
                foreach (var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.IsValidTarget() && minion.IsEnemy &&
                                minion.Distance(Player.ServerPosition) < spells[Spells.E].Range))
                {
                    var edmg = spells[Spells.E].GetDamage(minion);

                    if (minion.Health - edmg <= 0 && minion.Distance(Player.ServerPosition) <= spells[Spells.E].Range)
                    {
                        CastE(minion);
                    }
                }
            }
        }

        #endregion

        #region OnHarass
        private static void OnHarass()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = _menu.Item("ElEasy.Katarina.Harass.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.Harass.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Katarina.Harass.E").GetValue<bool>();
            var hMode = _menu.Item("ElEasy.Katarina.Harass.Mode").GetValue<StringList>().SelectedIndex;

            switch (hMode)
            {
                case 0:
                    if (useQ && spells[Spells.Q].IsReady())
                    {
                        spells[Spells.Q].CastOnUnit(target);
                    }
                    break;

                case 1:
                    if (useQ && useW)
                    {
                        if (spells[Spells.Q].IsReady())
                        {
                            spells[Spells.Q].Cast(target);
                        }
                        
                        if (spells[Spells.W].IsInRange(target) && spells[Spells.W].IsReady())
                        {
                            spells[Spells.W].Cast();
                        }
                    }
                    break;

                case 2:
                    if (useQ && useW && useE)
                    {
                        if (spells[Spells.Q].IsReady())
                        {
                            spells[Spells.Q].Cast(target);
                        }

                        if (spells[Spells.E].IsReady()) //&& !target.UnderTurret(true) -- need to create a on/off for this
                        {
                            CastE(target);
                        }

                        if (spells[Spells.W].IsReady())
                        {
                            spells[Spells.W].Cast();
                        }                        
                    }
                    break;
            }
        }

        #endregion

        #region OnCombo
        private static void OnCombo()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid)
            {
                return;
            }

            UseItems(target);

            var useQ = _menu.Item("ElEasy.Katarina.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Katarina.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Katarina.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Katarina.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Katarina.Combo.Ignite").GetValue<bool>();
            var rSort = _menu.Item("ElEasy.Katarina.Combo.Sort").GetValue<StringList>();
            var forceR = _menu.Item("ElEasy.Katarina.Combo.R.Force").GetValue<bool>();
            var forceRCount = _menu.Item("ElEasy.Katarina.Combo.R.Force.Count").GetValue<Slider>().Value;

            var rdmg = spells[Spells.R].GetDamage(target, 1);

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) && !spells[Spells.Q].IsReady() && !spells[Spells.W].IsReady() && !spells[Spells.E].IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(spells[Spells.R].Range)) &&
                       spells[Spells.R].Instance.Name == "KatarinaR")
                {
                    if (target.Health - rdmg < 0 && rSort.SelectedIndex == 1 && !spells[Spells.E].IsReady())
                    {
                        Orbwalker.SetMovement(false);
                        Orbwalker.SetAttack(false);
                        spells[Spells.R].Cast();
                        _rStart = Environment.TickCount;
                    }
                    else if (rSort.SelectedIndex == 0 && !spells[Spells.E].IsReady() ||
                             forceR && Player.CountEnemiesInRange(spells[Spells.R].Range) <= forceRCount)
                    {
                        Orbwalker.SetMovement(false);
                        Orbwalker.SetAttack(false);
                        spells[Spells.R].Cast();
                        _rStart = Environment.TickCount;
                    }
                }
            }

            if (spells[Spells.R].Instance.Name != "KatarinaR")
                return;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                CastE(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
                return;
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region AssassinManager
        private static Obj_AI_Hero GetEnemy(float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Magical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
            {
                vDefaultRange = spells[Spells.R].Range;
            }

            if (!_menu.Item("AssassinActive").GetValue<bool>())
            {
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);
            }

            var assassinRange = _menu.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            _menu.Item("Assassin" + enemy.ChampionName) != null &&
                            _menu.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

            if (_menu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float) Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Killsteal

        private static void KillSteal()
        {
            var ks = _menu.Item("ElEasy.Katarina.Killsteal").GetValue<bool>();

            if (ks)
            {
                foreach (Obj_AI_Hero hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                ObjectManager.Player.Distance(hero.ServerPosition) <= spells[Spells.E].Range &&
                                !hero.IsMe && hero.IsValidTarget() && hero.IsEnemy && !hero.IsInvulnerable))
                {
                    var qdmg = spells[Spells.Q].GetDamage(hero);
                    var wdmg = spells[Spells.W].GetDamage(hero);
                    var edmg = spells[Spells.E].GetDamage(hero);

                    var markDmg = Player.CalcDamage(
                        hero, Damage.DamageType.Magical, Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);
                    float ignitedmg;

                    if (_ignite != SpellSlot.Unknown)
                    {
                        ignitedmg = (float) Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                    }
                    else
                    {
                        ignitedmg = 0f;
                    }

                    if (hero.HasBuff("katarinaqmark") && hero.Health - wdmg - markDmg < 0 && spells[Spells.W].IsReady() &&
                        spells[Spells.W].IsInRange(hero))
                    {
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - ignitedmg < 0 && _ignite.IsReady())
                    {
                        Player.Spellbook.CastSpell(_ignite, hero);
                    }


                    if (hero.Health - edmg < 0 && spells[Spells.E].IsReady())
                    {
                        spells[Spells.E].Cast(hero);
                    }

                    if (hero.Health - qdmg < 0 && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(hero))
                    {
                        spells[Spells.Q].Cast(hero);
                    }

                    if (hero.Health - edmg - wdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                    }

                    if (hero.Health - edmg - wdmg - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() &&
                        spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - wdmg - qdmg - markDmg < 0 && spells[Spells.E].IsReady() &&
                        spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - wdmg - qdmg - ignitedmg < 0 && spells[Spells.E].IsReady() &&
                        spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() && _ignite.IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                        Player.Spellbook.CastSpell(_ignite, hero);
                    }
                }

                foreach (Obj_AI_Base target in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            target =>
                                ObjectManager.Player.Distance(target.ServerPosition) <= spells[Spells.E].Range &&
                                !target.IsMe && target.IsTargetable && !target.IsInvulnerable))
                {
                    foreach (Obj_AI_Hero focus in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                focus =>
                                    focus.Distance(focus.ServerPosition) <= spells[Spells.Q].Range && focus.IsEnemy &&
                                    !focus.IsMe && !focus.IsInvulnerable && focus.IsValidTarget()))
                    {
                        var qdmg = spells[Spells.Q].GetDamage(focus);
                        var wdmg = spells[Spells.W].GetDamage(focus);
                        float ignitedmg;

                        if (_ignite != SpellSlot.Unknown)
                        {
                            ignitedmg = (float) Player.GetSummonerSpellDamage(focus, Damage.SummonerSpell.Ignite);
                        }
                        else
                        {
                            ignitedmg = 0f;
                        }

                        var markDmg = Player.CalcDamage(
                            focus, Damage.DamageType.Magical, Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);

                        if (focus.Health - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() &&
                            focus.Distance(target.ServerPosition) <= spells[Spells.Q].Range)
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                        }

                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range &&
                            focus.Health - qdmg - wdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                        }

                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range &&
                            focus.Health - qdmg - wdmg - markDmg < 0 && spells[Spells.E].IsReady() &&
                            spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                        }

                        if (focus.Distance(target.ServerPosition) <= 600 && focus.Health - qdmg - ignitedmg < 0 &&
                            spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && _ignite.IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            Player.Spellbook.CastSpell(_ignite, focus);
                        }

                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range &&
                            focus.Health - qdmg - wdmg - ignitedmg < 0 && spells[Spells.E].IsReady() &&
                            spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() && _ignite.IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                            Player.Spellbook.CastSpell(_ignite, focus);
                        }
                    }
                }
            }
        }

        #endregion

        #region GetComboDamage   
        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;
            }

            return (float)damage;
        }

        #endregion

        #region Draw

        private static void OnDraw(EventArgs args)
        {
            var drawOff = _menu.Item("ElEasy.Katarina.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Katarina.Draw.Q").GetValue<Circle>();
            var drawW = _menu.Item("ElEasy.Katarina.Draw.W").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Katarina.Draw.E").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Katarina.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);

            if (drawW.Active)
                if (spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);

           if (drawE.Active)
                if (spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);

              if (drawR.Active)
                 if (spells[Spells.R].Level > 0)
                     Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
        }

        #endregion

        #region Menu

        private static void Initialize()
        {
            _menu = new Menu("ElKatarina", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.E", "Use E").SetValue(true));

            cMenu.SubMenu("E").AddItem(new MenuItem("ElEasy.Katarina.E.Legit", "Legit E").SetValue(false));
            cMenu.SubMenu("E")
                .AddItem(new MenuItem("ElEasy.Katarina.E.Delay", "E Delay").SetValue(new Slider(1000, 0, 2000)));

            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Katarina.Combo.R", "Use R").SetValue(true));
            cMenu.SubMenu("R")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Combo.Sort", "R:").SetValue(
                        new StringList(new[] { "Normal", "Smart" })));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Katarina.Combo.R.Force", "Force R").SetValue(false));
            cMenu.SubMenu("R")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Combo.R.Force.Count", "Force R when in range:").SetValue(
                        new Slider(3, 0, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var iMenu = new Menu("Items", "Items");
            iMenu.AddItem(new MenuItem("ElEasy.Katarina.Items.hextech", "Use Hextech Gunblade").SetValue(true));
            _menu.AddSubMenu(iMenu);

            var wMenu = new Menu("Wardjump", "Wardjump");
            wMenu.AddItem(new MenuItem("ElEasy.Katarina.Wardjump", "Wardjump key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            _menu.AddSubMenu(wMenu);


            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.E", "Use E").SetValue(true));

            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.AutoHarass.Activated", "Auto harass", true).SetValue(
                        new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Katarina.AutoHarass.Q", "Use Q").SetValue(true));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Katarina.AutoHarass.W", "Use W").SetValue(true));

            hMenu.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Harass.Mode", "Harass mode:").SetValue(
                        new StringList(new[] { "Q", "Q - W", "Q - E - W" })));

            _menu.AddSubMenu(hMenu);

            var ksMenu = new Menu("Killsteal", "Killsteal");
            ksMenu.AddItem(new MenuItem("ElEasy.Katarina.Killsteal", "Killsteal").SetValue(true));
            _menu.AddSubMenu(ksMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.E", "Use E").SetValue(false));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.E", "Use E").SetValue(false));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.E", "Use E").SetValue(false));

            _menu.AddSubMenu(clearMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.R", "Draw R").SetValue(new Circle()));

            var dmgAfterE = new MenuItem("ElEasy.Katarina.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill = new MenuItem("ElEasy.Katarina.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();
        }

        #endregion
    }
}
