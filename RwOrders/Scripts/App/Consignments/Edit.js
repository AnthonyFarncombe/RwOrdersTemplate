$(function () {
    $(".bootstrap-datepicker").datepicker({
        format: "dd/mm/yyyy",
        calendarWeeks: true,
        autoclose: true,
        todayHighlight: true
    });
});

var app = angular.module("app", []);

app.controller("ConsignmentController", ["$scope", "$http", "dataService", function ($scope, $http, dataService) {
    $scope.items = dataService.items;

    $scope.addNew = function () {
        var item = {
            ID: 0,
            ProductID: null,
            Description: "",
            Quantity: 0,
            UnitPrice: 0
        };
        $scope.items.push(item);
    };

    $scope.addUnique = function () {

    };

    $scope.removeItem = function (item) {
        var index = $scope.items.indexOf(item);
        $scope.items.splice(index, 1);
    };
}]);

app.config(["$httpProvider", function ($httpProvider) {
    // initialize get if not there
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }

    // disable IE ajax request caching
    $httpProvider.defaults.headers.get["If-Modified-Since"] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    // extra
    $httpProvider.defaults.headers.get["Cache-Control"] = "no-cache";
    $httpProvider.defaults.headers.get["Pragma"] = "no-cache";
}]);