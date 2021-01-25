# enigma
GIRLS' FRONTLINE Statistical Tool

## 架构
以.NET Standard 2.0编写各模块的dll库，再以.Net Core和.Net Framework分别编写服务器和客户端。  
项目预计以以下几个模块组成：  
### proxy
http代理模块，负责完成对游戏数据的代理分析，提取所需数据，以event回调传出数据。  
代理中需要处理多用户登录以及维护每个用户的妖精配置信息。  
目前该模块已完成70%。  
### database
数据库模块，将游戏操作数据进行存储、分析，同时支持导出、导入、合并、删除数据，支持设置特殊活动时间段。  
预计以SQLite实现，分为建造数据库、打捞数据库，建造库以建造类型分表，打捞库以战役分表，并设置统计总表，定时更新。  
### web（main）
网页模块，通过网页展示统计数据，设置特殊时间段等。  