var LoginController = function ($scope, LoginFactory, $location, $window) {
    $scope.loginForm = {
        emailAddress: null,
        password: null,
        rememberMe: false,
        errorLoggingIn: false,
        errorPrompt: null
    };
    $scope.login = function () {
        var result = LoginFactory($scope.loginForm.emailAddress, $scope.loginForm.password, $scope.loginForm.rememberMe);
        result.then(function (response) {
            if (response.success) {
                $window.location.reload();
            } else { //this fires if we don't even reach webapi
                $scope.loginForm.errorPrompt = response.message;
                $scope.loginForm.errorLogginIn = true; //whether it's an entire new document html markup or just a string, we display error.
            }
        });
    };
};

LoginController.$inject = ['$scope', 'LoginFactory', '$location', '$window'];