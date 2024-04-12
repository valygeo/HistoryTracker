getData = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);

    $.ajax({
        type: "GET",
        url: "/" + encodedGithubUrl + "/get-summary-data",
        dataType: "json", 
        success: function (response) {
            console.log(response.statistics);
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