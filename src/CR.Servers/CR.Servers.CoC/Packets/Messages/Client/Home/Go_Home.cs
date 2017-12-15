﻿using CR.Servers.CoC.Core;
using CR.Servers.CoC.Core.Network;
using CR.Servers.CoC.Logic;
using CR.Servers.CoC.Packets.Messages.Server.Battle;
using CR.Servers.CoC.Packets.Messages.Server.Home;
using CR.Servers.Extensions.Binary;
using CR.Servers.Logic.Enums;
using Microsoft.VisualBasic.CompilerServices;

namespace CR.Servers.CoC.Packets.Messages.Client.Home
{
    internal class Go_Home : Message
    {
        internal override short Type => 14101;

        public Go_Home(Device device, Reader reader) : base(device, reader)
        {
            // Space
        }

        internal int Mode;

        internal override void Decode()
        {
            Mode = Reader.ReadInt32();
        }

        internal override void Process()
        {
            if (Mode == 1)
            {
                Device.State = State.WAR_EMODE;
            }
            else
            {
                if (Device.State == State.IN_PC_BATTLE) //Replay
                    Device.State = State.LOGGED;
                else if (Device.State == State.IN_NPC_BATTLE) //Stream
                    Device.State = State.LOGGED;
                else if (Device.State == State.IN_AMICAL_BATTLE) //Stream
                    Device.State = State.LOGGED;
                else if (Device.State == State.SEARCH_BATTLE) //Search battle
                    Device.State = State.LOGGED;
                else if (this.Device.State == State.IN_1VS1_BATTLE)
                {

                    var userId = this.Device.GameMode.Level.Player.UserId;
                    var battleId = this.Device.GameMode.Level.Player.BattleIdV2;

                    var home = Resources.BattlesV2.GetPlayer(battleId, userId);
                    var enemy = Resources.BattlesV2.GetEnemy(battleId, userId);
                    var battle = Resources.BattlesV2[battleId];

                    home.Set_Replay_Info();
                    home.Finished = true;

                    /*if (Utils.IsOdd(Resources.Random.Next(1, 1000)))
                    {
                        Home.Replay_Info.Stats.Destruction_Percentage = Resources.Random.Next(49);
                    }
                    else
                    {
                        Home.Replay_Info.Stats.Destruction_Percentage = Resources.Random.Next(50, 100);
                        Home.Replay_Info.Stats.Attacker_Stars += 1;
                        if (Home.Replay_Info.Stats.Destruction_Percentage == 100)
                        {
                            Home.Replay_Info.Stats.TownHall_Destroyed = true;
                            Home.Replay_Info.Stats.Attacker_Stars += 2;
                        }
                    }*/

                    if (enemy.Finished)
                    {
                        new V2_Battle_Result(battle.GetEnemy(userId).GameMode.Device, battle).Send();
                    }

                    new V2_Battle_Result(this.Device, battle).Send();


                    this.Device.GameMode.Level.Player.BattleIdV2 = 0;
                    this.Device.State = State.LOGGED;
                }
            }

            new Own_Home_Data(Device).Send();
        }
    }
}
