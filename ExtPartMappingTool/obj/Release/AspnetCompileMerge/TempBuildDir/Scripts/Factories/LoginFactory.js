var LoginFactory = function ($http, $q) {
    return function (emailAddress, password, rememberMe) {
        var deferredObject = $q.defer();
        var req = {
            method: 'POST',
            url: '/Account/Login',
            headers: {
                'Content-Type': 'application/json'
            },
            data: { UserName: emailAddress, Password: password, RememberMe: rememberMe }
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

LoginFactory.$inject = ['$http', '$q'];