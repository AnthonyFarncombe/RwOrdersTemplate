var app = angular.module("app", []);

app.controller("ConsignmentController", ["$scope", "$http", function ($scope, $http) {
    $scope.items = [];
    $scope.currentItem = {};

    $(function () {
        $http.get("/Consignments/GetItems/" + consignmentId).then(function (response) {
            $scope.items = response.data;
        }).catch(function (response) {
            alert("Error code " + response.status + " encountered");
        });
    });

    $scope.submitBarcode = function () {
        if (!$("#barcode").val()) {
            $("#barcode").select();
            return;
        }
        $http.get("/Consignments/AddProduct/" + consignmentId + "?stockCode=" + $("#barcode").val()).then(function (response) {
            $scope.items = response.data;
            $("#barcode").val("");
            $("#barcode").select();
        }).catch(function (response) {
            $("#barcode").val("ERROR");
            $("#barcode").select();
        });
    };

    $scope.editItem = function (item) {
        $scope.currentItem = item;
        $("#editDescription").val(item.Description);
        $("#editQuantity").val(item.Quantity);
        $("#editUnitPrice").val(item.UnitPrice);
        $("#editItem").modal("show");
    };

    $scope.editItemConfirm = function () {
        var description = $("#editDescription").val();
        var quantity = $("#editQuantity").val();
        var unitPrice = $("#editUnitPrice").val();
        if (!description) {
            alert("Description is blank");
            return;
        }
        if (!isNumeric(quantity)) {
            alert("Enter quantity");
            return;
        }
        if (!isNumeric(unitPrice)) {
            alert("Enter price");
            return;
        }
        $http.post("/Consignments/SaveItem", {
            id: $scope.currentItem.ID,
            description: description,
            quantity: quantity,
            unitPrice: unitPrice
        }).then(function (response) {
            $scope.items = response.data;
            $("#editItem").modal("hide");
        }).catch(function (response) {
            alert("Error saving item");
        });
    };

    $scope.deleteItem = function (item) {
        $scope.currentItem = item;
        $("#deleteItem").modal("show");
    };

    $scope.deleteItemConfirm = function () {
        $http.post("/Consignments/DeleteItem", {
            id: $scope.currentItem.ID
        }).then(function (response) {
            $scope.items = response.data;
            $("#deleteItem").modal("hide");
        }).catch(function (response) {
            alert("Error deleting item");
        });
    };

    $scope.getTotal = function () {
        var total = 0;
        for (var i = 0; i < $scope.items.length; i++) {
            total += $scope.items[i].Quantity * $scope.items[i].UnitPrice;
        }
        return total;
    };
}]);

app.directive('capitalize', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, modelCtrl) {
            var capitalize = function (inputValue) {
                if (inputValue === undefined) inputValue = '';
                var capitalized = inputValue.toUpperCase();
                if (capitalized !== inputValue) {
                    modelCtrl.$setViewValue(capitalized);
                    modelCtrl.$render();
                }
                return capitalized;
            };
            modelCtrl.$parsers.push(capitalize);
            capitalize(scope[attrs.ngModel]);  // capitalize initial value
        }
    };
});

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

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}