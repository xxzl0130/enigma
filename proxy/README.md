# Proxy
代理程序，解析游戏数据并通过event发出。  

## 数据处理规则
[DataProcess.json](./Resource/DataProcess.json)  
以json的形式描述的数据处理规则，将游戏上行(request)和下行(response)中的字段提取出来。  
### 文件规则
每一对顶层的key/value为一条规则，key为url后缀，value为规则。  
value中分为request和response，对应http请求的两个部分。其中由若干对key/value组成，每对key/value的key为值的名称，value为一个string list，按顺序描述了所要提取的数据在整个json中的层级路径。  
目前仅支持每一层都是object，array要写自动处理有点复杂，不如丢到最终处理的地方进行进一步处理。  
### 规则列表

#### Gun/developGun
造枪的请求，没有直接区分普通和重型，而以`build_slot`区分，奇数为普通，偶数为重型；
`input_level`为重建档位，普通建造为0。  
建造请求后会直接返回结果`gun_id`，虽然没有直接显示在游戏里。。。  
请求示例：  
```json
{"mp":30,"ammo":30,"mre":30,"part":30,"build_slot":1,"input_level":0}
```
响应示例：  
```json
{"gun_id":"2"}
```

#### Gun/developMultiGun
批量建造的请求，规则和`Gun/developiGun`一致，返回的结果在`gun_ids`里，为一个数组。 
请求示例：
```json
{"mp":30,"ammo":30,"mre":30,"part":30,"input_level":0,"build_quick":0,"build_multi":2,"build_heavy":0}
```
响应示例：
```json
{"gun_ids":[{"id":"5","slot":1},{"id":"90","slot":3}]}
```

#### Mission/battleFinish
战斗结束的请求，`use_fairy_skill`只是一个bool，没有具体妖精信息，需要额外记录。  
`died_this_section`中是从头记录了所有死亡的敌人，需要读取数组最后一个。  
`battle_get_gun`为获得的枪的列表，可能为空。  
`battle_rank`的5为S胜，4为A胜。  
请求示例：
```json
{"spot_id":826,"if_enemy_die":true,"current_time":1611372127,"boss_hp":0,"mvp":72634017,"last_battle_info":"","use_fairy_skill":true,"use_skill_squads":[],"use_skill_ally_spots":[],"guns":[{"id":72634017,"life":565},{"id":127189285,"life":360},{"id":131085559,"life":350}],"user_rec":"{\"seed\":1169,\"record\":[]}","1000":{"10":25498,"11":25498,"12":25498,"13":25498,"15":11633,"16":0,"17":150,"33":900017,"40":77,"18":0,"19":0,"20":0,"21":0,"22":0,"23":0,"24":19416,"25":0,"26":19416,"27":5,"34":236,"35":236,"41":252,"42":0,"43":0,"44":0},"1001":{},"1002":{"72634017":{"47":1},"127189285":{"47":0},"131085559":{"47":0}},"1003":{"326549":{"9":1,"68":1}},"1005":{},"1007":{},"1008":{},"1009":{},"battle_damage":{}}
```
响应示例：
```json
{
    "died_this_section": {
        "enemy": [
            "366",
            "377"
        ],
        "ally": []
    },
    "battle_get_gun": [
        {
            "gun_with_user_id": "318471851",
            "gun_id": "40"
        }
    ],
    "free_exp": 5443,
    "user_exp": "126",
    "gun_exp": [
        {
            "gun_with_user_id": "72634017",
            "exp": "0"
        },
        {
            "gun_with_user_id": "127189285",
            "exp": "0"
        },
        {
            "gun_with_user_id": "131085559",
            "exp": "0"
        }
    ],
    "fairy_exp": 1088,
    "gun_life": [],
    "squad_exp": [],
    "battle_rank": "5"
}
```

#### Mission/endTurn
回合结束的请求，包括了整场战役结束。  
`battle_rank`的5为S胜，4为A胜。  
请求示例：
```json
请求无数据
```
响应示例：
```json
{
    "died_this_section": {
        "enemy": [
            "366",
            "377"
        ],
        "ally": []
    },
    "change_belong1": {
        "826": 1,
        "807": 1
    },
    "building_change_belong1": [],
    "mission_win_result": {
        "rank": "4",
        "medal4": 0,
        "open": {
            "55": 1,
            "51": 1
        },
        "user_exp": "126",
        "reward_gun": [
            {
                "gun_with_user_id": "318471882",
                "gun_id": "5"
            }
        ],
        "mission_info": {
            "turn": "1",
            "enemydie_num": "2",
            "enemydie_num_killbyfriend": "0",
            "gundie_num": "0",
            "mysquad_die_num": "0",
            "sangvisdie_num": "0"
        }
    }
}
```

