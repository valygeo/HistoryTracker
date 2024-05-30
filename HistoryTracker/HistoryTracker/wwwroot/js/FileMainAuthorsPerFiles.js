function createChart(data) {
    // Specify the chart’s dimensions.
    const width = 928;
    const height = width;

    // Create the color scale.
    const color = d3.scaleLinear()
        .domain([0, 5])
        .range(["hsl(210, 80%, 95%)", "hsl(210, 50%, 70%)"])
        .interpolate(d3.interpolateHcl);

    // Create the color scale for authors.
    const authorColor = d3.scaleOrdinal(d3.schemeCategory10);

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
        .attr("style", `max-width: 100%; height: auto; display: block; margin: 0 -14px; background: rgb(232, 242, 252); cursor: pointer;  border: solid;
         border-radius: 10px; border-color: #a3adc1;  background: linear-gradient(113deg, #afbcd91c, #c2cfd5);`);

    // Append the nodes.
    const node = svg.append("g")
        .selectAll("circle")
        .data(root.descendants().slice(1))
        .join("circle")
        .style("fill", function (d) {
            if (!d.children) {
                // If the node is a leaf node, color it based on the author
                return authorColor(d.data.mainAuthor);
            } else {
                // Otherwise, use the depth-based color scale
                return color(d.depth);
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
    // Generate legend
    const mainAuthors = Array.from(new Set(root.leaves().map(d => d.data.mainAuthor)));
    const legendContainer = d3.select("#legendContainer").append("svg")
        .attr("width", 300)
        .attr("height", mainAuthors.length * 40 + 10);

    const legend = legendContainer.selectAll(".legend")
        .data(mainAuthors)
        .enter().append("g")
        .attr("class", "legend")
        .attr("transform", (d, i) => `translate(0,${i * 20})`);

    legend.append("rect")
        .attr("x", 0)
        .attr("width", 18)
        .attr("height", 18)
        .style("fill", authorColor);

    legend.append("text")
        .attr("x", 24)
        .attr("y", 9)
        .attr("dy", ".35em")
        .style("text-anchor", "start")
        .text(d => d);

    // Add label for colors with no author
    mainAuthors.forEach(author => {
        if (!author) {
            legendContainer.append("text")
                .attr("x", 24)
                .attr("y", mainAuthors.indexOf(author) * 20 + 15)
                .style("text-anchor", "start")
                .text("No changes or module does not exist ");
        }
    });
    return svg.node();
}

document.addEventListener('DOMContentLoaded', function () {
    getHierarchyData();
});

const getHierarchyData = function () {
    const filePath = localStorage.getItem('filePathForFileMainAuthors');
    localStorage.removeItem('filePathForFileMainAuthors');
    $.ajax({
        type: "GET",
        url: "/chart-api-controller/file-main-authors-per-files",
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
