var app = angular.module("app", []);

app.controller("OrderController", ["$scope", "$http", "preloadItems", function ($scope, $http, preloadItems) {
    $scope.items = preloadItems;
    $scope.currentItem = {};
    $scope.tempItem = {};

    $scope.barcodeKeys = function (keyEvent) {
        if (keyEvent.which === 13) {
            findBarcode()
        }
    };

    $scope.findBarcode = function () {
        findBarcode()
    };

    function findBarcode() {
        var barcode = $("#barcode").val();
        if (!barcode) {
            $("#barcode").select();
            return;
        }
        for (var i = 0; i < $scope.items.length; i++) {
            if ($scope.items[i].StockCode === barcode) {
                $scope.items[i].Quantity++;
                $("#barcode").val("");
                $("#barcode").select();
                return;
            }
        }
        $http.get("/Orders/FindProduct?stockCode=" + barcode).success(function (response) {
            var item = {
                ID: 0,
                ProductID: response.ID,
                StockCode: response.StockCode,
                Description: response.Description,
                Quantity: 1,
                UnitPrice: response.UnitPrice
            };
            $scope.items.push(item);
            $("#barcode").val("");
            $("#barcode").select();
        }).error(function (error) {
            $("#barcode").select();
            alert("Not found");
        });
    }

    $scope.addItem = function () {
        $("#addItemTr").hide();
        $("#newItemTr").show();
        $("#newItemStockCode").select();
    };

    $scope.saveNewItem = function () {
        var item = {
            ID: 0,
            ProductID: 1,
            StockCode: "ABCD1234",
            Description: "Black Bow",
            Quantity: 1,
            UnitPrice: 6
        };
        $scope.items.push(item);
        $("#newItemTr").hide();
        $("#addItemTr").show();
        $("#barcode").select();
    };

    $scope.editItem = function (item) {
        $scope.currentItem = item;
        $scope.tempItem.Quantity = item.Quantity;
        $scope.tempItem.UnitPrice = item.UnitPrice;
        $("#editItem").modal("show");
    };

    $scope.saveEditedItem = function () {
        $scope.currentItem.Quantity = $scope.tempItem.Quantity;
        $scope.currentItem.UnitPrice = $scope.tempItem.UnitPrice;
        $("#editItem").modal("hide");
    };

    $scope.deleteItem = function (item) {
        for (var i = 0; i < $scope.items.length; i++) {
            if ($scope.items[i].ID === item.ID) {
                $scope.items.splice(i, 1);
                break;
            }
        }
    };

    $scope.orderTotal = function () {
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
                if (inputValue == undefined) inputValue = '';
                var capitalized = inputValue.toUpperCase();
                if (capitalized !== inputValue) {
                    modelCtrl.$setViewValue(capitalized);
                    modelCtrl.$render();
                }
                return capitalized;
            }
            modelCtrl.$parsers.push(capitalize);
            capitalize(scope[attrs.ngModel]);  // capitalize initial value
        }
    };
});

$(function () {
    $("#barcode").select();

    $("#barcode").keypress(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});