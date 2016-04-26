(function () {
    "use strict";

    angular.module(APPNAME)
         .factory('$dailyMessagesService', DailyMessagesServiceFactory);

    DailyMessagesServiceFactory.$inject = ['$baseService', '$mogome'];

    function DailyMessagesServiceFactory($baseService, $mogome) {

        var aMoGoMeServiceObject = mogome.services.dailyMessages;

        var newService = $baseService.merge(true, {}, aMoGoMeServiceObject, $baseService);

        return newService;
    }

})();