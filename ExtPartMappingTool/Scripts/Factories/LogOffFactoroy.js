var LogOffFactory = function ($http, $q) {
    return function () {
        var deferredObject = $q.defer();
        var req = {
            method: 'GET',
            url: '/Account/LogOff',
            headers: {
                'Content-Type': 'application/json'
            }
        }
        $http(req).then(function (response) { //a response will return with an obect indicating success of data retriveal or some known result. Error would indicate webapi wasn't even reached
            if (response.data.Success) {
                deferredObject.resolve({
                    success: true,
                    data: response.data // JSON.parse(response.data.JSON_RESPONSE_DATA)
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
    }

}

LogOffFactory.$inject = ['$http', '$q']