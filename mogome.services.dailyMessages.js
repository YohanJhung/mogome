mogome.services.dailyMessages = mogome.services.dailyMessages || {};

mogome.services.dailyMessages.add = function (message, onSuccess, onError) {

    var url = "/api/admin/daily/message";
    var settings = {
        cache: false
    , contentType: "application/x-www-form-urlencoded; charset=UTF-8"
    , data: message
    , dataType: "json"
    , success: function (response) {
        if (response && response.item) {
            message.id = response.item;
        }
        onSuccess(response, message);
    }
        , error: onError
        , type: "POST"
    
    }
    $.ajax(url, settings);
}

mogome.services.dailyMessages.getAll = function (onSuccess, onError) {

    var url = "/api/admin/daily/messages";
    var settings = {
        cache: false
        , contentType: "application/x-www-form-urlencoded; charset=UTF-8"
        , dataType: "json"
        , success: onSuccess
        , error: onError
        , type: "GET"
    };
    $.ajax(url, settings);
}

mogome.services.dailyMessages.update = function (message, onSuccess, onError) {
    var url = "/api/admin/daily/message/";

    var settings = {
        cache: false
        , contentType: "application/x-www-form-urlencoded; charset=UTF-8"
        , data: message
        , dataType: "json"
        , success: onSuccess
        , error: onError
        , type: "PUT"
    };

    $.ajax(url, settings);
}

mogome.services.dailyMessages.delete = function (message, onSuccess, onError) {

    var url = "/api/admin/daily/message/" + message.id;
    var settings = {
        cache: false
        , contentType: "application/x-www-form-urlencoded; charset=UTF-8"
        , dataType: "json"
        , success: function (response) {
            onSuccess(response, message)
        }
        , error: onError
        , type: "DELETE"
    };

    $.ajax(url, settings);
}