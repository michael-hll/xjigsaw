using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XJigsaw.Helper;
using SQLite;

namespace XJigsaw.Models
{
    public class Database
    {
        readonly SQLiteAsyncConnection databaseConn;

        public Database(string dbPath)
        {
            databaseConn = new SQLiteAsyncConnection(dbPath);
            databaseConn.CreateTableAsync<Jigsaw>().Wait();
            databaseConn.CreateTableAsync<Setting>().Wait();
            databaseConn.CreateTableAsync<Level>().Wait();
            databaseConn.CreateTableAsync<User>().Wait();
            databaseConn.CreateTableAsync<JigsawVersion>().Wait();
        }

        #region Setting

        public Task<List<Setting>> GetSettingAsync()
        {
            return databaseConn.Table<Setting>().ToListAsync();
        }

        public Task<int> SaveSettingAsync(Setting setting)
        {
            if (setting.ID != 0)
            {
                return databaseConn.UpdateAsync(setting);
            }
            else
            {
                return databaseConn.InsertAsync(setting);
            }
        }

        public Task<int> DeleteAllSettings()
        {
            return databaseConn.DeleteAllAsync<Setting>();
        }

        #endregion

        #region Jigsaw

        public Task<List<Jigsaw>> GetJigsawsAsync()
        {
            return databaseConn.Table<Jigsaw>().ToListAsync();
        }

        public Task<List<Jigsaw>> GetJigsawByID(int id)
        {
            return databaseConn.Table<Jigsaw>().Where(t => t.ID == id).ToListAsync();
        }

        public Task<List<Jigsaw>> GetLatestJigsawsAsync(int count)
        {
            var result = databaseConn.Table<Jigsaw>().OrderByDescending(t => t.ID).Take(count).ToListAsync();
            return result;
        }

        public Task<int> SaveJigsawAsync(Jigsaw jigsaw)
        {
            if (jigsaw.ID != 0)
            {
                jigsaw.UpdatedDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT);
                jigsaw.IsUpdated = false;
                return databaseConn.UpdateAsync(jigsaw);
            }
            else
            {
                return databaseConn.InsertAsync(jigsaw);
            }
        }

        public Task<int> DeleteAllJigsaws()
        {
            return databaseConn.DeleteAllAsync<Jigsaw>();
        }

        public Task<int> DeleteJigsawById(int id)
        {
            return databaseConn.DeleteAsync<Jigsaw>(id);
        }

        #endregion

        #region Level

        public Task<int> InsertLevelAsync(Level level)
        {
            return databaseConn.InsertAsync(level);
        }

        public Task<int> InsertAllLevelAsync(List<Level> levels)
        {
            return databaseConn.InsertAllAsync(levels);
        }

        public Task<List<Level>> GetAllLevelsAsync()
        {
            return databaseConn.Table<Level>().ToListAsync();
        }

        public Task<int> DeleteAllLevels()
        {
            return databaseConn.DeleteAllAsync<Level>();
        }

        public Task<int> UpdateLevelAysnc(Level level)
        {
            return databaseConn.UpdateAsync(level);
        }

        #endregion

        #region User
        public Task<int> InsertUserAsync(User user)
        {
            return databaseConn.InsertAsync(user);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return databaseConn.Table<User>().ToListAsync();
        }

        public Task<int> DeleteAllUsers()
        {
            return databaseConn.DeleteAllAsync<User>();
        }

        public Task<int> UpdateUser(User user)
        {
            return databaseConn.UpdateAsync(user);
        }
        #endregion

        #region Version
        public Task<List<JigsawVersion>> GetJigsawVersionAsync()
        {
            return databaseConn.Table<JigsawVersion>().ToListAsync();
        }

        public Task<int> SaveJigsawVersionAsync(JigsawVersion version)
        {
            if (version.ID != 0)
            {
                return databaseConn.UpdateAsync(version);
            }
            else
            {
                return databaseConn.InsertAsync(version);
            }
        }

        public Task<int> DeleteAllVersions()
        {
            return databaseConn.DeleteAllAsync<JigsawVersion>();
        }
        #endregion

    }
}
