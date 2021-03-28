function seconds2Str(sec){
    var s = sec % 60;
    var m = Math.floor(sec / 60) % 60;
    var h = Math.floor(sec / 60 / 60);
    var str = `${num2str(h,2)}:${num2str(m,2)}:${num2str(s,2)}`;
    return str;
}

function num2str(num, bit){
    var str = num.toString();
    while(str.length < bit){
        str = "0" + str;
    }
    return str;
}