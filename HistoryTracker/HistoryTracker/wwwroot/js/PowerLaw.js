function createChart(data) {
    // Specify the chart’s dimensions.
    const width = 1000;
    const height = 600;
    const marginTop = 20;
    const marginRight = 20;
    const marginBottom = 100; 
    const marginLeft = 100; 

    // Create the horizontal and vertical scales.
    const x = d3.scaleBand()
        .domain(data.map(d => d.entityPath))
        .range([marginLeft, width - marginRight])
        .padding(0.1);

    const y = d3.scaleLinear()
        .domain([0, d3.max(data, d => d.revisions)]).nice()
        .range([height - marginBottom, marginTop]);

    // Create the horizontal axis generator.
    const xAxis = g => g
        .attr("transform", `translate(0,${height - marginBottom})`)
        .call(d3.axisBottom(x).tickSizeOuter(0))
        .call(g => g.selectAll("text").remove())
        .attr("transform", "rotate(-40)")
        .style("text-anchor", "end");

    // Create the vertical axis generator.
    const yAxis = g => g
        .attr("transform", `translate(${marginLeft},0)`)
        .call(d3.axisLeft(y))
        .call(g => g.select(".domain").remove());

    // Create the SVG container.
    const svg = d3.create("svg")
        .attr("viewBox", [0, 0, width, height])
        .attr("width", width)
        .attr("height", height)
        .attr("style", "max-width: 100%; height: auto;");

    // Append the horizontal axis.
    svg.append("g").call(xAxis);

    // Append the vertical axis.
    svg.append("g").call(yAxis);

    svg.append("text")
        .attr("transform",
            "translate(" + (width / 2) + " ," +
            (height - marginBottom / 2) + ")")
        .style("text-anchor", "middle")
        .text("Module");

    const tooltip = d3.select("body").append("div")
        .attr("class", "tooltip")
        .style("opacity", 0);

    // Create the bars.
    svg.append("g")
        .selectAll("rect")
        .data(data)
        .enter().append("rect")
        .attr("x", d => x(d.entityPath))
        .attr("y", d => y(d.revisions))
        .attr("height", d => y(0) - y(d.revisions))
        .attr("width", x.bandwidth())
        .attr("fill", "steelblue")
        .on("mouseover", function (event, d) {
            tooltip.transition().duration(200).style("opacity", 1);
            tooltip.html(`<strong>Entity:</strong> ${d.entityPath}<br><strong>Revisions:</strong> ${d.revisions}`)
                .style("left", (event.pageX + 5) + "px")
                .style("top", (event.pageY - 28) + "px");
        })
        .on("mousemove", function (event) {
            tooltip.style("left", (event.pageX + 5) + "px")
                .style("top", (event.pageY - 28) + "px");
        })
        .on("mouseout", function () {
            tooltip.transition().duration(500).style("opacity", 0);
        });

    return svg.node();
}
document.addEventListener('DOMContentLoaded', function () {
    getDataForPowerLawChart();
});

const getDataForPowerLawChart = function () {
    let filePath = localStorage.getItem('filePathWithChangeFrequenciesPerFileForPowerLawChart');
    localStorage.removeItem('filePathWithChangeFrequenciesPerFileForPowerLawChart');
    $.ajax({
        type: "GET",
        url: "/chart-api-controller/power-law-change-frequencies-per-file",
        data: { filePath },
        dataType: "json",
        success: function (response) {
            processAndDisplayChart(response);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

const processAndDisplayChart = function (data) {
    let chartContainer = document.getElementById('chartContainer');
    chartContainer.appendChild(createChart(data));
}