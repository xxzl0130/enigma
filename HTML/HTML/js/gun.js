// 枪支类型
var types = ["", "HG", "SMG", "RF", "AR", "MG", "SG"];
// 枪信息列表
var gunInfo = null;
// 当前选择的枪
var curGun = null;
// 显示类型，1-普通，2-重型，3-全部
var dispType = 3;
// 名称table
var table = null;

function setupGun() {
    types.forEach(t => {
        if (t.length == 0)
            return;
        var html = `<div id="${t}" class="card fluid">
                    <h2 class="section double-padded">${t}</h2>
                    <div class="section" id="${t}-buttons"></div>
                </div>`;
        document.getElementById('content').innerHTML += html;
    });
    var xhr = new XMLHttpRequest();
    xhr.open("GET", host + "/static/gunInfo.json");
    xhr.onload = onGunInfoLoad;
    xhr.send();
}

function onGunInfoLoad() {
    gunInfo = JSON.parse(this.responseText);
    var ranks = [];
    for (var i = 0; i <= 5; ++i)
        ranks.push([]);
    for (var i = 0; i < 1000; ++i) {
        if (gunInfo[i] == null)
            continue;
        ranks[gunInfo[i].rank].push(gunInfo[i]);
    }
    for (var i = 5; i > 1; --i) {
        ranks[i].forEach(gun => {
            var type = types[gun.type];
            var html = `<button type="button" class="btn gun-${gun.rank}-star" id="${gun.name}" onclick="selectGun(${gun.id});">${gun.en_name}</button>`;
            document.getElementById(type + "-buttons").innerHTML += html;
        });
    }
    var tableXhr = new XMLHttpRequest();
    tableXhr.open("GET", host + "/static/table.json");
    tableXhr.onload = onTableLoad;
    tableXhr.send();
}

function onTableLoad() {
    table = JSON.parse(this.responseText);
    for (var i = 0; i < 1000; ++i){
        var id = `gun-${10000000 + i}`;
        if (table[id] != null) {
            document.getElementById(id).innerHTML = table[id] + ` | ` + gunInfo[i].en_name;
        }
    }
}

function selectGun(id) {
    curGun = gunInfo[id];
    var html = `<h2>${document.getElementById(curGun.name).innerHTML}</h2>
    <p>建造时间：${seconds2Str(curGun.develop_duration)}<br>
    星级：${curGun.rank}</p>`;
    document.getElementById("gunInfoCard").innerHTML = html;

    document.getElementById("content").style.display = "none";
    document.getElementById("details").style.display = "";
}

function retrurnContent() {
    document.getElementById("content").style.display = "";
    document.getElementById("details").style.display = "none";
}

function selectTime(){

}

function selectDispType(key){

}