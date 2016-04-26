(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('adminDailyMessagesController', AdminDailyMessagesController);

    AdminDailyMessagesController.$inject = ['$scope', '$baseController', '$uibModal', '$dailyMessagesService'];

    function AdminDailyMessagesController(
      $scope
      , $baseController
      , $uibModal
      , $dailyMessagesService) {

        var vm = this;
        vm.dailyMessages = null;

        vm.openModal = _openModal;
        vm.onClickToAdd = _onClickToAdd;
        vm.onClickToEdit = _onClickToEdit;
        vm.onClickToDelete = _onClickToDelete;
        vm.onGetAllSuccess = _onGetAllSuccess;
        vm.onGetAllError = _onGetAllError;
        vm.onAddSuccess = _onAddSuccess;
        vm.onAddError = _onAddError;
        vm.onUpdateSuccess = _onUpdateSuccess;
        vm.onUpdateError = _onUpdateError;
        vm.onDeleteSuccess = _onDeleteSuccess;
        vm.onDeleteError = _onDeleteError;

        $baseController.merge(vm, $baseController);

        vm.$scope = $scope;
        vm.$uibModal = $uibModal;
        vm.$dailyMessagesService = $dailyMessagesService;
        vm.notify = vm.$dailyMessagesService.getNotifier($scope);

        render();

        function render() {
            vm.$dailyMessagesService.getAll(vm.onGetAllSuccess, vm.onGetAllError);

        }

        function _openModal(message) {
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: '/Scripts/mogome/core/adminDaily/views/admin.daily.messages.modal.html',
                controller: 'dailyMessagesModalController as mc',
                size: 'md',
                resolve: {
                    item: function () {
                        return message
                    },
                }
            });

            modalInstance.result.then(
                function (dailyMessage) {
                    if (dailyMessage.id) {
                        vm.$dailyMessagesService.update(dailyMessage, vm.onUpdateSuccess, vm.onUpdateError);
                    } else {
                        vm.$dailyMessagesService.add(dailyMessage, vm.onAddSuccess, vm.onAddError);
                    }

                }
            , function () {
                console.log('Modal dismissed at: ' + new Date());
            });
        }

        function _onClickToAdd() {
            var currentMessage = {
                title: null,
                message: null
            }

            vm.openModal(currentMessage);
        }

        function _onClickToEdit(message) {
            var currentMessage = message;

            vm.openModal(currentMessage);
        }

        function _onClickToDelete(message) {
            vm.$dailyMessagesService.delete(message, vm.onDeleteSuccess, vm.onDeleteError);
        }

        function _onGetAllSuccess(data) {
            if (data && data.items) {

                vm.notify(function () {
                    vm.dailyMessages = data.items;
                });
            }
        }

        function _onGetAllError(jqXHR, textStatus, errorThrown) {
            console.log(errorThrown);

            if (errorThrown) {
                vm.$alertService.error(jqXHR.responseJSON);
            }
        }

        function _onAddSuccess(data, addedMessage) {
            if (data) {
                vm.notify(function () {
                    if (!vm.dailyMessages) {
                        vm.dailyMessages = [];
                    }

                    vm.dailyMessages.push(addedMessage);
                });

                console.log('success!');
                vm.$alertService.success('Message added');
            }
        }

        function _onAddError(jqXHR, textStatus, errorThrown) {
            console.log(errorThrown);

            if (errorThrown) {
                vm.$alertService.error(jqXHR.responseJSON);
            }
        }

        function _onUpdateSuccess(data) {
            if (data) {

                console.log('success!');
                vm.$alertService.success('Message updated');
            }
        }

        function _onUpdateError(jqXHR, textStatus, errorThrown) {
            console.log(errorThrown);

            if (errorThrown) {
                vm.$alertService.error(jqXHR.responseJSON);
            }
        }

        function _onDeleteSuccess(data, deletedMessage) {
            if (data) {
                var targetId = deletedMessage.id;

                vm.notify(function () {

                    var arrayLength = vm.dailyMessages.length;

                    for (var i = 0; i < arrayLength; i++) {
                        var currentMessage = vm.dailyMessages[i];

                        if (currentMessage.id == targetId) {
                            vm.dailyMessages.splice(i, 1);
                        }
                    }
                });

                console.log(vm.dailyMessages);
                vm.$alertService.success('Message deleted');
            }
        }

        function onDeleteNotify(targetId) {
            var arrayLength = vm.dailyMessages.length;

            for (var i = 0; i < arrayLength; i++) {
                var currentMessage = vm.dailyMessages[i];

                if (currentMessage.id == targetId) {
                    vm.dailyMessages.splice(i, 0);
                }
            }
        }

        function _onDeleteError(jqXHR, textStatus, errorThrown) {
            console.log(errorThrown);

            if (errorThrown) {
                vm.$alertService.error(jqXHR.responseJSON);
            }
        }

    }

})();