var chart;

$(document).ready(function () {
    $('#DateRange').on('DOMSubtreeModified', function () {
        var selectedRange = $(this).html();

        if (selectedRange !== '') {
            var dateRange = selectedRange.split(' - ');
            chart.destroy();
            drawRentalsChart(dateRange[0], dateRange[1]);
        }
    });
});

(function () {
    // Check if jQuery included
    if (typeof jQuery == 'undefined') {
        return;
    }

    // Check if daterangepicker included
    if (typeof $.fn.daterangepicker === 'undefined') {
        return;
    }

    var elements = [].slice.call(document.querySelectorAll('[data-kt-daterangepicker="true"]'));
    var start = moment().subtract(29, 'days');
    var end = moment();

    elements.map(function (element) {
        if (element.getAttribute("data-kt-initialized") === "1") {
            return;
        }

        var display = element.querySelector('div');
        var attrOpens = element.hasAttribute('data-kt-daterangepicker-opens') ? element.getAttribute('data-kt-daterangepicker-opens') : 'left';
        var range = element.getAttribute('data-kt-daterangepicker-range');

        var cb = function (start, end) {
            var current = moment();

            if (display) {
                if (current.isSame(start, "day") && current.isSame(end, "day")) {
                    display.innerHTML = start.format('D MM YYYY');
                } else {
                    display.innerHTML = start.format('D MM YYYY') + ' - ' + end.format('D MM YYYY');
                }
            }
        }

        if (range === "today") {
            start = moment();
            end = moment();
        }

        $(element).daterangepicker({
            startDate: start,
            endDate: end,
            opens: attrOpens,
            locale: daterangePickerLocale,
            ranges: daterangePickerRanges
        }, cb);

        cb(start, end);

        element.setAttribute("data-kt-initialized", "1");
    });
})();

drawRentalsChart();
drawSubscribersChart();

function drawRentalsChart(startDate = null, endDate = null) {
    var element = document.getElementById('RentalsPerDay');

    var height = parseInt(KTUtil.css(element, 'height'));
    var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
    var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
    var baseColor = KTUtil.getCssVariableValue('--kt-info');
    var lightColor = KTUtil.getCssVariableValue('--kt-info-light');

    if (!element) {
        return;
    }

    $.get({
        url: `/Dashboard/GetRentalsPerDay?startDate=${startDate}&endDate=${endDate}`,
        success: function (data) {

            var options = {
                series: [{
                    name: 'Books',
                    data: data.map(i => i.value)
                }],
                chart: {
                    fontFamily: 'inherit',
                    type: 'area',
                    height: height,
                    toolbar: {
                        show: false
                    }
                },
                plotOptions: {

                },
                legend: {
                    show: false
                },
                dataLabels: {
                    enabled: false
                },
                fill: {
                    type: 'solid',
                    opacity: 1
                },
                stroke: {
                    curve: 'smooth',
                    show: true,
                    width: 3,
                    colors: [baseColor]
                },
                xaxis: {
                    categories: data.map(i => i.label),
                    axisBorder: {
                        show: false,
                    },
                    axisTicks: {
                        show: false
                    },
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    },
                    crosshairs: {
                        position: 'front',
                        stroke: {
                            color: baseColor,
                            width: 1,
                            dashArray: 3
                        }
                    },
                    tooltip: {
                        enabled: true,
                        formatter: undefined,
                        offsetY: 0,
                        style: {
                            fontSize: '12px'
                        }
                    }
                },
                yaxis: {
                    tickAmount: Math.max(...data.map(d => d.value)),
                    min: 0,
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }
                },
                states: {
                    normal: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    hover: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    active: {
                        allowMultipleDataPointsSelection: false,
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    }
                },
                tooltip: {
                    style: {
                        fontSize: '12px'
                    }
                },
                colors: [lightColor],
                grid: {
                    borderColor: borderColor,
                    strokeDashArray: 4,
                    yaxis: {
                        lines: {
                            show: true
                        }
                    }
                },
                markers: {
                    strokeColor: baseColor,
                    strokeWidth: 3
                }
            };

            chart = new ApexCharts(element, options);
            chart.render();
        }
    });
}

function drawSubscribersChart() {

    $.get({
        url: '/Dashboard/GetSubscribersPerCity',
        success: function (figures) {
            var ctx = document.getElementById('SubscribersPerCity');

            // Define colors
            var primaryColor = KTUtil.getCssVariableValue('--kt-primary');
            var dangerColor = KTUtil.getCssVariableValue('--kt-danger');
            var successColor = KTUtil.getCssVariableValue('--kt-success');
            var warningColor = KTUtil.getCssVariableValue('--kt-warning');
            var infoColor = KTUtil.getCssVariableValue('--kt-info');

            // Define fonts
            var fontFamily = KTUtil.getCssVariableValue('--bs-font-sans-serif');

            // Chart data
            const data = {
                labels: figures.map(f => f.label),
                datasets: [{
                    data: figures.map(f => f.value),
                    backgroundColor: [
                        infoColor,
                        successColor,
                        warningColor,
                        primaryColor,
                        dangerColor,
                        '#5F91B6',
                        '#D3F6FC',
                        '#C8B0D2'
                    ],
                    borderRadius: 8
                }]
            };

            // Chart config
            const config = {
                type: 'doughnut',
                data: data,
                options: {
                    plugins: {
                        title: {
                            display: false,
                        }
                    },
                    responsive: true,
                },
                defaults: {
                    global: {
                        defaultFont: fontFamily
                    }
                }
            };

            new Chart(ctx, config);
        }
    });
}