using System.Threading.Tasks;
using SQLite;

namespace enigma
{
    namespace DataBase
    {
        public class DB
        {
            /// <summary>
            /// 单例对象
            /// </summary>
            private static readonly DB _instance = new DB();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static DB Instance => _instance;
            
            /// <summary>
            /// 日志接口，由外部提供实例
            /// </summary>
            public Serilog.ILogger Log = null;

            /// <summary>
            /// 数据库路径，需要在启动前提供或者在start函数中提供
            /// </summary>
            public string DataBasePath = "data.db";

            private SQLiteConnection _db = null;

            /// <summary>
            /// 启动数据库运行
            /// </summary>
            /// <param name="dataBasePath">数据库路径</param>
            public void Start(string dataBasePath = null)
            {
                if (dataBasePath != null)
                    this.DataBasePath = dataBasePath;
                _db = new SQLiteConnection(DataBasePath);

                // 创建表，这个库会自动处理数据结构变更和重复创建
                _db.CreateTable<DevelopGun>();
                _db.CreateTable<DevelopHeavyGun>();
                _db.CreateTable<ProduceEquip>();
                _db.CreateTable<DevelopEquip>();
                _db.CreateTable<DevelopHeavyEquip>();
                _db.CreateTable<BattleFinish>();
                _db.CreateTable<MissionFinish>();
            }
        }
    }
}