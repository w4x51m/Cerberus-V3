﻿using System;
using System.Collections.Generic;
using CR.Servers.CoC.Extensions.Helper;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using Newtonsoft.Json.Linq;

namespace CR.Servers.CoC.Logic.Manager
{
    internal class SpellProductionManager
    {
        internal Level Level;
        internal Timer Timer;
        internal List<ProductionItem> Productions;

        internal int InProductionCapacity
        {
            get
            {
                int Space = 0;

                foreach (ProductionItem Production in this.Productions)
                {
                    Space += ((SpellData)Production.ItemData).HousingSpace * Production.Count;
                }

                return Space;
            }
        }

        internal bool ProduceUnit => this.Productions.Exists(T => !T.Terminate);

        public SpellProductionManager(Level Level)
        {
            this.Level = Level;
            this.Timer = new Timer();
            this.Productions = new List<ProductionItem>();
        }

        internal void AddUnit(SpellData Data, int Count)
        {
            if (this.CanProduce(Data, Count))
            {
                if (this.Timer.Started)
                {
                    /*
                    ProductionItem First = this.Productions.Find(T => !T.Terminate);

                    if (First.Data == Data)
                    {
                        First.Count += Count;
                        return;
                    }
                    */

                    ProductionItem Last = this.Productions.FindLast(T => T.ItemData == Data && !T.Terminate);

                    if (Last != null)
                    {
                        Last.Count += Count;
                        return;
                    }
                }
                else
                {
                    this.Timer.StartTimer(this.Level.Player.LastTick, this.GetTrainingTime(Data));
                }

                this.Productions.Add(new ProductionItem(Data, Count));
            }
        }

        internal bool CanProduce(SpellData Spell, int Count)
        {
            if (!Spell.DisableProduction)
            {
                if ((Spell.UnitOfType == 1 ? this.Level.ComponentManager.MaxSpellForgeLevel : this.Level.ComponentManager.MaxDarkSpellForgeLevel) >= Spell.SpellForgeLevel - 1)
                {
                    if (this.Level.ComponentManager.TotalMaxHousing >= this.InProductionCapacity + Spell.HousingSpace * Count)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void FastForwardTime(int Seconds)
        {
            this.Timer.FastForward(Seconds);
        }

        internal int GetTrainingTime(SpellData Spell)
        {
            var SpellForges = (Spell.UnitOfType == 1 ? this.Level.ComponentManager.SpellForge : this.Level.ComponentManager.DarkSpellForge)
                .FindAll(SpellForge => SpellForge.GetUpgradeLevel() >= Spell.SpellForgeLevel - 1);

            if (SpellForges.Count != 0)
                return Spell.TrainingTime / SpellForges.Count;

            return Spell.TrainingTime;
        }

        internal void Load(JToken Json)
        {
            var Slots = (JArray)Json?["slots"];

            if (Slots != null)
            {
                foreach (var Token in Slots)
                {
                    var Item = new ProductionItem();
                    Item.Load(Token);
                    this.Productions.Add(Item);
                }

                if (this.Productions.Count > 0)
                {
                    if (JsonHelper.GetJsonNumber(Json, "t", out int Time))
                        this.Timer.StartTimer(this.Level.Player.LastTick, Time);
                }
            }
        }

        internal JObject Save()
        {
            var Json = new JObject();

            if (this.Timer.Started)
                Json.Add("t", this.Timer.GetRemainingSeconds(this.Level.Player.LastTick));

            var Slots = new JArray();

            foreach (var Production in this.Productions)
                Slots.Add(Production.Save());

            Json.Add("slots", Slots);

            return Json;
        }

        internal void Tick()
        {
            Console.WriteLine(this.Level.ComponentManager.TotalMaxHousing);
            Console.WriteLine(this.Level.ComponentManager.TotalMaxSpellHousing);
            if (this.Productions.Count > 0)
            {
                var AvailableStorage = this.Level.ComponentManager.TotalMaxSpellHousing - this.Level.Player.Spells.GetUnitsTotalCapacity();
                var CanAddProductionInPlayer = true;

                for (var i = 0; i < this.Productions.Count; i++)
                {
                    var Production = this.Productions[i];
                    var Character = (SpellData)Production.ItemData;

                    if (Production.Terminate)
                    {
                        if (i == 0)
                        {
                            while (Production.Count > 0)
                            {
                                if (AvailableStorage >= Character.HousingSpace)
                                {
                                    this.Level.Player.Spells.Add(Character, 1);

                                    AvailableStorage += Character.HousingSpace;

                                    Production.Count--;
                                }

                                break;
                            }

                            if (Production.Count <= 0)
                            {
                                this.Productions.RemoveAt(i--);
                            }
                        }
                    }
                    else
                    {
                        while (Production.Count > 0)
                        {
                            if (this.Timer.GetRemainingSeconds(this.Level.Player.LastTick) <= 0)
                            {
                                if (AvailableStorage >= Character.HousingSpace && CanAddProductionInPlayer)
                                {
                                    this.Level.Player.Spells.Add(Character, 1);

                                    AvailableStorage += Character.HousingSpace;
                                }
                                else
                                {
                                    if (i > 0)
                                    {
                                        ProductionItem Previous = this.Productions[i - 1];

                                        if (Previous.Terminate)
                                        {
                                            if (Previous.Data == Production.Data)
                                            {
                                                Previous.Count++;
                                            }
                                            else
                                            {
                                                this.Productions.Insert(i++,
                                                    new ProductionItem(Production.Data, 1, true));
                                            }
                                        }
                                        else
                                        {
                                            this.Productions.Insert(i++, new ProductionItem(Production.Data, 1, true));
                                        }
                                    }
                                    else
                                    {
                                        this.Productions.Insert(i++, new ProductionItem(Production.Data, 1, true));
                                    }

                                    CanAddProductionInPlayer = false;
                                }

                                Production.Count--;

                                this.Timer.IncreaseTimer(this.GetTrainingTime(Character));
                            }
                            else
                                break;
                        }

                        if (Production.Count <= 0)
                        {
                            this.Productions.RemoveAt(i--);
                        }
                    }

                    if (this.Timer.Started && !this.ProduceUnit)
                    {
                        this.Timer.StopTimer();
                    }
                }
            }
        }
    }
}