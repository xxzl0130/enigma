var GunLink = "/gun.html";
var EquipLink = "/equip.html";
var MissionLink = "/mission.html";
var navList = [
    {
        title: "人形建造记录",
        link: "/gun.html",
        sub: [
            {
                title: "HG",
                link: GunLink + "#HG"
            },
            {
                title: "SMG",
                link: GunLink + "#SMG"
            },
            {
                title: "RF",
                link: GunLink + "#RF"
            },
            {
                title: "AR",
                link: GunLink + "#AR"
            },
            {
                title: "MG",
                link: GunLink + "#MG"
            },
            {
                title: "SG",
                link: GunLink + "#SG"
            }
        ]
    },
    {
        title: "装备建造记录",
        link: "/equip.html",
        sub: [
            {
                title: "配件",
                link: EquipLink + "#配件"
            },
            {
                title: "弹匣",
                link: EquipLink + "#弹匣"
            },
            {
                title: "人形",
                link: EquipLink + "#人形"
            },
            {
                title: "战斗妖精",
                link: EquipLink + "#战斗妖精"
            },
            {
                title: "策略妖精",
                link: EquipLink + "#策略妖精"
            }
        ]
    },
    {
        title: "装备定向记录",
        link: "/equipProduce.html",
        sub: [
            {
                title: "配件",
                link: EquipLink + "#配件"
            },
            {
                title: "弹匣",
                link: EquipLink + "#弹匣"
            },
            {
                title: "人形",
                link: EquipLink + "#人形"
            }
        ]
    },
    {
        title: "战役打捞记录",
        link: "/mission.html",
        sub: null // TODO: 添加战役列表
    }
];

function setup(name) {
    // 添加header
    var html = `
            <span class="logo col-sm-3 col-md">${name}</span>
            <a class="button col-sm col-md" href="https://github.com/xxzl0130/enigma" target="_blank">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
                    stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                    style="height: 20px; vertical-align: text-top;">
                    <path
                        d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22">
                    </path>
                </svg>
                <span>&nbsp;Github</span></a>
            <a class="button col-sm col-md" href="/about.html">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
                    stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                    style="height: 20px; vertical-align: text-top;">
                    <path d="M20 14.66V20a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h5.34"></path>
                    <polygon points="18 2 22 6 12 16 8 16 8 12 18 2"></polygon>
                </svg>
                <span>&nbsp;About</span></a>
            <label for="doc-drawer-checkbox" class="button drawer-toggle col-sm"></label>
    `;
    document.getElementById('header').innerHTML = html;

    // 添加导航栏
    var navHtml = "";
    navList.forEach(it => {
        navHtml += `<a href="${it.link}">${it.title}</a>\n`;
        if (it.sub != null) {
            it.sub.forEach(sub => {
                navHtml += `<a href="${sub.link}" class="sublink-1">${sub.title}</a>`;
            });
        }
    });
    html = `
                <label for="doc-drawer-checkbox" class="button drawer-close"></label>
                ${navHtml}
    `;
    document.getElementById('nav-drawer').innerHTML = html;

    // 添加footer
    var footer = document.createElement("footer");
    footer.setAttribute("style", "text-align:center");
    footer.innerHTML = `<p>©2021 xuanxuan.tech | <a href="https://github.com/xxzl0130/enigma">GitHub</a> | <a href="/admin.html">Admin</a>
        </p>`;
    document.body.append(footer);
}