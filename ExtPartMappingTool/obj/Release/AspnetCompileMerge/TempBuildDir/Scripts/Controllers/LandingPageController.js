var LandingPageController = function ($scope, $http, $route, LogOffFactory, $window, $location) {
    $scope.models = {
        nameLabel: 'Ext Part Mapping Tool',
        development: false,
        devActive: false,
        prodActive: false
    };
    this.states= {
        Development: 1,
        Production: 2
    };
    this.currentStateHolder = {
        currentState: this.states.Development
    }
    $scope.developmentView = function () {
        $scope.models.devActive = true;
        $scope.models.prodActive = false;
        this.mainController.currentStateHolder.currentState = this.mainController.states.Development;
        $route.reload();
    }
    $scope.productionView = function () {
        $scope.models.devActive = false;
        $scope.models.prodActive = true;
        this.mainController.currentStateHolder.currentState = this.mainController.states.Production;
        $route.reload();
    }
    $scope.navbarProperties = {
        isCollapsed: true
    };
    $scope.logOff = function () {
        var result = LogOffFactory();
        result.then(function (response) {
            if (response.success) {
                $window.location.reload();
            } else { //this fires if we don't even reach webapi
                alert('Error logging out. Contact IT. But you can also clear your cookies. Go to settings in browser and clear cookies. But Also do contact IT please.')
            }
        });
    };
    $scope.toggleDevelopment = function () {
        if (this.mainController.currentStateHolder.currentState === this.mainController.states.Development) {
            this.mainController.currentStateHolder.currentState = this.mainController.states.Production;
        } else {
            this.mainController.currentStateHolder.currentState = this.mainController.states.Development;
        }
        //$scope.models.development = !$scope.models.development;
    }
    $scope.init = function () {
        if (this.mainController.currentStateHolder.currentState = 1) {
            $scope.models.devActive = true;
            $scope.models.prodActive = false;
        } else if (this.mainController.currentStateHolder.currentState = 2) {
            $scope.models.devActive = false;
            $scope.models.prodActive = true;
        }
    }
    $scope.init();
}

// The $inject property of every controller (and pretty much every other type of object in Angular) needs to be a string array equal to the controllers arguments, only as strings
LandingPageController.$inject = ['$scope', '$http', '$route', 'LogOffFactory', '$window', '$location'];