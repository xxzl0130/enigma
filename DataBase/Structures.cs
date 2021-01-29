using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace enigma
{
    namespace DataBase
    {
        // SQLite里的列必须是属性

        /// <summary>
        /// 普通建造枪
        /// </summary>
        [Table("GunDevelop")] // 表名调整为按类型统一开头，下同
        public class DevelopGun
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 人力
            /// </summary>
            public int mp { get; set; }
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo { get; set; }
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre { get; set; }
            /// <summary>
            /// 零件
            /// </summary>
            public int part{ get; set; }
            /// <summary>
            /// 获得的枪支的id
            /// </summary>
            public int gun_id { get; set; }
        }

        /// <summary>
        /// 重型建造枪
        /// </summary>
        [Table("GunDevelopHeavy")]
        public class DevelopHeavyGun
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 人力
            /// </summary>
            public int mp { get; set; }
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo { get; set; }
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre { get; set; }
            /// <summary>
            /// 零件
            /// </summary>
            public int part { get; set; }
            /// <summary>
            /// 获得的枪支的id
            /// </summary>
            public int gun_id { get; set; }
            /// <summary>
            /// 重建资源等级
            /// </summary>
            public int input_level { get; set; }
        }

        /// <summary>
        /// 推荐公式建造装备
        /// </summary>
        [Table("EquipProduce")]
        public class ProduceEquip
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 公式id
            /// </summary>
            public int formula_id { get; set; }
            /// <summary>
            /// 装备id
            /// </summary>
            public int equip_id { get; set; }
        }

        /// <summary>
        /// 普通建造装备
        /// </summary>
        [Table("EquipDevelop")]
        public class DevelopEquip
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 人力
            /// </summary>
            public int mp { get; set; }
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo { get; set; }
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre { get; set; }
            /// <summary>
            /// 零件
            /// </summary>
            public int part { get; set; }
            /// <summary>
            /// 获得的装备的id
            /// </summary>
            public int equip_id { get; set; }
        }

        /// <summary>
        /// 装备重型建造
        /// </summary>
        [Table("EquipDevelopHeavy")]
        public class DevelopHeavyEquip
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 人力
            /// </summary>
            public int mp { get; set; }
            /// <summary>
            /// 弹药
            /// </summary>
            public int ammo { get; set; }
            /// <summary>
            /// 口粮
            /// </summary>
            public int mre { get; set; }
            /// <summary>
            /// 零件
            /// </summary>
            public int part { get; set; }
            /// <summary>
            /// 获得的装备的id
            /// </summary>
            public int equip_id { get; set; }
            /// <summary>
            /// 重建材料等级
            /// </summary>
            public int input_level { get; set; }
            /// <summary>
            /// 妖精id
            /// </summary>
            public int fairy_id { get; set; }
            /// <summary>
            /// 被动技能
            /// </summary>
            public int passive_skill { get; set; }
            /// <summary>
            /// 质量等级？暂时未知的数据
            /// </summary>
            public int quality_lv { get; set; }
        }

        /// <summary>
        /// 一场战斗结束的数据
        /// </summary>
        [Table("MissionBattle")]
        public class BattleFinish
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 杀死的敌人编号
            /// </summary>
            public int enemy { get; set; }
            /// <summary>
            /// 战役id
            /// </summary>
            public int mission_id { get; set; }
            /// <summary>
            /// 战斗等级
            /// </summary>
            public int battle_rank { get; set; }
            /// <summary>
            /// 获得的枪的id，原始数据里是list但是数据库里只能存单个的，但一般只有一个
            /// </summary>
            public int gun_id { get; set; }
            /// <summary>
            /// 圣捞、指挥官皮肤特效可能会多一个掉落
            /// </summary>
            public int gun_id_extra { get; set; }
            /// <summary>
            /// 获得的装备的id，原始数据里是list但是数据库里只能存单个的，但一般只有一个
            /// </summary>
            public int equip_id { get; set; }
            /// <summary>
            /// 圣捞、指挥官皮肤特效可能会多一个掉落
            /// </summary>
            public int equip_id_extra { get; set; }
        }

        /// <summary>
        /// 整场战役结束的信息
        /// </summary>
        [Table("MissionFinish")]
        public class MissionFinish
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            /// <summary>
            /// 用户uid
            /// </summary>
            public string uid { get; set; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp { get; set; }
            /// <summary>
            /// 战役id
            /// </summary>
            public int mission_id { get; set; }
            /// <summary>
            /// 战役等级
            /// </summary>
            public int mission_rank { get; set; }
            /// <summary>
            /// 获得的枪的id，原始数据里是list但是数据库里只能存单个的
            /// </summary>
            public int reward_gun { get; set; }
            /// <summary>
            /// 圣捞、指挥官皮肤特效可能会多一个掉落
            /// </summary>
            public int reward_gun_extra { get; set; }
            /// <summary>
            /// 获得的装备id
            /// </summary>
            public int reward_equip { get; set; }
            /// <summary>
            /// 获得的装备id
            /// </summary>
            public int reward_equip_extra { get; set; }
        }
    }
}
