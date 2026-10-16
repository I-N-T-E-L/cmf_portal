(function() {
    if (typeof echarts === 'undefined') return;
    var raw = (window.CMF_PORTAL && window.CMF_PORTAL.analyticsChartDataJson) || '';
    var data;
    try { data = JSON.parse(raw); } catch (e) { return; }
    if (!data || !data.componentLabels || !data.componentLabels.length) return;

    // Component horizontal bar chart
    var c1 = echarts.init(document.getElementById('chartByComponent'));
    c1.setOption({
        tooltip: { trigger: 'axis' },
        legend: { data: ['Open', 'Implemented'], bottom: 0 },
        grid: { left: 100, right: 20, top: 20, bottom: 40 },
        xAxis: { type: 'value' },
        yAxis: {
            type: 'category',
            data: data.componentLabels.slice().reverse(),
            axisLabel: { fontSize: 11 }
        },
        series: [
            {
                name: 'Open',
                type: 'bar',
                stack: 'total',
                data: data.openCounts.slice().reverse(),
                itemStyle: { color: '#0068b5' }
            },
            {
                name: 'Implemented',
                type: 'bar',
                stack: 'total',
                data: data.implCounts.slice().reverse(),
                itemStyle: { color: '#107c10' }
            }
        ]
    });

    // Status donut chart
    var c2 = echarts.init(document.getElementById('chartByStatus'));
    var pieData = data.statusLabels.map(function(l, i) {
        return { name: l, value: data.statusCounts[i] };
    });
    c2.setOption({
        tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
        legend: { orient: 'vertical', right: 10, top: 'center' },
        series: [{
            type: 'pie',
            radius: ['40%', '70%'],
            center: ['40%', '50%'],
            data: pieData,
            label: { show: false },
            emphasis: { label: { show: true, fontSize: 14, fontWeight: 'bold' } }
        }]
    });

    // Milestone bar chart
    var c3 = echarts.init(document.getElementById('chartByMilestone'));
    c3.setOption({
        tooltip: { trigger: 'axis' },
        grid: { left: 55, right: 20, top: 20, bottom: 55 },
        xAxis: {
            type: 'category',
            data: data.milestoneLabels,
            axisLabel: { rotate: 30, fontSize: 11 }
        },
        yAxis: { type: 'value', name: 'Open Issues' },
        series: [{
            type: 'bar',
            data: data.milestoneCounts,
            itemStyle: { color: '#7c3aed', borderRadius: [4, 4, 0, 0] }
        }]
    });

    window.addEventListener('resize', function() {
        c1.resize();
        c2.resize();
        c3.resize();
    });
})();
