let currentConnection = null;
let currentChart = null;
let lastColumns = null;

$(document).ready(function() {
    // Handle integrated security checkbox
    $('#integratedSecurity').change(function() {
        $('#credentialsSection').toggle(!this.checked);
    });

    // Handle connection form submission
    $('#connectionForm').submit(function(e) {
        e.preventDefault();
        console.log('Form submitted'); // Debug log

        const connection = {
            server: $('#server').val(),
            database: $('#database').val(),
            integratedSecurity: $('#integratedSecurity').is(':checked'),
            username: $('#username').val(),
            password: $('#password').val()
        };

        console.log('Connection details:', connection); // Debug log

        // Test connection
        $.ajax({
            url: '/api/chart/test-connection',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(connection),
            success: function(response) {
                console.log('Response:', response); // Debug log
                if (response.success) {
                    currentConnection = connection;
                    loadDatabaseObjects();
                    $('#objectSelection').show();
                    alert('Connection successful!');
                } else {
                    alert('Connection failed: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error details:', xhr.responseText);
                alert('Connection failed: ' + (xhr.responseJSON?.message || error));
            }
        });
    });

    // Handle database object selection
    $('#databaseObject').change(function() {
        if (this.value) {
            loadObjectData(this.value);
            $('#chartConfig').show();
        } else {
            $('#chartConfig').hide();
        }
    });

    // Handle chart type selection
    $('#chartType').change(function() {
        updateFieldMappingInterface(lastColumns);
    });

    // Handle generate chart button
    $('#generateChart').click(function() {
        generateChart();
    });
});

function loadDatabaseObjects() {
    $.ajax({
        url: '/api/chart/database-objects',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(currentConnection),
        success: function(objects) {
            const select = $('#databaseObject');
            select.empty();
            select.append('<option value="">Select an object...</option>');
            
            objects.forEach(obj => {
                select.append(`<option value="${obj.schema}.${obj.name}">${obj.schema}.${obj.name} (${obj.type})</option>`);
            });
        }
    });
}

function loadObjectData(objectFullName) {
    const [schema, name] = objectFullName.split('.');
    $.ajax({
        url: `/api/chart/execute-query?schema=${schema}&objectName=${name}`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(currentConnection),
        success: function(response) {
            lastColumns = response.columns;
            updateFieldMappingInterface(lastColumns);
        }
    });
}

function updateFieldMappingInterface(columns) {
    const chartType = $('#chartType').val();
    const mappingDiv = $('#fieldMapping');
    mappingDiv.empty();

    if (!columns || columns.length === 0) {
        mappingDiv.append('<div class="text-muted">Select a data source to map fields.</div>');
        return;
    }

    // Add a hint for the user
    if (chartType === 'bar' || chartType === 'radar') {
        mappingDiv.append('<div class="mb-2 text-info">Tip: Use a view like <b>vw_SalesByRegion</b> for best results.</div>');
    } else if (chartType === 'line') {
        mappingDiv.append('<div class="mb-2 text-info">Tip: Use a function like <b>fn_SalesByDate</b> for best results.</div>');
    }

    const requiredFields = getRequiredFields(chartType);

    requiredFields.forEach(field => {
        const row = $('<div class="field-mapping-row">');
        const select = $('<select class="form-select">').attr('data-field', field);

        select.append('<option value="">Select field...</option>');
        columns.forEach(col => {
            select.append(`<option value="${col.name}">${col.name} (${col.dataType})</option>`);
        });

        row.append(`<label class="form-label">${field}:</label>`);
        row.append(select);
        mappingDiv.append(row);
    });
}

function getRequiredFields(chartType) {
    switch (chartType) {
        case 'line':
        case 'bar':
            return ['Labels (X-Axis)', 'Values (Y-Axis)'];
        case 'radar':
            return ['Labels (Categories)', 'Values (Data Points)'];
        default:
            return [];
    }
}

function generateChart() {
    const chartType = $('#chartType').val();
    const mappings = {};
    
    $('#fieldMapping select').each(function() {
        mappings[$(this).data('field')] = $(this).val();
    });

    const [schema, name] = $('#databaseObject').val().split('.');
    
    $.ajax({
        url: `/api/chart/execute-query?schema=${schema}&objectName=${name}`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(currentConnection),
        success: function(response) {
            renderChart(response.data, mappings, chartType);
            $('#chartDisplay').show();
        }
    });
}

function renderChart(data, mappings, chartType) {
    const ctx = document.getElementById('chartCanvas').getContext('2d');

    // Destroy previous chart if it exists
    if (window.currentChart) {
        window.currentChart.destroy();
    }

    const labels = data.map(row => row[mappings['Labels (X-Axis)'] || mappings['Labels (Categories)']]);
    const values = data.map(row => row[mappings['Values (Y-Axis)'] || mappings['Values (Data Points)']]);

    window.currentChart = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Dataset',
                data: values,
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
} 