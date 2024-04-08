getData = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);

    $.ajax({
        type: "GET",
        url: "/" + encodedGithubUrl + "/get-summary-data",
        dataType: "json", // Specifică tipul de date așteptat de la server
        success: function (response) {
            $('#summaryDataContainer').html(response.fileContent);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}