﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Extensions.Game;
using CR.Servers.CoC.Extensions.Helper;
using CR.Servers.CoC.Files;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using CR.Servers.CoC.Logic;
using CR.Servers.CoC.Logic.Enums;
using CR.Servers.Extensions.Binary;

namespace CR.Servers.CoC.Packets.Commands.Client
{
    internal class Buy_Building : Command
    {
        internal override int Type => 500;

        public Buy_Building(Device device, Reader reader) : base(device, reader)
        {
        }

        internal int X;
        internal int Y;

        internal BuildingData BuildingData;


        internal override void Decode()
        {
            this.X = Reader.ReadInt32();
            this.Y = Reader.ReadInt32();

            this.BuildingData = Reader.ReadData<BuildingData>();

            ExecuteSubTick = Reader.ReadInt32();
        }

        internal override void Execute()
        {
            if (this.Data != null)
            {
                var Level = Device.GameMode.Level;
                if (!Level.IsBuildingCapReached(this.BuildingData))
                {
                    BuildingClassData BuildingClassData = (BuildingClassData)CSV.Tables.Get(Gamefile.Building_Classes).GetData(this.BuildingData.BuildingClass);
                    ResourceData ResourceData = (ResourceData)CSV.Tables.Get(Gamefile.Resources).GetData(this.BuildingData.BuildResource);

                    if (BuildingClassData.CanBuy)
                    {
                        if (Level.GameObjectManager.Map == 0)
                        {
                            if (this.BuildingData.TownHallLevel[0] <=
                                Level.GameObjectManager.TownHall.GetUpgradeLevel() + 1)
                            {
                                if (this.BuildingData.IsWorker)
                                {
                                    int Cost;

                                    switch (Level.WorkerManager.WorkerCount)
                                    {
                                        case 1:
                                            Cost = Globals.WorkerCost2Nd;
                                            break;
                                        case 2:
                                            Cost = Globals.WorkerCost3Rd;
                                            break;
                                        case 3:
                                            Cost = Globals.WorkerCost4Th;
                                            break;
                                        case 4:
                                            Cost = Globals.WorkerCost5Th;
                                            break;

                                        default:
                                            Cost = this.BuildingData.BuildCost[0];
                                            break;
                                    }

                                    if (Level.Player.HasEnoughDiamonds(Cost))
                                    {
                                        Level.Player.UseDiamonds(Cost);
                                        this.StartConstruction(Level);
                                    }
                                }

                                if (Level.Player.Resources.GetCountByData(ResourceData) >=
                                    this.BuildingData.BuildCost[0])
                                {
                                    if (Level.WorkerManager.FreeWorkers > 0)
                                    {
                                        Level.Player.Resources.Remove(ResourceData, this.BuildingData.BuildCost[0]);
                                        this.StartConstruction(Level);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void StartConstruction(Level Level)
        {
            Building GameObject = new Building(this.BuildingData, Level);

            GameObject.SetUpgradeLevel(-1);

            GameObject.Position.X = this.X << 9;
            GameObject.Position.Y = this.Y << 9;

            Level.WorkerManager.AllocateWorker(GameObject);

            if (this.BuildingData.GetBuildTime(0) <= 0)
            {
                GameObject.FinishConstruction();
            }
            else
            {
                GameObject.ConstructionTimer = new Timer();
                GameObject.ConstructionTimer.StartTimer(Level.GameMode.Time, this.BuildingData.GetBuildTime(0));
            }

            Level.GameObjectManager.AddGameObject(GameObject, Level.Player.Map);
        }
    }
}