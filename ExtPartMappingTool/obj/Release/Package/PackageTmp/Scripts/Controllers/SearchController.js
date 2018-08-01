var SearchController = function ($scope, uiGridConstants, ExtPartDataService) {
    $scope.searchByCustomerNoHeader = 'Search By part';
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
    $scope.currentState = null;
    $scope.modelCheckBox = {
        byOldPartId: false,
        byNewPartId: false,
        byBoth : false,
        unCheckBoth: function () {
            $scope.modelCheckBox.byOldPartId = false;
            $scope.modelCheckBox.byNewPartId = false;
        },
        checkBoth: function () {
            $scope.modelCheckBox.byOldPartId = true;
            $scope.modelCheckBox.byNewPartId = true;
        },
        selected: function () {
            if ($scope.modelCheckBox.byBoth) {
                $scope.modelCheckBox.byBoth = false;
            } else {
                if($scope.modelCheckBox.byOldPartId &
                $scope.modelCheckBox.byNewPartId) {
                    $scope.modelCheckBox.byBoth = true;
                }
            }
        },
        checkedBoth: function () {
            if ($scope.modelCheckBox.byBoth) {
                $scope.modelCheckBox.checkBoth();
            } else {
                $scope.modelCheckBox.unCheckBoth();
            }
        }
    };
    $scope.searchAvailable = function () {
        if ($scope.modelCheckBox.byOldPartId || $scope.modelCheckBox.byNewPartId) {
            return true;
        }
        else return false;
    };
    $scope.searchModel = {
        oldPartId: null,
        newPartId: null
    };
    $scope.search = function () {
        if (!$scope.searchAvailable()) {
            alert("Use checkboxes to determine search criteria.");
            return;
        }
        $scope.editModel.showButton = false;
        var result = ExtPartDataService.search(
            $scope.modelCheckBox.byOldPartId === true? ($scope.searchModel.oldPartId == null? '': $scope.searchModel.oldPartId) : '',
            $scope.modelCheckBox.byNewPartId === true? ($scope.searchModel.newPartId ==null? '': $scope.searchModel.newPartId ): '',
            $scope.currentState
            );
        result.then(function (response) {
            if (response.success) {
                var data = response.data;
                var editable = [false, true, true];
                var columnDefs = [];
                var target = data[0];
                var col = 0;
                for (var k in target) {
                        if (target.hasOwnProperty(k)) {
                            if (String(k).length > 2) {
                                columnDefs.push({
                                    field: String(k), displayName: k, enableCellEdit: editable[col]
                                });
                            } else {
                                columnDefs.push({ field: String(k), displayName: k, cellClass: null });
                            }
                        }
                    col++;
                }
                for (var i = 0; i < data.length; i++) { //adding hasbeenEdited value for purpose of saving and tracking changes on ui
                    data[i].hasBeenEdited = false;
                }
                $scope.gridOptions.data = data;
                $scope.gridOptions.columnDefs = columnDefs;

                $scope.height.newHeight(data.length);

                //for div hide or appear stuff
                $scope.gridFilled = true;
                $scope.noCustomerFound = false;
                //$scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
            }
            else { //this fires if we don't even reach webapi or we throw an exception in server
                if (response.data == null) {
                    alert(response.message);
                } else
                    document.write(response.data);
            }
        }, function () { alert("don't think we'll ever get here") })
    };
    $scope.save = function () {
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
                var data = response.data;
                var editable = [false, true, true];
                var columnDefs = [];
                var target = data;
                var curr = 0;
                for (var k in target) {
                    if (target.hasOwnProperty(k)) {
                        columnDefs.push({ field: String(k), displayName: k, enableCellEdit: editable[curr] });
                    }
                    curr++;;
                }
                for (var i = 0; i < data.length; i++) { //adding hasbeenEdited value for purpose of saving and tracking changes on ui
                    data[i].hasBeenEdited = false;
                }
                //$scope.gridOptions.data = [data];
                $scope.gridOptions.columnDefs = columnDefs;

                //for div hide or appear stuff
                $scope.gridFilled = true;
                $scope.editModel.showButton = false;

                $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
            } else { //this fires if we don't even reach webapi
                document.write(response.data); //whether it's an entire new document html markup or just a string, we display error.
                //$scope.noCustomerFoundDialog
            }
        });
    };
    $scope.delete = function () {
        //$scope.gridOptions.data = vm.resultSimulatedData;
        var selectedRow = $scope.gridApi.selection.getSelectedRows(); //<--Property undefined error here
        if (selectedRow[0]) {
            var result = ExtPartDataService.delete(selectedRow[0], $scope.currentState);
            result.then(function (response) {
                if (response.success) {
                    $scope.gridOptions.data = [];
                    $scope.gridFilled = false;
                    $scope.editModel.showButton = false;
                } else { //this fires if we don't even reach webapi
                    alert(response.message);
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
    $scope.newEntryModel = {
        customerNumber: '',
        respresentativeId: null,
        commission: null
    }
    $scope.createNewCustomerCommissionEntry = function () {
        $scope.getReps();
        $scope.toggleModal();
    };
    $scope.gridFilled = false;
    $scope.gridOptions = {
        enableRowSelection: true,
        enableRowHeaderSelection: false,
        multiSelect: false,
        data: [{}] // 'washCoatData'
        , columnDefs: []
    };
    $scope.activeCustomers = null;
   
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
    $scope.editModel = {
        showButton: false,
        hasChanged: false
    };
    $scope.modalShown = false;
    $scope.toggleModal = function () {
        $scope.modalShown = !$scope.modalShown;
    };
    
}
SearchController.$inject = ['$scope' , 'uiGridConstants', 'ExtPartDataService'];