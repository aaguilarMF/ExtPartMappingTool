var UploadFileService = function ($http, $q) {
    this.commit = function (uploadEntries, currentState) {
        var deferredObject = $q.defer();

        var req = {
            method: 'POST',
            url: (currentState.currentState == 1 ? '/UploadFileDev/Commit' : '/UploadFileProd/Commit'),
            headers: {
                'Content-Type': 'application/json'
            },
            data: uploadEntries
        }
        $http(req).then(function (response) { //a response will return with an obect indicating success of data retriveal or some known result. Error would indicate webapi wasn't even reached
            if (response.data.Success) {
                deferredObject.resolve({
                    success: true,
                    data: JSON.parse(response.data.JSON_RESPONSE_DATA)
                });
            } else {
                deferredObject.resolve({
                    success: false,
                    message: response.data.Message
                });
            }

        }//     response.data);
        , function (error) { //it never goes in here
            deferredObject.resolve({
                success: false,
                data: error.data
            })
        });
        return deferredObject.promise;
    };
}

UploadFileService.$inject = ['$http', '$q'];