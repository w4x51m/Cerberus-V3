using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Magic.Royale.Core.Database;
using Magic.Royale.Logic;

namespace Magic.Royale.Core
{
    internal static class DatabaseManager
    {
        internal static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,                            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,                 NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.All,         ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //Formatting = Formatting.Indented
        };
        

        public static long GetMaxAllianceId()
        {
            try
            {
                return MySQL_V2.GetAllianceSeed();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to retrieve max alliance ID; check config.");
            }
            return -1;
        }

        public static long GetMaxPlayerId()
        {
            try
            {
                return MySQL_V2.GetPlayerSeed();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to retrieve max player ID; check config.");
            }
            return -1;
        }

        public static void CreateLevel(Avatar level)
        {
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    var newPlayer = new player
                    {
                        Id = level.UserId,

                        Avatar = JsonConvert.SerializeObject(level, Settings),
                    };

                    ctx.player.Add(newPlayer);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to create a new player account in database.");
            }
        }
       

        /*public void CreateAlliance(Alliance alliance)
        {
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    var newClan = new clan
                    {
                        ClanId = alliance.AllianceId,
                        LastUpdateTime = DateTime.Now,
                        Data = alliance.SaveToJson()
                    };

                    ctx.clan.Add(newClan);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to create a new alliance in database.");
            }
        }*/

        public static Avatar GetLevel(long userId)
        {
            Avatar avatar = default(Avatar);
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    var player = ctx.player.Find(userId);
                    if (player != null)
                    {
                        avatar = JsonConvert.DeserializeObject<Avatar>(player.Avatar);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, $"Exception while trying to get a level {userId} from the database.");

                // In case the level instance was already created before the exception.
                avatar = null;
            }
            return avatar;
        }

       /* public Alliance GetAlliance(long allianceId)
        {
            var alliance = default(Alliance);
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    var clan = ctx.clan.Find(allianceId);
                    if (clan != null)
                    {
                        alliance = new Alliance();
                        alliance.LoadFromJson(clan.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to get alliance from database.");

                // In case it fails to LoadFromJSON.
                alliance = null;
            }

            return alliance;
        }

        // Used whenever the clients searches for an alliance however no alliances is loaded in memory.
        public List<Alliance> GetAllAlliances()
        {
            var alliances = new List<Alliance>();
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    var clans = ctx.clan;
                    Parallel.ForEach(clans, c =>
                    {
                        Alliance alliance = new Alliance();
                        alliance.LoadFromJson(c.Data);
                        alliances.Add(alliance);
                    });
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Exception while trying to get all alliances from database.");
            }

            return alliances;
        }

        public void RemoveAlliance(Alliance alliance)
        {
            long allianceId = alliance.AllianceId;
            using (MysqlEntities ctx = new MysqlEntities())
            {
                var clan = ctx.clan.Find(allianceId);

                ctx.clan.Remove(clan);
                ctx.SaveChanges();
            }

            ObjectManager.RemoveInMemoryAlliance(allianceId);
        }

        public async Task Save(Alliance alliance)
        {
            using (MysqlEntities ctx = new MysqlEntities())
            {
                ctx.Configuration.AutoDetectChangesEnabled = false;
                ctx.Configuration.ValidateOnSaveEnabled = false;
                clan c = await ctx.clan.FindAsync((int)alliance.AllianceId);
                if (c != null)
                {
                    c.LastUpdateTime = DateTime.Now;
                    c.Data = alliance.SaveToJson();
                    ctx.Entry(c).State = EntityState.Modified;
                }
                await ctx.SaveChangesAsync();
            }
        }*/

        public static void Save(Avatar level)
        {
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    ctx.Configuration.AutoDetectChangesEnabled = false;

                    var player = ctx.player.Find(level.UserId);
                    if (player != null)
                    {
                        player.Avatar = JsonConvert.SerializeObject(level, Settings);

                        ctx.Entry(player).State = EntityState.Modified;
                    }

                    ctx.SaveChanges();
                }
            }
            catch (DbEntityValidationException ex)
            {
                ExceptionLogger.Log(ex, $"Exception while trying to save a level {level.UserId} to the database. Check error for more information.");
                foreach (var entry in ex.EntityValidationErrors)
                {
                    foreach (var errs in entry.ValidationErrors)
                        Logger.Error($"{errs.PropertyName}:{errs.ErrorMessage}");
                }
                throw;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, $"Exception while trying to save a level {level.UserId} to the database.");
                throw;
            }
        }

        public static async Task Save(List<Avatar> levels)
        {
            try
            {
                using (var ctx = new MysqlEntities())
                {
                    foreach (var pl in levels)
                    {
                        player p = await ctx.player.FindAsync(pl.UserId); //Maybe to use lock instead
                        if (p != null)
                        {
                            p.Avatar = JsonConvert.SerializeObject(pl);
                        }
                    }
                    await ctx.BulkSaveChangesAsync();
                }
            }
            catch (DbEntityValidationException ex)
            {
                ExceptionLogger.Log(ex, $"Exception while trying to save {levels.Count} of player to the database. Check error for more information.");
                foreach (var entry in ex.EntityValidationErrors)
                {
                    foreach (var errs in entry.ValidationErrors)
                        Logger.Error($"{errs.PropertyName}:{errs.ErrorMessage}");
                }
                throw;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, $"Exception while trying to savesave {levels.Count} of player to the database.");
                throw;
            }
        }

        /*public async Task Save(List<Alliance> alliances)
        {
            try
            {
                using (MysqlEntities ctx = new MysqlEntities())
                {
                    foreach (Alliance alliance in alliances)
                    {
                        lock (alliance)
                        {
                            clan c = ctx.clan.Find((int)alliance.AllianceId);
                            if (c != null)
                            {
                                c.LastUpdateTime = DateTime.Now;
                                c.Data = alliance.SaveToJson();
                                ctx.Entry(c).State = EntityState.Modified;
                            }
                        }
                    }
                    await ctx.BulkSaveChangesAsync();
                }
            }
            catch
            {
                // 1 Actual fuck given.
            }
        }*/
    }
}
