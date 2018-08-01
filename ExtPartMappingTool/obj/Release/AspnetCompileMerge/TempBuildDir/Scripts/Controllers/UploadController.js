var UploadController = function ($scope, FileUploader, UploadFileService, $route) {
    var uploader = new FileUploader();
    $scope.height = {
        height: 500,
        newHeight: function (numOfRows) {
            var requestedHeight = 32 + (30 * numOfRows);
            if (requestedHeight > 480) {
                $scope.height.height = 500;
            } else {
                $scope.height.height = requestedHeight;
            }
        }
    }
    $scope.fileChosen = false;
    $scope.uploader = uploader;
    $scope.currentState = null;
    uploader.url = null;
    $scope.gridFilled = false;
    $scope.gridOptions = {
        enableRowHeaderSelection: false,
        multiSelect: false,
        data: null,
        columnDefs: null
    };
    uploader.onSuccessItem = function (fileItem, response, status, headers) {
        var data = JSON.parse(response.JSON_RESPONSE_DATA);
        var columnDefs = [];
        var target = data[0];
        var col = 0;
        for (var k in target) {
            if (target.hasOwnProperty(k) && col >1) {
                columnDefs.push({ field: String(k), displayName: k});
            }
            col++
        }
        $scope.gridOptions.data = data;
        $scope.gridOptions.columnDefs = columnDefs;

        $scope.height.newHeight(data.length);
        //for div hide or appear stuff
        $scope.fileChosen = false;
        $scope.gridFilled = true;
    };
    uploader.onAfterAddingFile = function (item) {
        $scope.fileChosen = true;
    };
    $scope.commit = function () {
        var result = UploadFileService.commit($scope.gridOptions.data, $scope.currentState);
        result.then(function (response) {
            if (response.success) {
                
                
                alert("success");
                $scope.gridFilled = false;
                $route.reload();
            } else { //this fires if we don't even reach webapi
                document.write(response.data); //whether it's an entire new document html markup or just a string, we display error.
                //$scope.noCustomerFoundDialog
            }
        });
    }

    $scope.init = function (currentStateHolder) {
        $scope.currentState = currentStateHolder;
        if (currentStateHolder.currentState == this.mainController.states.Development) {
            uploader.url = "/UploadFileDev/Upload";
        } else {
            uploader.url = "/UploadFileProd/Upload";
        }
    }
}

UploadController.$inject = ['$scope', 'FileUploader', 'UploadFileService', '$route'];