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

        document.body.appendChild(script);
    },
    url: function urls(_url) {
        return new Promise(function (resolve, reject) {
            fp.loadJS(_url, resolve, reject);
        });
    }
}

fp.url("__cdnAPIUrl__").then(() => {   
    globalFP.capture("__apiUrl__", "__subscriberId__","__sessionKey__");
});

