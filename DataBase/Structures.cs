using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace enigma.DataBase
{
    // SQLite里的列必须是属性
    /// <summary>
    /// 数据库记录的基础定义
    /// </summary>
    [Serializable]
    public class RecordBase
    {
        [PrimaryKey, AutoIncrement] public int id { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public int timestamp { get; set; }
    }

    /// <summary>
    /// 普通建造枪
    /// </summary>
    [Table("GunDevelop")] // 表名调整为按类型统一开头，下同
    [Serializable]
    public class GunDevelop : RecordBase
    {
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
    }

    /// <summary>
    /// 普通建造统计表
    /// </summary>
    [Table("GunDevelopTotal")]
    [Serializable]
    public class GunDevelopTotal : GunDevelop
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出货总数
        /// </summary>
        public int valid_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double valid_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 重型建造枪
    /// </summary>
    [Table("GunDevelopHeavy")]
    [Serializable]
    public class GunDevelopHeavy : RecordBase
    {
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
    /// 重型人型建造统计表
    /// </summary>
    [Table("GunDevelopHeavyTotal")]
    [Serializable]
    public class GunDevelopHeavyTotal : GunDevelopHeavy
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出货总数
        /// </summary>
        public int valid_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double valid_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 推荐公式建造装备
    /// </summary>
    [Table("EquipProduce")]
    [Serializable]
    public class EquipProduce : RecordBase
    {
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
    /// 推荐公式建造装备统计表
    /// </summary>
    [Table("EquipProduceTotal")]
    [Serializable]
    public class EquipProduceTotal : EquipProduce
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出货总数
        /// </summary>
        public int valid_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double valid_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 普通建造装备
    /// </summary>
    [Table("EquipDevelop")]
    [Serializable]
    public class EquipDevelop : RecordBase
    {
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
    /// 普通建造装备统计表
    /// </summary>
    [Table("EquipDevelopTotal")]
    [Serializable]
    public class EquipDevelopTotal : EquipDevelop
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出货总数
        /// </summary>
        public int valid_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double valid_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 装备重型建造
    /// </summary>
    [Table("EquipDevelopHeavy")]
    [Serializable]
    public class EquipDevelopHeavy : RecordBase
    {
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
    /// 重型建造装备统计表
    /// </summary>
    [Table("EquipDevelopHeavyTotal")]
    [Serializable]
    public class EquipDevelopHeavyTotal : EquipDevelopHeavy
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出货总数
        /// </summary>
        public int valid_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double valid_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 一场战斗结束的数据
    /// </summary>
    [Table("MissionBattle")]
    [Serializable]
    public class MissionBattle : RecordBase
    {
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

        /// <summary>
        /// 是否使用妖精技能
        /// </summary>
        public bool use_fairy_skill { get; set; }

        /// <summary>
        /// 妖精技能等级
        /// </summary>
        public int fairy_skill_lv { get; set; }

        /// <summary>
        /// 使用的妖精的id
        /// </summary>
        public int use_fairy_id { get; set; }
    }

    /// <summary>
    /// 战斗结束的数据统计
    /// </summary>
    [Table("MissionBattleTotal")]
    [Serializable]
    public class MissionBattleTotal : RecordBase
    {
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
        /// 获得的枪的id
        /// </summary>
        public int gun_id { get; set; }

        /// <summary>
        /// 获得的装备的id
        /// </summary>
        public int equip_id { get; set; }

        /// <summary>
        /// 是否使用Lv.10搜救妖精技能
        /// </summary>
        public bool use_search_fairy { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出枪总数
        /// </summary>
        public int gun_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double gun_rate { get; set; }

        /// <summary>
        /// 有效出装备总数
        /// </summary>
        public int equip_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double equip_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }

    /// <summary>
    /// 整场战役结束的信息
    /// </summary>
    [Table("MissionFinish")]
    [Serializable]
    public class MissionFinish : RecordBase
    {
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
        public int gun_id { get; set; }

        /// <summary>
        /// 圣捞、指挥官皮肤特效可能会多一个掉落
        /// </summary>
        public int gun_id_extra { get; set; }

        /// <summary>
        /// 获得的装备id
        /// </summary>
        public int equip_id { get; set; }

        /// <summary>
        /// 获得的装备id
        /// </summary>
        public int equip_id_extra { get; set; }

        /// <summary>
        /// 备用，以免数据没有及时更新时查不到mission_id
        /// </summary>
        public int spot_id { get; set; }
    }

    /// <summary>
    /// 整场战役结束的信息统计
    /// </summary>
    [Table("MissionFinishTotal")]
    [Serializable]
    public class MissionFinishTotal : RecordBase
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int mission_id { get; set; }

        /// <summary>
        /// 战役等级
        /// </summary>
        public int mission_rank { get; set; }

        /// <summary>
        /// 获得的枪的id
        /// </summary>
        public int gun_id { get; set; }

        /// <summary>
        /// 获得的装备id
        /// </summary>
        public int equip_id { get; set; }


        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 有效出枪总数
        /// </summary>
        public int gun_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double gun_rate { get; set; }

        /// <summary>
        /// 有效出装备总数
        /// </summary>
        public int equip_total { get; set; }

        /// <summary>
        /// 出货率
        /// </summary>
        public double equip_rate { get; set; }

        /// <summary>
        /// 时间范围id
        /// </summary>
        public int time_id { get; set; }
    }
}