#### Equip/produceDevelop
定向建造装备的请求，不区分单次和多次，在`build_slot`以数组表示建造位置。  
同样的，装备建造的`build_slot`中奇数为普通，偶数为重型
`formula_id`为定向装备的id，没有直接表示公式。  
回复以数组表示得到的装备。  
请求示例：
```json
{"build_slots":[3,5],"recommended_formula_id":20,"if_quick":0}
```
响应示例：
```json
[{"slot":3,"equip_id":"29"},{"slot":5,"equip_id":"30"}]
```

#### Equip/develop
装备建造的请求，和造枪请求类似，没有直接区分普通和重型，而以`build_slot`区分，奇数为普通，偶数为重型；
`input_level`为重建档位，普通建造为0。  
建造请求后会直接返回结果，`type`代表装备，`type`为1代表妖精。  
装备中包含`equip_id`，妖精中包含`fairy_id`和`passive_skill`。  
请求示例（普通建造）：
```json
{"mp":10,"ammo":10,"mre":10,"part":10,"build_slot":7,"input_level":0}
```
响应示例（普通建造）：
```json
{
    "type": 0,
    "equip_id": 13
}
```
请求示例（重型建造）：
```json
{"mp":500,"ammo":500,"mre":500,"part":500,"build_slot":4,"input_level":2}
```
响应示例（重型建造）：
```json
{
    "type": 1,
    "fairy_id": 24,
    "passive_skill": 910102,
    "quality_lv": 1
}
```

#### Equip/developMulti
批量装备建造的请求，同上类似。  
请求示例：
```json
{"mp":500,"ammo":500,"mre":500,"part":500,"input_level":1,"build_quick":0,"build_multi":2,"build_heavy":1}
```
响应示例：
```json
{
    "equip_ids": [
        {
            "info": {
                "type": 0,
                "equip_id": 11
            },
            "slot": 6
        },
        {
            "info": {
                "type": 1,
                "fairy_id": 8,
                "passive_skill": 910114,
                "quality_lv": 1
            },
            "slot": 8
        }
    ]
}
```

#### Operation/finishOperation
后勤完成请求，抓取数据不足，对应关系不明
请求示例：
```json
{"operation_id":53}
```
响应示例：
```json
{"item_id":"3-1","big_success":1}
```

## 对编队中的妖精的处理
### Index/index
在登入的初始化数据中，存在`fairy_with_user_info`字段，列举了玩家所有的妖精。  
每个妖精信息中包含妖精类型`fairy_id`和所在队伍`team_id`，`team_id`为0时不在队伍中，>0时则在对应队伍中。  
```json
{
    "fairy_with_user_info": {
        "796229": {
            "id": "796229",
            "user_id": "1563276",
            "fairy_id": "22",
            "team_id": "4",
            "fairy_lv": "100",
            "fairy_exp": "9999000",
            "quality_lv": "5",
            "quality_exp": "550",
            "skill_lv": "10",
            "passive_skill": "910102",
            "is_locked": "1",
            "equip_id": "0",
            "adjust_count": "52",
            "last_adjust": "0",
            "passive_skill_collect": "910116,910114,910103,910108,910107,910109,910106,910104,910117,910110,910115,910112,910101,910105,910113,910111,910102",
            "skin": "0"
        }
}
```

### Fairy/teamFairy
在切换队伍妖精的时候，会向服务器发送请求：
```json
{"team_id":4,"fairy_with_user_id":796229}
```
这里要用`fairy_with_user_id`从index中的数据获取具体的妖精信息。  
特别注意，服务器的回复只有一个数字`1`，不是一般的json串。  

### Mission/teamMove
队伍移动时会上报队伍号和点号：
```json
{"person_type":1,"person_id":5,"from_spot_id":820,"to_spot_id":826,"move_type":1}
```
记录每个队伍的`spot_id`，在战斗结束的请求中有该id，反向对应找到队伍。  

### Mission/startMission
任务开始的时候会上报队伍和点号
```json
{"mission_id":40,"spots":[{"spot_id":582,"team_id":5},{"spot_id":587,"team_id":1}],"squad_spots":[],"sangvis_spots":[],"ally_id":1611371712}
```