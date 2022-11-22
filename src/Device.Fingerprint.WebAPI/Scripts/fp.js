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
                fp.log(portalAPIUrl, "Load Script", err.message + '|' + sessionKey);
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
    },
    checkDomReady: function () {

        console.log(3);
        var regex = new RegExp("loaded|complete|interactive");

        if (regex.test(document.readyState)) {
            fp.requireJS(cdnAPIUrl + '/scr/cdn/crt').then(() => {
                fp.requireJS(cdnAPIUrl + '/scr/cdn/ce').then(() => {
                    return fpCookie.storageHandler("gfp");
                }).then(res => {
                    var data = {
                        'Cookie': res,
                        'SessionKey': sessionKey,
                        'SubscriberId': subscriberId
                    };
                    crypt.encryptingWithKey(data).then(res => {
                        fp.post(portalAPIUrl + '/ce', res);
                    });

                });

                fp.requireJS(cdnAPIUrl + '/scr/cdn/hl').then(() => {
                    return fpHeadless.getHeadlessScore();
                }).then(res => {
                    var data = {
                        'HeadlessBrowser': {
                            "Score": res.Score,
                            "TestsRun": res.testsRun,
                            "TestsDetail": res.testsDetail
                        },
                        'SessionKey': sessionKey,
                        'SubscriberId': subscriberId
                    };

                    crypt.encryptingWithKey(data).then(res => {
                        fp.post(portalAPIUrl + '/hl', res);
                    });                    
                });

                fp.requireJS(cdnAPIUrl + '/scr/cdn/bi').then(() => {
                    return fpBrowserDetect.getBrowserInfo();
                }).then(res => {
                    var data = {
                        'BrowserDetect': {
                        "OSName": res.OSName,
                            "BrowserName": res.BrowserName,
                            "Platform": res.Platform,
                            "Vendor": res.Vendor,
                            "IsMobileDevice": res.IsMobileDevice
                        },
                        'SessionKey': sessionKey,
                        'SubscriberId': subscriberId
                    };

                    crypt.encryptingWithKey(data).then(res => {
                        fp.post(portalAPIUrl + '/bi', res);
                    });                    
                });

                fp.requireJS(cdnAPIUrl + '/scr/cdn/fp').then(() => {
                    return globalFP.getFP();
                }).then(res => {
                    var data = {
                        'fingerprint': {
                            "Id": res.Id,
                            "Attributes": res.Properties
                        },
                        'SessionKey': sessionKey,
                        'SubscriberId': subscriberId
                    };

                    crypt.encryptingWithKey(data).then(res => {
                        fp.post(portalAPIUrl + '/fp', res);
                    });                                        
                });
            });

            var gfpInput = document.getElementById("__fdi");
            if (gfpInput === null) {
                var container = document.forms[0] || document.body;
                gfpInput = document.createElement("INPUT");
                gfpInput.id = gfpInput.name = "__fdi";
                gfpInput.setAttribute("type", "hidden");
                gfpInput.setAttribute("value", sessionKey)
                container.appendChild(gfpInput);
            }
        }
    }
}

try {
    console.log(1);
    if (document.addEventListener) {
        console.log(2);
        document.addEventListener("DOMContentLoaded", e => {
            fp.checkDomReady();
        }, false);
    }
    else {
        console.log(3);
        fp.checkDomReady();
    }
} catch (exception) {

}
