// 信息列表
var missionInfo = null;
var campaignInfo = null;
// 名称table
var table = null;

function setupMission() {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", host + "/static/missionInfo.json");
    xhr.onload = onMissionInfoLoad;
    xhr.send();
}

function onCampaignInfoLoad() {
    campaignInfo = JSON.parse(this.responseText);
    var html = ``;
    for (var key in campaignInfo) {
        html += `<option value="${key}">${campaignInfo[key]}<\/option>\n`;
    }
    document.getElementById("SelectCampaign").innerHTML = html;
    selectCampaign();
}

function onMissionInfoLoad() {
    missionInfo = JSON.parse(this.responseText);

    var tableXhr = new XMLHttpRequest();
    tableXhr.open("GET", host + "/static/table.json");
    tableXhr.onload = onTableLoad;
    tableXhr.send();
}

function onTableLoad() {
    table = JSON.parse(this.responseText);
    var xhr = new XMLHttpRequest();
    xhr.open("GET", host + "/static/campaignInfo.json");
    xhr.onload = onCampaignInfoLoad;
    xhr.send();
}

function confirmMission() {
    var select = document.getElementById("SelectSub").selectedOptions[0];
    var html = `<h2>${select.text}</h2>`;
    document.getElementById("missionInfoCard").innerHTML = html;

    document.getElementById("content").style.display = "none";
    document.getElementById("details").style.display = "";
}

function retrurnContent() {
    document.getElementById("content").style.display = "";
    document.getElementById("details").style.display = "none";
}

function selectCampaign() {
    var campaign = document.getElementById("SelectCampaign").selectedOptions[0].value;
    var html = ``;
    for (var key in missionInfo) {
        var mission = missionInfo[key];
        if (mission.campaign == campaign && mission.name.startsWith("mission") && mission.if_emergency != 2) {
            var name = "";
            if (mission.campaign >= 0) {
                name += String(mission.campaign) + "-" + String(mission.sub);
                name += (mission.if_emergency == 1) ? "E" : ((mission.if_emergency == 3) ? "N" : " ");
            }
            name += table[mission.name].split("//")[0];
            html += `<option value="${mission.sub}">${name}<\/option>\n`;
        }
    }
    document.getElementById("SelectSub").innerHTML = html;
}