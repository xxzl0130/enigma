# enigma
GIRLS' FRONTLINE Statistical Tool

## 目标
[IOP出货统计](http://gfdb.baka.pw/statistician.html)提供了游戏内建造项目的统计数据，但是存在数据更新缓慢、没有区分生煎时间等问题，同时也没有各地图打捞的数据。  
本项目的目标在于，通过客户端程序采集每位用户的游戏数据，汇总至服务器实现全服的统计数据，实现对个人以及全服务器精确的、实时更新的、可区分活动时间段的建造、打捞统计数据。  
本项目以C#编写，利用.NET Standard/Core实现核心程序的跨平台通用，再分别构建客户端和服务端。不过本人对C#属于新手（以前都是写C++），因此这个项目也算是一个练手项目。  

## 架构
以.NET Standard 2.0编写各模块的dll库，再以.Net Core和.Net Framework分别编写服务器和客户端。  
项目预计以以下几个模块组成：  
### proxy
[http代理模块](./proxy)，负责完成对游戏数据的代理分析，提取所需数据，以event回调传出数据。  
代理中需要处理多用户登录以及维护每个用户的妖精配置信息。    
### database
数据库模块，将游戏操作数据进行存储、分析，同时支持导出、导入、合并、删除数据，支持设置特殊活动时间段。  
以SQLite实现，以建造类型、战役分表，并设置统计总表，定时更新。  
### web（main）
网页模块，通过网页展示统计数据，设置特殊时间段等。  

## 记录
### gun_info
type:  
1. HG
2. SMG
3. RF
4. AR
5. MG
6. SG

### table
`/sdcard/Android/data/com.sunborn.girlsfrontline.cn/files/Android/New/asset_texttable.ab`  
用AssetStudio拆