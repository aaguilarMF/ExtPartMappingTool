var ViewAllExtPartDataController = function ($scope, uiGridConstants, ExtPartDataService) {
    $scope.height = {
        height: 500,
        newHeight: function (numOfRows) {
            var requestedHeight = 30 + (30 * numOfRows);
            if (requestedHeight > 480) {
                $scope.height.height = 500;
            } else {
                $scope.height.height = requestedHeight;
            }

        }
    }
    $scope.editModel = {
        showButton: false,
        hasChanged: false
    };
    $scope.gridFilled = true;
    $scope.currentState = null;
    $scope.init = function (currentStateHolder) {
        $scope.currentState = currentStateHolder;
        $scope.populateExtPartMappingsTable();
    }
    $scope.populateExtPartMappingsTable = function () {
        var result = ExtPartDataService.getAllExtPartData($scope.currentState);
        result.then(function (response) {
            if (response.success) {
                var data = response.data;
                var columnDefs = [];
                var editable = [false, true, true];
                var target = data[0];
                var col = 0;
                for (var k in target) {
                        if (target.hasOwnProperty(k)) {
                           columnDefs.push({ field: String(k), displayName: k, enableCellEdit: editable[col] });
                        }

                    col++;
                }
                for (var i = 0; i < data.length; i++) { //adding variable that notifies if value in cell has been edited.
                    data[i].hasBeenEdited = false;
                }
                $scope.gridOptions.data = data;
                $scope.gridOptions.columnDefs = columnDefs;

                $scope.height.newHeight(data.length);
                //for div hide or appear stuff
                $scope.gridFilled = false;
                $scope.gridFilled = true;

            }
            else { 
                $scope.gridFilled = false;
            }
        }, function () { alert("don't think we'll ever get here") })
    };
    $scope.gridOptions = {
        enableRowSelection: true,
        enableRowHeaderSelection: false,
        multiSelect: false,
        data: null,
        columnDefs: null
    }
    $scope.gridOptions.onRegisterApi = function (gridApi) {
        //set gridApi on scope
        $scope.gridApi = gridApi;
        gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef, newValue, oldValue) {
            if (!$scope.editModel.hasChanged) {
                if (!(newValue === oldValue)) {
                    $scope.editModel.showButton = true;
                }
            }
            if (!(newValue === oldValue)) {
                colDef.cellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
                    if (row.entity.Id == rowEntity.Id) {
                        row.entity.hasBeenEdited = true;
                        return "redtext";
                    }
                    else
                        return null;
                };
            }
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);

            //Do your REST call here via $http.get or $http.post

            //Alert to show what info about the edit is available
            //alert('Column: ' + colDef.name + ' ID: ' + rowEntity.id + ' Name: ' + rowEntity.name + ' Age: ' + rowEntity.age);
        });

    };
    $scope.deleteRow = function () {
        //$scope.gridOptions.data = vm.resultSimulatedData;
        var selectedRows = $scope.gridApi.selection.getSelectedRows();
        var row = selectedRows[0];
        if (row) {
            if (window.confirm("Are you sure you want to delete Id: " + row.Id + "?")) {
            } else {
                return;
            }
        } else {
            alert('Select a row in order to delete');
            return;
        }
        if (row) {
            var result = ExtPartDataService.delete(row, $scope.currentState);
            result.then(function (response) {
                if (response.success) {
                    $scope.populateExtPartMappingsTable();
                    $scope.editModel.showButton = false;
                } else { //this fires if we don't even reach webapi
                    document.write(response.data); //whether it's an entire new document html markup or just a string, we display error.
                    //document.body.innerHTML = response.data;
                    //$scope.noCustomerFoundDialog
                }
            });
        } else {
            alert('Select a row by clicking on it first');
        }
        /*$timeout(function () {
            if (vm.gridApi.selection.selectedRow) {
                vm.gridApi.selection.selectRow(vm.gridOptions.data[0]);
            }
        });*/
    };
    $scope.saveEdit = function () {
        var selectedRows = $scope.gridApi.selection.getSelectedRows();
        var row = selectedRows[0];
        
        if (row) {
            alert('Selected Row: ' + row.Id + '.');
        } else {
            alert('Select a row whose changes you want to save');
            return;
        }
        if (!row.hasBeenEdited) {
            alert("please select a row that has been ediited");
            return;
        }
        var result = ExtPartDataService.save(row, $scope.currentState);
        result.then(function (response) {
            if (response.success) {
                $scope.populateExtPartMappingsTable();
                $scope.editModel.showButton = false;
            } else { //this fires if we don't even reach webapi
                document.write(response.data); //whether it's an entire new document html markup or just a string, we display error.
                //$scope.noCustomerFoundDialog
            }
        });
    };

    $scope.dummyServiceFunction = function (state) { //either one or two for dev or prod
        switch (state) {
            case 1:
                return [{firstName: "in develeopment!!!"}]
                break;
            case 2:
                return [{ firstName: "in production omg!!!" }]
                break;
        }
    }

    //$scope.populateExtPartMappingsTable();
};

ViewAllExtPartDataController.$inject = ['$scope',  'uiGridConstants', 'ExtPartDataService'];