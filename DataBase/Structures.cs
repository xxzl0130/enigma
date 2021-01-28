using System;
using System.Collections.Generic;
using System.Text;

namespace enigma
{
    namespace DataBase
    {
        /// <summary>
        /// 一条记录的基本信息
        /// </summary>
        public class RecordBase
        {
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid;
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp;
        }

        /// <summary>
        /// 普通建造枪
        /// </summary>
        public class DevelopGun : RecordBase
        {
            /// <summary>
            /// 人力
            /// </summary>
            public int mp;
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo;
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre;
            /// <summary>
            /// 零件
            /// </summary>
            public int part;
            /// <summary>
            /// 获得的枪支的id
            /// </summary>
            public int gun_id;
        }

        /// <summary>
        /// 重型建造枪
        /// </summary>
        public class DevelopHeavyGun : DevelopGun
        {
            /// <summary>
            /// 重建资源等级
            /// </summary>
            public int input_level;
        }

        /// <summary>
        /// 推荐公式建造装备
        /// </summary>
        public class ProduceEquip : RecordBase
        {
            /// <summary>
            /// 公式id
            /// </summary>
            public int formula_id;
            /// <summary>
            /// 装备id
            /// </summary>
            public int equip_id;
        }

        /// <summary>
        /// 普通建造装备
        /// </summary>
        public class DevelopEquip : RecordBase
        {
            /// <summary>
            /// 人力
            /// </summary>
            public int mp;
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo;
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre;
            /// <summary>
            /// 零件
            /// </summary>
            public int part;
            /// <summary>
            /// 获得的装备的id
            /// </summary>
            public int equip_id;
        }

        /// <summary>
        /// 装备重型建造
        /// </summary>
        public class DevelopHeavyEquip : DevelopEquip
        {
            /// <summary>
            /// 重建材料等级
            /// </summary>
            public int input_level;
            /// <summary>
            /// 妖精id
            /// </summary>
            public int fairy_id;
            /// <summary>
            /// 被动技能
            /// </summary>
            public int passive_skill;
            /// <summary>
            /// 质量等级？暂时未知的数据
            /// </summary>
            public int quality_lv;
        }

        /// <summary>
        /// 一场战斗结束的数据
        /// </summary>
        public class BattleFinish : RecordBase
        {
            /// <summary>
            /// 杀死的敌人编号
            /// </summary>
            public int enemy;
            /// <summary>
            /// 获得的枪的id，原始数据里是list但是数据库里只能存单个的
            /// </summary>
            public int gun_id;
            /// <summary>
            /// 战斗等级
            /// </summary>
            public int battle_rank;
            /// <summary>
            /// 战役id
            /// </summary>
            public int mission_id;
        }

        public class MissionFinish : RecordBase
        {
            /// <summary>
            /// 获得的枪的id，原始数据里是list但是数据库里只能存单个的
            /// </summary>
            public int reward_gun;
            /// <summary>
            /// 获得的枪的id，原始数据里是list但是数据库里只能存单个的
            /// </summary>
            public int reward_gun_extra;
            /// <summary>
            /// 战役等级
            /// </summary>
            public int mission_rank;
            /// <summary>
            /// 战役id
            /// </summary>
            public int mission_id;
        }
    }
}
