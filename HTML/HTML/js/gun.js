var types = ["", "HG", "SMG", "RF", "AR", "MG", "SG"];
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
    xhr.open("GET", "http://" + this.location.host + "/static/gun_info.json");
    xhr.onload = onGunInfoLoad;
    xhr.send();
}

var gunInfo = null;

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
            var html = `<button type="button" class="btn gun-${gun.rank}-star" id="${gun.en_name}" onclick="selectGun(${gun.id});">${gun.en_name}</button>`;
            document.getElementById(type + "-buttons").innerHTML += html;
        });
    }
}

function selectGun(id) {
    var gun = gunInfo[id];
    var html = `<h2>${gun.en_name}</h2>
    <p>建造时间：${seconds2Str(gun.develop_duration)}<br>
    星级：${gun.rank}</p>`;
    document.getElementById("gunInfoCard").innerHTML = html;

    document.getElementById("content").style.display = "none";
    document.getElementById("details").style.display = "";
}

function retrurnContent() {
    document.getElementById("content").style.display = "";
    document.getElementById("details").style.display = "none";
}

function sortDetailTable(key){
    console.log(key);
}