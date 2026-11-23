$.getJSON('/Admin/Dashboard/GetChartData', function(data){
    new Chart(document.getElementById("revenueChart"), {
        type: 'line',
        data: {
            labels: data.monthlyRevenue.map(r => r.label),
            datasets: [{
                label: 'Revenue',
                data: data.monthlyRevenue.map(r => r.revenue),
                borderWidth: 1,
            }]
        }
    });
    new Chart(document.getElementById("ordersChart"), {
        type: 'bar',
        data: {
            labels: data.monthlyOrders.map(r => r.label),
            datasets: [{
                label: 'Orders',
                data: data.monthlyOrders.map(r => r.count),
                borderWidth: 1,
            }]
        }
    });
    new Chart(document.getElementById("statusChart"), {
        type: 'doughnut',
        data: {
            labels: data.statusBreakdown.map(r => r.status),
            datasets: [{
                label: 'Order Status',
                data: data.statusBreakdown.map(r => r.count),
                borderWidth: 1,
            }]
        }
    });
    new Chart(document.getElementById("categoryChart"), {
        type: 'bar',
        data: {
            labels: data.productsPerCategory.map(r => r.category),
            datasets: [{
                label: 'Products by Category',
                data: data.productsPerCategory.map(r => r.count),
                borderWidth: 1,
            }]
        }
    });
})