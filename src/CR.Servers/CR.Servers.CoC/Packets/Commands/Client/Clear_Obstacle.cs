﻿using CR.Servers.CoC.Core;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using CR.Servers.CoC.Logic;
using CR.Servers.Extensions.Binary;

namespace CR.Servers.CoC.Packets.Commands.Client
{
    internal class Clear_Obstacle : Command
    {
        internal override int Type => 507;

        public Clear_Obstacle(Device device, Reader reader) : base(device, reader)
        {
        }

        internal int ObstacleId;

        internal override void Decode()
        {
            this.ObstacleId = Reader.ReadInt32();
            base.Decode();
        }

        internal override void Execute()
        {
            var Level = this.Device.GameMode.Level;

            var GameObject = Level.GameObjectManager.Filter.GetObstacleById(this.ObstacleId);

            if (GameObject != null)
            {
                var Obstacle = GameObject as Obstacle;
                if (Obstacle != null)
                {
                    ObstacleData Data = Obstacle.ObstacleData;
                    ResourceData ResourceData = Data.ClearResourceData;

                    if (ResourceData != null)
                    {
                        if (Level.Player.Resources.GetCountByData(ResourceData) >= Data.ClearCost)
                        {
                            if (Level.GameObjectManager.Map == 0 ? Level.WorkerManager.FreeWorkers > 0 : Level.WorkerManagerV2.FreeWorkers > 0)
                            {
                                Level.Player.Resources.Remove(ResourceData, Data.ClearCost);
                                Obstacle.StartClearing();
                            }
                            else
                                Logging.Error(this.GetType(), "Unable to start clearing the Obstacle. The player doesn't have any free worker.");
                        }
                        else
                            Logging.Error(this.GetType(), "Unable to start clearing the Obstacle. The player doesn't have enough resources.");
                    }
                    else
                        Logging.Error(this.GetType(), "Unable to start clearing the Obstacle. The resources data is null.");
                }
                else
                    Logging.Error(this.GetType(), "Unable to start clearing the Obstacle. GameObject is not valid or not exist.");
            }
            else
                Logging.Error(this.GetType(), "Unable to start clearing the Obstacle. GameObject is null");
        }
    }
}