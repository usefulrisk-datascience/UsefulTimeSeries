$(document).ready(function(){
    let start=Date.now()
    console.log("c'est parti")
    $.get("/jj",function (data){
        const d1 = new Date();
        let h = addZero(d1.getHours(), 2);
        let m = addZero(d1.getMinutes(), 2);
        let s = addZero(d1.getSeconds(), 2);
        let ms = addZero(d1.getMilliseconds(), 3);
        let time = h + ":" + m + ":" + s + ":" + ms;
        console.log(time)
        console.log(data)
        //OneChart est pour un TsContainer et contient donc plusieurs charts
        MakeOneChart('Plot3',data)
        $('#SeriesName').text(data['Original'].Calculs.Name)
        $('#Tables').append('<div id="TablesOriginal" class="col"></div>')
        MakeSummaryTable('Original',data['Original'].Calculs)
        //MakeFullTable('Original',data['Original'].Data)
        /*
        $('#Tables').append('<div id="TablesCleaned" class="col"></div>')
        MakeSummaryTable('Cleaned',data['Cleaned'].Calculs)
        MakeFullTable('Cleaned',data['Cleaned'].Data)
        */
        for (const[key,value]of Object.entries(data)){
            if (key=='Cleaned' || key=='Regularized'||key=='Rejected'){
                $('#Tables').append('<div id="Tables'+key+'" class="col"></div>')
                MakeSummaryTable(key,value.Calculs)
            }
        }
        const d2 = new Date();
         h = addZero(d2.getHours(), 2);
         m = addZero(d2.getMinutes(), 2);
         s = addZero(d2.getSeconds(), 2);
         ms = addZero(d2.getMilliseconds(), 3);
         time = h + ":" + m + ":" + s + ":" + ms;
        console.log(time)
    });
})
function addZero(x, n) {
    while (x.toString().length < n) {
        x = "0" + x;
    }
    return x;
}
function MakeOneChart(idgraph,data){
    var traceArr=[];
    datagraph=[];
    for (var ts in data){
        if (ts!="Rejected"){
            let trace={
                x:data[ts].Data.FullChron,
                y:data[ts].Data.FullMeas,
                mode:'lines',
                name:ts,
                line:{
                    width:1
                }
            }
            traceArr.push(trace);
        }else {
            let traceRej={
                x:data["Rejected"].Data.FullChron,
                y:data["Rejected"].Data.FullMeas,
                mode:'markers',
                marker:{size:3},
                name:'Rejected',
            }
            traceArr.push(traceRej);
        }

    }

    datagraph=traceArr
    let layoutgraph = {
        font: {
            size:9,
            color:"darkseagreen",
        },
        borderStyle:{
            color: "darkseagreen",
        },
        borderColor:"darkseagreen",
        title:data.Name,
        xaxis: {
            showgrid: true,
            zeroline: false,
            showline: true,
            mirror: 'ticks',
            gridcolor: 'gray',
            gridwidth: 1,
            zerolinecolor: 'gray',
            zerolinewidth: 1,
            linecolor: 'gray',
            linewidth: 1,
        },
        yaxis: {
            showgrid: true,
            zeroline: true,
            showline: true,
            mirror: 'ticks',
            gridcolor: 'gray',
            gridwidth: 1,
            zerolinecolor: 'darkseagreen',
            zerolinewidth: 2,
            linecolor: 'darkseagreen',
            linewidth: 1
        },
        showlegend: true,
        legend: {"orientation": "h"}
    };
    Plotly.newPlot(idgraph, datagraph,layoutgraph);
}
function MakeSummaryTable(idsummtable,summ){
    $('#Tables'+idsummtable).append('<div id="summ'+idsummtable+'" class="col"></div>')
    idsummtable='#summ'+idsummtable;
    //$(idsummtable).empty();
    $(idsummtable).append('<table></table>');
    $(idsummtable).append('<tbody></tbody>');
    if (summ!=null){
        $(idsummtable+'> tbody').append('<tr><td>'+summ.TsType+'</td><td></td></tr>');
        $(idsummtable+'> tbody').append('<tr><td>Length: </td><td>' + summ.Len + '</td></tr>');
        $(idsummtable+'> tbody').append('<tr><td></td><td class="subtitle"><strong>Time</strong></td><td><strong>Value at</strong></td>');
        $(idsummtable+'> tbody').append('<tr><td>Frst Obs: </td><td>' + summ.FirstObs.Chron + '</td><td>'+ toFixedwNull(summ.FirstObs.Meas,2)+'</td>');
        $(idsummtable+'> tbody').append('<tr><td>Last Obs: </td><td>' + summ.LastObs.Chron + '</td><td>'+ toFixedwNull(summ.LastObs.Meas,2)+'</td>');
        $(idsummtable+'> tbody').append('<tr><td>Min Val: </td><td>' + toFixedwNull(summ.Min.Meas,2) + '</td><td>'+ summ.Min.Chron + '</td>');
        $(idsummtable+'> tbody').append('<tr><td>Max Val: </td><td>' + toFixedwNull(summ.Max.Meas,2) + '</td><td>'+ summ.Max.Chron + '</td>');
        $(idsummtable+'> tbody').append('<tr><td>Avg Val: </td><td>' + toFixedwNull(summ.MsMean,2) + '</td>');
        $(idsummtable+'> tbody').append('<tr><td>St.Dev.: </td><td>' + toFixedwNull(summ.Msstd,2) + '</td>');

    }
}
function MakeFullTable(idfulltable,series) {
    $('#Tables'+idfulltable).append('<div id="full'+idfulltable+'" class="col"></div>')
    idfulltable='#full'+idfulltable;
    //$(idfulltable).append('<caption style="caption-side: top">'+series.Ts[seriesnbr].Name+ ' Data</caption>');
    $(idfulltable).append('<tbody></tbody>');
    $(idfulltable).append('<thead>'+series.Name+'</thead>');
    $(idfulltable +'> thead').append('<th>Idx</th><th>Chron</th><th>Meas</th><th>Dchron</th><th>Dmeas</th>');
    if (series!=null){
        for (k = 0; k < series.FullChron.length; k++) {
        //for (k = 0; k < 10000; k++) {
            $(idfulltable+'> tbody').append('<tr><td>' +k+'</td><td>'+ series.FullChron[k] + '</td><td>' + toFixedwNull(series.FullMeas[k],4) + '</td><td>' + toReadableDchron(series.FullDchron[k]) + '</td><td>' + toFixedwNull(series.FullDmeas[k],4) +'</td></tr>');
        }
    }
}
function toFixedwNull(obj,dec){
    if (obj!=null){
        return obj.toFixed(dec)
    }else {
        return null
    }
}
function toReadableDchron(obj){
    if (obj!=null){
        disp=obj/1000000000
        if (disp <60.0){
                return disp.toFixed(2)+"s"}
        else if (disp<3600.0){
            mm=Math.floor(disp/60);
            ss=(disp%60).toFixed(2)
            return mm+ "m"+ss+"s"
        }else if (disp <86400) {
            hh = Math.floor(disp / 3600)
            mm = Math.floor((disp%3600) / 60)
            ss = Math.floor(mm % 60)
            return hh+"h"+mm+ "m"+ss+"s"
        }else {
            //console.log(disp)
            dd= Math.floor(disp/86400)
            //console.log(dd)
            hh = Math.floor((disp % 86400)/3600)
            //console.log(hh)
            mm = Math.floor((disp % 86400)%3600/60)
            //console.log(mm)
            ss = ((disp % 86400)%3600%60)
            //console.log(ss)
            return dd+"d"+hh+"h"+mm+ "m"+ss+"s"
        }

    }else {
        return null
    }

}