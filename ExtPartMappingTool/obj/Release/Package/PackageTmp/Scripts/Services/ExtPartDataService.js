var ExtPartDataService = function ($http, $q) {
    this.getAllExtPartData = function (currentState) {
        var deferredObject = $q.defer();
        $http.get(
            (currentState.currentState == 1 ? '/NestedViews/ExtPartMappings' : '/Production/ExtPartMappings')
            )
            .then(function (response) { //a response will return with an obect indicating success of data retriveal or some known result. Error would indicate webapi wasn't even reached
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
    }
    this.delete = function (entry, currentState) {
        var deferredObject = $q.defer();

        var req = {
            method: 'POST',
            url: (currentState.currentState == 1 ? '/NestedViews/Delete' : '/Production/Delete'),
            headers: {
                'Content-Type': 'application/json'
            },
            data: entry
        }
        $http(req).then(function (response) { //a response will return with an obect indicating success of data retriveal or some known result. Error would indicate webapi wasn't even reached
            if (response.data.Success) {
                deferredObject.resolve({
                    success: true,
                    data: null
                });
            } else {
                deferredObject.resolve({ //this is where most our expected errors come to
                    success: false,
                    message: response.data.Message
                });
            }

        }//     response.data);
        , function (error) { //it never goes in here unless it's an exception or httperror returning.
            deferredObject.resolve({
                success: false,
                data: error.data
            })
        });
        return deferredObject.promise;
    };
    this.save = function (extPartMappingnEntry, currentState) {
        var deferredObject = $q.defer();

        var req = {
            method: 'POST',
            url: (currentState.currentState == 1 ? '/NestedViews/Save' : '/Production/Save'),
            headers: {
                'Content-Type': 'application/json'
            },
            data: extPartMappingnEntry
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
    this.search = function (oldPartId, newPartId, currentState) {
        var deferredObject = $q.defer();

        $http.get(
            (currentState.currentState == 1 ? '/NestedViews/Search' : '/Production/Search') + '?oldPartId=' + oldPartId + '&newPartId='+ newPartId
        ).then(function (response) { //a response will return with an obect indicating success of data retriveal or some known result. Error would indicate webapi wasn't even reached
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
    }
}

ExtPartDataService.$inject = ['$http', '$q'];