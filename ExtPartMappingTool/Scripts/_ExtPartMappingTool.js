var ExtPartMappingTool = angular.module('ExtPartMappingTool', ['ngRoute', 'ui.router', 'ui.grid', 'ngAnimate', 'ui.grid.edit', 'ui.bootstrap', 'ui.grid.selection', 'angularFileUpload']);


//Controllers
ExtPartMappingTool.controller('LandingPageController', LandingPageController);
ExtPartMappingTool.controller('NestedViewsController', NestedViewsController);
ExtPartMappingTool.controller('ViewAllExtPartDataController', ViewAllExtPartDataController);
ExtPartMappingTool.controller('SearchController', SearchController); 
ExtPartMappingTool.controller('AddRecordController', AddRecordController);
ExtPartMappingTool.controller('UploadController', UploadController);
ExtPartMappingTool.controller('LoginController', LoginController);

//Services
ExtPartMappingTool.service('ExtPartDataService', ExtPartDataService);
ExtPartMappingTool.service('UploadFileService', UploadFileService);

//Factories
ExtPartMappingTool.factory('LoginFactory', LoginFactory);
ExtPartMappingTool.factory('LogOffFactory', LogOffFactory);
ExtPartMappingTool.factory('AuthHttpRequestInterceptor', AuthHttpRequestInterceptor);

//Directives
ExtPartMappingTool.directive('modalDialog', ModalDialogDirective);

var configFunction = function ($routeProvider, $stateProvider, $httpProvider) {

    $stateProvider.
        state('/searchCustomerNo', {
            templateUrl: '/Commission/SearchByCustomerNo'
        });
    $routeProvider.
        when('/DevelopmentView', {
            templateUrl: '/NestedViews/MainView'
        })
        .when('/ProductionView', {
            templateUrl: '/NestedViews/MainView'
        });
    $httpProvider.interceptors.push('AuthHttpRequestInterceptor');
}
configFunction.$inject = ['$routeProvider','$stateProvider', '$httpProvider'];
ExtPartMappingTool.config(configFunction);