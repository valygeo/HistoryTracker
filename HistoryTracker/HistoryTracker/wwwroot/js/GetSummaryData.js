getData = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);

    $.ajax({
        type: "GET",
        url: "/" + encodedGithubUrl + "/get-summary-data",
        dataType: "json", 
        success: function (response) {
            $('#summaryDataContainer').html(
                '<p>Number of commits: ' + response.statistics.numberOfCommits + '</p>' +
                '<p>Number of authors: ' + response.statistics.numberOfAuthors + '</p>' +
                '<p>Number of entities: ' + response.statistics.numberOfEntities + '</p>' +
                '<p>Number of entitites changed: ' + response.statistics.numberOfEntitiesChanged + '</p>'
            );
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
getChangeFrequencies = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);

    $.ajax({
        type: "GET",
        url: "/" + encodedGithubUrl + "/get-change-frequencies",
        dataType: "json",
        success: function (response) {
            $('#changeFrequenciesContainer').html(
                
            );
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function redirectToChartPage() {
    window.location.href = '/Chart'; 
}
function showLoader() {
    var loader = document.getElementById("loader");
    loader.style.display = "block";
}

function hideLoader() {
    var loader = document.getElementById("loader");
    loader.style.display = "none";
}


getComplexityMetrics = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);

    $.ajax({
        type: "GET",
        url: "/" + encodedGithubUrl + "/get-complexity-metrics",
        dataType: "json",
        beforeSend: function () {
            showLoader();
        },
        success: function (response) {
            var hotspotsButton = document.getElementById("displayHotspotsFrequencyAndComplexityButton");
            complexityAndFrequenciesPerFilePath = response;
            hideLoader();
            hotspotsButton.style.display = "block";
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
            hideLoader();
        }
    });
}

