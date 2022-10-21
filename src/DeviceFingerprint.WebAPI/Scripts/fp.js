var fp = {
    loadJS: function loadJS(_url, resolve, reject) {
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
    }
}
var apiUrl = '__apiUrl__';
var subscriberId = '__subscriberId__';
var sessionKey = '__sessionKey__';
var cdnAPIUrl = '__cdnAPIUrl__';

try {
    fp.url(cdnAPIUrl)
        .then(() => {
            fp.post(apiUrl + '/ctl', {
                'SessionKey': sessionKey,
                'SubscriberId': subscriberId,
                'Url': window.location && window.location.href
                    ? window.location.href
                    : "",
                'UserAgent': navigator.userAgent,
                'Information': 'Load script successful'
            });
        })
        .then(() => {
            globalFP.capture(apiUrl, subscriberId, sessionKey);
        })
        .then(() => {
            fp.post(apiUrl + '/ctl', {
                'SessionKey': sessionKey,
                'SubscriberId': subscriberId,
                'Url': window.location && window.location.href
                    ? window.location.href
                    : "",
                'UserAgent': navigator.userAgent,
                'Information': 'Capture data successful'
            });
        })
        .catch((error) => {
            fp.log(apiUrl, "Capture Data", error.message + '|' + sessionKey);
        });
}
catch (err) {
    fp.log(apiUrl, "Load API", err.message);
}




