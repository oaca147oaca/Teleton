$(document).ready(function () {
    //peticion api para obtener informacion, esto lo voy a hacer con bd
    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: urlBase + "/Graficas/DataPastel2",
        error: function () {
            alert("ocurrio un error");
        },
        success: function (data) {
            GraficaPastel2(data);
        }
    })
});


function GraficaPastel2(data) {
    var total = 0;
    for (var i = 0; i < data.length; i++) {
        total += data[i].y;
    }


    Highcharts.chart('container2', {
        chart: {
            type: 'pie'
        },
        title: {
            text: 'Total alcancías: ' + total
        },
        subtitle: {
            text: 'Estados de las alcancías'
        },
        

        accessibility: {
            announceNewData: {
                enabled: true
            },
            point: {
                valueSuffix: '%'
            }
        },

        plotOptions: {
            series: {
                dataLabels: {
                    enabled: true,
                    format: '{point.name}: {point.y:.0f}'
                }
            }
        },

        tooltip: {
            headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
            pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.0f}</b><br/>'
        },

        series: [
            {
                name: "Alcancías",
                colorByPoint: true,
                data: data
            }
        ],
        drilldown: {
            series: [
                {
                    name: "Disponibles",
                    id: "Disponibles",
                    data: data
                },
                {
                    name: "Entregadas",
                    id: "Entregaas",
                    data: data
                }, {
                    name: "Recibidas",
                    id: "Recibidas",
                    data: data
                },
                {
                    name: "Contabilizadas",
                    id: "Contabilizadas",
                    data: data
                },
                {
                    name: "Desvinculadas",
                    id: "Desvinculadas",
                    data: data
                },
            ]
        }
    });
}