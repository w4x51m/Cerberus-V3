﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Core;
using CR.Servers.CoC.Extensions;
using CR.Servers.CoC.Extensions.Game;
using CR.Servers.CoC.Extensions.Helper;
using CR.Servers.CoC.Files.CSV_Helpers;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using Newtonsoft.Json.Linq;

namespace CR.Servers.CoC.Logic
{
    internal class Trap : GameObject
    {


        private int UpgradeLevel;
        internal bool NeedRepair;

        internal Timer ConstructionTimer;
        internal TrapData TrapData => (TrapData)this.Data;

        internal override int HeightInTiles => this.TrapData.Height;

        internal override int WidthInTiles => this.TrapData.Width;

        internal override int Type => 4;

        internal override int VillageType => this.TrapData.VillageType;

        internal int RemainingConstructionTime => ConstructionTimer?.GetRemainingSeconds(this.Level.Player.LastTick) ?? 0;

        internal bool Constructing => this.ConstructionTimer != null;

        internal bool UpgradeAvailable
        {
            get
            {
                if (!this.Constructing)
                {
                    var Data = this.TrapData;

                    if (Data.MaxLevel > this.UpgradeLevel)
                    {
                        if (this.VillageType == 1)
                        {
                            return this.Level.GameObjectManager.TownHall2.GetUpgradeLevel() + 1 >= Data.TownHallLevel[this.UpgradeLevel + 1];
                        }
                        return this.Level.GameObjectManager.TownHall.GetUpgradeLevel() + 1 >=  Data.TownHallLevel[this.UpgradeLevel + 1];
                    }
                }

                return false;
            }
        }

        internal CombatComponent CombatComponent => this.TryGetComponent(1, out Component Component) ? (CombatComponent)Component : null;

        internal int GetUpgradeLevel() => this.UpgradeLevel;

        public Trap(Data Data, Level Level) : base(Data, Level)
        {
            if (this.TrapData.HasAltMode || this.TrapData.DirectionCount > 0)
            {
                AddComponent(new CombatComponent(this));
            }
        }

        internal void StartUpgrade()
        {   
            int Time = this.TrapData.GetBuildTime(this.UpgradeLevel + 1);

            if (!this.Constructing)
            {
                if (this.VillageType == 0)
                {
                    this.Level.WorkerManager.AllocateWorker(this);
                }
                else
                {
                    this.Level.WorkerManagerV2.AllocateWorker(this);
                }

                if (Time <= 0)
                {
                    this.FinishConstruction();
                }
                else
                {
                    this.ConstructionTimer = new Timer();
                    this.ConstructionTimer.StartTimer(this.Level.Player.LastTick, Time);
                }
            }
        }

        internal void SpeedUpConstruction()
        {
            if (this.Level.Player != null)
            {
                if (this.Constructing)
                {
                    int Cost = GamePlayUtil.GetSpeedUpCost(this.RemainingConstructionTime, this.VillageType, 100);
                    if (this.Level.Player.HasEnoughDiamonds(Cost))
                    {
                        this.Level.Player.UseDiamonds(Cost);
                        this.FinishConstruction();
                    }
                }
            }
        }

        internal void FinishConstruction(bool NoWorker = false)
        {

            TrapData Data = this.TrapData;
            if (this.UpgradeLevel + 1 > Data.MaxLevel)
            {
                Logging.Error(this.GetType(), "Unable to upgrade the building because the level is out of range! - " + Data.Name + ".");
                this.SetUpgradeLevel(Data.MaxLevel);
            }
            else
                this.SetUpgradeLevel(this.UpgradeLevel + 1);

            if (!NoWorker)
            {
                if (this.VillageType == 0)
                {
                    this.Level.WorkerManager.DeallocateWorker(this);
                }
                else
                {
                    this.Level.WorkerManagerV2.DeallocateWorker(this);
                }
            }


            this.Level.Player.AddExperience(GamePlayUtil.TimeToXp(Data.GetBuildTime(this.UpgradeLevel)));
            this.ConstructionTimer = null;

        }

