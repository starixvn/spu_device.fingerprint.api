var apiUrl = '__apiUrl__';
var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var cdnAPIUrl = '__cdnAPIUrl__';
var portalAPIUrl = '__portalAPIUrl__';

var fp = {
    requireJS: function (_url) {
        return new Promise(function (resolve, reject) {
            try {
                var script = document.createElement('script');
                script.type = 'text/javascript';
                script.src = _url;
                script.async = true;
                script.defer = true;
                script.addEventListener('load', function () {
                    return resolve(script);
                }, false);
                script.addEventListener('error', function () {
                    return reject(script);
                }, false);

                document.head.appendChild(script);
            }
            catch (err) {
                fp.log(apiUrl, "Load Script", err.message + '|' + sessionKey);
            }
        });
    },
    post: function (url, data) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
        });
    },
    log: function (url, functionName, exception) {
        var data = {
            function: functionName,
            exception: exception,
        };
        fp.post(url + "/cle", data);
    }
}

fp.requireJS(cdnAPIUrl + '/scr/cdn/cookie').then(() => {
    return fpCookie.storageHandler("gfp");
}).then(res => {
    var data = {
        'Cookie': res,
        'RedisKey': sessionKey,
        'SubscriberId': subscriberId
    };

    fp.post(portalAPIUrl + '/cookie', data);
});

fp.requireJS(cdnAPIUrl + '/scr/cdn/headless').then(() => {
    return fpHeadless.getHeadlessScore();
}).then(res => {
    var data = {
        'HeadlessBrowser': {
            "Score": res.Score,
            "TestsRun": res.testsRun,
            "TestsDetail": res.testsDetail
        },
        'RedisKey': sessionKey,
        'SubscriberId': subscriberId
    };

    fp.post(portalAPIUrl + '/headless', data);
});

fp.requireJS(cdnAPIUrl + '/scr/cdn/browserinfo').then(() => {
    return fpBrowserDetect.getBrowserInfo();
}).then(res => {
    var data = {
        'BrowserDetect': {
            "OsName": res.OsName,
            "BrowserName": res.BrowserName,
            "Platform": res.Platform,
            "Vendor": res.Vendor,
            "IsMobileDevice": res.IsMobileDevice
        },
        'RedisKey': sessionKey,
        'SubscriberId': subscriberId
    };

    fp.post(portalAPIUrl + '/browserinfo', data);
});

fp.requireJS(cdnAPIUrl + '/scr/cdn/fingerprint').then(() => {
    return globalFP.getFP();
}).then(res => {
    var data = {
        'fingerprint': {
            "Id": res.Id,
            "Attributes": res.Properties
        },
        'RedisKey': sessionKey,
        'SubscriberId': subscriberId
    };

    fp.post(portalAPIUrl + '/fingerprint', data);
});

var gfpInput = document.getElementById("__gfpValidation");
if (gfpInput === null) {
    var container = document.forms[0] || document.body;
    gfpInput = document.createElement("INPUT");
    gfpInput.id = gfpInput.name = "__gfpValidation";
    gfpInput.setAttribute("type", "hidden");
    gfpInput.setAttribute("value", sessionKey + ':' + subscriberId)
    container.appendChild(gfpInput);
}



