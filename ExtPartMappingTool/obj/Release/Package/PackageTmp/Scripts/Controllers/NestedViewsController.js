var NestedViewsController = function($scope){
    $scope.states = {
        Development: 1,
        Production: 2
    };
    $scope.tabs = {
        search: "/Templates/Search.html",
        view: "/Templates/ViewAllExtPartData.html",
        addRecord: "/Templates/AddRecord.html",
        uploadFile: "/Templates/UploadFile.html"
    };
    $scope.currentTab = null;
    $scope.nameOfTab = '(not in anything)'
    $scope.currentState = null;
    $scope.init = function (globalStateHolder) {
        $scope.currentState= globalStateHolder;
    };
}

NestedViewsController.$inject = ['$scope'];