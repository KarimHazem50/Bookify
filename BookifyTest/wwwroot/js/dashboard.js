var chart;

$(document).ready(function () {
    HandleDateRangePicker();

    DrawRentalsChart();

    DrawSubscribersChart();

    $("#DateSelected").on("DOMSubtreeModified", function (e) {
        if (e.target.textContent != '') {
            var dataRange = e.target.textContent.split(" - ");
            chart.destroy();
            DrawRentalsChart(dataRange[0], dataRange[1]);
        }
    })
})



function DrawRentalsChart(startDate = null, endDate = null) {

    $.get({
        url: `/Dashboard/GetRentalsPerDay?startDate=${startDate}&endDate=${endDate}`,
        success: function (data) {
            var options = {
                chart: {
                    type: 'area',
                    height: '400px',
                    toolbar: {
                        show: false
                    }
                },
                dataLabels: {
                    enabled: false
                },
                stroke: {
                    curve: 'smooth',
                    show: true,
                    width: 2,
                    colors: ["#042b99"]
                },
                tooltip: {
                    theme: "dark",
                    style: {
                        fontSize: '12px'
                    }
                },
                colors: ["#0673e8"],
                series: [{
                    name: 'Books',
                    data: data.map(item => item.value)
                }],
                yaxis: {
                    min: 0,
                    tickAmount: Math.max(...data.map(item => item.value)),
                    labels: {
                        style: {
                            colors: "#0673e8",
                            fontSize: '12px'
                        }
                    }
                },
                xaxis: {
                    categories: data.map(item => item.label),
                    labels: {
                        style: {
                            colors: "#0673e8",
                            fontSize: '12px'
                        }
                    },
                },
                markers: {
                    strokeColor: "#000000",
                    strokeWidth: 2
                },
            }
            chart = new ApexCharts(document.querySelector("#RentalsPerDay"), options);
            chart.render();
        }
    });

}


function HandleDateRangePicker() {

    var start = moment().subtract(29, 'days');
    var end = moment();

    function cb(start, end) {
        $('#DateRangeBicker span').html(start.format('D MMM YYYY') + ' - ' + end.format('D MMM YYYY'));
    }

    $('#DateRangeBicker').daterangepicker({
        startDate: start,
        endDate: end,
        maxDate: new Date(),
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        }
    }, cb);

    cb(start, end);

    document.querySelector(".ranges").style.color = 'initial'
};


function DrawSubscribersChart() {
    $.get({
        url: "/Dashboard/GetSubscribersByCity",
        success: function (data) {
            var options = {
                chart: {
                    type: 'donut',
                },
                series: data.map(item => parseInt(item.value)),
                labels: data.map(item => item.label),
            }
            var SubscribersChart = new ApexCharts(document.querySelector("#SubscribersPerCity"), options);
            SubscribersChart.render();
        }
    })
}