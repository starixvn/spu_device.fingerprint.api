/* global Detecas */
/*jshint bitwise: false */

Detecas.Rc4 = (function () {
    "use strict";

    // refer: https://gist.github.com/farhadi/2185197
    function encrypt(key, str) {
        var s = [], j = 0, x, res = "";
        var i;
        for (i = 0; i < 256; i++) {
            s[i] = i;
        }
        for (i = 0; i < 256; i++) {
            j = (j + s[i] + key.charCodeAt(i % key.length)) % 256;
            x = s[i];
            s[i] = s[j];
            s[j] = x;
        }
        i = 0;
        j = 0;
        for (var y = 0; y < str.length; y++) {
            i = (i + 1) % 256;
            j = (j + s[i]) % 256;
            x = s[i];
            s[i] = s[j];
            s[j] = x;
            res += String.fromCharCode(str.charCodeAt(y) ^ s[(s[i] + s[j]) % 256]);
        }

        return res;
    }

    function decrypt(key, str) {
        return encrypt(key, str);
    }

    return {
        encrypt: encrypt,
        decrypt: decrypt
    };
})();