
function validateAndShowMessageForRepositoryUrl() {
    let isValid = true;
    const githubUrlPattern = /^(https?:\/\/)?(www\.)?github\.com\/[A-Za-z0-9_-]+(\/[A-Za-z0-9_-]+)?(\.git)?\/?$/;
    const githubUrlInput = document.getElementById('githubUrl');
    const errorMessage = document.getElementById('error-message-for-repository-url-input');


    if (githubUrlInput.value.trim() === '' || !githubUrlPattern.test(githubUrlInput.value)) {
        githubUrlInput.style.borderColor = 'red';
        githubUrlInput.style.borderWidth = '2px';
        githubUrlInput.style.borderStyle = 'solid';
        errorMessage.style.display = 'block'; 
        isValid = false;
    } else {
        githubUrlInput.style.borderColor = '';
        githubUrlInput.style.borderWidth = '';
        githubUrlInput.style.borderStyle = '';
        errorMessage.style.display = 'none'; 
    }
    githubUrlInput.addEventListener('click', function (event) {
        errorMessage.style.display = 'none';
        githubUrlInput.style.borderColor = '';
        githubUrlInput.style.borderWidth = '';
        githubUrlInput.style.borderStyle = '';
    });
    return isValid;
}

function validateAndShowMessageForEndDate() {
    let isValid = true;
    const errorMessage = document.getElementById('error-message-for-end-date');
    const endDateInput = document.getElementById('endDate');
    const currentDate = new Date();
    const endDate = new Date(endDateInput.value.trim());
    if (endDateInput.value.trim() === '' || endDate > currentDate) {
        endDateInput.style.borderColor = 'red';
        endDateInput.style.borderWidth = '2px';
        endDateInput.style.borderStyle = 'solid';
        errorMessage.style.display = 'block';
        isValid = false;
    } else {
        endDateInput.style.borderColor = '';
        endDateInput.style.borderWidth = '';
        endDateInput.style.borderStyle = '';
        errorMessage.style.display = 'none';
    }
    endDateInput.addEventListener('click', function (event) {
        errorMessage.style.display = 'none';
        endDateInput.style.borderColor = '';
        endDateInput.style.borderWidth = '';
        endDateInput.style.borderStyle = '';
    });
    return isValid;
}


   
const getSummaryData = function () {
    var isValid = validateAndShowMessageForRepositoryUrl();
    if (isValid) {
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
  
}
const getChangeFrequencies = function () {
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

const redirectToChartPage = function () {
    window.location.href = "/Chart";
}
const redirectToChartForSpecificPeriodPage = function () {
    window.location.href = "/Chart/hotspots-frequency-and-complexity-for-specific-period";
}
const redirectToChartForFileMainAuthorsPerFile = function () {
    window.location.href = "/Chart/file-main-authors-per-files";
}
const redirectToChartForPowerLawPage = function () {
    window.location.href = "/Chart/power-law-change-frequencies-per-file";
}
const showLoader = function() {
    var loader = document.getElementById("loader");
    loader.style.display = "block";
}

const hideLoader = function() {
    var loader = document.getElementById("loader");
    loader.style.display = "none";
}
getComplexityMetrics = function () {
    var isValid = validateAndShowMessageForRepositoryUrl();
    if (isValid) {
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
                localStorage.setItem('filePathForHotspotsFrequencyAndComplexityFromAllTime', response);
                hideLoader();
                hotspotsButton.style.display = "block";
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
                hideLoader();
            }
        });
    }
}
 const getComplexityMetricsForSpecificPeriod = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);
    var endDatePeriod = $('#endDate').val();
     var isValid = validateAndShowMessageForEndDate();
     if (isValid) {
         $.ajax({
             type: "GET",
             url: "/get-complexity-metrics-for-specific-period",
             data: {
                 RepositoryUrl: encodedGithubUrl,
                 endDatePeriod: endDatePeriod
             },
             dataType: "json",
             beforeSend: function () {
                 showLoader();
             },
             success: function (response) {
                 if (response.error) {
                     displayError(response.error);
                     hideLoader();
                 }
                 else {
                     var hotspotsButton = document.getElementById("displayHotspotsFrequencyAndComplexityButtonForSpecificPeriod");
                     localStorage.setItem('filePathForHotspotsFrequencyAndComplexityForSpecificPeriod', response);
                     hideLoader();
                     hotspotsButton.style.display = "block";
                 }
             },
             error: function (xhr, status, error) {
                 console.error(xhr.responseText);
                 hideLoader();
             }
         });
     }

}

const getMainAuthorsPerModule = function () {
    var githubUrl = $('#githubUrl').val();
    var encodedGithubUrl = encodeURIComponent(githubUrl);
    var endDatePeriod = $('#endDate').val();
    var isValidRepositoryUrl = validateAndShowMessageForRepositoryUrl();
    var isValidEndDate = validateAndShowMessageForEndDate();
    if (isValidRepositoryUrl && isValidEndDate) {
        $.ajax({
            type: "GET",
            url: "/get-main-authors-per-modules-by-revisions",
            data: {
                RepositoryUrl: encodedGithubUrl,
                endDatePeriod: endDatePeriod
            },
            dataType: "json",
            beforeSend: function () {
                showLoader();
            },
            success: function (response) {
                if (response.error) {
                    displayError(response.error);
                    hideLoader();
                }
                else {
                    var hotspotsButton = document.getElementById("displayFileMainAuthors");
                    localStorage.setItem('filePathForFileMainAuthors', response);
                    hideLoader();
                    hotspotsButton.style.display = "block";
                }
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
                hideLoader();
            }
        });
    }

}

const getMetricsForPowerLaw = function () {
    var isValid = validateAndShowMessageForRepositoryUrl();
    if (isValid) {
        var githubUrl = $('#githubUrl').val();
        var encodedGithubUrl = encodeURIComponent(githubUrl);

        $.ajax({
            type: "GET",
            url: "/" + encodedGithubUrl + "/get-metrics-for-power-law",
            dataType: "json",
            beforeSend: function () {
                showLoader();
            },
            success: function (response) {
                if (response.error) {
                    displayError(response.error);
                    hideLoader();
                }
                else {
                    var powerLawButton = document.getElementById("displayPowerLaw");
                    localStorage.setItem('filePathWithChangeFrequenciesPerFileForPowerLawChart', response);
                    hideLoader();
                    powerLawButton.style.display = "block";
                }
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
                hideLoader();
            }
        });
    }
}
const displayError = function (errorMessage) {
    var endDateInput = document.getElementById('endDate');
    var errorParagraph = document.getElementById("error-message-for-end-date");
    errorParagraph.innerText = errorMessage;
    errorParagraph.style.display = "inline";
    endDateInput.style.borderColor = 'red';
    endDateInput.style.borderWidth = '2px';
    endDateInput.style.borderStyle = 'solid';
    endDateInput.addEventListener('click', function (event) {
        endDateInput.style.borderColor = '';
        endDateInput.style.borderWidth = '';
        endDateInput.style.borderStyle = '';
        errorParagraph.innerText = "";
    });
}
