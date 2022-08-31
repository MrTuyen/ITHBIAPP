var PostDataAjax = function (url, data, callBack, timeout) {
    //if (!timeout)
    //    timeout = 60000;
    timeout = 0;
    var tokenString = "";
    var uuid = "";
    var storeID = 0;

    $.ajax({
        url: url,
        type: "POST",
        headers: {
            "Token-String": tokenString,
            "Device-UUID": uuid,
        },

        timeout: timeout,
        cache: true,
        crossDomain: true,
        contentType: "application/json; charset=utf-8;",
        dataType: "json",
        data: data,
        processData: true,
        beforeSend: function () { },
        async: true,
        tryCount: 0,
        retryLimit: 3,

        success: function (response) {
            if (response) {
                setTimeout(function () {
                    callBack(response);
                }, 10);
            } else {
                setTimeout(callBack, 10);
            }
        },

        error: function (error) {
            LoadingHide();
            toastr.error(error.statusText);
        }
    });
};


