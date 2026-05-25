function readJson(id) {
    const element = document.getElementById(id);

    if (!element) {
        return [];
    }

    try {
        return JSON.parse(element.textContent || "[]");
    } catch {
        return [];
    }
}

function hasCanvas(id) {
    return document.getElementById(id) !== null;
}

function chartColors() {
    return [
        "#2563eb",
        "#16a34a",
        "#dc2626",
        "#f59e0b",
        "#7c3aed",
        "#0891b2",
        "#db2777",
        "#475569"
    ];
}

function createBarChart(id, labels, values, label, color) {
    if (!hasCanvas(id) || !labels.length || !values.length || typeof Chart === "undefined") {
        return;
    }

    new Chart(document.getElementById(id), {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: values,
                backgroundColor: color || "#2563eb",
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createLineChart(id, labels, values, label) {
    if (!hasCanvas(id) || !labels.length || !values.length || typeof Chart === "undefined") {
        return;
    }

    new Chart(document.getElementById(id), {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: values,
                borderColor: "#2563eb",
                backgroundColor: "rgba(37, 99, 235, 0.12)",
                tension: 0.35,
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createPieChart(id, labels, values, label) {
    if (!hasCanvas(id) || !labels.length || !values.length || typeof Chart === "undefined") {
        return;
    }

    new Chart(document.getElementById(id), {
        type: "pie",
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: values,
                backgroundColor: chartColors()
            }]
        },
        options: {
            responsive: true
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    const gunlukSatisLabels = readJson("gunlukSatisLabelsJson");
    const gunlukSatisValues = readJson("gunlukSatisValuesJson");

    const gelirGiderLabels = readJson("gelirGiderLabelsJson");
    const gelirGiderValues = readJson("gelirGiderValuesJson");

    const kategoriSatisLabels = readJson("kategoriSatisLabelsJson");
    const kategoriSatisValues = readJson("kategoriSatisValuesJson");

    const enCokSatilanLabels = readJson("enCokSatilanLabelsJson");
    const enCokSatilanValues = readJson("enCokSatilanValuesJson");

    const kritikStokLabels = readJson("kritikStokLabelsJson");
    const kritikStokValues = readJson("kritikStokValuesJson");

    const aylikGelirGiderLabels = readJson("aylikGelirGiderLabelsJson");
    const aylikGelirValues = readJson("aylikGelirValuesJson");
    const aylikGiderValues = readJson("aylikGiderValuesJson");

    createLineChart("gunlukSatisChart", gunlukSatisLabels, gunlukSatisValues, "Net Satış");
    createBarChart("gelirGiderChart", gelirGiderLabels, gelirGiderValues, "Tutar", "#16a34a");
    createPieChart("kategoriSatisChart", kategoriSatisLabels, kategoriSatisValues, "Kategori Satışı");
    createBarChart("enCokSatilanChart", enCokSatilanLabels, enCokSatilanValues, "Satılan Adet", "#7c3aed");
    createBarChart("kritikStokChart", kritikStokLabels, kritikStokValues, "Stok Miktarı", "#f59e0b");

    if (hasCanvas("aylikGelirGiderChart") &&
        aylikGelirGiderLabels.length &&
        typeof Chart !== "undefined") {
        new Chart(document.getElementById("aylikGelirGiderChart"), {
            type: "bar",
            data: {
                labels: aylikGelirGiderLabels,
                datasets: [
                    {
                        label: "Gelir",
                        data: aylikGelirValues,
                        backgroundColor: "#16a34a",
                        borderRadius: 6
                    },
                    {
                        label: "Gider",
                        data: aylikGiderValues,
                        backgroundColor: "#dc2626",
                        borderRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
});