﻿
function createChart(data) {
    // Specify the chart’s dimensions.
    const width = 928;
    const height = width;

    // Create the color scale.
    const color = d3.scaleLinear()
        .domain([0, 5])
        .range(["hsl(210, 80%, 95%)", "hsl(210, 50%, 70%)"])
        .interpolate(d3.interpolateHcl);

    // Compute the layout.
    const pack = data => d3.pack()
        .size([width, height])
        .padding(3)
        (d3.hierarchy(data)
            .sum(d => d.value)
            .sort((a, b) => b.value - a.value));
    const root = pack(data);

    // Create the SVG container.
    const svg = d3.create("svg")
        .attr("viewBox", `-${width / 2} -${height / 2} ${width} ${height}`)
        .attr("width", width)
        .attr("height", height)
        .attr("style", `max-width:auto; height: auto; display: block; margin: 0 -14px; background:linear-gradient(113deg,  #afbcd91c, #c2cfd5); cursor: pointer;  position: fixed; left:10%; top:-640%; border: solid; border-radius: 10px; border-color: #a3adc1;`);

    // Append the nodes.
    const node = svg.append("g")
        .selectAll("circle")
        .data(root.descendants().slice(1))
        .join("circle")
        .style("fill", function (d) {
            if (d.data.weight > 0.0) {
                return `rgba(255, 0, 0, ${d.data.weight})`;
            } else if (d.children) {
                return color(d.depth); 
            } else {
                return "WhiteSmoke"; 
            }
        })
        .style("fill-opacity", function (d) {
            return d.weight; 
        })
        .attr("pointer-events", d => !d.children ? "none" : null)
        .on("mouseover", function () { d3.select(this).attr("stroke", "#000"); })
        .on("mouseout", function () { d3.select(this).attr("stroke", null); })
        .on("click", (event, d) => focus !== d && (zoom(event, d), event.stopPropagation()));

    // Append the text labels.
    const label = svg.append("g")
        .style("font", "10px sans-serif")
        .attr("pointer-events", "none")
        .attr("text-anchor", "middle")
        .selectAll("text")
        .data(root.descendants())
        .join("text")
        .style("fill-opacity", d => d.parent === root ? 1 : 0)
        .style("display", d => d.parent === root ? "inline" : "none")
        .text(d => d.data.name);

    // Create the zoom behavior and zoom immediately in to the initial focus node.
    svg.on("click", (event) => zoom(event, root));
    let focus = root;
    let view;
    zoomTo([focus.x, focus.y, focus.r * 2]);

    function zoomTo(v) {
        const k = width / v[2];

        view = v;

        label.attr("transform", d => `translate(${(d.x - v[0]) * k},${(d.y - v[1]) * k})`);
        node.attr("transform", d => `translate(${(d.x - v[0]) * k},${(d.y - v[1]) * k})`);
        node.attr("r", d => d.r * k);
    }

    function zoom(event, d) {
        const focus0 = focus;

        focus = d;

        const transition = svg.transition()
            .duration(event.altKey ? 7500 : 750)
            .tween("zoom", d => {
                const i = d3.interpolateZoom(view, [focus.x, focus.y, focus.r * 2]);
                return t => zoomTo(i(t));
            });

        label
            .filter(function (d) { return d.parent === focus || this.style.display === "inline"; })
            .transition(transition)
            .style("fill-opacity", d => d.parent === focus ? 1 : 0)
            .on("start", function (d) { if (d.parent === focus) this.style.display = "inline"; })
            .on("end", function (d) { if (d.parent !== focus) this.style.display = "none"; });
    }
    return svg.node();
}

document.addEventListener('DOMContentLoaded', function () {
    getHierarchyDataForSpecificPeriod();
});

const getHierarchyDataForSpecificPeriod = function () {
    const filePath = localStorage.getItem('filePathForHotspotsFrequencyAndComplexityForSpecificPeriod');
    localStorage.removeItem('filePathForHotspotsFrequencyAndComplexityForSpecificPeriod');
    $.ajax({
        type: "GET",
        url: "/chart-api-controller/hotspots-frequencies-and-complexity-for-specific-period",
        data: { filePath: filePath },
        dataType: "json",
        success: function (response) {
            processDataAndDisplayChart(response);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function processDataAndDisplayChart(response) {
    const chart = createChart({ name: "flare", children: response });
    const chartContainer = document.getElementById('chartContainer');
    chartContainer.appendChild(chart);
}
