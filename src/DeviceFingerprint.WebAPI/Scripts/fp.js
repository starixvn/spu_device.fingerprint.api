var fp = {

    requireJs: function (_url, callback) {
        var script = document.createElement("script");
        script.src = _url;
        script.async = true;
        script.defer = true;
        // IE < 7, does not support onload
        if (callback) {
            script.onreadystatechange = function () {
                if (script.readyState === "loaded" || script.readyState === "complete") {
                    // no need to be notified again
                    script.onreadystatechange = null;
                    // notify user
                    callback();
                }
            };

            // other browsers
            script.onload = function () {
                callback();
            };
        }

        // append and execute script
        document.documentElement.firstChild.appendChild(script);
    },
    loadJS: function loadJS(_url) {
        return new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.type = 'text/javascript';
            script.src = _url;
            script.async = true;
            script.defer = true;
            script.onerror = function (err) {
                reject(err, script);
            };
            script.onload = script.onreadystatechange = function () {
                if (!this.readyState || this.readyState == 'complete') {

                    script.onreadystatechange = null;

                    resolve();
                }
            };
            document.head.appendChild(script);
        });
    },
    url: function urls(_url) {
        return new Promise(function (resolve, reject) {
            fp.loadJS(_url, resolve, reject);
        });
    },
    post: function postData(url, data) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
        });
    },
    log: function LogData(url, functionName, exception) {
        var data = {
            function: functionName,
            exception: exception,
        };
        fp.post(url + "/cle", data);
    },

}
var apiUrl = '__apiUrl__';
var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var cdnAPIUrl = '__cdnAPIUrl__';

try {


    //fp.requireJs(cdnAPIUrl, () => {
    //    globalFP
    //        .capture(apiUrl, subscriberId, sessionKey)
    //        .catch((error) => {
    //        fp.log(apiUrl, "Capture Data", error.code + '|' + error.name + '|' + error.stack + '|' + error.message + '|' + sessionKey);
    //    });
    //});

    fp.loadJS(cdnAPIUrl)
        .then(() => {
            Promise.resolve(fp.post(apiUrl + '/ctl', {
                'SessionKey': sessionKey,
                'SubscriberId': subscriberId,
                'Url': window.location && window.location.href
                    ? window.location.href
                    : "",
                'UserAgent': navigator.userAgent,
                'Information': 'Load script successful'
            }));

            try {
                globalFP.capture(apiUrl, subscriberId, sessionKey);
            }
            catch (err) {
                fp.log(apiUrl, "Capture Data", err.code + '|' + err.name + '|' + err.stack + '|' + err.message + '|' + sessionKey);
            }
            

            Promise.resolve(fp.post(apiUrl + '/ctl', {
                'SessionKey': sessionKey,
                'SubscriberId': subscriberId,
                'Url': window.location && window.location.href
                    ? window.location.href
                    : "",
                'UserAgent': navigator.userAgent,
                'Information': 'Capture data successful'
            }));
        })
        .catch((error) => {
            fp.log(apiUrl, "Load Script", error.code + '|' + error.name + '|' + error.stack + '|' + error.message + '|' + sessionKey);
        });
}
catch (err) {
    fp.log(apiUrl, "Load API", err.message);
}