        internal void CancelConstruction()
        {
            if (this.Constructing)
            {           
                //Alt resource not supported yet

                var resourceCount = (int)((this.TrapData.BuildCost[this.UpgradeLevel + 1] * Globals.BuildCancelMultiplier * (long)1374389535) >> 32);
                resourceCount = Math.Max((resourceCount >> 5) + (resourceCount >> 31), 0);

                this.Level.Player.Resources.Plus(this.TrapData.GlobalId, resourceCount);

                if (this.VillageType == 0)
                {
                    this.Level.WorkerManager.DeallocateWorker(this);
                }
                else
                {
                    this.Level.WorkerManagerV2.DeallocateWorker(this);
                }

                this.ConstructionTimer = null;
                if (UpgradeLevel == -1)
                    this.Level.GameObjectManager.RemoveGameObject(this, this.VillageType); //Should never happend since supercell disable this
            }
        }

        internal void SetUpgradeLevel(int UpgradeLevel)
        {
            this.UpgradeLevel = UpgradeLevel;
        }

        internal override void FastForwardTime(int Seconds)
        {
            if (this.Constructing)
            {
                this.ConstructionTimer.FastForward(Seconds);
            }

            base.FastForwardTime(Seconds);
        }

        internal override void Tick()
        {
            if (this.Constructing)
            {
                if (this.ConstructionTimer.GetRemainingSeconds(this.Level.Player.LastTick) <= 0)
                {
                    this.FinishConstruction();
                }
            }

            base.Tick();
        }

        internal override void Load(JToken Json)
        {
            TrapData Data = this.TrapData;

            if (JsonHelper.GetJsonNumber(Json, "const_t", out int ConstructionTime) && JsonHelper.GetJsonNumber(Json, "const_t_end", out int ConstructionTimeEnd))
            {
                if (ConstructionTime > -1)
                {
                    var startTime = (int) TimeUtils.ToUnixTimestamp(this.Level.Player.LastTick);
                    var duration = ConstructionTimeEnd - startTime;
                    if (duration < 0)
                        duration = 0;
                    //ConstructionTime = Math.Min(ConstructionTime, Data.GetBuildTime(this.UpgradeLevel + 1));

                    this.ConstructionTimer = new Timer();
                    this.ConstructionTimer.StartTimer(this.Level.Player.LastTick, duration);
                    if (this.VillageType == 0)
                        this.Level.WorkerManager.AllocateWorker(this);
                    else
                    {
                        if (this.UpgradeLevel > -1)
                            this.Level.WorkerManagerV2.AllocateWorker(this);
                    }

                }
            }

            if (JsonHelper.GetJsonNumber(Json, "lvl", out int Level))
            {
                if (Level < -1)
                {
                    Logging.Error(this.GetType(), "An error has been throwed when the loading of building - Load an illegal upgrade level. Level : " + Level);
                    this.SetUpgradeLevel(0);
                }
                else if (Level > Data.MaxLevel)
                {
                    Logging.Error(this.GetType(), $"An error has been throwed when the loading of building - Loaded upgrade level {Level + 1} is over max! (max = {Data.MaxLevel + 1}) id {this.Id} data id {Data.GlobalId}");
                    this.SetUpgradeLevel(Data.MaxLevel);
                }
                else
                    this.SetUpgradeLevel(Level);
            }

            JsonHelper.GetJsonBoolean(Json, "needs_repair", out this.NeedRepair);

            base.Load(Json);
        }

        internal override void Save(JObject Json)
        {
            Json.Add("lvl", this.UpgradeLevel);

            if (this.ConstructionTimer != null)
            {
                Json.Add("const_t", this.ConstructionTimer.GetRemainingSeconds(this.Level.Player.LastTick));
                Json.Add("const_t_end", this.ConstructionTimer.EndTime);
            }

            if (this.NeedRepair)
            {
                Json.Add("needs_repair", this.NeedRepair);
            }

            base.Save(Json);
        }
    }
}
