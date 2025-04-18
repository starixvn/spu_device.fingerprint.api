// ---- /Detecas.Base64.js ----
/* global GfpPickle */
/*jshint bitwise: false */
/*jslint eqeq: true*/


var fpsStartTime = new Date();
var fpsTimeElapsed = 0;
var fpsIsStop = false;
var fpsMaxTime = 4000; // (total = 5s) use remaining 1s  for GfpPickle.Browser, GfpPickle.Device and GfpPickle.Core
var fpsIsMob = /iPhone|iPad|iPod|Android|BlackBerry|IEMobile/i.test(navigator.userAgent);

function fpsChkIntr(item, maxtime) {

    if (!fpsIsMob) {
        return false;
    }

    if (fpsIsStop == true) {
        return true;
    }

    if (maxtime === null) {
        maxtime = fpsMaxTime;
    }

    fpsTimeElapsed = new Date() - fpsStartTime;

    if (fpsTimeElapsed >= maxtime) {
        fpsIsStop = true;

        diInput = document.getElementById("__di");

        // create hidden input __di
        var container = document.forms[0] || document.body;
        diInput = document.createElement("INPUT");
        diInput.id = diInput.name = "__di";
        diInput.setAttribute("type", "hidden");
        container.appendChild(diInput);

        return true;
    }
    return false;
}

GfpPickle.Base64 = (function () {
    "use strict";

    var Base = (function () {
        function F() { }

        return {
            extend: function (overrides) {
                // Spawn
                F.prototype = this;
                var subtype = new F();

                // Augment
                if (overrides) {
                    subtype.mixIn(overrides);
                }

                // Create default initializer
                if (!subtype.hasOwnProperty('init')) {
                    subtype.init = function () {
                        subtype.$super.init.apply(this, arguments);
                    };
                }

                // Initializer's prototype is the subtype object
                subtype.init.prototype = subtype;

                // Reference supertype
                subtype.$super = this;

                return subtype;
            },
            create: function () {
                var instance = this.extend();
                instance.init.apply(instance, arguments);

                return instance;
            },
            init: function () {
            },
            mixIn: function (properties) {
                for (var propertyName in properties) {
                    if (properties.hasOwnProperty(propertyName)) {
                        this[propertyName] = properties[propertyName];
                    }
                }

                // IE won't copy toString using the loop above
                if (properties.hasOwnProperty('toString')) {
                    this.toString = properties.toString;
                }
            },
            clone: function () {
                return this.init.prototype.extend(this);
            }
        };
    }());

    var WordArray = Base.extend({
        init: function (words, sigBytes) {
            words = this.words = words || [];

            if (sigBytes !== undefined) {
                this.sigBytes = sigBytes;
            } else {
                this.sigBytes = words.length * 4;
            }
        },
        toString: function (encoder) {
            return (encoder || Utf8).stringify(this);
        },
        concat: function (wordArray) {
            // Shortcuts
            var thisWords = this.words;
            var thatWords = wordArray.words;
            var thisSigBytes = this.sigBytes;
            var thatSigBytes = wordArray.sigBytes;

            // Clamp excess bits
            this.clamp();

            // Concat
            var i;
            if (thisSigBytes % 4) {
                // Copy one byte at a time
                for (i = 0; i < thatSigBytes; i++) {
                    var thatByte = (thatWords[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
                    thisWords[(thisSigBytes + i) >>> 2] |= thatByte << (24 - ((thisSigBytes + i) % 4) * 8);
                }
            } else if (thatWords.length > 0xffff) {
                // Copy one word at a time
                for (i = 0; i < thatSigBytes; i += 4) {
                    thisWords[(thisSigBytes + i) >>> 2] = thatWords[i >>> 2];
                }
            } else {
                // Copy all words at once
                thisWords.push.apply(thisWords, thatWords);
            }
            this.sigBytes += thatSigBytes;

            // Chainable
            return this;
        },
        clamp: function () {
            // Shortcuts
            var words = this.words;
            var sigBytes = this.sigBytes;

            // Clamp
            words[sigBytes >>> 2] &= 0xffffffff << (32 - (sigBytes % 4) * 8);
            words.length = Math.ceil(sigBytes / 4);
        },
        clone: function () {
            var clone = Base.clone.call(this);
            clone.words = this.words.slice(0);

            return clone;
        },
        random: function (nBytes) {
            var words = [];
            for (var i = 0; i < nBytes; i += 4) {
                words.push((Math.random() * 0x100000000) | 0);
            }

            return new WordArray.init(words, nBytes);
        }
    });

    var Latin1 = {
        stringify: function (wordArray) {
            // Shortcuts
            var words = wordArray.words;
            var sigBytes = wordArray.sigBytes;

            // Convert
            var latin1Chars = [];
            for (var i = 0; i < sigBytes; i++) {
                var bite = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
                latin1Chars.push(String.fromCharCode(bite));
            }

            return latin1Chars.join('');
        },
        parse: function (latin1Str) {
            // Shortcut
            var latin1StrLength = latin1Str.length;

            // Convert
            var words = [];
            for (var i = 0; i < latin1StrLength; i++) {
                words[i >>> 2] |= (latin1Str.charCodeAt(i) & 0xff) << (24 - (i % 4) * 8);
            }

            return new WordArray.init(words, latin1StrLength);
        }
    };

    var Utf8 = {
        stringify: function (wordArray) {
            try {
                return decodeURIComponent(escape(Latin1.stringify(wordArray)));
            } catch (e) {
                throw new Error('Malformed UTF-8 data');
            }
        },
        parse: function (utf8Str) {
            return Latin1.parse(unescape(encodeURIComponent(utf8Str)));
        }
    };

    // refer: https://code.google.com/p/crypto-js/source/browse/tags/3.1.2/src/enc-base64.js
    var _map = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';

    function encode(input) {
        var wordArray = Utf8.parse(input);

        // Shortcuts
        var words = wordArray.words;
        var sigBytes = wordArray.sigBytes;
        var map = _map;

        // Clamp excess bits
        wordArray.clamp();

        // Convert
        var base64Chars = [];
        for (var i = 0; i < sigBytes; i += 3) {
            var byte1 = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
            var byte2 = (words[(i + 1) >>> 2] >>> (24 - ((i + 1) % 4) * 8)) & 0xff;
            var byte3 = (words[(i + 2) >>> 2] >>> (24 - ((i + 2) % 4) * 8)) & 0xff;

            var triplet = (byte1 << 16) | (byte2 << 8) | byte3;

            for (var j = 0; (j < 4) && (i + j * 0.75 < sigBytes); j++) {
                base64Chars.push(map.charAt((triplet >>> (6 * (3 - j))) & 0x3f));
            }
        }

        // Add padding
        var paddingChar = map.charAt(64);
        if (paddingChar) {
            while (base64Chars.length % 4) {
                base64Chars.push(paddingChar);
            }
        }

        return base64Chars.join('');
    }

    function decode(base64Str) {
        // Shortcuts
        var base64StrLength = base64Str.length;
        var map = _map;

        // Ignore padding
        var paddingChar = map.charAt(64);
        if (paddingChar) {
            var paddingIndex = base64Str.indexOf(paddingChar);
            if (paddingIndex !== -1) {
                base64StrLength = paddingIndex;
            }
        }

        // Convert
        var words = [];
        var nBytes = 0;
        for (var i = 0; i < base64StrLength; i++) {
            if (i % 4) {
                var bits1 = map.indexOf(base64Str.charAt(i - 1)) << ((i % 4) * 2);
                var bits2 = map.indexOf(base64Str.charAt(i)) >>> (6 - (i % 4) * 2);
                words[nBytes >>> 2] |= (bits1 | bits2) << (24 - (nBytes % 4) * 8);
                nBytes++;
            }
        }

        var wordArray = WordArray.create(words, nBytes);
        return wordArray.toString(Utf8);
    }

    return {
        encode: encode,
        decode: decode
    };
})();

// ---- /Detecas.Extensions.js ----
/* jshint ignore:start */

(function () {
    "use strict";

    // refer: http://stackoverflow.com/questions/3629183/why-doesnt-indexof-work-on-an-array-ie8
    if (!Array.prototype.indexOf) {
        Array.prototype.indexOf = function (elt /*, from*/) {
            var len = this.length >>> 0;

            var from = Number(arguments[1]) || 0;
            from = (from < 0)
                ? Math.ceil(from)
                : Math.floor(from);
            if (from < 0)
                from += len;

            for (; from < len; from++) {
                if (from in this && this[from] === elt)
                    return from;
            }

            return -1;
        };
    }

    // refer: http://stackoverflow.com/questions/16186930/jquery-foreach-not-working-in-ie8
    if (typeof Array.prototype.forEach != 'function') {
        Array.prototype.forEach = function (callback) {
            for (var i = 0; i < this.length; i++) {
                callback.apply(this, [this[i], i, this]);
            }
        };
    }

    // refer: http://stackoverflow.com/questions/7153470/why-wont-filter-work-in-interent-explorer-8
    if (!Array.prototype.filter) {
        Array.prototype.filter = function (fun /*, thisp */) {
            "use strict";

            if (this === void 0 || this === null)
                throw new TypeError();

            var t = Object(this);
            var len = t.length >>> 0;
            if (typeof fun !== "function")
                throw new TypeError();

            var res = [];
            var thisp = arguments[1];
            for (var i = 0; i < len; i++) {
                if (i in t) {
                    var val = t[i]; // in case fun mutates this
                    if (fun.call(thisp, val, i, t))
                        res.push(val);
                }
            }

            return res;
        };
    }

    // refer: http://stackoverflow.com/questions/2308134/trim-in-javascript-not-working-in-ie
    if (typeof String.prototype.trim !== 'function') {
        String.prototype.trim = function () {
            return this.replace(/^\s+|\s+$/g, '');
        }
    }
})();

// ---- /Detecas.Hex.js ----
/* global GfpPickle */

GfpPickle.Hex = (function () {
    "use strict";

    // refer: http://stackoverflow.com/questions/21647928/javascript-unicode-string-to-hex
    function encode(str) {
        var hex = "";
        for (var i = 0; i < str.length; i++) {
            hex += "" + str.charCodeAt(i).toString(16);
        }
        return hex;
    }

    function decode(hex) {
        var str = "";
        for (var i = 0; i < hex.length; i += 2) {
            str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
        }
        return str;
    }

    return {
        encode: encode,
        decode: decode
    };
})();

// ---- /Detecas.JSON.js ----
/* global GfpPickle */

GfpPickle.JSON = (function (window) {
    "use strict";

    // JSON2: https://github.com/douglascrockford/JSON-js/blob/master/json2.js
    // source: https://cdnjs.cloudflare.com/ajax/libs/json2/20150503/json2.min.js
    /* jshint ignore:start */
    "object" != typeof JSON && (JSON = {}), function () { "use strict"; function f(t) { return 10 > t ? "0" + t : t } function this_value() { return this.valueOf() } function quote(t) { return rx_escapable.lastIndex = 0, rx_escapable.test(t) ? '"' + t.replace(rx_escapable, function (t) { var e = meta[t]; return "string" == typeof e ? e : "\\u" + ("0000" + t.charCodeAt(0).toString(16)).slice(-4) }) + '"' : '"' + t + '"' } function str(t, e) { var r, n, o, u, f, a = gap, i = e[t]; switch (i && "object" == typeof i && "function" == typeof i.toJSON && (i = i.toJSON(t)), "function" == typeof rep && (i = rep.call(e, t, i)), typeof i) { case "string": return quote(i); case "number": return isFinite(i) ? i + "" : "null"; case "boolean": case "null": return i + ""; case "object": if (!i) return "null"; if (gap += indent, f = [], "[object Array]" === Object.prototype.toString.apply(i)) { for (u = i.length, r = 0; u > r; r += 1) f[r] = str(r, i) || "null"; return o = 0 === f.length ? "[]" : gap ? "[\n" + gap + f.join(",\n" + gap) + "\n" + a + "]" : "[" + f.join(",") + "]", gap = a, o } if (rep && "object" == typeof rep) for (u = rep.length, r = 0; u > r; r += 1) "string" == typeof rep[r] && (n = rep[r], o = str(n, i), o && f.push(quote(n) + (gap ? ": " : ":") + o)); else for (n in i) Object.prototype.hasOwnProperty.call(i, n) && (o = str(n, i), o && f.push(quote(n) + (gap ? ": " : ":") + o)); return o = 0 === f.length ? "{}" : gap ? "{\n" + gap + f.join(",\n" + gap) + "\n" + a + "}" : "{" + f.join(",") + "}", gap = a, o } } var rx_one = /^[\],:{}\s]*$/, rx_two = /\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, rx_three = /"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, rx_four = /(?:^|:|,)(?:\s*\[)+/g, rx_escapable = /[\\\"\u0000-\u001f\u007f-\u009f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, rx_dangerous = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g; "function" != typeof Date.prototype.toJSON && (Date.prototype.toJSON = function () { return isFinite(this.valueOf()) ? this.getUTCFullYear() + "-" + f(this.getUTCMonth() + 1) + "-" + f(this.getUTCDate()) + "T" + f(this.getUTCHours()) + ":" + f(this.getUTCMinutes()) + ":" + f(this.getUTCSeconds()) + "Z" : null }, Boolean.prototype.toJSON = this_value, Number.prototype.toJSON = this_value, String.prototype.toJSON = this_value); var gap, indent, meta, rep; "function" != typeof JSON.stringify && (meta = { "\b": "\\b", "	": "\\t", "\n": "\\n", "\f": "\\f", "\r": "\\r", '"': '\\"', "\\": "\\\\" }, JSON.stringify = function (t, e, r) { var n; if (gap = "", indent = "", "number" == typeof r) for (n = 0; r > n; n += 1) indent += " "; else "string" == typeof r && (indent = r); if (rep = e, e && "function" != typeof e && ("object" != typeof e || "number" != typeof e.length)) throw Error("JSON.stringify"); return str("", { "": t }) }), "function" != typeof JSON.parse && (JSON.parse = function (text, reviver) { function walk(t, e) { var r, n, o = t[e]; if (o && "object" == typeof o) for (r in o) Object.prototype.hasOwnProperty.call(o, r) && (n = walk(o, r), void 0 !== n ? o[r] = n : delete o[r]); return reviver.call(t, e, o) } var j; if (text += "", rx_dangerous.lastIndex = 0, rx_dangerous.test(text) && (text = text.replace(rx_dangerous, function (t) { return "\\u" + ("0000" + t.charCodeAt(0).toString(16)).slice(-4) })), rx_one.test(text.replace(rx_two, "@").replace(rx_three, "]").replace(rx_four, ""))) return j = eval("(" + text + ")"), "function" == typeof reviver ? walk({ "": j }, "") : j; throw new SyntaxError("JSON.parse") }) }();
    /* jshint ignore:end */

    var defaultMaxDepth = 6;
    var defaultArrayMaxLength = 50;
    var seen; // Same variable used for all stringifications
    var stripFuncs;

    /*jshint -W121 */
    Date.prototype.toPrunedJSON = Date.prototype.toJSON;
    String.prototype.toPrunedJSON = String.prototype.toJSON;
    /*jshint +W121 */

    var escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
        meta = { // table of character substitutions
            '\b': "\\b",
            '\t': "\\t",
            '\n': "\\n",
            '\f': "\\f",
            '\r': "\\r",
            '"': "\\\"",
            '\\': "\\\\"
        };

    function quote(string) {
        escapable.lastIndex = 0;
        return escapable.test(string) ? "\"" + string.replace(escapable, function (a) {
            var c = meta[a];
            return typeof c === "string" ? c : "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4);
        }) + "\"" : "\"" + string + "\"";
    }

    function str(key, holder, depthDecr, arrayMaxLength) {
        var i, k, v, length, partial, value = holder[key];
        if (value && typeof value === "object" && typeof value.toPrunedJSON === "function") {
            value = value.toPrunedJSON(key);
        }

        switch (typeof value) {
            case "string":
                return quote(value);
            case "number":
                return isFinite(value) ? String(value) : "null";
            case "boolean":
            case "null":
                return String(value);
            case "object":

                if (!value) {
                    return "null";
                }

                if (depthDecr <= 0 || seen.indexOf(value) !== -1) {
                    return "\"{*}\""; // pruned
                }

                seen.push(value);

                partial = [];

                if (Object.prototype.toString.apply(value) === "[object Array]") {
                    length = Math.min(value.length, arrayMaxLength);
                    for (i = 0; i < length; i += 1) {
                        partial[i] = str(i, value, depthDecr - 1, arrayMaxLength) || "null";
                    }
                    return partial.length ? "[" + partial.join(",") + "]" : "[]";
                }

                // Sort key first
                var keys = [];

                for (k in value) {
                    // Key must be a valid name
                    try {
                        if (value[k] && k.match(/^[a-zA-Z0-9]+$/)) {
                            keys.push(k);
                        }
                    } catch (e) { }
                }

                keys.sort();

                var c = keys.length;
                for (i = 0; i < c; i++) {
                    k = keys[i];
                    try {
                        v = str(k, value, depthDecr - 1, arrayMaxLength);
                        if (typeof v === "string") {
                            v = v.replace(/[^\u0000-\u007F]/g, ""); // Remove non-ascii characters
                            if (typeof stripFuncs[k] === "function") { // There is a inject function to remove a portion of string
                                partial.push(quote(k) + ":" + stripFuncs[k].call(JSON, v));
                            } else {
                                partial.push(quote(k) + ":" + v);
                            }
                        }
                    } catch (e) {
                        // This try/catch due to forbidden accessors on some objects
                    }
                }

                return partial.length ? "{" + partial.join(",") + "}" : "{}";
        }
    }

    var JSON = JSON || window.JSON;
    JSON.prune = function (value, option) {
        // depthDecr, arrayMaxLength, stripFuncs
        seen = [];
        option = option || {};
        option.depthDecr = option.depthDecr || defaultMaxDepth;
        option.arrayMaxLength = option.arrayMaxLength || defaultArrayMaxLength;
        stripFuncs = option.stripFuncs || {};
        return str("", {
            '': value
        }, option.depthDecr, option.arrayMaxLength);
    };

    return JSON;
})(window);

// ---- /Detecas.Md5.js ----
/* global GfpPickle */

// https://github.com/iReal/FastMD5/blob/master/lib/md5.js
GfpPickle.Md5 = (function (g) {
    "use strict";

    var $0 = [], // result
        $1 = [], // tail
        $2 = [], // blocks
        $3 = [], // s1
        $4 = ("0123456789abcdef").split(""), // hex
        $5 = [], // s2
        $6 = [], // state
        $7 = false, // is state created
        $8 = 0, // len_cache
        $9 = 0, // len
        BUF = [];

    // use Int32Array if defined
    if (g.Int32Array) {
        $1 = new Int32Array(16);
        $2 = new Int32Array(16);
        $3 = new Int32Array(4);
        $5 = new Int32Array(4);
        $6 = new Int32Array(4);
        BUF = new Int32Array(4);
    } else {
        var i;
        for (i = 0; i < 16; i++) $1[i] = $2[i] = 0;
        for (i = 0; i < 4; i++) $3[i] = $5[i] = $6[i] = BUF[i] = 0;
    }

    // fill s1
    $3[0] = 128;
    $3[1] = 32768;
    $3[2] = 8388608;
    $3[3] = -2147483648;

    // fill s2
    $5[0] = 0;
    $5[1] = 8;
    $5[2] = 16;
    $5[3] = 24;

    function encode(s) {
        var utf = "",
            enc = "",
            start = 0,
            end = 0;

        for (var i = 0, j = s.length; i < j; i++) {
            var c = s.charCodeAt(i);

            if (c < 128) {
                end++;
                continue;
            } else if (c < 2048)
                enc = String.fromCharCode((c >> 6) | 192, (c & 63) | 128);
            else
                enc = String.fromCharCode((c >> 12) | 224, ((c >> 6) & 63) | 128, (c & 63) | 128);

            if (end > start)
                utf += s.slice(start, end);

            utf += enc;
            start = end = i + 1;
        }

        if (end > start)
            utf += s.slice(start, j);

        return utf;
    }

    function md5_update(s) {
        var i, I;

        s += "";
        $7 = false;
        $8 = $9 = s.length;

        if ($9 > 63) {
            getBlocks(s.substring(0, 64));
            md5cycle($2);
            $7 = true;

            for (i = 128; i <= $9; i += 64) {
                getBlocks(s.substring(i - 64, i));
                md5cycleAdd($2);
            }

            s = s.substring(i - 64);
            $9 = s.length;
        }

        $1[0] = $1[1] = $1[2] = $1[3] =
            $1[4] = $1[5] = $1[6] = $1[7] =
            $1[8] = $1[9] = $1[10] = $1[11] =
            $1[12] = $1[13] = $1[14] = $1[15] = 0;

        for (i = 0; i < $9; i++) {
            I = i & 3;
            if (I === 0)
                $1[i >> 2] = s.charCodeAt(i);
            else
                $1[i >> 2] |= s.charCodeAt(i) << $5[I];
        }
        $1[i >> 2] |= $3[i & 3];

        if (i > 55) {
            if ($7) md5cycleAdd($1);
            else {
                md5cycle($1);
                $7 = true;
            }

            return md5cycleAdd([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, $8 << 3, 0]);
        }

        $1[14] = $8 << 3;

        if ($7) md5cycleAdd($1);
        else md5cycle($1);
    }

    function getBlocks(s) {
        for (var i = 16; i--;) {
            var I = i << 2;
            $2[i] = s.charCodeAt(I) + (s.charCodeAt(I + 1) << 8) + (s.charCodeAt(I + 2) << 16) + (s.charCodeAt(I + 3) << 24);
        }
    }

    function md5(data, ascii, arrayOutput) {
        md5_update(ascii ? data : encode(data));

        var tmp = $6[0]; $0[1] = $4[tmp & 15];
        $0[0] = $4[(tmp >>= 4) & 15];
        $0[3] = $4[(tmp >>= 4) & 15];
        $0[2] = $4[(tmp >>= 4) & 15];
        $0[5] = $4[(tmp >>= 4) & 15];
        $0[4] = $4[(tmp >>= 4) & 15];
        $0[7] = $4[(tmp >>= 4) & 15];
        $0[6] = $4[(tmp >>= 4) & 15];

        tmp = $6[1]; $0[9] = $4[tmp & 15];
        $0[8] = $4[(tmp >>= 4) & 15];
        $0[11] = $4[(tmp >>= 4) & 15];
        $0[10] = $4[(tmp >>= 4) & 15];
        $0[13] = $4[(tmp >>= 4) & 15];
        $0[12] = $4[(tmp >>= 4) & 15];
        $0[15] = $4[(tmp >>= 4) & 15];
        $0[14] = $4[(tmp >>= 4) & 15];

        tmp = $6[2]; $0[17] = $4[tmp & 15];
        $0[16] = $4[(tmp >>= 4) & 15];
        $0[19] = $4[(tmp >>= 4) & 15];
        $0[18] = $4[(tmp >>= 4) & 15];
        $0[21] = $4[(tmp >>= 4) & 15];
        $0[20] = $4[(tmp >>= 4) & 15];
        $0[23] = $4[(tmp >>= 4) & 15];
        $0[22] = $4[(tmp >>= 4) & 15];

        tmp = $6[3]; $0[25] = $4[tmp & 15];
        $0[24] = $4[(tmp >>= 4) & 15];
        $0[27] = $4[(tmp >>= 4) & 15];
        $0[26] = $4[(tmp >>= 4) & 15];
        $0[29] = $4[(tmp >>= 4) & 15];
        $0[28] = $4[(tmp >>= 4) & 15];
        $0[31] = $4[(tmp >>= 4) & 15];
        $0[30] = $4[(tmp >>= 4) & 15];

        return arrayOutput ? $0 : $0.join("");
    }

    function R(q, a, b, x, s1, s2, t) {
        a += q + x + t;
        return ((a << s1 | a >>> s2) + b) << 0;
    }

    function md5cycle(k) {
        md5_rounds(0, 0, 0, 0, k);

        $6[0] = (BUF[0] + 1732584193) << 0;
        $6[1] = (BUF[1] - 271733879) << 0;
        $6[2] = (BUF[2] - 1732584194) << 0;
        $6[3] = (BUF[3] + 271733878) << 0;
    }

    function md5cycleAdd(k) {
        md5_rounds($6[0], $6[1], $6[2], $6[3], k);

        $6[0] = (BUF[0] + $6[0]) << 0;
        $6[1] = (BUF[1] + $6[1]) << 0;
        $6[2] = (BUF[2] + $6[2]) << 0;
        $6[3] = (BUF[3] + $6[3]) << 0;
    }

    function md5_rounds(a, b, c, d, k) {
        var bc, da;

        if ($7) {
            a = R(((c ^ d) & b) ^ d, a, b, k[0], 7, 25, -680876936);
            d = R(((b ^ c) & a) ^ c, d, a, k[1], 12, 20, -389564586);
            c = R(((a ^ b) & d) ^ b, c, d, k[2], 17, 15, 606105819);
            b = R(((d ^ a) & c) ^ a, b, c, k[3], 22, 10, -1044525330);
        } else {
            a = k[0] - 680876937;
            a = ((a << 7 | a >>> 25) - 271733879) << 0;
            d = k[1] - 117830708 + ((2004318071 & a) ^ -1732584194);
            d = ((d << 12 | d >>> 20) + a) << 0;
            c = k[2] - 1126478375 + (((a ^ -271733879) & d) ^ -271733879);
            c = ((c << 17 | c >>> 15) + d) << 0;
            b = k[3] - 1316259209 + (((d ^ a) & c) ^ a);
            b = ((b << 22 | b >>> 10) + c) << 0;
        }

        a = R(((c ^ d) & b) ^ d, a, b, k[4], 7, 25, -176418897);
        d = R(((b ^ c) & a) ^ c, d, a, k[5], 12, 20, 1200080426);
        c = R(((a ^ b) & d) ^ b, c, d, k[6], 17, 15, -1473231341);
        b = R(((d ^ a) & c) ^ a, b, c, k[7], 22, 10, -45705983);
        a = R(((c ^ d) & b) ^ d, a, b, k[8], 7, 25, 1770035416);
        d = R(((b ^ c) & a) ^ c, d, a, k[9], 12, 20, -1958414417);
        c = R(((a ^ b) & d) ^ b, c, d, k[10], 17, 15, -42063);
        b = R(((d ^ a) & c) ^ a, b, c, k[11], 22, 10, -1990404162);
        a = R(((c ^ d) & b) ^ d, a, b, k[12], 7, 25, 1804603682);
        d = R(((b ^ c) & a) ^ c, d, a, k[13], 12, 20, -40341101);
        c = R(((a ^ b) & d) ^ b, c, d, k[14], 17, 15, -1502002290);
        b = R(((d ^ a) & c) ^ a, b, c, k[15], 22, 10, 1236535329);

        a = R(((b ^ c) & d) ^ c, a, b, k[1], 5, 27, -165796510);
        d = R(((a ^ b) & c) ^ b, d, a, k[6], 9, 23, -1069501632);
        c = R(((d ^ a) & b) ^ a, c, d, k[11], 14, 18, 643717713);
        b = R(((c ^ d) & a) ^ d, b, c, k[0], 20, 12, -373897302);
        a = R(((b ^ c) & d) ^ c, a, b, k[5], 5, 27, -701558691);
        d = R(((a ^ b) & c) ^ b, d, a, k[10], 9, 23, 38016083);
        c = R(((d ^ a) & b) ^ a, c, d, k[15], 14, 18, -660478335);
        b = R(((c ^ d) & a) ^ d, b, c, k[4], 20, 12, -405537848);
        a = R(((b ^ c) & d) ^ c, a, b, k[9], 5, 27, 568446438);
        d = R(((a ^ b) & c) ^ b, d, a, k[14], 9, 23, -1019803690);
        c = R(((d ^ a) & b) ^ a, c, d, k[3], 14, 18, -187363961);
        b = R(((c ^ d) & a) ^ d, b, c, k[8], 20, 12, 1163531501);
        a = R(((b ^ c) & d) ^ c, a, b, k[13], 5, 27, -1444681467);
        d = R(((a ^ b) & c) ^ b, d, a, k[2], 9, 23, -51403784);
        c = R(((d ^ a) & b) ^ a, c, d, k[7], 14, 18, 1735328473);
        b = R(((c ^ d) & a) ^ d, b, c, k[12], 20, 12, -1926607734);

        bc = b ^ c;
        a = R(bc ^ d, a, b, k[5], 4, 28, -378558);
        d = R(bc ^ a, d, a, k[8], 11, 21, -2022574463);
        da = d ^ a;
        c = R(da ^ b, c, d, k[11], 16, 16, 1839030562);
        b = R(da ^ c, b, c, k[14], 23, 9, -35309556);
        bc = b ^ c;
        a = R(bc ^ d, a, b, k[1], 4, 28, -1530992060);
        d = R(bc ^ a, d, a, k[4], 11, 21, 1272893353);
        da = d ^ a;
        c = R(da ^ b, c, d, k[7], 16, 16, -155497632);
        b = R(da ^ c, b, c, k[10], 23, 9, -1094730640);
        bc = b ^ c;
        a = R(bc ^ d, a, b, k[13], 4, 28, 681279174);
        d = R(bc ^ a, d, a, k[0], 11, 21, -358537222);
        da = d ^ a;
        c = R(da ^ b, c, d, k[3], 16, 16, -722521979);
        b = R(da ^ c, b, c, k[6], 23, 9, 76029189);
        bc = b ^ c;
        a = R(bc ^ d, a, b, k[9], 4, 28, -640364487);
        d = R(bc ^ a, d, a, k[12], 11, 21, -421815835);
        da = d ^ a;
        c = R(da ^ b, c, d, k[15], 16, 16, 530742520);
        b = R(da ^ c, b, c, k[2], 23, 9, -995338651);

        a = R(c ^ (b | ~d), a, b, k[0], 6, 26, -198630844);
        d = R(b ^ (a | ~c), d, a, k[7], 10, 22, 1126891415);
        c = R(a ^ (d | ~b), c, d, k[14], 15, 17, -1416354905);
        b = R(d ^ (c | ~a), b, c, k[5], 21, 11, -57434055);
        a = R(c ^ (b | ~d), a, b, k[12], 6, 26, 1700485571);
        d = R(b ^ (a | ~c), d, a, k[3], 10, 22, -1894986606);
        c = R(a ^ (d | ~b), c, d, k[10], 15, 17, -1051523);
        b = R(d ^ (c | ~a), b, c, k[1], 21, 11, -2054922799);
        a = R(c ^ (b | ~d), a, b, k[8], 6, 26, 1873313359);
        d = R(b ^ (a | ~c), d, a, k[15], 10, 22, -30611744);
        c = R(a ^ (d | ~b), c, d, k[6], 15, 17, -1560198380);
        b = R(d ^ (c | ~a), b, c, k[13], 21, 11, 1309151649);
        a = R(c ^ (b | ~d), a, b, k[4], 6, 26, -145523070);
        d = R(b ^ (a | ~c), d, a, k[11], 10, 22, -1120210379);
        c = R(a ^ (d | ~b), c, d, k[2], 15, 17, 718787259);
        b = R(d ^ (c | ~a), b, c, k[9], 21, 11, -343485551);

        BUF[0] = a;
        BUF[1] = b;
        BUF[2] = c;
        BUF[3] = d;
    }

    return {
        hash: md5
    };
})(window);

// ---- /Detecas.Rc4.js ----
/* global GfpPickle */
/*jshint bitwise: false */

GfpPickle.Rc4 = (function () {
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

// ---- /Detecas.Sha1.js ----
/* global GfpPickle */

GfpPickle.Sha1 = (function () {
    "use strict";

    // refer: https://code.google.com/p/tiny-sha1/downloads/detail?name=tinySHA1.r4.js&can=2&q=
    /* jshint ignore:start */
    function SHA1(s) { function U(a, b, c) { while (0 < c--) a.push(b) } function L(a, b) { return (a << b) | (a >>> (32 - b)) } function P(a, b, c) { return a ^ b ^ c } function A(a, b) { var c = (b & 0xFFFF) + (a & 0xFFFF), d = (b >>> 16) + (a >>> 16) + (c >>> 16); return ((d & 0xFFFF) << 16) | (c & 0xFFFF) } var B = "0123456789abcdef"; return (function (a) { var c = [], d = a.length * 4, e; for (var i = 0; i < d; i++) { e = a[i >> 2] >> ((3 - (i % 4)) * 8); c.push(B.charAt((e >> 4) & 0xF) + B.charAt(e & 0xF)) } return c.join('') }((function (a, b) { var c, d, e, f, g, h = a.length, v = 0x67452301, w = 0xefcdab89, x = 0x98badcfe, y = 0x10325476, z = 0xc3d2e1f0, M = []; U(M, 0x5a827999, 20); U(M, 0x6ed9eba1, 20); U(M, 0x8f1bbcdc, 20); U(M, 0xca62c1d6, 20); a[b >> 5] |= 0x80 << (24 - (b % 32)); a[(((b + 65) >> 9) << 4) + 15] = b; for (var i = 0; i < h; i += 16) { c = v; d = w; e = x; f = y; g = z; for (var j = 0, O = []; j < 80; j++) { O[j] = j < 16 ? a[j + i] : L(O[j - 3] ^ O[j - 8] ^ O[j - 14] ^ O[j - 16], 1); var k = (function (a, b, c, d, e) { var f = (e & 0xFFFF) + (a & 0xFFFF) + (b & 0xFFFF) + (c & 0xFFFF) + (d & 0xFFFF), g = (e >>> 16) + (a >>> 16) + (b >>> 16) + (c >>> 16) + (d >>> 16) + (f >>> 16); return ((g & 0xFFFF) << 16) | (f & 0xFFFF) })(j < 20 ? (function (t, a, b) { return (t & a) ^ (~t & b) }(d, e, f)) : j < 40 ? P(d, e, f) : j < 60 ? (function (t, a, b) { return (t & a) ^ (t & b) ^ (a & b) }(d, e, f)) : P(d, e, f), g, M[j], O[j], L(c, 5)); g = f; f = e; e = L(d, 30); d = c; c = k } v = A(v, c); w = A(w, d); x = A(x, e); y = A(y, f); z = A(z, g) } return [v, w, x, y, z] }((function (t) { var a = [], b = 255, c = t.length * 8; for (var i = 0; i < c; i += 8) { a[i >> 5] |= (t.charCodeAt(i / 8) & b) << (24 - (i % 32)) } return a }(s)).slice(), s.length * 8)))) }
    /* jshint ignore:end */

    /* global SHA1 */
    return {
        hash: SHA1
    };
})();

// ---- /Detecas.BotD.js ----
GfpPickle.BotD =
    (function () {
        'use strict';

        /******************************************************************************
        Copyright (c) Microsoft Corporation.
    
        Permission to use, copy, modify, and/or distribute this software for any
        purpose with or without fee is hereby granted.
    
        THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
        REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
        AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
        INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
        LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
        OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
        PERFORMANCE OF THIS SOFTWARE.
        ***************************************************************************** */
        /* global Reflect, Promise */

        var extendStatics = function (d, b) {
            extendStatics = Object.setPrototypeOf ||
                ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
                function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
            return extendStatics(d, b);
        };

        function __extends(d, b) {
            if (typeof b !== "function" && b !== null)
                throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
            extendStatics(d, b);
            function __() { this.constructor = d; }
            d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
        }

        function __awaiter(thisArg, _arguments, P, generator) {
            function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
            return new (P || (P = Promise))(function (resolve, reject) {
                function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
                function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
                function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
                step((generator = generator.apply(thisArg, _arguments || [])).next());
            });
        }

        function __generator(thisArg, body) {
            var _ = { label: 0, sent: function () { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
            return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function () { return this; }), g;
            function verb(n) { return function (v) { return step([n, v]); }; }
            function step(op) {
                if (f) throw new TypeError("Generator is already executing.");
                while (g && (g = 0, op[0] && (_ = 0)), _) try {
                    if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                    if (y = 0, t) op = [op[0] & 2, t.value];
                    switch (op[0]) {
                        case 0: case 1: t = op; break;
                        case 4: _.label++; return { value: op[1], done: false };
                        case 5: _.label++; y = op[1]; op = [0]; continue;
                        case 7: op = _.ops.pop(); _.trys.pop(); continue;
                        default:
                            if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                            if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                            if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                            if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                            if (t[2]) _.ops.pop();
                            _.trys.pop(); continue;
                    }
                    op = body.call(thisArg, _);
                } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
                if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
            }
        }

        function __spreadArray(to, from, pack) {
            if (pack || arguments.length === 2) for (var i = 0, l = from.length, ar; i < l; i++) {
                if (ar || !(i in from)) {
                    if (!ar) ar = Array.prototype.slice.call(from, 0, i);
                    ar[i] = from[i];
                }
            }
            return to.concat(ar || Array.prototype.slice.call(from));
        }

        /**
         * Fingerprint BotD v1.1.10 - Copyright (c) FingerprintJS, Inc, 2023 (https://fingerprint.com)
         * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php) license.
         */

        var version = "1.1.10";

        /**
         * Enum for types of bots.
         * Specific types of bots come first, followed by automation technologies.
         *
         * @readonly
         * @enum {string}
         */
        var BotKind = {
            // Object is used instead of Typescript enum to avoid emitting IIFE which might be affected by further tree-shaking.
            // See example of compiled enums https://stackoverflow.com/q/47363996)
            Awesomium: 'awesomium',
            Cef: 'cef',
            CefSharp: 'cefsharp',
            CoachJS: 'coachjs',
            Electron: 'electron',
            FMiner: 'fminer',
            Geb: 'geb',
            NightmareJS: 'nightmarejs',
            Phantomas: 'phantomas',
            PhantomJS: 'phantomjs',
            Rhino: 'rhino',
            Selenium: 'selenium',
            Sequentum: 'sequentum',
            SlimerJS: 'slimerjs',
            WebDriverIO: 'webdriverio',
            WebDriver: 'webdriver',
            HeadlessChrome: 'headless_chrome',
            Unknown: 'unknown',
        };
        /**
         * Bot detection error.
         */
        var BotdError = /** @class */ (function (_super) {
            __extends(BotdError, _super);
            /**
             * Creates a new BotdError.
             *
             * @class
             */
            function BotdError(state, message) {
                var _this = _super.call(this, message) || this;
                _this.state = state;
                _this.name = 'BotdError';
                Object.setPrototypeOf(_this, BotdError.prototype);
                return _this;
            }
            return BotdError;
        }(Error));

        function detectAppVersion(_a) {
            var appVersion = _a.appVersion;
            if (appVersion.state !== 0 /* State.Success */)
                return false;
            if (/headless/i.test(appVersion.value))
                return BotKind.HeadlessChrome;
            if (/electron/i.test(appVersion.value))
                return BotKind.Electron;
            if (/slimerjs/i.test(appVersion.value))
                return BotKind.SlimerJS;
        }

        function arrayIncludes(arr, value) {
            return arr.indexOf(value) !== -1;
        }
        function strIncludes(str, value) {
            return str.indexOf(value) !== -1;
        }
        function arrayFind(array, callback) {
            if ('find' in array)
                return array.find(callback);
            for (var i = 0; i < array.length; i++) {
                if (callback(array[i], i, array))
                    return array[i];
            }
            return undefined;
        }

        function getObjectProps(obj) {
            return Object.getOwnPropertyNames(obj);
        }
        function includes(arr) {
            var keys = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                keys[_i - 1] = arguments[_i];
            }
            var _loop_1 = function (key) {
                if (typeof key === 'string') {
                    if (arrayIncludes(arr, key))
                        return { value: true };
                }
                else {
                    var match = arrayFind(arr, function (value) { return key.test(value); });
                    if (match != null)
                        return { value: true };
                }
            };
            for (var _a = 0, keys_1 = keys; _a < keys_1.length; _a++) {
                var key = keys_1[_a];
                var state_1 = _loop_1(key);
                if (typeof state_1 === "object")
                    return state_1.value;
            }
            return false;
        }
        function countTruthy(values) {
            return values.reduce(function (sum, value) { return sum + (value ? 1 : 0); }, 0);
        }

        function detectDocumentAttributes(_a) {
            var documentElementKeys = _a.documentElementKeys;
            if (documentElementKeys.state !== 0 /* State.Success */)
                return false;
            if (includes(documentElementKeys.value, 'selenium', 'webdriver', 'driver')) {
                return BotKind.Selenium;
            }
        }

        function detectErrorTrace(_a) {
            var errorTrace = _a.errorTrace;
            if (errorTrace.state !== 0 /* State.Success */)
                return false;
            if (/PhantomJS/i.test(errorTrace.value))
                return BotKind.PhantomJS;
        }

        function getBrowserEngineKind() {
            var _a, _b;
            // Based on research in October 2020. Tested to detect Chromium 42-86.
            var w = window;
            var n = navigator;
            if (countTruthy([
                'webkitPersistentStorage' in n,
                'webkitTemporaryStorage' in n,
                n.vendor.indexOf('Google') === 0,
                'webkitResolveLocalFileSystemURL' in w,
                'BatteryManager' in w,
                'webkitMediaStream' in w,
                'webkitSpeechGrammar' in w,
            ]) >= 4) {
                return "chromium" /* BrowserEngineKind.Chromium */;
            }
            if (countTruthy([
                'ApplePayError' in w,
                'CSSPrimitiveValue' in w,
                'Counter' in w,
                n.vendor.indexOf('Apple') === 0,
                'getStorageUpdates' in n,
                'WebKitMediaKeys' in w,
            ]) >= 4) {
                return "webkit" /* BrowserEngineKind.Webkit */;
            }
            if (countTruthy([
                'buildID' in navigator,
                'MozAppearance' in ((_b = (_a = document.documentElement) === null || _a === void 0 ? void 0 : _a.style) !== null && _b !== void 0 ? _b : {}),
                'onmozfullscreenchange' in w,
                'mozInnerScreenX' in w,
                'CSSMozDocumentRule' in w,
                'CanvasCaptureMediaStream' in w,
            ]) >= 4) {
                return "gecko" /* BrowserEngineKind.Gecko */;
            }
            return "unknown" /* BrowserEngineKind.Unknown */;
        }
        function getBrowserKind() {
            var _a;
            var userAgent = (_a = navigator.userAgent) === null || _a === void 0 ? void 0 : _a.toLowerCase();
            if (strIncludes(userAgent, 'wechat')) {
                return "wechat" /* BrowserKind.WeChat */;
            }
            else if (strIncludes(userAgent, 'firefox')) {
                return "firefox" /* BrowserKind.Firefox */;
            }
            else if (strIncludes(userAgent, 'opera') || strIncludes(userAgent, 'opr')) {
                return "opera" /* BrowserKind.Opera */;
            }
            else if (strIncludes(userAgent, 'chrome')) {
                return "chrome" /* BrowserKind.Chrome */;
            }
            else if (strIncludes(userAgent, 'safari')) {
                return "safari" /* BrowserKind.Safari */;
            }
            else if (strIncludes(userAgent, 'trident') || strIncludes(userAgent, 'msie')) {
                return "internet_explorer" /* BrowserKind.IE */;
            }
            else {
                return "unknown" /* BrowserKind.Unknown */;
            }
        }
        // Source: https://github.com/fingerprintjs/fingerprintjs/blob/master/src/utils/browser.ts#L223
        function isAndroid() {
            var browserEngineKind = getBrowserEngineKind();
            var isItChromium = browserEngineKind === "chromium" /* BrowserEngineKind.Chromium */;
            var isItGecko = browserEngineKind === "gecko" /* BrowserEngineKind.Gecko */;
            // Only 2 browser engines are presented on Android.
            // Actually, there is also Android 4.1 browser, but it's not worth detecting it at the moment.
            if (!isItChromium && !isItGecko)
                return false;
            var w = window;
            // Chrome removes all words "Android" from `navigator` when desktop version is requested
            // Firefox keeps "Android" in `navigator.appVersion` when desktop version is requested
            return (countTruthy([
                'onorientationchange' in w,
                'orientation' in w,
                isItChromium && !('SharedWorker' in w),
                isItGecko && /android/i.test(navigator.appVersion),
            ]) >= 2);
        }
        // Source: https://github.com/fingerprintjs/fingerprintjs/blob/43a84516755440a89f3093a3b27a021d3d7351b6/src/utils/browser.ts#L100C1-L113C2
        function isDesktopSafari() {
            var w = window;
            return (countTruthy([
                'safari' in w,
                !('ongestureend' in w),
                !('TouchEvent' in w),
                !('orientation' in w),
                'HTMLElement' in w && !('autocapitalize' in HTMLElement.prototype),
                'Document' in w && 'pointerLockElement' in Document.prototype,
            ]) >= 4);
        }
        function getDocumentFocus() {
            if (document.hasFocus === undefined) {
                return false;
            }
            return document.hasFocus();
        }
        function isChromium86OrNewer() {
            // Checked in Chrome 85 vs Chrome 86 both on desktop and Android
            var w = window;
            return (countTruthy([
                !('MediaSettingsRange' in w),
                'RTCEncodedAudioFrame' in w,
                '' + w.Intl === '[object Intl]',
                '' + w.Reflect === '[object Reflect]',
            ]) >= 3);
        }

        function detectEvalLengthInconsistency(_a) {
            var evalLength = _a.evalLength;
            if (evalLength.state !== 0 /* State.Success */)
                return;
            var length = evalLength.value;
            var browser = getBrowserKind();
            var browserEngine = getBrowserEngineKind();
            if (browserEngine == "unknown" /* BrowserEngineKind.Unknown */) {
                return false;
            }
            return ((length === 37 && !arrayIncludes(["webkit" /* BrowserEngineKind.Webkit */, "gecko" /* BrowserEngineKind.Gecko */], browserEngine)) ||
                (length === 39 && !arrayIncludes(["internet_explorer" /* BrowserKind.IE */], browser)) ||
                (length === 33 && !arrayIncludes(["chromium" /* BrowserEngineKind.Chromium */], browserEngine)));
        }

        function detectFunctionBind(_a) {
            var functionBind = _a.functionBind;
            if (functionBind.state === -2 /* State.NotFunction */)
                return BotKind.PhantomJS;
        }

        function detectLanguagesLengthInconsistency(_a) {
            var languages = _a.languages;
            if (languages.state === 0 /* State.Success */ && languages.value.length === 0) {
                return BotKind.HeadlessChrome;
            }
        }

        function detectMimeTypesConsistent(_a) {
            var mimeTypesConsistent = _a.mimeTypesConsistent;
            if (mimeTypesConsistent.state === 0 /* State.Success */ && !mimeTypesConsistent.value) {
                return BotKind.Unknown;
            }
        }

        function detectPluginsArray(_a) {
            var pluginsArray = _a.pluginsArray;
            if (pluginsArray.state === 0 /* State.Success */ && !pluginsArray.value)
                return BotKind.HeadlessChrome;
        }

        function detectPluginsLengthInconsistency(_a) {
            var pluginsLength = _a.pluginsLength;
            if (pluginsLength.state !== 0 /* State.Success */)
                return;
            var browserKind = getBrowserKind();
            var browserEngineKind = getBrowserEngineKind();
            if (browserKind !== "chrome" /* BrowserKind.Chrome */ || isAndroid() || browserEngineKind !== "chromium" /* BrowserEngineKind.Chromium */)
                return;
            if (pluginsLength.value === 0)
                return BotKind.HeadlessChrome;
        }

        function detectProcess(_a) {
            var _b;
            var process = _a.process;
            if (process.state !== 0 /* State.Success */)
                return false;
            if (process.value.type === 'renderer' || ((_b = process.value.versions) === null || _b === void 0 ? void 0 : _b.electron) != null)
                return BotKind.Electron;
        }

        function detectProductSub(_a) {
            var productSub = _a.productSub;
            if (productSub.state !== 0 /* State.Success */)
                return false;
            var browserKind = getBrowserKind();
            if ((browserKind === "chrome" /* BrowserKind.Chrome */ ||
                browserKind === "safari" /* BrowserKind.Safari */ ||
                browserKind === "opera" /* BrowserKind.Opera */ ||
                browserKind === "wechat" /* BrowserKind.WeChat */) &&
                productSub.value !== '20030107') {
                return BotKind.Unknown;
            }
            else if (browserKind == "firefox" /* BrowserKind.Firefox */ && productSub.value === '20030107') {
                return BotKind.Unknown;
            }
        }

        function detectUserAgent(_a) {
            var userAgent = _a.userAgent;
            if (userAgent.state !== 0 /* State.Success */)
                return false;
            if (/PhantomJS/i.test(userAgent.value))
                return BotKind.PhantomJS;
            if (/Headless/i.test(userAgent.value))
                return BotKind.HeadlessChrome;
            if (/Electron/i.test(userAgent.value))
                return BotKind.Electron;
            if (/slimerjs/i.test(userAgent.value))
                return BotKind.SlimerJS;
        }

        function detectWebDriver(_a) {
            var webDriver = _a.webDriver;
            if (webDriver.state === 0 /* State.Success */ && webDriver.value)
                return BotKind.HeadlessChrome;
        }

        function detectWebGL(_a) {
            var webGL = _a.webGL;
            if (webGL.state === 0 /* State.Success */) {
                var _b = webGL.value, vendor = _b.vendor, renderer = _b.renderer;
                if (vendor == 'Brian Paul' && renderer == 'Mesa OffScreen') {
                    return BotKind.HeadlessChrome;
                }
            }
        }

        function detectWindowExternal(_a) {
            var windowExternal = _a.windowExternal;
            if (windowExternal.state !== 0 /* State.Success */)
                return false;
            if (/Sequentum/i.test(windowExternal.value))
                return BotKind.Sequentum;
        }

        function detectWindowSize(_a) {
            var _b;
            var windowSize = _a.windowSize;
            if (windowSize.state !== 0 /* State.Success */)
                return false;
            var _c = windowSize.value, outerWidth = _c.outerWidth, outerHeight = _c.outerHeight;
            // When a page is opened in a new tab without focusing it right away, the window outer size is 0x0
            if (!getDocumentFocus())
                return;
            if (getBrowserKind() == "safari" /* BrowserKind.Safari */) {
                var userAgent_1 = (_b = navigator.userAgent) === null || _b === void 0 ? void 0 : _b.toLowerCase();
                var listExcluded = ['iphone; cpu iphone os 10', 'iphone; cpu iphone os 11', 'iphone; cpu iphone os 12',
                    'ipad; cpu os 10', 'ipad; cpu os 11', 'ipad; cpu os 12'];
                var checkInclude = listExcluded.some(function (keyword) {
                    return userAgent_1.includes(keyword);
                });
                if (checkInclude) {
                    return;
                }
            }
            if (outerWidth === 0 && outerHeight === 0)
                return BotKind.HeadlessChrome;
        }

        function excludeRules(ruleName) {
            var _a;
            var userAgent = (_a = navigator.userAgent) === null || _a === void 0 ? void 0 : _a.toLowerCase();
            if (ruleName == 'detectNotificationPermissions') {
                if (strIncludes(userAgent, 'samsungbrowser') || strIncludes(userAgent, 'heytapbrowser')) {
                    return true;
                }
            }
            if (ruleName == 'detectVmFromMemoryScreenDimensions') {
                var tvUserAgentRegex = /smart[- ]?tv|apple\s?tv|googletv|android tv|opera tv|dtvnetbrowser|sonycebrowser|bravia 4k|netflix|pov_tv-hdmi-200bt|dlnadoc\/|hbbtv|tv\s?safari|roku\/dvp|nsz-gs7\/gx70|crkey|aftm|aftt|gtv100|nintendobrowser|playstation/i;
                if (tvUserAgentRegex.test(userAgent)) {
                    return true;
                }
            }
            return false;
        }

        function detectVmFromMemoryScreenDimensions(_a) {
            var deviceMemory = _a.deviceMemory, screenDimensions = _a.screenDimensions;
            if (deviceMemory.state !== 0 /* State.Success */ || screenDimensions.state !== 0 /* State.Success */)
                return;
            if (excludeRules('detectVmFromMemoryScreenDimensions'))
                return false;
            // If it's a desktop device with 1GB RAM and screen bigger than 1024x768, it's probably a VM
            if (deviceMemory.value <= 1 && screenDimensions.value.width >= 1024 && screenDimensions.value.height >= 768) {
                return BotKind.Unknown;
            }
            return false;
        }

        function detectDistinctiveProperties(_a) {
            var distinctiveProps = _a.distinctiveProps;
            if (distinctiveProps.state !== 0 /* State.Success */)
                return false;
            var value = distinctiveProps.value;
            var bot;
            for (bot in value)
                if (value[bot])
                    return bot;
        }

        // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
        var detectors = {
            detectAppVersion: detectAppVersion,
            detectDocumentAttributes: detectDocumentAttributes,
            detectErrorTrace: detectErrorTrace,
            detectEvalLengthInconsistency: detectEvalLengthInconsistency,
            detectFunctionBind: detectFunctionBind,
            detectLanguagesLengthInconsistency: detectLanguagesLengthInconsistency,
            detectPluginsArray: detectPluginsArray,
            detectPluginsLengthInconsistency: detectPluginsLengthInconsistency,
            detectProcess: detectProcess,
            detectUserAgent: detectUserAgent,
            detectWebDriver: detectWebDriver,
            detectWebGL: detectWebGL,
            detectWindowExternal: detectWindowExternal,
            detectWindowSize: detectWindowSize,
            detectMimeTypesConsistent: detectMimeTypesConsistent,
            detectProductSub: detectProductSub,
            detectVmFromMemoryScreenDimensions: detectVmFromMemoryScreenDimensions,
            detectDistinctiveProperties: detectDistinctiveProperties,
        };

        function getAppVersion() {
            var appVersion = navigator.appVersion;
            if (appVersion == undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.appVersion is undefined');
            }
            return appVersion;
        }

        function getDocumentElementKeys() {
            if (document.documentElement === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'document.documentElement is undefined');
            }
            var documentElement = document.documentElement;
            if (typeof documentElement.getAttributeNames !== 'function') {
                throw new BotdError(-2 /* State.NotFunction */, 'document.documentElement.getAttributeNames is not a function');
            }
            return documentElement.getAttributeNames();
        }

        function getErrorTrace() {
            try {
                // eslint-disable-next-line @typescript-eslint/ban-ts-comment
                // @ts-ignore
                null[0]();
            }
            catch (error) {
                if (error instanceof Error && error['stack'] != null) {
                    return error.stack.toString();
                }
            }
            throw new BotdError(-3 /* State.UnexpectedBehaviour */, 'errorTrace signal unexpected behaviour');
        }

        function getEvalLength() {
            return eval.toString().length;
        }

        function getFunctionBind() {
            if (Function.prototype.bind === undefined) {
                throw new BotdError(-2 /* State.NotFunction */, 'Function.prototype.bind is undefined');
            }
            return Function.prototype.bind.toString();
        }

        function getLanguages() {
            var n = navigator;
            var result = [];
            var language = n.language || n.userLanguage || n.browserLanguage || n.systemLanguage;
            if (language !== undefined) {
                result.push([language]);
            }
            if (Array.isArray(n.languages)) {
                var browserEngine = getBrowserEngineKind();
                // Starting from Chromium 86, there is only a single value in `navigator.language` in Incognito mode:
                // the value of `navigator.language`. Therefore, the value is ignored in this browser.
                if (!(browserEngine === "chromium" /* BrowserEngineKind.Chromium */ && isChromium86OrNewer())) {
                    result.push(n.languages);
                }
            }
            else if (typeof n.languages === 'string') {
                var languages = n.languages;
                if (languages) {
                    result.push(languages.split(','));
                }
            }
            return result;
        }

        function areMimeTypesConsistent() {
            if (navigator.mimeTypes === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.mimeTypes is undefined');
            }
            var mimeTypes = navigator.mimeTypes;
            var isConsistent = Object.getPrototypeOf(mimeTypes) === MimeTypeArray.prototype;
            for (var i = 0; i < mimeTypes.length; i++) {
                isConsistent && (isConsistent = Object.getPrototypeOf(mimeTypes[i]) === MimeType.prototype);
            }
            return isConsistent;
        }

        function getPluginsArray() {
            if (navigator.plugins === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.plugins is undefined');
            }
            if (window.PluginArray === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'window.PluginArray is undefined');
            }
            return navigator.plugins instanceof PluginArray;
        }

        function getPluginsLength() {
            if (navigator.plugins === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.plugins is undefined');
            }
            if (navigator.plugins.length === undefined) {
                throw new BotdError(-3 /* State.UnexpectedBehaviour */, 'navigator.plugins.length is undefined');
            }
            return navigator.plugins.length;
        }

        function getProcess() {
            if (window.process === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'window.process is undefined');
            }
            return window.process;
        }

        function getProductSub() {
            var productSub = navigator.productSub;
            if (productSub === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.productSub is undefined');
            }
            return productSub;
        }

        function getUserAgent() {
            return navigator.userAgent;
        }

        function getWebDriver() {
            if (navigator.webdriver == undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.webdriver is undefined');
            }
            return navigator.webdriver;
        }

        function getWebGL() {
            var canvasElement = document.createElement('canvas');
            if (typeof canvasElement.getContext !== 'function') {
                throw new BotdError(-2 /* State.NotFunction */, 'HTMLCanvasElement.getContext is not a function');
            }
            var webGLContext = canvasElement.getContext('webgl');
            if (webGLContext === null) {
                throw new BotdError(-4 /* State.Null */, 'WebGLRenderingContext is null');
            }
            if (typeof webGLContext.getParameter !== 'function') {
                throw new BotdError(-2 /* State.NotFunction */, 'WebGLRenderingContext.getParameter is not a function');
            }
            var vendor = webGLContext.getParameter(webGLContext.VENDOR);
            var renderer = webGLContext.getParameter(webGLContext.RENDERER);
            return { vendor: vendor, renderer: renderer };
        }

        function getWindowExternal() {
            if (window.external === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'window.external is undefined');
            }
            var external = window.external;
            if (typeof external.toString !== 'function') {
                throw new BotdError(-2 /* State.NotFunction */, 'window.external.toString is not a function');
            }
            return external.toString();
        }

        function getWindowSize() {
            return {
                outerWidth: window.outerWidth,
                outerHeight: window.outerHeight,
                innerWidth: window.innerWidth,
                innerHeight: window.innerHeight,
            };
        }

        function getDeviceMemory() {
            if (navigator.deviceMemory === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'navigator.deviceMemory is undefined');
            }
            return navigator.deviceMemory;
        }

        function getScreenDimensions() {
            if (screen === undefined) {
                throw new BotdError(-1 /* State.Undefined */, 'screen is undefined');
            }
            return { width: screen.width, height: screen.height };
        }

        function checkDistinctiveProperties() {
            var _a;
            // The order in the following list matters, because specific types of bots come first, followed by automation technologies.
            var distinctivePropsList = (_a = {},
                _a[BotKind.Awesomium] = {
                    window: ['awesomium'],
                },
                _a[BotKind.Cef] = {
                    window: ['RunPerfTest'],
                },
                _a[BotKind.CefSharp] = {
                    window: ['CefSharp'],
                },
                _a[BotKind.CoachJS] = {
                    window: ['emit'],
                },
                _a[BotKind.FMiner] = {
                    window: ['fmget_targets'],
                },
                _a[BotKind.Geb] = {
                    window: ['geb'],
                },
                _a[BotKind.NightmareJS] = {
                    window: ['__nightmare', 'nightmare'],
                },
                _a[BotKind.Phantomas] = {
                    window: ['__phantomas'],
                },
                _a[BotKind.PhantomJS] = {
                    window: ['callPhantom', '_phantom'],
                },
                _a[BotKind.Rhino] = {
                    window: ['spawn'],
                },
                _a[BotKind.Selenium] = {
                    window: ['_Selenium_IDE_Recorder', '_selenium', 'calledSelenium', /^([a-z]){3}_.*_(Array|Promise|Symbol)$/],
                    document: ['__selenium_evaluate', 'selenium-evaluate', '__selenium_unwrapped'],
                },
                _a[BotKind.WebDriverIO] = {
                    window: ['wdioElectron'],
                },
                _a[BotKind.WebDriver] = {
                    window: [
                        'webdriver',
                        '__webdriverFunc',
                        '__lastWatirAlert',
                        '__lastWatirConfirm',
                        '__lastWatirPrompt',
                        '_WEBDRIVER_ELEM_CACHE',
                        'ChromeDriverw',
                    ],
                    document: [
                        '__webdriver_script_fn',
                        '__driver_evaluate',
                        '__webdriver_evaluate',
                        '__fxdriver_evaluate',
                        '__driver_unwrapped',
                        '__webdriver_unwrapped',
                        '__fxdriver_unwrapped',
                        '__webdriver_script_fn',
                        '__webdriver_script_func',
                        '__webdriver_script_function',
                        '$cdc_asdjflasutopfhvcZLmcf',
                        '$cdc_asdjflasutopfhvcZLmcfl_',
                        '$chrome_asyncScriptInfo',
                        '__$webdriverAsyncExecutor',
                    ],
                },
                _a[BotKind.HeadlessChrome] = {
                    window: ['domAutomation', 'domAutomationController'],
                },
                _a);
            var botName;
            var result = {};
            var windowProps = getObjectProps(window);
            var documentProps = [];
            if (window.document !== undefined)
                documentProps = getObjectProps(window.document);
            for (botName in distinctivePropsList) {
                var props = distinctivePropsList[botName];
                if (props !== undefined) {
                    var windowContains = props.window === undefined ? false : includes.apply(void 0, __spreadArray([windowProps], props.window, false));
                    var documentContains = props.document === undefined || !documentProps.length ? false : includes.apply(void 0, __spreadArray([documentProps], props.document, false));
                    result[botName] = windowContains || documentContains;
                }
            }
            return result;
        }

        function getBrowserInfo() {
            return {
                browserEngineKind: getBrowserEngineKind(),
                browserKind: getBrowserKind(),
                isAndroid: isAndroid(),
                isDesktopSafari: isDesktopSafari(),
                isChromium86OrNewer: isChromium86OrNewer(),
            };
        }

        var sources = {
            userAgent: getUserAgent,
            appVersion: getAppVersion,
            windowSize: getWindowSize,
            pluginsLength: getPluginsLength,
            pluginsArray: getPluginsArray,
            errorTrace: getErrorTrace,
            productSub: getProductSub,
            windowExternal: getWindowExternal,
            mimeTypesConsistent: areMimeTypesConsistent,
            evalLength: getEvalLength,
            webGL: getWebGL,
            webDriver: getWebDriver,
            languages: getLanguages,
            documentElementKeys: getDocumentElementKeys,
            functionBind: getFunctionBind,
            process: getProcess,
            deviceMemory: getDeviceMemory,
            screenDimensions: getScreenDimensions,
            distinctiveProps: checkDistinctiveProperties,
            browserInfo: getBrowserInfo,
        };

        /**
         * Class representing a bot detector.
         *
         * @class
         * @implements {BotDetectorInterface}
         */
        var BotDetector = /** @class */ (function () {
            function BotDetector() {
                this.components = undefined;
                this.detections = undefined;
            }
            BotDetector.prototype.getComponents = function () {
                return this.components;
            };
            BotDetector.prototype.getDetections = function () {
                return this.detections;
            };
            // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
            BotDetector.prototype.getSources = function () {
                return sources;
            };
            // eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
            BotDetector.prototype.getDetectors = function () {
                return detectors;
            };
            /**
             * @inheritdoc
             */
            BotDetector.prototype.detect = function () {
                if (this.components === undefined) {
                    throw new Error("BotDetector.detect can't be called before BotDetector.collect");
                }
                var components = this.components;
                var detectors = this.getDetectors();
                var detections = {};
                var finalDetection = {
                    bot: false,
                };
                for (var detectorName in detectors) {
                    var detector = detectors[detectorName];
                    var detectorRes = detector(components);
                    var detection = { bot: false };
                    if (typeof detectorRes === 'string') {
                        detection = { bot: true, botKind: detectorRes };
                    }
                    else if (detectorRes) {
                        detection = { bot: true, botKind: BotKind.Unknown };
                    }
                    detections[detectorName] = detection;
                    if (detection.bot) {
                        finalDetection = detection;
                    }
                }
                this.detections = detections;
                return finalDetection;
            };
            /**
             * @inheritdoc
             */
            BotDetector.prototype.collect = function () {
                return __awaiter(this, void 0, void 0, function () {
                    var sources, components, sourcesKeys;
                    var _this = this;
                    return __generator(this, function (_a) {
                        switch (_a.label) {
                            case 0:
                                sources = this.getSources();
                                components = {};
                                sourcesKeys = Object.keys(sources);
                                return [4 /*yield*/, Promise.all(sourcesKeys.map(function (sourceKey) {
                                    return __awaiter(_this, void 0, void 0, function () {
                                        var res, _a, _b, error_1;
                                        var _c;
                                        return __generator(this, function (_d) {
                                            switch (_d.label) {
                                                case 0:
                                                    res = sources[sourceKey];
                                                    _d.label = 1;
                                                case 1:
                                                    _d.trys.push([1, 3, , 4]);
                                                    _a = components;
                                                    _b = sourceKey;
                                                    _c = {};
                                                    return [4 /*yield*/, res()];
                                                case 2:
                                                    _a[_b] = (_c.value = _d.sent(),
                                                        _c.state = 0 /* State.Success */,
                                                        _c);
                                                    return [3 /*break*/, 4];
                                                case 3:
                                                    error_1 = _d.sent();
                                                    if (error_1 instanceof BotdError) {
                                                        components[sourceKey] = {
                                                            state: error_1.state,
                                                            error: "".concat(error_1.name, ": ").concat(error_1.message),
                                                        };
                                                    }
                                                    else {
                                                        components[sourceKey] = {
                                                            state: -3 /* State.UnexpectedBehaviour */,
                                                            error: error_1 instanceof Error ? "".concat(error_1.name, ": ").concat(error_1.message) : String(error_1),
                                                        };
                                                    }
                                                    return [3 /*break*/, 4];
                                                case 4: return [2 /*return*/];
                                            }
                                        });
                                    });
                                }))];
                            case 1:
                                _a.sent();
                                this.components = components;
                                return [2 /*return*/, this.components];
                        }
                    });
                });
            };
            return BotDetector;
        }());

        /**
         * Sends an unpersonalized AJAX request to collect installation statistics
         */
        function monitor() {
            // The FingerprintJS CDN (https://github.com/fingerprintjs/cdn) replaces `window.__fpjs_d_m` with `true`
            if (window.__fpjs_d_m || Math.random() >= 0.001) {
                return;
            }
            try {
                var request = new XMLHttpRequest();
                request.open('get', "https://m1.openfpcdn.io/botd/v".concat(version, "/npm-monitoring"), true);
                request.send();
            }
            catch (error) {
                // console.error is ok here because it's an unexpected error handler
                // eslint-disable-next-line no-console
                console.error(error);
            }
        }
        function load(_a) {
            var _b = _a === void 0 ? {} : _a, _c = _b.monitoring, monitoring = _c === void 0 ? true : _c;
            return __awaiter(this, void 0, void 0, function () {
                var detector;
                return __generator(this, function (_d) {
                    switch (_d.label) {
                        case 0:
                            if (monitoring) {
                                monitor();
                            }
                            detector = new BotDetector();
                            return [4 /*yield*/, detector.collect()];
                        case 1:
                            _d.sent();
                            return [2 /*return*/, detector];
                    }
                });
            });
        }

        function getResult() {
            try {
                var botdPromise = load({
                    monitoring: false
                });
                return botdPromise
                    .then(function (botd) {
                        var resultDetect = botd.detect();
                        var detections = botd.getDetections();
                        var botDetections = Object.keys(detections).filter(function (key) { return detections[key].bot; }).join(',');
                        var botComponents = '';
                        if (resultDetect && resultDetect.bot) {
                            botComponents = JSON.stringify(botd.getComponents());
                        }
                        return {
                            result: resultDetect,
                            detection: botDetections,
                            component: botComponents,
                            isVer3: true
                        };
                    }).catch(function (error) {
                        return {
                            error: error.message
                        };
                    });
            }
            catch (error) {
                return Promise.resolve({ error: error.message });
            }
        }

        return getResult;

    })();


// ---- /Detecas.Debug.js ----
/* global GfpPickle */

GfpPickle.DebugData = {
    startTime: (new Date()).getTime(),
    version: '',
    exceptions: [],
    executions: [],
    storages: [],
    devices: [],
    enable: true
};

GfpPickle.Debug = (function (document, json, debugData, hiddenFieldId, undefined) {
    "use strict";

    var getTime = function () {
        var now = (new Date()).getTime();
        return now - debugData.startTime;
    };

    var createElement = function (container, jsonString, id) {
        var diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = hiddenFieldId;
        diInput.setAttribute("type", "hidden");
        diInput.value = jsonString;
        container.appendChild(diInput);
    };
    var bindData = function () {
        var elByNames = document.getElementsByName(hiddenFieldId);
        var jsonString = json.stringify(debugData);

        if (elByNames == null || elByNames.length <= 0) {
            var forms = document.forms;
            if (forms != null && forms.length != 0) {
                for (var i = 0; i < forms.length; i++) {
                    createElement(document.forms[i], jsonString, (i == 0 ? hiddenFieldId : hiddenFieldId + i));
                }
            }
            else {
                createElement(document.body, jsonString, hiddenFieldId);
            }
        }
        else {
            if (elByNames != null) {
                for (var i = 0; i < elByNames.length; i++) {

                    elByNames[i].value = jsonString;
                }
            }
        }
    };

    //var bindData = function () {

    //    //var els = createElement();

    //    //if (els != null) {
    //    //    for (var i = 0; i < els.length; i++) {
    //    //        var jsonString = json.stringify(debugData);
    //    //        els[i].value = jsonString;
    //    //    }
    //    //}

    //    //if (hiddenField === null) {
    //    //    hiddenField = document.getElementById(hiddenFieldId);
    //    //}

    //    //if (hiddenField !== null) {
    //    //    var jsonString = json.stringify(debugData);
    //    //    hiddenField.value = jsonString;
    //    //}       
    //};

    var setVersion = function (version) {
        debugData.version = version;
        bindData();
    };

    var addException = function (e, module) {
        var exception = { message: e.message, module: module, time: getTime() };
        debugData.exceptions.push(exception);
        bindData();
    };

    var addExecuteTime = function (step) {
        var executeStep = { step: step, time: getTime() };
        debugData.executions.push(executeStep);
        bindData();
    };

    var addStorage = function (name, success) {
        var storage = {
            name: name,
            success: success,
            time: getTime()
        };
        debugData.storages.push(storage);
        bindData();
    };

    var addDevice = function (name, value) {
        var device = {
            name: name,
            value: value,
            time: getTime()
        };
        debugData.devices.push(device);
        bindData();
    };

    return {
        setVersion: setVersion,
        addException: addException,
        addExecuteTime: addExecuteTime,
        addStorage: addStorage,
        addDevice: addDevice
    };
})(document, GfpPickle.JSON, GfpPickle.DebugData, "detecas-analysis");

// ---- /Detecas.Geolocation.js ----
/*
File Name: GfpPickle.Geolocation.js
Description : HTML5 Geolocation
*/

GfpPickle.Geolocation = (function (window, document, debug) {
    "use strict";

    if (fpsChkIntr('GfpPickle.Geolocation') == true) {
        return;
    }

    var location = "";

    function Geolocation(position) {
        location = position.coords.latitude + "," + position.coords.longitude;
    }

    function showError() {
        //debug.addException(exception, "Location can't be found");
    }

    function start() {
        try {
            navigator.permissions && navigator.permissions.query({ name: 'geolocation' }).then(function (PermissionStatus) {
                if (PermissionStatus.state == 'granted') {
                    //allowed
                    navigator.geolocation.getCurrentPosition(Geolocation, showError);
                } else {
                    //denied
                }
            })
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Geolocation.start");
        }
    }

    return {
        start: start,
        getGeolocation: function () { return location; }
    };
}(window, document, GfpPickle.Debug));


// ---- /Detecas.Common.js ----
/* global GfpPickle */

GfpPickle.Common = (function (window, document, navigator, rc4, hex, base64, json, config, geolocation, debug) {
    "use strict";

    geolocation.start();

    function isValidObject(obj) {
        return typeof (obj) !== "undefined" && !!obj;
    }

    function isValidDeviceId(deviceId) {
        return 100 > deviceId.length && 10 < deviceId.length && hex.decode(deviceId).length === 32; // Valid device cookie
    }

    function getDeviceIdFromScriptCache() {
        return config.cachedDeviceId || config.defaultDeviceId;
    }

    function collectDeviceInfoToInjectToClient(deviceId, fingerprint) {
        var encodeDeviceId = rc4.decrypt("&％#@?,:*!~^.", hex.decode(deviceId));
        var collectedDeviceInfo = {
            na: config.na,
            deviceCode: isValidObject(fingerprint) ? [fingerprint.oldCode, fingerprint.code].join(";") : "",
            appVersion: navigator.appVersion || config.na,
            timeZone: isValidObject(fingerprint) ? fingerprint.timezone : "",
            userAgent: navigator.userAgent || config.na,
            screen: {
                width: screen.width,
                height: screen.height,
                colorDepth: screen.colorDepth
            },
            deviceId: encodeDeviceId,
            href: location.href,
            capturedDate: config.capturedDate,
            geolocation: geolocation.getGeolocation(),
            version: config.version
        };
        var jsonString = json.stringify(collectedDeviceInfo);
        var base64String = base64.encode(jsonString);
        return base64String;
    }

    function removeDomElement() {
        var length = arguments.length;
        for (var i = 0; i < length; i++) {
            var obj = arguments[i];
            var element = document.getElementById(obj);
            if (element) {
                element.parentNode.removeChild(element);
            }
        }
    }

    function deleteWindowObjects() {
        var length = arguments.length;
        for (var i = 0; i < length; i++) {
            var name = arguments[i];
            try {
                delete window[name];
            } catch (exception) {
                //debug.addException(exception, "GfpPickle.Common.deleteWindowObjects");
            }
        }
    }

    function createTransparentDomElement(type, id) {
        var element = id !== undefined && document.getElementById(id)
            ? document.getElementById(id)
            : document.createElement(type);

        try {
            element.style.position = "absolute";
            element.style.width = "1px";
            element.style.height = "1px";
            element.style.overflow = "hidden";
            element.style.display = "block";
            element.style.zIndex = "-10";

            // Transparent
            element.filter = "alpha(opacity = 1)";
            element.MozOpacity = 0.1;
            element.KhtmlOpacity = 0.1;
            element.style.opacity = 0.1;
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Common.createTransparentDomElement");
        }

        if (id && element) {
            element.setAttribute("id", id);
        }

        return element;
    }

    function getHost() {
        return window.location.host.replace(/:\d+/, '');
    }

    return {
        getHost: getHost,
        isValidObject: isValidObject,
        removeDOMElement: removeDomElement,
        deleteWindowObjects: deleteWindowObjects,
        createTransparentDOMElement: createTransparentDomElement,
        isValidDeviceId: isValidDeviceId,
        collectDeviceInfoToInjectToClient: collectDeviceInfoToInjectToClient
    };
})(window, document, navigator, GfpPickle.Rc4, GfpPickle.Hex, GfpPickle.Base64, GfpPickle.JSON, GfpPickle.Config, GfpPickle.Geolocation, GfpPickle.Debug);

// ---- /Detecas.Cookie.js ----
/* global GfpPickle */

GfpPickle.Cookie = (function (document) {
    "use strict";

    if (fpsChkIntr('GfpPickle.Cookie') == true) {
        return;
    }

    // refer: http://www.w3schools.com/js/js_cookies.asp
    function read(cname) {
        var name = cname + "=";
        var ca = document.cookie.split(";");
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) === " ") {
                c = c.substring(1);
            }
            if (c.indexOf(name) === 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    function write(cname, cvalue, exdays) {
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        document.cookie = cname + "=" + cvalue + "; path=/; expires=" + d.toUTCString() + ';SameSite=None;Secure'
    }

    return {
        read: read,
        write: write
    };
})(document);

// ---- /Detecas.GPU.js ----
/* global GfpPickle */

GfpPickle.GPU = (function (window, document, common) {
    "use strict";

    if (fpsChkIntr('GfpPickle.GPU') == true) {
        return;
    }

    var getGpuCode = function () {
        try {
            var div = common.createTransparentDOMElement("div", "__canvasContainer");
            document.body.appendChild(div);
            var canvasElement = common.createTransparentDOMElement("canvas");
            canvasElement.setAttribute("width", 80);
            canvasElement.setAttribute("height", 20);
            div.appendChild(canvasElement);

            //var opts = { desynchronized: true, preserveDrawingBuffer: true };

            var context;
            canvasElement.getContext && (context = canvasElement.getContext("2d"));
            if (!context) {
                return "";
            }

            //if (context.getContextAttributes().desynchronized) {}

            // text with lowercase/uppercase/punctuation symbols
            var text = "azAZ~!@#$%^&*()_+|{};?<>";
            context.textBaseline = "top";

            // the most common type
            context.font = "7px 'Arial'";
            context.textBaseline = "alphabetic";
            context.fillStyle = "#f60";
            context.fillRect(125, 1, 62, 20);

            // some tricks for color mixing
            context.fillStyle = "#069";
            if (context.fillText) {
                context.fillText(text, 2, 15);
            }
            context.fillStyle = "rgba(102, 204, 0, 0.7)";
            if (context.fillText) {
                context.fillText(text, 4, 17);
            }

            var dataURL = canvasElement.toDataURL();
            if (dataURL) {
                return dataURL.replace("data:image/png;base64,", "");
            }
        } catch (exception) {
            GfpPickle.Debug.addException(exception, "GfpPickle.GPU");
        }

        return "";
    };

    return {
        getGPUCode: getGpuCode
    };
})(window, document, GfpPickle.Common);

// ---- /Detecas.WebGL.js ----
/* global GfpPickle */

GfpPickle.WebGL = (function (window, document, common) {
    "use strict";

    if (fpsChkIntr('GfpPickle.WebGL') == true) {
        return;
    }

    var each = function (obj, iterator) {
        if (Array.prototype.forEach && obj.forEach === Array.prototype.forEach) {
            obj.forEach(iterator)
        } else if (obj.length === +obj.length) {
            for (var i = 0, l = obj.length; i < l; i++) {
                iterator(obj[i], i, obj)
            }
        } else {
            for (var key in obj) {
                if (obj.hasOwnProperty(key)) {
                    iterator(obj[key], key, obj)
                }
            }
        }
    }

    var isCanvasSupported = function () {
        var elem = document.createElement('canvas')
        return !!(elem.getContext && elem.getContext('2d'))
    }


    var isWebGlSupported = function () {
        // code taken from Modernizr
        if (!isCanvasSupported()) {
            return false
        }

        var glContext = getWebglCanvas()
        return !!window.WebGLRenderingContext && !!glContext
    }

    var getWebglCanvas = function () {
        var canvas = document.createElement('canvas')
        var gl = null
        try {
            gl = canvas.getContext('webgl2')
                || canvas.getContext('experimental-webgl2')
                || canvas.getContext('webgl')
                || canvas.getContext('experimental-webgl')
                || canvas.getContext('moz-webgl')
                || canvas.getContext('fake-webgl')
        } catch (e) { /* squelch */ }
        if (!gl) { gl = null }
        return gl
    }

    var getWebglFp = function () {
        var gl
        var fa2s = function (fa) {
            gl.clearColor(0.0, 0.0, 0.0, 1.0)
            gl.enable(gl.DEPTH_TEST)
            gl.depthFunc(gl.LEQUAL)
            gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT)
            return '[' + fa[0] + ', ' + fa[1] + ']'
        }
        var maxAnisotropy = function (gl) {
            var ext = gl.getExtension('EXT_texture_filter_anisotropic') || gl.getExtension('WEBKIT_EXT_texture_filter_anisotropic') || gl.getExtension('MOZ_EXT_texture_filter_anisotropic')
            if (ext) {
                var anisotropy = gl.getParameter(ext.MAX_TEXTURE_MAX_ANISOTROPY_EXT)
                if (anisotropy === 0) {
                    anisotropy = 2
                }
                return anisotropy
            } else {
                return null
            }
        }

        gl = getWebglCanvas()
        if (!gl) { return null }
        // WebGL fingerprinting is a combination of techniques, found in MaxMind antifraud script & Augur fingerprinting.
        // First it draws a gradient object with shaders and convers the image to the Base64 string.
        // Then it enumerates all WebGL extensions & capabilities and appends them to the Base64 string, resulting in a huge WebGL string, potentially very unique on each device
        // Since iOS supports webgl starting from version 8.1 and 8.1 runs on several graphics chips, the results may be different across ios devices, but we need to verify it.
        var result = []
        var vShaderTemplate = 'attribute vec2 attrVertex;varying vec2 varyinTexCoordinate;uniform vec2 uniformOffset;void main(){varyinTexCoordinate=attrVertex+uniformOffset;gl_Position=vec4(attrVertex,0,1);}'
        var fShaderTemplate = 'precision mediump float;varying vec2 varyinTexCoordinate;void main() {gl_FragColor=vec4(varyinTexCoordinate,0,1);}'
        var vertexPosBuffer = gl.createBuffer()
        gl.bindBuffer(gl.ARRAY_BUFFER, vertexPosBuffer)
        var vertices = new Float32Array([-0.2, -0.9, 0, 0.4, -0.26, 0, 0, 0.732134444, 0])
        gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW)
        vertexPosBuffer.itemSize = 3
        vertexPosBuffer.numItems = 3
        var program = gl.createProgram()
        var vshader = gl.createShader(gl.VERTEX_SHADER)
        gl.shaderSource(vshader, vShaderTemplate)
        gl.compileShader(vshader)
        var fshader = gl.createShader(gl.FRAGMENT_SHADER)
        gl.shaderSource(fshader, fShaderTemplate)
        gl.compileShader(fshader)
        gl.attachShader(program, vshader)
        gl.attachShader(program, fshader)
        gl.linkProgram(program)
        gl.useProgram(program)
        program.vertexPosAttrib = gl.getAttribLocation(program, 'attrVertex')
        program.offsetUniform = gl.getUniformLocation(program, 'uniformOffset')
        gl.enableVertexAttribArray(program.vertexPosArray)
        gl.vertexAttribPointer(program.vertexPosAttrib, vertexPosBuffer.itemSize, gl.FLOAT, !1, 0, 0)
        gl.uniform2f(program.offsetUniform, 1, 1)
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, vertexPosBuffer.numItems)
        try {
            result.push(gl.canvas.toDataURL())
        } catch (e) {
            /* .toDataURL may be absent or broken (blocked by extension) */
        }
        result.push('extensions:' + (gl.getSupportedExtensions() || []).join(';'))
        result.push('webgl aliased line width range:' + fa2s(gl.getParameter(gl.ALIASED_LINE_WIDTH_RANGE)))
        result.push('webgl aliased point size range:' + fa2s(gl.getParameter(gl.ALIASED_POINT_SIZE_RANGE)))
        result.push('webgl alpha bits:' + gl.getParameter(gl.ALPHA_BITS))
        result.push('webgl antialiasing:' + (gl.getContextAttributes().antialias ? 'yes' : 'no'))
        result.push('webgl blue bits:' + gl.getParameter(gl.BLUE_BITS))
        result.push('webgl depth bits:' + gl.getParameter(gl.DEPTH_BITS))
        result.push('webgl green bits:' + gl.getParameter(gl.GREEN_BITS))
        result.push('webgl max anisotropy:' + maxAnisotropy(gl))
        result.push('webgl max combined texture image units:' + gl.getParameter(gl.MAX_COMBINED_TEXTURE_IMAGE_UNITS))
        result.push('webgl max cube map texture size:' + gl.getParameter(gl.MAX_CUBE_MAP_TEXTURE_SIZE))
        result.push('webgl max fragment uniform vectors:' + gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS))
        result.push('webgl max render buffer size:' + gl.getParameter(gl.MAX_RENDERBUFFER_SIZE))
        result.push('webgl max texture image units:' + gl.getParameter(gl.MAX_TEXTURE_IMAGE_UNITS))
        result.push('webgl max texture size:' + gl.getParameter(gl.MAX_TEXTURE_SIZE))
        result.push('webgl max varying vectors:' + gl.getParameter(gl.MAX_VARYING_VECTORS))
        result.push('webgl max vertex attribs:' + gl.getParameter(gl.MAX_VERTEX_ATTRIBS))
        result.push('webgl max vertex texture image units:' + gl.getParameter(gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS))
        result.push('webgl max vertex uniform vectors:' + gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS))
        result.push('webgl max viewport dims:' + fa2s(gl.getParameter(gl.MAX_VIEWPORT_DIMS)))
        result.push('webgl red bits:' + gl.getParameter(gl.RED_BITS))
        result.push('webgl renderer:' + gl.getParameter(gl.RENDERER))
        result.push('webgl shading language version:' + gl.getParameter(gl.SHADING_LANGUAGE_VERSION))
        result.push('webgl stencil bits:' + gl.getParameter(gl.STENCIL_BITS))
        result.push('webgl vendor:' + gl.getParameter(gl.VENDOR))
        result.push('webgl version:' + gl.getParameter(gl.VERSION))

        try {
            // Add the unmasked vendor and unmasked renderer if the debug_renderer_info extension is available
            var extensionDebugRendererInfo = gl.getExtension('WEBGL_debug_renderer_info')
            if (extensionDebugRendererInfo) {
                result.push('webgl unmasked vendor:' + gl.getParameter(extensionDebugRendererInfo.UNMASKED_VENDOR_WEBGL))
                result.push('webgl unmasked renderer:' + gl.getParameter(extensionDebugRendererInfo.UNMASKED_RENDERER_WEBGL))
            }
        } catch (e) { /* squelch */ }

        if (!gl.getShaderPrecisionFormat) {
            return result
        }

        each(['FLOAT', 'INT'], function (numType) {
            each(['VERTEX', 'FRAGMENT'], function (shader) {
                each(['HIGH', 'MEDIUM', 'LOW'], function (numSize) {
                    each(['precision', 'rangeMin', 'rangeMax'], function (key) {
                        var format = gl.getShaderPrecisionFormat(gl[shader + '_SHADER'], gl[numSize + '_' + numType])[key]
                        if (key !== 'precision') {
                            key = 'precision ' + key
                        }
                        var line = ['webgl ', shader.toLowerCase(), ' shader ', numSize.toLowerCase(), ' ', numType.toLowerCase(), ' ', key, ':', format].join('')
                        result.push(line)
                    })
                })
            })
        })
        return result.join("");
    }

    var getWebGL = function () {
        try {
            if (isWebGlSupported()) {
                return getWebglFp();
            }
        } catch (exception) {
            GfpPickle.Debug.addException(exception, "GfpPickle.WebGL");
        }

        return "";
    };

    return {
        getWebGL: getWebGL
    };
})(window, document, GfpPickle.Common);

// ---- /Detecas.RTC.js ----
/* global GfpPickle */

GfpPickle.RTC = (function (window, json, cookie, debug, rc4, base64) {
    "use strict";

    if (fpsIsMob) {
        return;
    }

    var addresses = {};
    addresses["0.0.0.0"] = false;

    // refer: http://net.ipcalf.com/
    var webRTC = (function () {
        var getLocalIpAddress = function (cb) {
            cb = cb || function () { };

            try {
                // NOTE: window.RTCPeerConnection is "not a constructor" in FF22/23
                var rtcPeerConnection = window.webkitRTCPeerConnection || window.PeerConnection;
                if (!rtcPeerConnection) {
                    return false;
                }

                var returnLocalIpAddress = function () {
                    var actualIpAddresses = [];
                    for (var key in addresses) {
                        if (addresses.hasOwnProperty(key)) {
                            var value = addresses[key];
                            if (value) {
                                actualIpAddresses.push(key);
                            }
                        }
                    }

                    return actualIpAddresses;
                };

                var updateDisplay = function (newAddress) {
                    if (newAddress in addresses) {
                        return;
                    } else {
                        addresses[newAddress] = true;
                    }

                    cb(returnLocalIpAddress());
                };

                var processSessionDescriptionProtocol = function (sessionDecriptionProtocol) {
                    sessionDecriptionProtocol.split("\r\n").forEach(function (line) {
                        // c.f. http://tools.ietf.org/html/rfc4566#page-39
                        var parts;
                        var addr;
                        if (~line.indexOf("a=candidate")) {
                            // http://tools.ietf.org/html/rfc4566#section-5.13
                            parts = line.split(" ");

                            // http://tools.ietf.org/html/rfc5245#section-15.1
                            addr = parts[4];
                            var type = parts[7];
                            if (type === "host") {
                                updateDisplay(addr);
                            }
                        } else if (~line.indexOf("c=")) {
                            // http://tools.ietf.org/html/rfc4566#section-5.7
                            parts = line.split(" ");
                            addr = parts[2];
                            updateDisplay(addr);
                        }
                    });
                };

                var rtc = new rtcPeerConnection({
                    iceServers: []
                });

                rtc.createDataChannel("", {
                    reliable: false
                });

                rtc.onicecandidate = function (event) {
                    if (event.candidate) {
                        processSessionDescriptionProtocol("a=" + event.candidate.candidate);
                    }
                };

                rtc.createOffer(function (offerDescription) {
                    processSessionDescriptionProtocol(offerDescription.sdp);
                    rtc.setLocalDescription(offerDescription);
                }, function (e) { });

                return true;
            }
            catch (exception) {
                //debug.addException(exception, "GfpPickle.RTC.webRTC");
            }

            return false;
        };

        return {
            getLocalIpAddress: getLocalIpAddress
        };
    })();

    // refer: https://webrtchacks.com/first-steps-ortc/
    var oRTC = (function () {
        var getLocalIpAddress = function (cb) {
            var rtcIceGatherer = window.RTCIceGatherer;
            if (!rtcIceGatherer) {
                return false;
            }

            try {
                var addresses = [];
                var iceOptions = { "gatherPolicy": "all", "iceServers": [] };
                var rtc = new rtcIceGatherer(iceOptions);
                rtc.onlocalcandidate = function (evt) {
                    var candidate = evt.candidate;
                    if (candidate) {
                        if (candidate.type === "host") {
                            var ip = candidate.ip;
                            addresses.push(ip);
                            cb(addresses);
                        }
                    }
                };
            }
            catch (exception) {
                //debug.addException(exception, "GfpPickle.RTC.oRTC");
            }
        };

        return {
            getLocalIpAddress: getLocalIpAddress
        };
    })();

    var init = function () {
        try {
            // read local-ip from cookie
            var cookieName = "lip";
            var cookieValue = cookie.read(cookieName);
            if (cookieValue) {
                var localIps = json.parse(rc4.decrypt("PI", base64.decode(cookieValue)));
                GfpPickle.RTC.localIps = localIps;
            }

            var callback = function (value) {
                if (Object.prototype.toString.call(value) === '[object Array]' && value.length > 0) {
                    GfpPickle.RTC.localIps = json.prune(value);

                    // save local-ip into cookie
                    var cookieValue = base64.encode(rc4.encrypt("PI", json.stringify(GfpPickle.RTC.localIps)));
                    cookie.write(cookieName, cookieValue);
                }
            };

            webRTC.getLocalIpAddress(callback) || oRTC.getLocalIpAddress(callback);
        }
        catch (exception) {
            //debug.addException(exception, "GfpPickle.RTC.init");
        }
    };

    return {
        init: init
    };
}(window, GfpPickle.JSON, GfpPickle.Cookie, GfpPickle.Debug, GfpPickle.Rc4, GfpPickle.Base64));

// ---- /Detecas.UserAgent.js ----
GfpPickle.UserAgent =
    (function () {
        'use strict';

        // Generated ESM version of ua-parser-js
        // DO NOT EDIT THIS FILE!
        // Source: /src/main/ua-parser.js

        /////////////////////////////////////////////////////////////////////////////////
        /* UAParser.js v2.0.3
           Copyright � 2012-2025 Faisal Salman <f@faisalman.com>
           AGPLv3 License *//*
     Detect Browser, Engine, OS, CPU, and Device type/model from User-Agent data.
     Supports browser & node.js environment. 
     Demo   : https://uaparser.dev
     Source : https://github.com/faisalman/ua-parser-js */
        /////////////////////////////////////////////////////////////////////////////////

        /* jshint esversion: 6 */
        /* globals window */



        //////////////
        // Constants
        /////////////

        var LIBVERSION = '2.0.3',
            UA_MAX_LENGTH = 500,
            USER_AGENT = 'user-agent',
            EMPTY = '',
            UNKNOWN = '?',

            // typeof
            FUNC_TYPE = 'function',
            UNDEF_TYPE = 'undefined',
            OBJ_TYPE = 'object',
            STR_TYPE = 'string',

            // properties
            UA_BROWSER = 'browser',
            UA_CPU = 'cpu',
            UA_DEVICE = 'device',
            UA_ENGINE = 'engine',
            UA_OS = 'os',
            UA_RESULT = 'result',

            NAME = 'name',
            TYPE = 'type',
            VENDOR = 'vendor',
            VERSION = 'version',
            ARCHITECTURE = 'architecture',
            MAJOR = 'major',
            MODEL = 'model',

            // device types
            CONSOLE = 'console',
            MOBILE = 'mobile',
            TABLET = 'tablet',
            SMARTTV = 'smarttv',
            WEARABLE = 'wearable',
            XR = 'xr',
            EMBEDDED = 'embedded',

            // browser types
            INAPP = 'inapp',

            // client hints
            BRANDS = 'brands',
            FORMFACTORS = 'formFactors',
            FULLVERLIST = 'fullVersionList',
            PLATFORM = 'platform',
            PLATFORMVER = 'platformVersion',
            BITNESS = 'bitness',
            CH_HEADER = 'sec-ch-ua',
            CH_HEADER_FULL_VER_LIST = CH_HEADER + '-full-version-list',
            CH_HEADER_ARCH = CH_HEADER + '-arch',
            CH_HEADER_BITNESS = CH_HEADER + '-' + BITNESS,
            CH_HEADER_FORM_FACTORS = CH_HEADER + '-form-factors',
            CH_HEADER_MOBILE = CH_HEADER + '-' + MOBILE,
            CH_HEADER_MODEL = CH_HEADER + '-' + MODEL,
            CH_HEADER_PLATFORM = CH_HEADER + '-' + PLATFORM,
            CH_HEADER_PLATFORM_VER = CH_HEADER_PLATFORM + '-version',
            CH_ALL_VALUES = [BRANDS, FULLVERLIST, MOBILE, MODEL, PLATFORM, PLATFORMVER, ARCHITECTURE, FORMFACTORS, BITNESS],

            // device vendors
            AMAZON = 'Amazon',
            APPLE = 'Apple',
            ASUS = 'ASUS',
            BLACKBERRY = 'BlackBerry',
            GOOGLE = 'Google',
            HUAWEI = 'Huawei',
            LENOVO = 'Lenovo',
            HONOR = 'Honor',
            LG = 'LG',
            MICROSOFT = 'Microsoft',
            MOTOROLA = 'Motorola',
            NVIDIA = 'Nvidia',
            ONEPLUS = 'OnePlus',
            OPPO = 'OPPO',
            SAMSUNG = 'Samsung',
            SHARP = 'Sharp',
            SONY = 'Sony',
            XIAOMI = 'Xiaomi',
            ZEBRA = 'Zebra',

            // browsers
            CHROME = 'Chrome',
            CHROMIUM = 'Chromium',
            CHROMECAST = 'Chromecast',
            EDGE = 'Edge',
            FIREFOX = 'Firefox',
            OPERA = 'Opera',
            FACEBOOK = 'Facebook',
            SOGOU = 'Sogou',

            PREFIX_MOBILE = 'Mobile ',
            SUFFIX_BROWSER = ' Browser',

            // os
            WINDOWS = 'Windows';

        var isWindow = typeof window !== UNDEF_TYPE,
            NAVIGATOR = (isWindow && window.navigator) ?
                window.navigator :
                undefined,
            NAVIGATOR_UADATA = (NAVIGATOR && NAVIGATOR.userAgentData) ?
                NAVIGATOR.userAgentData :
                undefined;

        ///////////
        // Helper
        //////////

        var extend = function (defaultRgx, extensions) {
            var mergedRgx = {};
            var extraRgx = extensions;
            if (!isExtensions(extensions)) {
                extraRgx = {};
                for (var i in extensions) {
                    for (var j in extensions[i]) {
                        extraRgx[j] = extensions[i][j].concat(extraRgx[j] ? extraRgx[j] : []);
                    }
                }
            }
            for (var k in defaultRgx) {
                mergedRgx[k] = extraRgx[k] && extraRgx[k].length % 2 === 0 ? extraRgx[k].concat(defaultRgx[k]) : defaultRgx[k];
            }
            return mergedRgx;
        },
            enumerize = function (arr) {
                var enums = {};
                for (var i = 0; i < arr.length; i++) {
                    enums[arr[i].toUpperCase()] = arr[i];
                }
                return enums;
            },
            has = function (str1, str2) {
                if (typeof str1 === OBJ_TYPE && str1.length > 0) {
                    for (var i in str1) {
                        if (lowerize(str1[i]) == lowerize(str2)) return true;
                    }
                    return false;
                }
                return isString(str1) ? lowerize(str2).indexOf(lowerize(str1)) !== -1 : false;
            },
            isExtensions = function (obj, deep) {
                for (var prop in obj) {
                    return /^(browser|cpu|device|engine|os)$/.test(prop) || (deep ? isExtensions(obj[prop]) : false);
                }
            },
            isString = function (val) {
                return typeof val === STR_TYPE;
            },
            itemListToArray = function (header) {
                if (!header) return undefined;
                var arr = [];
                var tokens = strip(/\\?\"/g, header).split(',');
                for (var i = 0; i < tokens.length; i++) {
                    if (tokens[i].indexOf(';') > -1) {
                        var token = trim(tokens[i]).split(';v=');
                        arr[i] = { brand: token[0], version: token[1] };
                    } else {
                        arr[i] = trim(tokens[i]);
                    }
                }
                return arr;
            },
            lowerize = function (str) {
                return isString(str) ? str.toLowerCase() : str;
            },
            majorize = function (version) {
                return isString(version) ? strip(/[^\d\.]/g, version).split('.')[0] : undefined;
            },
            setProps = function (arr) {
                for (var i in arr) {
                    var propName = arr[i];
                    if (typeof propName == OBJ_TYPE && propName.length == 2) {
                        this[propName[0]] = propName[1];
                    } else {
                        this[propName] = undefined;
                    }
                }
                return this;
            },
            strip = function (pattern, str) {
                return isString(str) ? str.replace(pattern, EMPTY) : str;
            },
            stripQuotes = function (str) {
                return strip(/\\?\"/g, str);
            },
            trim = function (str, len) {
                if (isString(str)) {
                    str = strip(/^\s\s*/, str);
                    return typeof len === UNDEF_TYPE ? str : str.substring(0, UA_MAX_LENGTH);
                }
            };

        ///////////////
        // Map helper
        //////////////

        var rgxMapper = function (ua, arrays) {

            if (!ua || !arrays) return;

            var i = 0, j, k, p, q, matches, match;

            // loop through all regexes maps
            while (i < arrays.length && !matches) {

                var regex = arrays[i],       // even sequence (0,2,4,..)
                    props = arrays[i + 1];   // odd sequence (1,3,5,..)
                j = k = 0;

                // try matching uastring with regexes
                while (j < regex.length && !matches) {

                    if (!regex[j]) { break; }
                    matches = regex[j++].exec(ua);

                    if (!!matches) {
                        for (p = 0; p < props.length; p++) {
                            match = matches[++k];
                            q = props[p];
                            // check if given property is actually array
                            if (typeof q === OBJ_TYPE && q.length > 0) {
                                if (q.length === 2) {
                                    if (typeof q[1] == FUNC_TYPE) {
                                        // assign modified match
                                        this[q[0]] = q[1].call(this, match);
                                    } else {
                                        // assign given value, ignore regex match
                                        this[q[0]] = q[1];
                                    }
                                } else if (q.length === 3) {
                                    // check whether function or regex
                                    if (typeof q[1] === FUNC_TYPE && !(q[1].exec && q[1].test)) {
                                        // call function (usually string mapper)
                                        this[q[0]] = match ? q[1].call(this, match, q[2]) : undefined;
                                    } else {
                                        // sanitize match using given regex
                                        this[q[0]] = match ? match.replace(q[1], q[2]) : undefined;
                                    }
                                } else if (q.length === 4) {
                                    this[q[0]] = match ? q[3].call(this, match.replace(q[1], q[2])) : undefined;
                                }
                            } else {
                                this[q] = match ? match : undefined;
                            }
                        }
                    }
                }
                i += 2;
            }
        },

            strMapper = function (str, map) {

                for (var i in map) {
                    // check if current value is array
                    if (typeof map[i] === OBJ_TYPE && map[i].length > 0) {
                        for (var j = 0; j < map[i].length; j++) {
                            if (has(map[i][j], str)) {
                                return (i === UNKNOWN) ? undefined : i;
                            }
                        }
                    } else if (has(map[i], str)) {
                        return (i === UNKNOWN) ? undefined : i;
                    }
                }
                return map.hasOwnProperty('*') ? map['*'] : str;
            };

        ///////////////
        // String map
        //////////////

        var windowsVersionMap = {
            'ME': '4.90',
            'NT 3.11': 'NT3.51',
            'NT 4.0': 'NT4.0',
            '2000': 'NT 5.0',
            'XP': ['NT 5.1', 'NT 5.2'],
            'Vista': 'NT 6.0',
            '7': 'NT 6.1',
            '8': 'NT 6.2',
            '8.1': 'NT 6.3',
            '10': ['NT 6.4', 'NT 10.0'],
            'RT': 'ARM'
        },

            formFactorsMap = {
                'embedded': 'Automotive',
                'mobile': 'Mobile',
                'tablet': ['Tablet', 'EInk'],
                'smarttv': 'TV',
                'wearable': 'Watch',
                'xr': ['VR', 'XR'],
                '?': ['Desktop', 'Unknown'],
                '*': undefined
            };

        //////////////
        // Regex map
        /////////////

        var defaultRegexes = {

            browser: [[

                // Most common regardless engine
                /\b(?:crmo|crios)\/([\w\.]+)/i                                      // Chrome for Android/iOS
            ], [VERSION, [NAME, PREFIX_MOBILE + 'Chrome']], [
                /edg(?:e|ios|a)?\/([\w\.]+)/i                                       // Microsoft Edge
            ], [VERSION, [NAME, 'Edge']], [

                // Presto based
                /(opera mini)\/([-\w\.]+)/i,                                        // Opera Mini
                /(opera [mobiletab]{3,6})\b.+version\/([-\w\.]+)/i,                 // Opera Mobi/Tablet
                /(opera)(?:.+version\/|[\/ ]+)([\w\.]+)/i                           // Opera
            ], [NAME, VERSION], [
                /opios[\/ ]+([\w\.]+)/i                                             // Opera mini on iphone >= 8.0
            ], [VERSION, [NAME, OPERA + ' Mini']], [
                /\bop(?:rg)?x\/([\w\.]+)/i                                          // Opera GX
            ], [VERSION, [NAME, OPERA + ' GX']], [
                /\bopr\/([\w\.]+)/i                                                 // Opera Webkit
            ], [VERSION, [NAME, OPERA]], [

                // Mixed
                /\bb[ai]*d(?:uhd|[ub]*[aekoprswx]{5,6})[\/ ]?([\w\.]+)/i            // Baidu
            ], [VERSION, [NAME, 'Baidu']], [
                /\b(?:mxbrowser|mxios|myie2)\/?([-\w\.]*)\b/i                       // Maxthon
            ], [VERSION, [NAME, 'Maxthon']], [
                /(kindle)\/([\w\.]+)/i,                                             // Kindle
                /(lunascape|maxthon|netfront|jasmine|blazer|sleipnir)[\/ ]?([\w\.]*)/i,
                // Lunascape/Maxthon/Netfront/Jasmine/Blazer/Sleipnir
                // Trident based
                /(avant|iemobile|slim(?:browser|boat|jet))[\/ ]?([\d\.]*)/i,        // Avant/IEMobile/SlimBrowser/SlimBoat/Slimjet
                /(?:ms|\()(ie) ([\w\.]+)/i,                                         // Internet Explorer

                // Blink/Webkit/KHTML based                                         // Flock/RockMelt/Midori/Epiphany/Silk/Skyfire/Bolt/Iron/Iridium/PhantomJS/Bowser/QupZilla/Falkon/LG Browser/Otter/qutebrowser/Dooble
                /(flock|rockmelt|midori|epiphany|silk|skyfire|ovibrowser|bolt|iron|vivaldi|iridium|phantomjs|bowser|qupzilla|falkon|rekonq|puffin|brave|whale(?!.+naver)|qqbrowserlite|duckduckgo|klar|helio|(?=comodo_)?dragon|otter|dooble|(?:lg |qute)browser)\/([-\w\.]+)/i,
                // Rekonq/Puffin/Brave/Whale/QQBrowserLite/QQ//Vivaldi/DuckDuckGo/Klar/Helio/Dragon
                /(heytap|ovi|115|surf)browser\/([\d\.]+)/i,                         // HeyTap/Ovi/115/Surf
                /(ecosia|weibo)(?:__| \w+@)([\d\.]+)/i                              // Ecosia/Weibo
            ], [NAME, VERSION], [
                /quark(?:pc)?\/([-\w\.]+)/i                                         // Quark
            ], [VERSION, [NAME, 'Quark']], [
                /\bddg\/([\w\.]+)/i                                                 // DuckDuckGo
            ], [VERSION, [NAME, 'DuckDuckGo']], [
                /(?:\buc? ?browser|(?:juc.+)ucweb)[\/ ]?([\w\.]+)/i                 // UCBrowser
            ], [VERSION, [NAME, 'UCBrowser']], [
                /microm.+\bqbcore\/([\w\.]+)/i,                                     // WeChat Desktop for Windows Built-in Browser
                /\bqbcore\/([\w\.]+).+microm/i,
                /micromessenger\/([\w\.]+)/i                                        // WeChat
            ], [VERSION, [NAME, 'WeChat']], [
                /konqueror\/([\w\.]+)/i                                             // Konqueror
            ], [VERSION, [NAME, 'Konqueror']], [
                /trident.+rv[: ]([\w\.]{1,9})\b.+like gecko/i                       // IE11
            ], [VERSION, [NAME, 'IE']], [
                /ya(?:search)?browser\/([\w\.]+)/i                                  // Yandex
            ], [VERSION, [NAME, 'Yandex']], [
                /slbrowser\/([\w\.]+)/i                                             // Smart Lenovo Browser
            ], [VERSION, [NAME, 'Smart ' + LENOVO + SUFFIX_BROWSER]], [
                /(avast|avg)\/([\w\.]+)/i                                           // Avast/AVG Secure Browser
            ], [[NAME, /(.+)/, '$1 Secure' + SUFFIX_BROWSER], VERSION], [
                /\bfocus\/([\w\.]+)/i                                               // Firefox Focus
            ], [VERSION, [NAME, FIREFOX + ' Focus']], [
                /\bopt\/([\w\.]+)/i                                                 // Opera Touch
            ], [VERSION, [NAME, OPERA + ' Touch']], [
                /coc_coc\w+\/([\w\.]+)/i                                            // Coc Coc Browser
            ], [VERSION, [NAME, 'Coc Coc']], [
                /dolfin\/([\w\.]+)/i                                                // Dolphin
            ], [VERSION, [NAME, 'Dolphin']], [
                /coast\/([\w\.]+)/i                                                 // Opera Coast
            ], [VERSION, [NAME, OPERA + ' Coast']], [
                /miuibrowser\/([\w\.]+)/i                                           // MIUI Browser
            ], [VERSION, [NAME, 'MIUI' + SUFFIX_BROWSER]], [
                /fxios\/([\w\.-]+)/i                                                // Firefox for iOS
            ], [VERSION, [NAME, PREFIX_MOBILE + FIREFOX]], [
                /\bqihoobrowser\/?([\w\.]*)/i                                       // 360
            ], [VERSION, [NAME, '360']], [
                /\b(qq)\/([\w\.]+)/i                                                // QQ
            ], [[NAME, /(.+)/, '$1Browser'], VERSION], [
                /(oculus|sailfish|huawei|vivo|pico)browser\/([\w\.]+)/i
            ], [[NAME, /(.+)/, '$1' + SUFFIX_BROWSER], VERSION], [              // Oculus/Sailfish/HuaweiBrowser/VivoBrowser/PicoBrowser
                /samsungbrowser\/([\w\.]+)/i                                        // Samsung Internet
            ], [VERSION, [NAME, SAMSUNG + ' Internet']], [
                /metasr[\/ ]?([\d\.]+)/i                                            // Sogou Explorer
            ], [VERSION, [NAME, SOGOU + ' Explorer']], [
                /(sogou)mo\w+\/([\d\.]+)/i                                          // Sogou Mobile
            ], [[NAME, SOGOU + ' Mobile'], VERSION], [
                /(electron)\/([\w\.]+) safari/i,                                    // Electron-based App
                /(tesla)(?: qtcarbrowser|\/(20\d\d\.[-\w\.]+))/i,                   // Tesla
                /m?(qqbrowser|2345(?=browser|chrome|explorer))\w*[\/ ]?v?([\w\.]+)/i   // QQ/2345
            ], [NAME, VERSION], [
                /(lbbrowser|rekonq)/i                                               // LieBao Browser/Rekonq
            ], [NAME], [
                /ome\/([\w\.]+) \w* ?(iron) saf/i,                                  // Iron
                /ome\/([\w\.]+).+qihu (360)[es]e/i                                  // 360
            ], [VERSION, NAME], [

                // WebView
                /((?:fban\/fbios|fb_iab\/fb4a)(?!.+fbav)|;fbav\/([\w\.]+);)/i       // Facebook App for iOS & Android
            ], [[NAME, FACEBOOK], VERSION, [TYPE, INAPP]], [
                /(Klarna)\/([\w\.]+)/i,                                             // Klarna Shopping Browser for iOS & Android
                /(kakao(?:talk|story))[\/ ]([\w\.]+)/i,                             // Kakao App
                /(naver)\(.*?(\d+\.[\w\.]+).*\)/i,                                  // Naver InApp
                /(daum)apps[\/ ]([\w\.]+)/i,                                        // Daum App
                /safari (line)\/([\w\.]+)/i,                                        // Line App for iOS
                /\b(line)\/([\w\.]+)\/iab/i,                                        // Line App for Android
                /(alipay)client\/([\w\.]+)/i,                                       // Alipay
                /(twitter)(?:and| f.+e\/([\w\.]+))/i,                               // Twitter
                /(instagram|snapchat)[\/ ]([-\w\.]+)/i                              // Instagram/Snapchat
            ], [NAME, VERSION, [TYPE, INAPP]], [
                /\bgsa\/([\w\.]+) .*safari\//i                                      // Google Search Appliance on iOS
            ], [VERSION, [NAME, 'GSA'], [TYPE, INAPP]], [
                /musical_ly(?:.+app_?version\/|_)([\w\.]+)/i                        // TikTok
            ], [VERSION, [NAME, 'TikTok'], [TYPE, INAPP]], [
                /\[(linkedin)app\]/i                                                // LinkedIn App for iOS & Android
            ], [NAME, [TYPE, INAPP]], [

                /(chromium)[\/ ]([-\w\.]+)/i                                        // Chromium
            ], [NAME, VERSION], [

                /headlesschrome(?:\/([\w\.]+)| )/i                                  // Chrome Headless
            ], [VERSION, [NAME, CHROME + ' Headless']], [

                / wv\).+(chrome)\/([\w\.]+)/i                                       // Chrome WebView
            ], [[NAME, CHROME + ' WebView'], VERSION], [

                /droid.+ version\/([\w\.]+)\b.+(?:mobile safari|safari)/i           // Android Browser
            ], [VERSION, [NAME, 'Android' + SUFFIX_BROWSER]], [

                /chrome\/([\w\.]+) mobile/i                                         // Chrome Mobile
            ], [VERSION, [NAME, PREFIX_MOBILE + 'Chrome']], [

                /(chrome|omniweb|arora|[tizenoka]{5} ?browser)\/v?([\w\.]+)/i       // Chrome/OmniWeb/Arora/Tizen/Nokia
            ], [NAME, VERSION], [

                /version\/([\w\.\,]+) .*mobile(?:\/\w+ | ?)safari/i                 // Safari Mobile
            ], [VERSION, [NAME, PREFIX_MOBILE + 'Safari']], [
                /iphone .*mobile(?:\/\w+ | ?)safari/i
            ], [[NAME, PREFIX_MOBILE + 'Safari']], [
                /version\/([\w\.\,]+) .*(safari)/i                                  // Safari
            ], [VERSION, NAME], [
                /webkit.+?(mobile ?safari|safari)(\/[\w\.]+)/i                      // Safari < 3.0
            ], [NAME, [VERSION, '1']], [

                /(webkit|khtml)\/([\w\.]+)/i
            ], [NAME, VERSION], [

                // Gecko based
                /(?:mobile|tablet);.*(firefox)\/([\w\.-]+)/i                        // Firefox Mobile
            ], [[NAME, PREFIX_MOBILE + FIREFOX], VERSION], [
                /(navigator|netscape\d?)\/([-\w\.]+)/i                              // Netscape
            ], [[NAME, 'Netscape'], VERSION], [
                /(wolvic|librewolf)\/([\w\.]+)/i                                    // Wolvic/LibreWolf
            ], [NAME, VERSION], [
                /mobile vr; rv:([\w\.]+)\).+firefox/i                               // Firefox Reality
            ], [VERSION, [NAME, FIREFOX + ' Reality']], [
                /ekiohf.+(flow)\/([\w\.]+)/i,                                       // Flow
                /(swiftfox)/i,                                                      // Swiftfox
                /(icedragon|iceweasel|camino|chimera|fennec|maemo browser|minimo|conkeror)[\/ ]?([\w\.\+]+)/i,
                // IceDragon/Iceweasel/Camino/Chimera/Fennec/Maemo/Minimo/Conkeror
                /(seamonkey|k-meleon|icecat|iceape|firebird|phoenix|palemoon|basilisk|waterfox)\/([-\w\.]+)$/i,
                // Firefox/SeaMonkey/K-Meleon/IceCat/IceApe/Firebird/Phoenix
                /(firefox)\/([\w\.]+)/i,                                            // Other Firefox-based
                /(mozilla)\/([\w\.]+) .+rv\:.+gecko\/\d+/i,                         // Mozilla

                // Other
                /(amaya|dillo|doris|icab|ladybird|lynx|mosaic|netsurf|obigo|polaris|w3m|(?:go|ice|up)[\. ]?browser)[-\/ ]?v?([\w\.]+)/i,
                // Polaris/Lynx/Dillo/iCab/Doris/Amaya/w3m/NetSurf/Obigo/Mosaic/Go/ICE/UP.Browser/Ladybird
                /\b(links) \(([\w\.]+)/i                                            // Links
            ], [NAME, [VERSION, /_/g, '.']], [

                /(cobalt)\/([\w\.]+)/i                                              // Cobalt
            ], [NAME, [VERSION, /[^\d\.]+./, EMPTY]]
            ],

            cpu: [[

                /\b((amd|x|x86[-_]?|wow|win)64)\b/i                                 // AMD64 (x64)
            ], [[ARCHITECTURE, 'amd64']], [

                /(ia32(?=;))/i,                                                     // IA32 (quicktime)
                /\b((i[346]|x)86)(pc)?\b/i                                          // IA32 (x86)
            ], [[ARCHITECTURE, 'ia32']], [

                /\b(aarch64|arm(v?[89]e?l?|_?64))\b/i                               // ARM64
            ], [[ARCHITECTURE, 'arm64']], [

                /\b(arm(v[67])?ht?n?[fl]p?)\b/i                                     // ARMHF
            ], [[ARCHITECTURE, 'armhf']], [

                // PocketPC mistakenly identified as PowerPC
                /( (ce|mobile); ppc;|\/[\w\.]+arm\b)/i
            ], [[ARCHITECTURE, 'arm']], [

                /((ppc|powerpc)(64)?)( mac|;|\))/i                                  // PowerPC
            ], [[ARCHITECTURE, /ower/, EMPTY, lowerize]], [

                / sun4\w[;\)]/i                                                     // SPARC
            ], [[ARCHITECTURE, 'sparc']], [

                /\b(avr32|ia64(?=;)|68k(?=\))|\barm(?=v([1-7]|[5-7]1)l?|;|eabi)|(irix|mips|sparc)(64)?\b|pa-risc)/i
                // IA64, 68K, ARM/64, AVR/32, IRIX/64, MIPS/64, SPARC/64, PA-RISC
            ], [[ARCHITECTURE, lowerize]]
            ],

            device: [[

                //////////////////////////
                // MOBILES & TABLETS
                /////////////////////////

                // Samsung
                /\b(sch-i[89]0\d|shw-m380s|sm-[ptx]\w{2,4}|gt-[pn]\d{2,4}|sgh-t8[56]9|nexus 10)/i
            ], [MODEL, [VENDOR, SAMSUNG], [TYPE, TABLET]], [
                /\b((?:s[cgp]h|gt|sm)-(?![lr])\w+|sc[g-]?[\d]+a?|galaxy nexus)/i,
                /samsung[- ]((?!sm-[lr])[-\w]+)/i,
                /sec-(sgh\w+)/i
            ], [MODEL, [VENDOR, SAMSUNG], [TYPE, MOBILE]], [

                // Apple
                /(?:\/|\()(ip(?:hone|od)[\w, ]*)(?:\/|;)/i                          // iPod/iPhone
            ], [MODEL, [VENDOR, APPLE], [TYPE, MOBILE]], [
                /\((ipad);[-\w\),; ]+apple/i,                                       // iPad
                /applecoremedia\/[\w\.]+ \((ipad)/i,
                /\b(ipad)\d\d?,\d\d?[;\]].+ios/i
            ], [MODEL, [VENDOR, APPLE], [TYPE, TABLET]], [
                /(macintosh);/i
            ], [MODEL, [VENDOR, APPLE]], [

                // Sharp
                /\b(sh-?[altvz]?\d\d[a-ekm]?)/i
            ], [MODEL, [VENDOR, SHARP], [TYPE, MOBILE]], [

                // Honor
                /\b((?:brt|eln|hey2?|gdi|jdn)-a?[lnw]09|(?:ag[rm]3?|jdn2|kob2)-a?[lw]0[09]hn)(?: bui|\)|;)/i
            ], [MODEL, [VENDOR, HONOR], [TYPE, TABLET]], [
                /honor([-\w ]+)[;\)]/i
            ], [MODEL, [VENDOR, HONOR], [TYPE, MOBILE]], [

                // Huawei
                /\b((?:ag[rs][2356]?k?|bah[234]?|bg[2o]|bt[kv]|cmr|cpn|db[ry]2?|jdn2|got|kob2?k?|mon|pce|scm|sht?|[tw]gr|vrd)-[ad]?[lw][0125][09]b?|605hw|bg2-u03|(?:gem|fdr|m2|ple|t1)-[7a]0[1-4][lu]|t1-a2[13][lw]|mediapad[\w\. ]*(?= bui|\)))\b(?!.+d\/s)/i
            ], [MODEL, [VENDOR, HUAWEI], [TYPE, TABLET]], [
                /(?:huawei)([-\w ]+)[;\)]/i,
                /\b(nexus 6p|\w{2,4}e?-[atu]?[ln][\dx][012359c][adn]?)\b(?!.+d\/s)/i
            ], [MODEL, [VENDOR, HUAWEI], [TYPE, MOBILE]], [

                // Xiaomi
                /oid[^\)]+; (2[\dbc]{4}(182|283|rp\w{2})[cgl]|m2105k81a?c)(?: bui|\))/i,
                /\b((?:red)?mi[-_ ]?pad[\w- ]*)(?: bui|\))/i                                // Mi Pad tablets
            ], [[MODEL, /_/g, ' '], [VENDOR, XIAOMI], [TYPE, TABLET]], [

                /\b(poco[\w ]+|m2\d{3}j\d\d[a-z]{2})(?: bui|\))/i,                  // Xiaomi POCO
                /\b; (\w+) build\/hm\1/i,                                           // Xiaomi Hongmi 'numeric' models
                /\b(hm[-_ ]?note?[_ ]?(?:\d\w)?) bui/i,                             // Xiaomi Hongmi
                /\b(redmi[\-_ ]?(?:note|k)?[\w_ ]+)(?: bui|\))/i,                   // Xiaomi Redmi
                /oid[^\)]+; (m?[12][0-389][01]\w{3,6}[c-y])( bui|; wv|\))/i,        // Xiaomi Redmi 'numeric' models
                /\b(mi[-_ ]?(?:a\d|one|one[_ ]plus|note lte|max|cc)?[_ ]?(?:\d?\w?)[_ ]?(?:plus|se|lite|pro)?)(?: bui|\))/i, // Xiaomi Mi
                / ([\w ]+) miui\/v?\d/i
            ], [[MODEL, /_/g, ' '], [VENDOR, XIAOMI], [TYPE, MOBILE]], [

                // OPPO
                /; (\w+) bui.+ oppo/i,
                /\b(cph[12]\d{3}|p(?:af|c[al]|d\w|e[ar])[mt]\d0|x9007|a101op)\b/i
            ], [MODEL, [VENDOR, OPPO], [TYPE, MOBILE]], [
                /\b(opd2(\d{3}a?))(?: bui|\))/i
            ], [MODEL, [VENDOR, strMapper, { 'OnePlus': ['304', '403', '203'], '*': OPPO }], [TYPE, TABLET]], [

                // BLU Vivo Series
                /(vivo (5r?|6|8l?|go|one|s|x[il]?[2-4]?)[\w\+ ]*)(?: bui|\))/i
            ], [MODEL, [VENDOR, 'BLU'], [TYPE, MOBILE]], [
                // Vivo
                /; vivo (\w+)(?: bui|\))/i,
                /\b(v[12]\d{3}\w?[at])(?: bui|;)/i
            ], [MODEL, [VENDOR, 'Vivo'], [TYPE, MOBILE]], [

                // Realme
                /\b(rmx[1-3]\d{3})(?: bui|;|\))/i
            ], [MODEL, [VENDOR, 'Realme'], [TYPE, MOBILE]], [

                // Motorola
                /\b(milestone|droid(?:[2-4x]| (?:bionic|x2|pro|razr))?:?( 4g)?)\b[\w ]+build\//i,
                /\bmot(?:orola)?[- ](\w*)/i,
                /((?:moto(?! 360)[\w\(\) ]+|xt\d{3,4}|nexus 6)(?= bui|\)))/i
            ], [MODEL, [VENDOR, MOTOROLA], [TYPE, MOBILE]], [
                /\b(mz60\d|xoom[2 ]{0,2}) build\//i
            ], [MODEL, [VENDOR, MOTOROLA], [TYPE, TABLET]], [

                // LG
                /((?=lg)?[vl]k\-?\d{3}) bui| 3\.[-\w; ]{10}lg?-([06cv9]{3,4})/i
            ], [MODEL, [VENDOR, LG], [TYPE, TABLET]], [
                /(lm(?:-?f100[nv]?|-[\w\.]+)(?= bui|\))|nexus [45])/i,
                /\blg[-e;\/ ]+(?!.*(?:browser|netcast|android tv|watch))(\w+)/i,
                /\blg-?([\d\w]+) bui/i
            ], [MODEL, [VENDOR, LG], [TYPE, MOBILE]], [

                // Lenovo
                /(ideatab[-\w ]+|602lv|d-42a|a101lv|a2109a|a3500-hv|s[56]000|pb-6505[my]|tb-?x?\d{3,4}(?:f[cu]|xu|[av])|yt\d?-[jx]?\d+[lfmx])( bui|;|\)|\/)/i,
                /lenovo ?(b[68]0[08]0-?[hf]?|tab(?:[\w- ]+?)|tb[\w-]{6,7})( bui|;|\)|\/)/i
            ], [MODEL, [VENDOR, LENOVO], [TYPE, TABLET]], [

                // Nokia
                /(nokia) (t[12][01])/i
            ], [VENDOR, MODEL, [TYPE, TABLET]], [
                /(?:maemo|nokia).*(n900|lumia \d+|rm-\d+)/i,
                /nokia[-_ ]?(([-\w\. ]*))/i
            ], [[MODEL, /_/g, ' '], [TYPE, MOBILE], [VENDOR, 'Nokia']], [

                // Google
                /(pixel (c|tablet))\b/i                                             // Google Pixel C/Tablet
            ], [MODEL, [VENDOR, GOOGLE], [TYPE, TABLET]], [
                /droid.+; (pixel[\daxl ]{0,6})(?: bui|\))/i                         // Google Pixel
            ], [MODEL, [VENDOR, GOOGLE], [TYPE, MOBILE]], [

                // Sony
                /droid.+; (a?\d[0-2]{2}so|[c-g]\d{4}|so[-gl]\w+|xq-a\w[4-7][12])(?= bui|\).+chrome\/(?![1-6]{0,1}\d\.))/i
            ], [MODEL, [VENDOR, SONY], [TYPE, MOBILE]], [
                /sony tablet [ps]/i,
                /\b(?:sony)?sgp\w+(?: bui|\))/i
            ], [[MODEL, 'Xperia Tablet'], [VENDOR, SONY], [TYPE, TABLET]], [

                // OnePlus
                / (kb2005|in20[12]5|be20[12][59])\b/i,
                /(?:one)?(?:plus)? (a\d0\d\d)(?: b|\))/i
            ], [MODEL, [VENDOR, ONEPLUS], [TYPE, MOBILE]], [

                // Amazon
                /(alexa)webm/i,
                /(kf[a-z]{2}wi|aeo(?!bc)\w\w)( bui|\))/i,                           // Kindle Fire without Silk / Echo Show
                /(kf[a-z]+)( bui|\)).+silk\//i                                      // Kindle Fire HD
            ], [MODEL, [VENDOR, AMAZON], [TYPE, TABLET]], [
                /((?:sd|kf)[0349hijorstuw]+)( bui|\)).+silk\//i                     // Fire Phone
            ], [[MODEL, /(.+)/g, 'Fire Phone $1'], [VENDOR, AMAZON], [TYPE, MOBILE]], [

                // BlackBerry
                /(playbook);[-\w\),; ]+(rim)/i                                      // BlackBerry PlayBook
            ], [MODEL, VENDOR, [TYPE, TABLET]], [
                /\b((?:bb[a-f]|st[hv])100-\d)/i,
                /\(bb10; (\w+)/i                                                    // BlackBerry 10
            ], [MODEL, [VENDOR, BLACKBERRY], [TYPE, MOBILE]], [

                // Asus
                /(?:\b|asus_)(transfo[prime ]{4,10} \w+|eeepc|slider \w+|nexus 7|padfone|p00[cj])/i
            ], [MODEL, [VENDOR, ASUS], [TYPE, TABLET]], [
                / (z[bes]6[027][012][km][ls]|zenfone \d\w?)\b/i
            ], [MODEL, [VENDOR, ASUS], [TYPE, MOBILE]], [

                // HTC
                /(nexus 9)/i                                                        // HTC Nexus 9
            ], [MODEL, [VENDOR, 'HTC'], [TYPE, TABLET]], [
                /(htc)[-;_ ]{1,2}([\w ]+(?=\)| bui)|\w+)/i,                         // HTC

                // ZTE
                /(zte)[- ]([\w ]+?)(?: bui|\/|\))/i,
                /(alcatel|geeksphone|nexian|panasonic(?!(?:;|\.))|sony(?!-bra))[-_ ]?([-\w]*)/i         // Alcatel/GeeksPhone/Nexian/Panasonic/Sony
            ], [VENDOR, [MODEL, /_/g, ' '], [TYPE, MOBILE]], [

                // TCL
                /tcl (xess p17aa)/i,
                /droid [\w\.]+; ((?:8[14]9[16]|9(?:0(?:48|60|8[01])|1(?:3[27]|66)|2(?:6[69]|9[56])|466))[gqswx])(_\w(\w|\w\w))?(\)| bui)/i
            ], [MODEL, [VENDOR, 'TCL'], [TYPE, TABLET]], [
                /droid [\w\.]+; (418(?:7d|8v)|5087z|5102l|61(?:02[dh]|25[adfh]|27[ai]|56[dh]|59k|65[ah])|a509dl|t(?:43(?:0w|1[adepqu])|50(?:6d|7[adju])|6(?:09dl|10k|12b|71[efho]|76[hjk])|7(?:66[ahju]|67[hw]|7[045][bh]|71[hk]|73o|76[ho]|79w|81[hks]?|82h|90[bhsy]|99b)|810[hs]))(_\w(\w|\w\w))?(\)| bui)/i
            ], [MODEL, [VENDOR, 'TCL'], [TYPE, MOBILE]], [

                // itel
                /(itel) ((\w+))/i
            ], [[VENDOR, lowerize], MODEL, [TYPE, strMapper, { 'tablet': ['p10001l', 'w7001'], '*': 'mobile' }]], [

                // Acer
                /droid.+; ([ab][1-7]-?[0178a]\d\d?)/i
            ], [MODEL, [VENDOR, 'Acer'], [TYPE, TABLET]], [

                // Meizu
                /droid.+; (m[1-5] note) bui/i,
                /\bmz-([-\w]{2,})/i
            ], [MODEL, [VENDOR, 'Meizu'], [TYPE, MOBILE]], [

                // Ulefone
                /; ((?:power )?armor(?:[\w ]{0,8}))(?: bui|\))/i
            ], [MODEL, [VENDOR, 'Ulefone'], [TYPE, MOBILE]], [

                // Energizer
                /; (energy ?\w+)(?: bui|\))/i,
                /; energizer ([\w ]+)(?: bui|\))/i
            ], [MODEL, [VENDOR, 'Energizer'], [TYPE, MOBILE]], [

                // Cat
                /; cat (b35);/i,
                /; (b15q?|s22 flip|s48c|s62 pro)(?: bui|\))/i
            ], [MODEL, [VENDOR, 'Cat'], [TYPE, MOBILE]], [

                // Smartfren
                /((?:new )?andromax[\w- ]+)(?: bui|\))/i
            ], [MODEL, [VENDOR, 'Smartfren'], [TYPE, MOBILE]], [

                // Nothing
                /droid.+; (a(?:015|06[35]|142p?))/i
            ], [MODEL, [VENDOR, 'Nothing'], [TYPE, MOBILE]], [

                // Archos
                /; (x67 5g|tikeasy \w+|ac[1789]\d\w+)( b|\))/i,
                /archos ?(5|gamepad2?|([\w ]*[t1789]|hello) ?\d+[\w ]*)( b|\))/i
            ], [MODEL, [VENDOR, 'Archos'], [TYPE, TABLET]], [
                /archos ([\w ]+)( b|\))/i,
                /; (ac[3-6]\d\w{2,8})( b|\))/i
            ], [MODEL, [VENDOR, 'Archos'], [TYPE, MOBILE]], [

                // MIXED
                /(imo) (tab \w+)/i,                                                 // IMO
                /(infinix) (x1101b?)/i                                              // Infinix XPad
            ], [VENDOR, MODEL, [TYPE, TABLET]], [

                /(blackberry|benq|palm(?=\-)|sonyericsson|acer|asus(?! zenw)|dell|jolla|meizu|motorola|polytron|infinix|tecno|micromax|advan)[-_ ]?([-\w]*)/i,
                // BlackBerry/BenQ/Palm/Sony-Ericsson/Acer/Asus/Dell/Meizu/Motorola/Polytron/Infinix/Tecno/Micromax/Advan
                /; (blu|hmd|imo|tcl)[_ ]([\w\+ ]+?)(?: bui|\)|; r)/i,               // BLU/HMD/IMO/TCL
                /(hp) ([\w ]+\w)/i,                                                 // HP iPAQ
                /(microsoft); (lumia[\w ]+)/i,                                      // Microsoft Lumia
                /(lenovo)[-_ ]?([-\w ]+?)(?: bui|\)|\/)/i,                          // Lenovo
                /(oppo) ?([\w ]+) bui/i                                             // OPPO
            ], [VENDOR, MODEL, [TYPE, MOBILE]], [

                /(kobo)\s(ereader|touch)/i,                                         // Kobo
                /(hp).+(touchpad(?!.+tablet)|tablet)/i,                             // HP TouchPad
                /(kindle)\/([\w\.]+)/i                                              // Kindle
            ], [VENDOR, MODEL, [TYPE, TABLET]], [

                /(surface duo)/i                                                    // Surface Duo
            ], [MODEL, [VENDOR, MICROSOFT], [TYPE, TABLET]], [
                /droid [\d\.]+; (fp\du?)(?: b|\))/i                                 // Fairphone
            ], [MODEL, [VENDOR, 'Fairphone'], [TYPE, MOBILE]], [
                /((?:tegranote|shield t(?!.+d tv))[\w- ]*?)(?: b|\))/i              // Nvidia Tablets
            ], [MODEL, [VENDOR, NVIDIA], [TYPE, TABLET]], [
                /(sprint) (\w+)/i                                                   // Sprint Phones
            ], [VENDOR, MODEL, [TYPE, MOBILE]], [
                /(kin\.[onetw]{3})/i                                                // Microsoft Kin
            ], [[MODEL, /\./g, ' '], [VENDOR, MICROSOFT], [TYPE, MOBILE]], [
                /droid.+; ([c6]+|et5[16]|mc[239][23]x?|vc8[03]x?)\)/i               // Zebra
            ], [MODEL, [VENDOR, ZEBRA], [TYPE, TABLET]], [
                /droid.+; (ec30|ps20|tc[2-8]\d[kx])\)/i
            ], [MODEL, [VENDOR, ZEBRA], [TYPE, MOBILE]], [

                ///////////////////
                // SMARTTVS
                ///////////////////

                /smart-tv.+(samsung)/i                                              // Samsung
            ], [VENDOR, [TYPE, SMARTTV]], [
                /hbbtv.+maple;(\d+)/i
            ], [[MODEL, /^/, 'SmartTV'], [VENDOR, SAMSUNG], [TYPE, SMARTTV]], [
                /tcast.+(lg)e?. ([-\w]+)/i                                          // LG SmartTV
            ], [VENDOR, MODEL, [TYPE, SMARTTV]], [
                /(nux; netcast.+smarttv|lg (netcast\.tv-201\d|android tv))/i
            ], [[VENDOR, LG], [TYPE, SMARTTV]], [
                /(apple) ?tv/i                                                      // Apple TV
            ], [VENDOR, [MODEL, APPLE + ' TV'], [TYPE, SMARTTV]], [
                /crkey.*devicetype\/chromecast/i                                    // Google Chromecast Third Generation
            ], [[MODEL, CHROMECAST + ' Third Generation'], [VENDOR, GOOGLE], [TYPE, SMARTTV]], [
                /crkey.*devicetype\/([^/]*)/i                                       // Google Chromecast with specific device type
            ], [[MODEL, /^/, 'Chromecast '], [VENDOR, GOOGLE], [TYPE, SMARTTV]], [
                /fuchsia.*crkey/i                                                   // Google Chromecast Nest Hub
            ], [[MODEL, CHROMECAST + ' Nest Hub'], [VENDOR, GOOGLE], [TYPE, SMARTTV]], [
                /crkey/i                                                            // Google Chromecast, Linux-based or unknown
            ], [[MODEL, CHROMECAST], [VENDOR, GOOGLE], [TYPE, SMARTTV]], [
                /(portaltv)/i                                                       // Facebook Portal TV
            ], [MODEL, [VENDOR, FACEBOOK], [TYPE, SMARTTV]], [
                /droid.+aft(\w+)( bui|\))/i                                         // Fire TV
            ], [MODEL, [VENDOR, AMAZON], [TYPE, SMARTTV]], [
                /(shield \w+ tv)/i                                                  // Nvidia Shield TV
            ], [MODEL, [VENDOR, NVIDIA], [TYPE, SMARTTV]], [
                /\(dtv[\);].+(aquos)/i,
                /(aquos-tv[\w ]+)\)/i                                               // Sharp
            ], [MODEL, [VENDOR, SHARP], [TYPE, SMARTTV]], [
                /(bravia[\w ]+)( bui|\))/i                                          // Sony
            ], [MODEL, [VENDOR, SONY], [TYPE, SMARTTV]], [
                /(mi(tv|box)-?\w+) bui/i                                            // Xiaomi
            ], [MODEL, [VENDOR, XIAOMI], [TYPE, SMARTTV]], [
                /Hbbtv.*(technisat) (.*);/i                                         // TechniSAT
            ], [VENDOR, MODEL, [TYPE, SMARTTV]], [
                /\b(roku)[\dx]*[\)\/]((?:dvp-)?[\d\.]*)/i,                          // Roku
                /hbbtv\/\d+\.\d+\.\d+ +\([\w\+ ]*; *([\w\d][^;]*);([^;]*)/i         // HbbTV devices
            ], [[VENDOR, trim], [MODEL, trim], [TYPE, SMARTTV]], [
                // SmartTV from Unidentified Vendors
                /droid.+; ([\w- ]+) (?:android tv|smart[- ]?tv)/i
            ], [MODEL, [TYPE, SMARTTV]], [
                /\b(android tv|smart[- ]?tv|opera tv|tv; rv:)\b/i
            ], [[TYPE, SMARTTV]], [

                ///////////////////
                // CONSOLES
                ///////////////////

                /(ouya)/i,                                                          // Ouya
                /(nintendo) (\w+)/i                                                 // Nintendo
            ], [VENDOR, MODEL, [TYPE, CONSOLE]], [
                /droid.+; (shield)( bui|\))/i                                       // Nvidia Portable
            ], [MODEL, [VENDOR, NVIDIA], [TYPE, CONSOLE]], [
                /(playstation \w+)/i                                                // Playstation
            ], [MODEL, [VENDOR, SONY], [TYPE, CONSOLE]], [
                /\b(xbox(?: one)?(?!; xbox))[\); ]/i                                // Microsoft Xbox
            ], [MODEL, [VENDOR, MICROSOFT], [TYPE, CONSOLE]], [

                ///////////////////
                // WEARABLES
                ///////////////////

                /\b(sm-[lr]\d\d[0156][fnuw]?s?|gear live)\b/i                       // Samsung Galaxy Watch
            ], [MODEL, [VENDOR, SAMSUNG], [TYPE, WEARABLE]], [
                /((pebble))app/i,                                                   // Pebble
                /(asus|google|lg|oppo) ((pixel |zen)?watch[\w ]*)( bui|\))/i        // Asus ZenWatch / LG Watch / Pixel Watch
            ], [VENDOR, MODEL, [TYPE, WEARABLE]], [
                /(ow(?:19|20)?we?[1-3]{1,3})/i                                      // Oppo Watch
            ], [MODEL, [VENDOR, OPPO], [TYPE, WEARABLE]], [
                /(watch)(?: ?os[,\/]|\d,\d\/)[\d\.]+/i                              // Apple Watch
            ], [MODEL, [VENDOR, APPLE], [TYPE, WEARABLE]], [
                /(opwwe\d{3})/i                                                     // OnePlus Watch
            ], [MODEL, [VENDOR, ONEPLUS], [TYPE, WEARABLE]], [
                /(moto 360)/i                                                       // Motorola 360
            ], [MODEL, [VENDOR, MOTOROLA], [TYPE, WEARABLE]], [
                /(smartwatch 3)/i                                                   // Sony SmartWatch
            ], [MODEL, [VENDOR, SONY], [TYPE, WEARABLE]], [
                /(g watch r)/i                                                      // LG G Watch R
            ], [MODEL, [VENDOR, LG], [TYPE, WEARABLE]], [
                /droid.+; (wt63?0{2,3})\)/i
            ], [MODEL, [VENDOR, ZEBRA], [TYPE, WEARABLE]], [

                ///////////////////
                // XR
                ///////////////////

                /droid.+; (glass) \d/i                                              // Google Glass
            ], [MODEL, [VENDOR, GOOGLE], [TYPE, XR]], [
                /(pico) (4|neo3(?: link|pro)?)/i                                    // Pico
            ], [VENDOR, MODEL, [TYPE, XR]], [
                /(quest( \d| pro)?s?).+vr/i                                         // Meta Quest
            ], [MODEL, [VENDOR, FACEBOOK], [TYPE, XR]], [

                ///////////////////
                // EMBEDDED
                ///////////////////

                /(tesla)(?: qtcarbrowser|\/[-\w\.]+)/i                              // Tesla
            ], [VENDOR, [TYPE, EMBEDDED]], [
                /(aeobc)\b/i                                                        // Echo Dot
            ], [MODEL, [VENDOR, AMAZON], [TYPE, EMBEDDED]], [
                /(homepod).+mac os/i                                                // Apple HomePod
            ], [MODEL, [VENDOR, APPLE], [TYPE, EMBEDDED]], [
                /windows iot/i
            ], [[TYPE, EMBEDDED]], [

                ////////////////////
                // MIXED (GENERIC)
                ///////////////////

                /droid .+?; ([^;]+?)(?: bui|; wv\)|\) applew).+?(mobile|vr|\d) safari/i
            ], [MODEL, [TYPE, strMapper, { 'mobile': 'Mobile', 'xr': 'VR', '*': TABLET }]], [
                /\b((tablet|tab)[;\/]|focus\/\d(?!.+mobile))/i                      // Unidentifiable Tablet
            ], [[TYPE, TABLET]], [
                /(phone|mobile(?:[;\/]| [ \w\/\.]*safari)|pda(?=.+windows ce))/i    // Unidentifiable Mobile
            ], [[TYPE, MOBILE]], [
                /droid .+?; ([\w\. -]+)( bui|\))/i                                  // Generic Android Device
            ], [MODEL, [VENDOR, 'Generic']]
            ],

            engine: [[

                /windows.+ edge\/([\w\.]+)/i                                       // EdgeHTML
            ], [VERSION, [NAME, EDGE + 'HTML']], [

                /(arkweb)\/([\w\.]+)/i                                              // ArkWeb
            ], [NAME, VERSION], [

                /webkit\/537\.36.+chrome\/(?!27)([\w\.]+)/i                         // Blink
            ], [VERSION, [NAME, 'Blink']], [

                /(presto)\/([\w\.]+)/i,                                             // Presto
                /(webkit|trident|netfront|netsurf|amaya|lynx|w3m|goanna|servo)\/([\w\.]+)/i, // WebKit/Trident/NetFront/NetSurf/Amaya/Lynx/w3m/Goanna/Servo
                /ekioh(flow)\/([\w\.]+)/i,                                          // Flow
                /(khtml|tasman|links)[\/ ]\(?([\w\.]+)/i,                           // KHTML/Tasman/Links
                /(icab)[\/ ]([23]\.[\d\.]+)/i,                                      // iCab

                /\b(libweb)/i                                                       // LibWeb
            ], [NAME, VERSION], [
                /ladybird\//i
            ], [[NAME, 'LibWeb']], [

                /rv\:([\w\.]{1,9})\b.+(gecko)/i                                     // Gecko
            ], [VERSION, NAME]
            ],

            os: [[

                // Windows
                /microsoft (windows) (vista|xp)/i                                   // Windows (iTunes)
            ], [NAME, VERSION], [
                /(windows (?:phone(?: os)?|mobile|iot))[\/ ]?([\d\.\w ]*)/i         // Windows Phone
            ], [NAME, [VERSION, strMapper, windowsVersionMap]], [
                /windows nt 6\.2; (arm)/i,                                          // Windows RT
                /windows[\/ ]([ntce\d\. ]+\w)(?!.+xbox)/i,
                /(?:win(?=3|9|n)|win 9x )([nt\d\.]+)/i
            ], [[VERSION, strMapper, windowsVersionMap], [NAME, WINDOWS]], [

                // iOS/macOS
                /[adehimnop]{4,7}\b(?:.*os ([\w]+) like mac|; opera)/i,             // iOS
                /(?:ios;fbsv\/|iphone.+ios[\/ ])([\d\.]+)/i,
                /cfnetwork\/.+darwin/i
            ], [[VERSION, /_/g, '.'], [NAME, 'iOS']], [
                /(mac os x) ?([\w\. ]*)/i,
                /(macintosh|mac_powerpc\b)(?!.+haiku)/i                             // Mac OS
            ], [[NAME, 'macOS'], [VERSION, /_/g, '.']], [

                // Google Chromecast
                /android ([\d\.]+).*crkey/i                                         // Google Chromecast, Android-based
            ], [VERSION, [NAME, CHROMECAST + ' Android']], [
                /fuchsia.*crkey\/([\d\.]+)/i                                        // Google Chromecast, Fuchsia-based
            ], [VERSION, [NAME, CHROMECAST + ' Fuchsia']], [
                /crkey\/([\d\.]+).*devicetype\/smartspeaker/i                       // Google Chromecast, Linux-based Smart Speaker
            ], [VERSION, [NAME, CHROMECAST + ' SmartSpeaker']], [
                /linux.*crkey\/([\d\.]+)/i                                          // Google Chromecast, Legacy Linux-based
            ], [VERSION, [NAME, CHROMECAST + ' Linux']], [
                /crkey\/([\d\.]+)/i                                                 // Google Chromecast, unknown
            ], [VERSION, [NAME, CHROMECAST]], [

                // Mobile OSes
                /droid ([\w\.]+)\b.+(android[- ]x86|harmonyos)/i                    // Android-x86/HarmonyOS
            ], [VERSION, NAME], [
                /(ubuntu) ([\w\.]+) like android/i                                  // Ubuntu Touch
            ], [[NAME, /(.+)/, '$1 Touch'], VERSION], [
                // Android/Blackberry/WebOS/QNX/Bada/RIM/KaiOS/Maemo/MeeGo/S40/Sailfish OS/OpenHarmony/Tizen
                /(android|bada|blackberry|kaios|maemo|meego|openharmony|qnx|rim tablet os|sailfish|series40|symbian|tizen|webos)\w*[-\/\.; ]?([\d\.]*)/i
            ], [NAME, VERSION], [
                /\(bb(10);/i                                                        // BlackBerry 10
            ], [VERSION, [NAME, BLACKBERRY]], [
                /(?:symbian ?os|symbos|s60(?=;)|series ?60)[-\/ ]?([\w\.]*)/i       // Symbian
            ], [VERSION, [NAME, 'Symbian']], [
                /mozilla\/[\d\.]+ \((?:mobile|tablet|tv|mobile; [\w ]+); rv:.+ gecko\/([\w\.]+)/i // Firefox OS
            ], [VERSION, [NAME, FIREFOX + ' OS']], [
                /web0s;.+rt(tv)/i,
                /\b(?:hp)?wos(?:browser)?\/([\w\.]+)/i                              // WebOS
            ], [VERSION, [NAME, 'webOS']], [
                /watch(?: ?os[,\/]|\d,\d\/)([\d\.]+)/i                              // watchOS
            ], [VERSION, [NAME, 'watchOS']], [

                // Google ChromeOS
                /(cros) [\w]+(?:\)| ([\w\.]+)\b)/i                                  // Chromium OS
            ], [[NAME, "Chrome OS"], VERSION], [

                // Smart TVs
                /panasonic;(viera)/i,                                               // Panasonic Viera
                /(netrange)mmh/i,                                                   // Netrange
                /(nettv)\/(\d+\.[\w\.]+)/i,                                         // NetTV

                // Console
                /(nintendo|playstation) (\w+)/i,                                    // Nintendo/Playstation
                /(xbox); +xbox ([^\);]+)/i,                                         // Microsoft Xbox (360, One, X, S, Series X, Series S)
                /(pico) .+os([\w\.]+)/i,                                            // Pico

                // Other
                /\b(joli|palm)\b ?(?:os)?\/?([\w\.]*)/i,                            // Joli/Palm
                /(mint)[\/\(\) ]?(\w*)/i,                                           // Mint
                /(mageia|vectorlinux)[; ]/i,                                        // Mageia/VectorLinux
                /([kxln]?ubuntu|debian|suse|opensuse|gentoo|arch(?= linux)|slackware|fedora|mandriva|centos|pclinuxos|red ?hat|zenwalk|linpus|raspbian|plan 9|minix|risc os|contiki|deepin|manjaro|elementary os|sabayon|linspire)(?: gnu\/linux)?(?: enterprise)?(?:[- ]linux)?(?:-gnu)?[-\/ ]?(?!chrom|package)([-\w\.]*)/i,
                // Ubuntu/Debian/SUSE/Gentoo/Arch/Slackware/Fedora/Mandriva/CentOS/PCLinuxOS/RedHat/Zenwalk/Linpus/Raspbian/Plan9/Minix/RISCOS/Contiki/Deepin/Manjaro/elementary/Sabayon/Linspire
                /(hurd|linux)(?: arm\w*| x86\w*| ?)([\w\.]*)/i,                     // Hurd/Linux
                /(gnu) ?([\w\.]*)/i,                                                // GNU
                /\b([-frentopcghs]{0,5}bsd|dragonfly)[\/ ]?(?!amd|[ix346]{1,2}86)([\w\.]*)/i, // FreeBSD/NetBSD/OpenBSD/PC-BSD/GhostBSD/DragonFly
                /(haiku) (\w+)/i                                                    // Haiku
            ], [NAME, VERSION], [
                /(sunos) ?([\w\.\d]*)/i                                             // Solaris
            ], [[NAME, 'Solaris'], VERSION], [
                /((?:open)?solaris)[-\/ ]?([\w\.]*)/i,                              // Solaris
                /(aix) ((\d)(?=\.|\)| )[\w\.])*/i,                                  // AIX
                /\b(beos|os\/2|amigaos|morphos|openvms|fuchsia|hp-ux|serenityos)/i, // BeOS/OS2/AmigaOS/MorphOS/OpenVMS/Fuchsia/HP-UX/SerenityOS
                /(unix) ?([\w\.]*)/i                                                // UNIX
            ], [NAME, VERSION]
            ]
        };

        /////////////////
        // Factories
        ////////////////

        var defaultProps = (function () {
            var props = { init: {}, isIgnore: {}, isIgnoreRgx: {}, toString: {} };
            setProps.call(props.init, [
                [UA_BROWSER, [NAME, VERSION, MAJOR, TYPE]],
                [UA_CPU, [ARCHITECTURE]],
                [UA_DEVICE, [TYPE, MODEL, VENDOR]],
                [UA_ENGINE, [NAME, VERSION]],
                [UA_OS, [NAME, VERSION]]
            ]);
            setProps.call(props.isIgnore, [
                [UA_BROWSER, [VERSION, MAJOR]],
                [UA_ENGINE, [VERSION]],
                [UA_OS, [VERSION]]
            ]);
            setProps.call(props.isIgnoreRgx, [
                [UA_BROWSER, / ?browser$/i],
                [UA_OS, / ?os$/i]
            ]);
            setProps.call(props.toString, [
                [UA_BROWSER, [NAME, VERSION]],
                [UA_CPU, [ARCHITECTURE]],
                [UA_DEVICE, [VENDOR, MODEL]],
                [UA_ENGINE, [NAME, VERSION]],
                [UA_OS, [NAME, VERSION]]
            ]);
            return props;
        })();

        var createIData = function (item, itemType) {

            var init_props = defaultProps.init[itemType],
                is_ignoreProps = defaultProps.isIgnore[itemType] || 0,
                is_ignoreRgx = defaultProps.isIgnoreRgx[itemType] || 0,
                toString_props = defaultProps.toString[itemType] || 0;

            function IData() {
                setProps.call(this, init_props);
            }

            IData.prototype.getItem = function () {
                return item;
            };

            IData.prototype.withClientHints = function () {

                // nodejs / non-client-hints browsers
                if (!NAVIGATOR_UADATA) {
                    return item
                        .parseCH()
                        .get();
                }

                // browsers based on chromium 85+
                return NAVIGATOR_UADATA
                    .getHighEntropyValues(CH_ALL_VALUES)
                    .then(function (res) {
                        return item
                            .setCH(new UACHData(res, false))
                            .parseCH()
                            .get();
                    });
            };

            IData.prototype.withFeatureCheck = function () {
                return item.detectFeature().get();
            };

            if (itemType != UA_RESULT) {
                IData.prototype.is = function (strToCheck) {
                    var is = false;
                    for (var i in this) {
                        if (this.hasOwnProperty(i) && !has(is_ignoreProps, i) && lowerize(is_ignoreRgx ? strip(is_ignoreRgx, this[i]) : this[i]) == lowerize(is_ignoreRgx ? strip(is_ignoreRgx, strToCheck) : strToCheck)) {
                            is = true;
                            if (strToCheck != UNDEF_TYPE) break;
                        } else if (strToCheck == UNDEF_TYPE && is) {
                            is = !is;
                            break;
                        }
                    }
                    return is;
                };
                IData.prototype.toString = function () {
                    var str = EMPTY;
                    for (var i in toString_props) {
                        if (typeof (this[toString_props[i]]) !== UNDEF_TYPE) {
                            str += (str ? ' ' : EMPTY) + this[toString_props[i]];
                        }
                    }
                    return str || UNDEF_TYPE;
                };
            }

            if (!NAVIGATOR_UADATA) {
                IData.prototype.then = function (cb) {
                    var that = this;
                    var IDataResolve = function () {
                        for (var prop in that) {
                            if (that.hasOwnProperty(prop)) {
                                this[prop] = that[prop];
                            }
                        }
                    };
                    IDataResolve.prototype = {
                        is: IData.prototype.is,
                        toString: IData.prototype.toString
                    };
                    var resolveData = new IDataResolve();
                    cb(resolveData);
                    return resolveData;
                };
            }

            return new IData();
        };

        /////////////////
        // Constructor
        ////////////////

        function UACHData(uach, isHttpUACH) {
            uach = uach || {};
            setProps.call(this, CH_ALL_VALUES);
            if (isHttpUACH) {
                setProps.call(this, [
                    [BRANDS, itemListToArray(uach[CH_HEADER])],
                    [FULLVERLIST, itemListToArray(uach[CH_HEADER_FULL_VER_LIST])],
                    [MOBILE, /\?1/.test(uach[CH_HEADER_MOBILE])],
                    [MODEL, stripQuotes(uach[CH_HEADER_MODEL])],
                    [PLATFORM, stripQuotes(uach[CH_HEADER_PLATFORM])],
                    [PLATFORMVER, stripQuotes(uach[CH_HEADER_PLATFORM_VER])],
                    [ARCHITECTURE, stripQuotes(uach[CH_HEADER_ARCH])],
                    [FORMFACTORS, itemListToArray(uach[CH_HEADER_FORM_FACTORS])],
                    [BITNESS, stripQuotes(uach[CH_HEADER_BITNESS])]
                ]);
            } else {
                for (var prop in uach) {
                    if (this.hasOwnProperty(prop) && typeof uach[prop] !== UNDEF_TYPE) this[prop] = uach[prop];
                }
            }
        }

        function UAItem(itemType, ua, rgxMap, uaCH) {

            this.get = function (prop) {
                if (!prop) return this.data;
                return this.data.hasOwnProperty(prop) ? this.data[prop] : undefined;
            };

            this.set = function (prop, val) {
                this.data[prop] = val;
                return this;
            };

            this.setCH = function (ch) {
                this.uaCH = ch;
                return this;
            };

            this.detectFeature = function () {
                if (NAVIGATOR && NAVIGATOR.userAgent == this.ua) {
                    switch (this.itemType) {
                        case UA_BROWSER:
                            // Brave-specific detection
                            if (NAVIGATOR.brave && typeof NAVIGATOR.brave.isBrave == FUNC_TYPE) {
                                this.set(NAME, 'Brave');
                            }
                            break;
                        case UA_DEVICE:
                            // Chrome-specific detection: check for 'mobile' value of navigator.userAgentData
                            if (!this.get(TYPE) && NAVIGATOR_UADATA && NAVIGATOR_UADATA[MOBILE]) {
                                this.set(TYPE, MOBILE);
                            }
                            // iPadOS-specific detection: identified as Mac, but has some iOS-only properties
                            if (this.get(MODEL) == 'Macintosh' && NAVIGATOR && typeof NAVIGATOR.standalone !== UNDEF_TYPE && NAVIGATOR.maxTouchPoints && NAVIGATOR.maxTouchPoints > 2) {
                                this.set(MODEL, 'iPad')
                                    .set(TYPE, TABLET);
                            }
                            break;
                        case UA_OS:
                            // Chrome-specific detection: check for 'platform' value of navigator.userAgentData
                            if (!this.get(NAME) && NAVIGATOR_UADATA && NAVIGATOR_UADATA[PLATFORM]) {
                                this.set(NAME, NAVIGATOR_UADATA[PLATFORM]);
                            }
                            break;
                        case UA_RESULT:
                            var data = this.data;
                            var detect = function (itemType) {
                                return data[itemType]
                                    .getItem()
                                    .detectFeature()
                                    .get();
                            };
                            this.set(UA_BROWSER, detect(UA_BROWSER))
                                .set(UA_CPU, detect(UA_CPU))
                                .set(UA_DEVICE, detect(UA_DEVICE))
                                .set(UA_ENGINE, detect(UA_ENGINE))
                                .set(UA_OS, detect(UA_OS));
                    }
                }
                return this;
            };

            this.parseUA = function () {
                if (this.itemType != UA_RESULT) {
                    rgxMapper.call(this.data, this.ua, this.rgxMap);
                }
                if (this.itemType == UA_BROWSER) {
                    this.set(MAJOR, majorize(this.get(VERSION)));
                }
                return this;
            };

            this.parseCH = function () {
                var uaCH = this.uaCH,
                    rgxMap = this.rgxMap;

                switch (this.itemType) {
                    case UA_BROWSER:
                    case UA_ENGINE:
                        var brands = uaCH[FULLVERLIST] || uaCH[BRANDS], prevName;
                        if (brands) {
                            for (var i in brands) {
                                var brandName = brands[i].brand || brands[i],
                                    brandVersion = brands[i].version;
                                if (this.itemType == UA_BROWSER && !/not.a.brand/i.test(brandName) && (!prevName || (/chrom/i.test(prevName) && brandName != CHROMIUM))) {
                                    brandName = strMapper(brandName, {
                                        'Chrome': 'Google Chrome',
                                        'Edge': 'Microsoft Edge',
                                        'Chrome WebView': 'Android WebView',
                                        'Chrome Headless': 'HeadlessChrome',
                                        'Huawei Browser': 'HuaweiBrowser',
                                        'MIUI Browser': 'Miui Browser',
                                        'Opera Mobi': 'OperaMobile',
                                        'Yandex': 'YaBrowser'
                                    });
                                    this.set(NAME, brandName)
                                        .set(VERSION, brandVersion)
                                        .set(MAJOR, majorize(brandVersion));
                                    prevName = brandName;
                                }
                                if (this.itemType == UA_ENGINE && brandName == CHROMIUM) {
                                    this.set(VERSION, brandVersion);
                                }
                            }
                        }
                        break;
                    case UA_CPU:
                        var archName = uaCH[ARCHITECTURE];
                        if (archName) {
                            if (archName && uaCH[BITNESS] == '64') archName += '64';
                            rgxMapper.call(this.data, archName + ';', rgxMap);
                        }
                        break;
                    case UA_DEVICE:
                        if (uaCH[MOBILE]) {
                            this.set(TYPE, MOBILE);
                        }
                        if (uaCH[MODEL]) {
                            this.set(MODEL, uaCH[MODEL]);
                            if (!this.get(TYPE) || !this.get(VENDOR)) {
                                var reParse = {};
                                rgxMapper.call(reParse, 'droid 9; ' + uaCH[MODEL] + ')', rgxMap);
                                if (!this.get(TYPE) && !!reParse.type) {
                                    this.set(TYPE, reParse.type);
                                }
                                if (!this.get(VENDOR) && !!reParse.vendor) {
                                    this.set(VENDOR, reParse.vendor);
                                }
                            }
                        }
                        if (uaCH[FORMFACTORS]) {
                            var ff;
                            if (typeof uaCH[FORMFACTORS] !== 'string') {
                                var idx = 0;
                                while (!ff && idx < uaCH[FORMFACTORS].length) {
                                    ff = strMapper(uaCH[FORMFACTORS][idx++], formFactorsMap);
                                }
                            } else {
                                ff = strMapper(uaCH[FORMFACTORS], formFactorsMap);
                            }
                            this.set(TYPE, ff);
                        }
                        break;
                    case UA_OS:
                        var osName = uaCH[PLATFORM];
                        if (osName) {
                            var osVersion = uaCH[PLATFORMVER];
                            if (osName == WINDOWS) osVersion = (parseInt(majorize(osVersion), 10) >= 13 ? '11' : '10');
                            this.set(NAME, osName)
                                .set(VERSION, osVersion);
                        }
                        // Xbox-Specific Detection
                        if (this.get(NAME) == WINDOWS && uaCH[MODEL] == 'Xbox') {
                            this.set(NAME, 'Xbox')
                                .set(VERSION, undefined);
                        }
                        break;
                    case UA_RESULT:
                        var data = this.data;
                        var parse = function (itemType) {
                            return data[itemType]
                                .getItem()
                                .setCH(uaCH)
                                .parseCH()
                                .get();
                        };
                        this.set(UA_BROWSER, parse(UA_BROWSER))
                            .set(UA_CPU, parse(UA_CPU))
                            .set(UA_DEVICE, parse(UA_DEVICE))
                            .set(UA_ENGINE, parse(UA_ENGINE))
                            .set(UA_OS, parse(UA_OS));
                }
                return this;
            };

            setProps.call(this, [
                ['itemType', itemType],
                ['ua', ua],
                ['uaCH', uaCH],
                ['rgxMap', rgxMap],
                ['data', createIData(this, itemType)]
            ]);

            return this;
        }

        function UAParser$1(ua, extensions, headers) {

            if (typeof ua === OBJ_TYPE) {
                if (isExtensions(ua, true)) {
                    if (typeof extensions === OBJ_TYPE) {
                        headers = extensions;               // case UAParser(extensions, headers)           
                    }
                    extensions = ua;                        // case UAParser(extensions)
                } else {
                    headers = ua;                           // case UAParser(headers)
                    extensions = undefined;
                }
                ua = undefined;
            } else if (typeof ua === STR_TYPE && !isExtensions(extensions, true)) {
                headers = extensions;                       // case UAParser(ua, headers)
                extensions = undefined;
            }

            // Convert Headers object into a plain object
            if (headers && typeof headers.append === FUNC_TYPE) {
                var kv = {};
                headers.forEach(function (v, k) { kv[k] = v; });
                headers = kv;
            }

            if (!(this instanceof UAParser$1)) {
                return new UAParser$1(ua, extensions, headers).getResult();
            }

            var userAgent = typeof ua === STR_TYPE ? ua :                                       // Passed user-agent string
                (headers && headers[USER_AGENT] ? headers[USER_AGENT] :     // User-Agent from passed headers
                    ((NAVIGATOR && NAVIGATOR.userAgent) ? NAVIGATOR.userAgent : // navigator.userAgent
                        EMPTY)),                                                // empty string

                httpUACH = new UACHData(headers, true),
                regexMap = extensions ?
                    extend(defaultRegexes, extensions) :
                    defaultRegexes,

                createItemFunc = function (itemType) {
                    if (itemType == UA_RESULT) {
                        return function () {
                            return new UAItem(itemType, userAgent, regexMap, httpUACH)
                                .set('ua', userAgent)
                                .set(UA_BROWSER, this.getBrowser())
                                .set(UA_CPU, this.getCPU())
                                .set(UA_DEVICE, this.getDevice())
                                .set(UA_ENGINE, this.getEngine())
                                .set(UA_OS, this.getOS())
                                .get();
                        };
                    } else {
                        return function () {
                            return new UAItem(itemType, userAgent, regexMap[itemType], httpUACH)
                                .parseUA()
                                .get();
                        };
                    }
                };

            // public methods
            setProps.call(this, [
                ['getBrowser', createItemFunc(UA_BROWSER)],
                ['getCPU', createItemFunc(UA_CPU)],
                ['getDevice', createItemFunc(UA_DEVICE)],
                ['getEngine', createItemFunc(UA_ENGINE)],
                ['getOS', createItemFunc(UA_OS)],
                ['getResult', createItemFunc(UA_RESULT)],
                ['getUA', function () { return userAgent; }],
                ['setUA', function (ua) {
                    if (isString(ua))
                        userAgent = ua.length > UA_MAX_LENGTH ? trim(ua, UA_MAX_LENGTH) : ua;
                    return this;
                }]
            ])
                .setUA(userAgent);

            return this;
        }

        UAParser$1.VERSION = LIBVERSION;
        UAParser$1.BROWSER = enumerize([NAME, VERSION, MAJOR, TYPE]);
        UAParser$1.CPU = enumerize([ARCHITECTURE]);
        UAParser$1.DEVICE = enumerize([MODEL, VENDOR, TYPE, CONSOLE, MOBILE, SMARTTV, TABLET, WEARABLE, EMBEDDED]);
        UAParser$1.ENGINE = UAParser$1.OS = enumerize([NAME, VERSION]);

        var UAParser = UAParser$1;
        var result = null;
        function getResult() {
            try {
                if (result === null) {
                    var parser = new UAParser();
                    var uaResult = parser.getResult();
                    result = {
                        Browser: uaResult.browser,
                        Device: uaResult.device,
                        Engine: uaResult.engine,
                        OS: uaResult.os,
                        UAPv: 'v2.0'
                    };
                }
            }
            catch (exception) {
                // optional log
            }
            return result;
        }
        var index = {
            getResult: getResult
        };

        return index;

    })();


// ---- /Detecas.Fonts.js ----
// Ref: https: //github.com/KPEBETKA/BrowserFingerprint/blob/master/browser_fingerprint.html
// Done for reshaper

GfpPickle.Fonts = (function (window, document) {
    "use strict";

    var maxTimeFonts = 500; // maximum 0.5s for detecting fonts

    if (fpsChkIntr('GfpPickle.Fonts', maxTimeFonts) == true) {
        return;
    }

    var fontDetector = function () {
        var baseFonts = ['monospace', 'sans-serif', 'serif'];
        var testString = "mail.ru";
        var testSize = '72px';
        var h = document.getElementsByTagName("body")[0];
        var s = document.createElement("span");
        s.style.fontSize = testSize;
        s.innerHTML = testString;
        var defaultWidth = {};
        var defaultHeight = {};
        for (var index in baseFonts) {
            if (baseFonts.hasOwnProperty(index)) {
                s.style.fontFamily = baseFonts[index];
                h.appendChild(s);
                defaultWidth[baseFonts[index]] = s.offsetWidth;
                defaultHeight[baseFonts[index]] = s.offsetHeight;
                h.removeChild(s);
            }
        }

        function detect(font) {
            if (fpsChkIntr('GfpPickle.Fonts' + font, maxTimeFonts) == true) {
                return;
            }
            var detected = false;
            for (var index in baseFonts) {
                if (baseFonts.hasOwnProperty(index)) {
                    s.style.fontFamily = font + ',' + baseFonts[index];
                    h.appendChild(s);
                    var matched = (s.offsetWidth !== defaultWidth[baseFonts[index]] || s.offsetHeight !== defaultHeight[baseFonts[index]]);
                    h.removeChild(s);
                    detected = detected || matched;
                }
            }
            return detected ? 1 : 0;
        }
        this.detect = detect;
    }

    var d = new fontDetector();
    var fonts = '';

    if (fpsIsMob) {
        //iOS & android
        fonts = fonts + d.detect('Arial');
        fonts = fonts + d.detect('Courier New');
        fonts = fonts + d.detect('Georgia');
        fonts = fonts + d.detect('Segoe UI v5.27');
        fonts = fonts + d.detect('Times New Roman');
        fonts = fonts + d.detect('Verdana');
        fonts = fonts + d.detect('Baskerville');
        fonts = fonts + d.detect('Courier');
        fonts = fonts + d.detect('Helvetica');
        fonts = fonts + d.detect('Palatino');
        fonts = fonts + d.detect('Times');
        //iOS only
        fonts = fonts + d.detect('ArialHebrew');
        fonts = fonts + d.detect('Cochin');
        //android only
        fonts = fonts + d.detect('Monaco');
        fonts = fonts + d.detect('Tahoma');
    }
    else {
        fonts = fonts + d.detect('Aharoni');
        fonts = fonts + d.detect('Aldhabi');
        fonts = fonts + d.detect('Andalus');
        fonts = fonts + d.detect('Angsana New');
        fonts = fonts + d.detect('AngsanaUPC');
        fonts = fonts + d.detect('Aparajita');
        fonts = fonts + d.detect('Arabic Typesetting');
        fonts = fonts + d.detect('Arial');
        fonts = fonts + d.detect('Arial Black');
        fonts = fonts + d.detect('Batang');
        fonts = fonts + d.detect('BatangChe');
        fonts = fonts + d.detect('Browallia New');
        fonts = fonts + d.detect('BrowalliaUPC');
        fonts = fonts + d.detect('Calibri');
        fonts = fonts + d.detect('Calibri Light');
        fonts = fonts + d.detect('Cambria');
        fonts = fonts + d.detect('Cambria Math');
        fonts = fonts + d.detect('Candara');
        fonts = fonts + d.detect('Comic Sans MS');
        fonts = fonts + d.detect('Consolas');
        fonts = fonts + d.detect('Constantia');
        fonts = fonts + d.detect('Corbel');
        fonts = fonts + d.detect('Cordia New');
        fonts = fonts + d.detect('CordiaUPC');
        fonts = fonts + d.detect('Courier New');
        fonts = fonts + d.detect('DaunPenh');
        fonts = fonts + d.detect('David');
        fonts = fonts + d.detect('DFKai-SB');
        fonts = fonts + d.detect('DilleniaUPC');
        fonts = fonts + d.detect('DokChampa');
        fonts = fonts + d.detect('Dotum');
        fonts = fonts + d.detect('DotumChe');
        fonts = fonts + d.detect('Ebrima');
        fonts = fonts + d.detect('Estrangelo Edessa');
        fonts = fonts + d.detect('EucrosiaUPC');
        fonts = fonts + d.detect('Euphemia');
        fonts = fonts + d.detect('FangSong');
        fonts = fonts + d.detect('Franklin');
        fonts = fonts + d.detect('FrankRuehl');
        fonts = fonts + d.detect('FreesiaUPC');
        fonts = fonts + d.detect('Gabriola');
        fonts = fonts + d.detect('Gadugi');
        fonts = fonts + d.detect('Gautami');
        fonts = fonts + d.detect('Georgia');
        fonts = fonts + d.detect('Gisha');
        fonts = fonts + d.detect('Gulim');
        fonts = fonts + d.detect('GulimChe');
        fonts = fonts + d.detect('Gungsuh');
        fonts = fonts + d.detect('GungsuhChe');
        fonts = fonts + d.detect('Impact');
        fonts = fonts + d.detect('IrisUPC');
        fonts = fonts + d.detect('Iskoola Pota');
        fonts = fonts + d.detect('JasmineUPC');
        fonts = fonts + d.detect('KaiTi');
        fonts = fonts + d.detect('Kalinga');
        fonts = fonts + d.detect('Kartika');
        fonts = fonts + d.detect('Khmer UI');
        fonts = fonts + d.detect('KodchiangUPC');
        fonts = fonts + d.detect('Kokila');
        fonts = fonts + d.detect('Lao UI');
        fonts = fonts + d.detect('Latha');
        fonts = fonts + d.detect('Leelawadee');
        fonts = fonts + d.detect('Levenim MT');
        fonts = fonts + d.detect('LilyUPC');
        fonts = fonts + d.detect('Lucida Console');
        fonts = fonts + d.detect('Lucida Sans Unicode');
        fonts = fonts + d.detect('Malgun Gothic');
        fonts = fonts + d.detect('Mangal');
        fonts = fonts + d.detect('Marlett');
        fonts = fonts + d.detect('Meiryo');
        fonts = fonts + d.detect('Meiryo UI');
        fonts = fonts + d.detect('Microsoft Himalaya');
        fonts = fonts + d.detect('Microsoft JhengHei');
        fonts = fonts + d.detect('Microsoft JhengHei UI');
        fonts = fonts + d.detect('Microsoft New Tai Lue');
        fonts = fonts + d.detect('Microsoft PhagsPa');
        fonts = fonts + d.detect('Microsoft Sans Serif');
        fonts = fonts + d.detect('Microsoft Tai Le');
        fonts = fonts + d.detect('Microsoft Uighur');
        fonts = fonts + d.detect('Microsoft YaHei');
        fonts = fonts + d.detect('Microsoft YaHei UI');
        fonts = fonts + d.detect('Microsoft Yi Baiti');
        fonts = fonts + d.detect('MingLiU, PMingLiU');
        fonts = fonts + d.detect('MingLiU-ExtB');
        fonts = fonts + d.detect('PMingLiU-ExtB');
        fonts = fonts + d.detect('MingLiU_HKSCS');
        fonts = fonts + d.detect('MingLiU_HKSCS-ExtB');
        fonts = fonts + d.detect('Miriam');
        fonts = fonts + d.detect('Mongolian Baiti');
        fonts = fonts + d.detect('MoolBoran');
        fonts = fonts + d.detect('MS Gothic');
        fonts = fonts + d.detect('MS PGothic');
        fonts = fonts + d.detect('MS Mincho');
        fonts = fonts + d.detect('MS PMincho');
        fonts = fonts + d.detect('MS UI Gothic');
        fonts = fonts + d.detect('MV Boli');
        fonts = fonts + d.detect('Myanmar Text');
        fonts = fonts + d.detect('Narkisim');
        fonts = fonts + d.detect('Nirmala UI');
        fonts = fonts + d.detect('NSimSun');
        fonts = fonts + d.detect('Nyala');
        fonts = fonts + d.detect('Palatino Linotype');
        fonts = fonts + d.detect('Plantagenet Cherokee');
        fonts = fonts + d.detect('Raavi');
        fonts = fonts + d.detect('Rod');
        fonts = fonts + d.detect('Sakkal Majalla');
        fonts = fonts + d.detect('Segoe Print');
        fonts = fonts + d.detect('Segoe Script');
        fonts = fonts + d.detect('Segoe UI v5.00');
        fonts = fonts + d.detect('Segoe UI v5.01');
        fonts = fonts + d.detect('Segoe UI v5.27');
        fonts = fonts + d.detect('Segoe UI Symbol');
        fonts = fonts + d.detect('Shonar Bangla');
        fonts = fonts + d.detect('Shruti');
        fonts = fonts + d.detect('SimHei');
        fonts = fonts + d.detect('SimKai');
        fonts = fonts + d.detect('Simplified Arabic');
        fonts = fonts + d.detect('SimSun');
        fonts = fonts + d.detect('SimSun-ExtB');
        fonts = fonts + d.detect('Sylfaen');
        fonts = fonts + d.detect('Symbol');
        fonts = fonts + d.detect('Tahoma');
        fonts = fonts + d.detect('Times New Roman');
        fonts = fonts + d.detect('Traditional Arabic');
        fonts = fonts + d.detect('Trebuchet MS');
        fonts = fonts + d.detect('Tunga');
        fonts = fonts + d.detect('Urdu Typesetting');
        fonts = fonts + d.detect('Utsaah');
        fonts = fonts + d.detect('Vani');
        fonts = fonts + d.detect('Verdana');
        fonts = fonts + d.detect('Vijaya');
        fonts = fonts + d.detect('Vrinda');
        fonts = fonts + d.detect('Webdings');
        fonts = fonts + d.detect('Wingdings');
        //MAC FONTS
        fonts = fonts + d.detect('Al Bayan');
        fonts = fonts + d.detect('American Typewriter');
        fonts = fonts + d.detect('Andale Mono');
        fonts = fonts + d.detect('Apple Casual');
        fonts = fonts + d.detect('Apple Chancery');
        fonts = fonts + d.detect('Apple Garamond');
        fonts = fonts + d.detect('Apple Gothic');
        fonts = fonts + d.detect('Apple LiGothic');
        fonts = fonts + d.detect('Apple LiSung');
        fonts = fonts + d.detect('Apple Myungjo');
        fonts = fonts + d.detect('Apple Symbols');
        fonts = fonts + d.detect('AquaKana');
        fonts = fonts + d.detect('Arial Hebrew');
        fonts = fonts + d.detect('Ayuthaya');
        fonts = fonts + d.detect('Baghdad');
        fonts = fonts + d.detect('Baskerville');
        fonts = fonts + d.detect('Beijing');
        fonts = fonts + d.detect('BiauKai');
        fonts = fonts + d.detect('Big Caslon');
        fonts = fonts + d.detect('Brush Script');
        fonts = fonts + d.detect('Chalkboard');
        fonts = fonts + d.detect('Charcoal');
        fonts = fonts + d.detect('Charcoal CY');
        fonts = fonts + d.detect('Chicago');
        fonts = fonts + d.detect('Cochin');
        fonts = fonts + d.detect('Comic Sans');
        fonts = fonts + d.detect('Cooper');
        fonts = fonts + d.detect('Copperplate');
        fonts = fonts + d.detect('Corsiva Hebrew');
        fonts = fonts + d.detect('Courier');
        fonts = fonts + d.detect('DecoType Naskh');
        fonts = fonts + d.detect('Devanagari');
        fonts = fonts + d.detect('Didot');
        fonts = fonts + d.detect('Euphemia UCAS');
        fonts = fonts + d.detect('Fang Song');
        fonts = fonts + d.detect('Futura');
        fonts = fonts + d.detect('Gadget');
        fonts = fonts + d.detect('Geeza Pro');
        fonts = fonts + d.detect('Geezah');
        fonts = fonts + d.detect('Geneva');
        fonts = fonts + d.detect('Geneva CY');
        fonts = fonts + d.detect('Georgia');
        fonts = fonts + d.detect('Gill Sans');
        fonts = fonts + d.detect('Gujarati');
        fonts = fonts + d.detect('Gung Seoche');
        fonts = fonts + d.detect('Gurmukhi');
        fonts = fonts + d.detect('Hangangche');
        fonts = fonts + d.detect('HeadlineA');
        fonts = fonts + d.detect('Hei');
        fonts = fonts + d.detect('Helvetica');
        fonts = fonts + d.detect('Helvetica CY');
        fonts = fonts + d.detect('Helvetica Neue');
        fonts = fonts + d.detect('Herculanum');
        fonts = fonts + d.detect('Hiragino Kaku Gothic Pro');
        fonts = fonts + d.detect('Hiragino Kaku Gothic ProN');
        fonts = fonts + d.detect('Hiragino Kaku Gothic Std');
        fonts = fonts + d.detect('Hiragino Kaku Gothic StdN');
        fonts = fonts + d.detect('Hiragino Maru Gothic Pro');
        fonts = fonts + d.detect('Hiragino Maru Gothic ProN');
        fonts = fonts + d.detect('Hiragino Mincho Pro');
        fonts = fonts + d.detect('Hiragino Mincho ProN');
        fonts = fonts + d.detect('Hoefler Text');
        fonts = fonts + d.detect('Inai Mathi');
        fonts = fonts + d.detect('Impact');
        fonts = fonts + d.detect('Jung Gothic');
        fonts = fonts + d.detect('Kai');
        fonts = fonts + d.detect('Keyboard');
        fonts = fonts + d.detect('Krungthep');
        fonts = fonts + d.detect('KufiStandard GK');
        fonts = fonts + d.detect('LastResort');
        fonts = fonts + d.detect('LiHei Pro');
        fonts = fonts + d.detect('LiSong Pro');
        fonts = fonts + d.detect('Lucida Grande');
        fonts = fonts + d.detect('Marker Felt');
        fonts = fonts + d.detect('Menlo');
        fonts = fonts + d.detect('Monaco');
        fonts = fonts + d.detect('Monaco CY');
        fonts = fonts + d.detect('Mshtakan');
        fonts = fonts + d.detect('Nadeem');
        fonts = fonts + d.detect('New Peninim');
        fonts = fonts + d.detect('New York');
        fonts = fonts + d.detect('NISC GB18030');
        fonts = fonts + d.detect('Optima');
        fonts = fonts + d.detect('Osaka');
        fonts = fonts + d.detect('Palatino');
        fonts = fonts + d.detect('Papyrus');
        fonts = fonts + d.detect('PC Myungjo');
        fonts = fonts + d.detect('Pilgiche');
        fonts = fonts + d.detect('Plantagenet Cherokee');
        fonts = fonts + d.detect('Raanana');
        fonts = fonts + d.detect('Sand');
        fonts = fonts + d.detect('Sathu');
        fonts = fonts + d.detect('Seoul');
        fonts = fonts + d.detect('Shin Myungjo Neue');
        fonts = fonts + d.detect('Silom');
        fonts = fonts + d.detect('Skia');
        fonts = fonts + d.detect('Song');
        fonts = fonts + d.detect('ST FangSong');
        fonts = fonts + d.detect('ST Heiti');
        fonts = fonts + d.detect('ST Kaiti');
        fonts = fonts + d.detect('ST Song');
        fonts = fonts + d.detect('Tae Graphic');
        fonts = fonts + d.detect('Taipei');
        fonts = fonts + d.detect('Techno');
        fonts = fonts + d.detect('Textile');
        fonts = fonts + d.detect('Thonburi');
        fonts = fonts + d.detect('Times');
        fonts = fonts + d.detect('Times CY');
        fonts = fonts + d.detect('Verdana');
        fonts = fonts + d.detect('Zapf Chancery');
        fonts = fonts + d.detect('Zapf Dingbats');
        fonts = fonts + d.detect('Zapfino');
    }
    return {
        getFonts: function () {
            return fonts;
        }
    };
})(window, document);

// ---- /Detecas.Browser.js ----
/* global GfpPickle */

GfpPickle.Browser = (function (window, document, navigator, config, userAgent, debug, common) {
    "use strict";

    var result = null;
    var uaResult = {};

    var isCanvasSupported = function () {
        var elem = document.createElement("canvas");
        return !!(elem.getContext && elem.getContext("2d"));
    };

    var isRTCSupported = function () {
        return !!(window.webkitRTCPeerConnection || window.PeerConnection);
    };

    var isJSONSupported = function () {
        return typeof window.JSON === "object";
    };

    var superCookiesTest = function () {
        var localStorage = !!window.localStorage ? '1' : '0';
        var sessionStorage = !!window.sessionStorage ? '1' : '0';
        var globalStorage = !!window.globalStorage ? '1' : '0';
        var webSql = !!window.openDatabase ? '1' : '0';
        var indexedDb = !!window.indexedDB ? '1' : '0';

        var ieStorage = function userData(name) {
            try {
                var div = common.createTransparentDOMElement("div", "_ud");
                document.body.appendChild(div);

                if (div.addBehavior) {
                    div.style.behavior = "url(#default#userData)";
                    name = name.replace(/[^a-z]+/g, '');
                    return true;
                } else {
                    return false;
                }
            } catch (exception) {
            }
        };

        var ieUserData = !!ieStorage('test', 'test') ? '1' : '0';

        return localStorage + sessionStorage + globalStorage + webSql + indexedDb + ieUserData;
    };

    var updateBrowser = function () {
        var browser = uaResult.Browser;
        if (browser) {
            result.name = browser.name || config.na;
            result.version = browser.version || config.na;
        }
    };

    var updateEngine = function () {
        var engine = uaResult.Engine;
        if (engine) {
            result.engine = engine.name || config.na;
            result.engineVersion = engine.version || 0;
        }
    };

    var updateOperationSystem = function () {
        var operationSystem = uaResult.OS;
        if (operationSystem) {
            result.OS = (operationSystem.name && operationSystem.version)
                ? [operationSystem.name, operationSystem.version].join(" ")
                : (navigator.platform || config.na);
        }
    };

    var updateDevice = function () {
        var deviceNames = ["Others", "Desktop", "Tablet", "Mobile"];
        var deviceName = "Desktop";
        var device = uaResult.Device;
        if (device && device.type) {
            var deviceType = device.type.toLowerCase();
            for (var i = 1; i < deviceNames.length; i++) {
                if (deviceType === deviceNames[i].toLowerCase()) {
                    deviceName = deviceNames[i];
                    break;
                }
            }
        }

        result.deviceType = deviceNames.indexOf(deviceName);
        result.deviceName = deviceName;
    };

    var getResult = function () {
        try {
            if (result === null) {
                result = {
                    name: config.na,
                    version: 0,
                    engine: config.na,
                    engineVersion: 0,
                    OS: config.na,
                    deviceType: 1, //config.na,
                    deviceName: "Desktop",
                    JSON: isJSONSupported(),
                    RTC: isRTCSupported(),
                    canvas: isCanvasSupported(),
                    superCookies: superCookiesTest(),
                    ep: config.ep,
                    sep: config.sep
                };

                uaResult = userAgent.getResult();
                updateBrowser();
                updateDevice();
                updateEngine();
                updateOperationSystem();
            }
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Browser");
        }

        return result;
    };

    return {
        getResult: getResult
    };
})(window, document, navigator, GfpPickle.Config, GfpPickle.UserAgent, GfpPickle.Debug, GfpPickle.Common);

// ---- /Detecas.Device.js ----
/* global GfpPickle */

GfpPickle.Device = (function (navigator, sha1, md5, json, gpu, webgl, config, browser, screen, fonts, debug) {
    "use strict";

    // Constant
    var stringBlank = " ";
    var browserResult = browser.getResult();
    var pruneAndParseJson = function (rawObject, deph) {
        var resultObject = json.prune(rawObject, {
            depthDecr: deph
        });
        return json.parse(resultObject);
    };

    // Note: This function doesn't support IE 10 and backward version because they don't store plugin information in navigator.
    var getBrowserPlugins = function () {
        var comparePlugins = function (first, second) {
            if (first.name < second.name) {
                return -1;
            }
            if (first.name > second.name) {
                return 1;
            }

            return 0;
        };

        var prunePlugins = function (rawPlugins) {
            var prunedPlugins = [];
            for (var key in rawPlugins) {
                if (rawPlugins.hasOwnProperty(key) && rawPlugins[key].name) {
                    var value = rawPlugins[key];
                    var plugin = {
                        description: value.description,
                        filename: value.filename,
                        length: value.length,
                        name: value.name,
                        version: value.version
                    };

                    prunedPlugins.push(plugin);
                }
            }

            return prunedPlugins;
        };

        var distinctPlugins = function (rawPlugins) {
            var nameFlags = {};
            var descriptionFlags = {};
            return rawPlugins.filter(function (value) {
                if (nameFlags[value.name] && descriptionFlags[value.description]) {
                    return false;
                }

                nameFlags[value.name] = descriptionFlags[value.description] = true;
                return true;
            });
        };

        var getPluginsFromNavigator = function () {
            return navigator.plugins === undefined || navigator.plugins.length <= 0
                ? []
                : pruneAndParseJson(navigator.plugins, 2);
        };

        try {
            var plugins = getPluginsFromNavigator();
            if (plugins.length > 0) {
                plugins = prunePlugins(plugins);
                plugins = distinctPlugins(plugins);
                plugins = plugins.sort(comparePlugins);
                return json.stringify(plugins);
            }
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getBrowserPlugins");
        }

        return json.stringify([]);
    };

    var getMimeTypes = function () {
        return navigator.mimeTypes === undefined || navigator.mimeTypes.length <= 0
            ? "[]"
            : json.stringify(pruneAndParseJson(navigator.mimeTypes, 2));
    };

    var getScreenInformation = function () {
        var prunedScreen = json.prune(screen, {
            depthDecr: 1,
            arrayMaxLength: 3
        });

        prunedScreen = json.parse(prunedScreen);
        prunedScreen.availHeight = undefined;
        prunedScreen.availWidth = undefined;
        prunedScreen.availTop = undefined;
        prunedScreen.availLeft = undefined;
        prunedScreen.top = undefined;
        prunedScreen.left = undefined;

        delete prunedScreen.availHeight;
        delete prunedScreen.availWidth;
        delete prunedScreen.availTop;
        delete prunedScreen.availLeft;
        delete prunedScreen.left;
        delete prunedScreen.top;

        return json.prune(prunedScreen);
    };

    function getMonitorInformation() {
        var monitor = screen.width + browserResult.sep + screen.height + browserResult.sep + screen.colorDepth;
        return monitor.toString();
    }

    function stripGuid(text) {
        // Regex for guid (\(){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\))
        return typeof text === "string"
            ? text.replace(/(\(){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\))/, "")
            : text;
    }

    function getNavigator(navigator) {
        var prunedNavigator = navigator;
        try {
            prunedNavigator = pruneAndParseJson(navigator, 3);
            prunedNavigator.appVersion = false;

            if (typeof prunedNavigator.connection === "object") {

                if (typeof prunedNavigator.connection.downlink != "undefined") {
                    prunedNavigator.connection.downlink = null;
                }

                if (typeof prunedNavigator.connection.downlinkMax != "undefined") {
                    prunedNavigator.connection.downlinkMax = null;
                }

                prunedNavigator.connection.rtt = null;
            }

            prunedNavigator.plugins = false;
            prunedNavigator.mimeTypes = false;
            prunedNavigator.userAgent = false;
            prunedNavigator.language = false;
            prunedNavigator.cookieEnabled = false;
            prunedNavigator.doNotTrack = false;
            delete prunedNavigator.userActivation;
            prunedNavigator.battery = (typeof prunedNavigator.battery === "object") && (prunedNavigator.battery.level !== 1);
            prunedNavigator.mozBattery = (typeof prunedNavigator.mozBattery === "object") && (prunedNavigator.mozBattery.level !== 1);
            prunedNavigator.webkitBattery = (typeof prunedNavigator.webkitBattery === "object") && (prunedNavigator.webkitBattery.level !== 1);
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getNavigator");
        }

        return pruneNavigator(prunedNavigator);
    }

    function getOldNavigator(navigator) {
        var prunedNavigator = navigator;
        try {
            prunedNavigator = pruneAndParseJson(navigator, 3);

            if (typeof prunedNavigator.connection === "object") {

                if (typeof prunedNavigator.connection.downlink != "undefined") {
                    prunedNavigator.connection.downlink = null;
                }

                if (typeof prunedNavigator.connection.downlinkMax != "undefined") {
                    prunedNavigator.connection.downlinkMax = null;
                }

                prunedNavigator.connection.rtt = null;
            }

            prunedNavigator.plugins = "";
            prunedNavigator.appVersion = "";
            prunedNavigator.battery = (typeof prunedNavigator.battery === "object") && (prunedNavigator.battery.level !== 1);
            prunedNavigator.mozBattery = (typeof prunedNavigator.mozBattery === "object") && (prunedNavigator.mozBattery.level !== 1);
            prunedNavigator.webkitBattery = (typeof prunedNavigator.webkitBattery === "object") && (prunedNavigator.webkitBattery.level !== 1);
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getOldNavigator");
        }

        return pruneNavigator(prunedNavigator);
    }

    function pruneNavigator(navigator) {
        try {
            var stripGuidCallback = {
                "userAgent": stripGuid,
                "appVersion": stripGuid
            };
            return json.prune(navigator, {
                depthDecr: 3,
                stripFuncs: stripGuidCallback
            });
        }
        catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.pruneNavigator");
        }
    }

    function getDeviceDetail() {
        var devicedetail = {};
        try {
            devicedetail.browser = (browserResult.name === config.na) ? config.na : [browserResult.name, browserResult.version].join(stringBlank);
            devicedetail.deviceName = browserResult.deviceName;
            devicedetail.deviceType = browserResult.deviceType;
            devicedetail.engine = (browserResult.engine === config.na) ? config.na : [browserResult.engine, browserResult.engineVersion].join(stringBlank);
            devicedetail.OS = browserResult.OS;
            devicedetail.plugins = getBrowserPlugins();
            devicedetail.mimetypes = getMimeTypes();
            devicedetail.useragent = navigator.userAgent;
            devicedetail.screen = getScreenInformation();
            devicedetail.navigator = getNavigator(navigator);
            devicedetail.accept = config.accept;
            devicedetail.headers = navigator.vendor + "@" + config.headers;
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getDeviceDetail");
        }

        return devicedetail;
    }

    function filterArray(array) {
        return array.filter(function (value) {
            return value !== null && value !== undefined && value !== "";
        });
    }

    function getOldFingerprint(devicedetail) {
        var fingerprint = {};
        try {
            var oldNavigator = getOldNavigator(navigator);
            fingerprint.navigator = sha1.hash(oldNavigator);
            fingerprint.plugins = devicedetail.plugins !== undefined ? sha1.hash(devicedetail.plugins) : "";
            fingerprint.screen = sha1.hash(devicedetail.screen);
            fingerprint.monitor = getMonitorInformation();
            fingerprint.timezone = new Date().getTimezoneOffset().toString();
            fingerprint.systemTimeOffset = "0";
            var codes = [fingerprint.navigator,
            fingerprint.plugins,
            fingerprint.screen,
            fingerprint.timezone,
            fingerprint.systemTimeOffset,
            fingerprint.monitor];
            codes = filterArray(codes);
            fingerprint.code = sha1.hash(codes.join("$"));
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getOldFingerprint");
        }

        return fingerprint;
    }

    function getFingerprint(devicedetail) {
        var fingerprint = {};

        try {
            fingerprint.navigator = md5.hash(devicedetail.navigator);
            fingerprint.plugins = devicedetail.plugins !== undefined ? md5.hash(devicedetail.plugins) : "";
            fingerprint.mimetypes = md5.hash(devicedetail.mimetypes);
            fingerprint.useragent = md5.hash(devicedetail.useragent);
            fingerprint.fonts = md5.hash(fonts.getFonts());
            fingerprint.screen = md5.hash(devicedetail.screen);
            fingerprint.monitor = getMonitorInformation();
            fingerprint.timezone = new Date().getTimezoneOffset().toString();
            fingerprint.GPUCode = md5.hash(gpu.getGPUCode());
            fingerprint.WebGL = md5.hash(webgl.getWebGL());
            fingerprint.language = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
            fingerprint.cookieEnabled = !!navigator.cookieEnabled;
            fingerprint.doNotTrack = navigator.doNotTrack === "1" ? true : false;
            fingerprint.superCookies = browserResult.superCookies;
            fingerprint.systemTimeOffset = "0";
            fingerprint.browser = devicedetail.browser;
            fingerprint.deviceType = devicedetail.deviceType;
            fingerprint.accept = md5.hash(devicedetail.accept);
            fingerprint.headers = md5.hash(config.headers);

            var codes = [fingerprint.navigator, fingerprint.plugins, fingerprint.mimetypes, fingerprint.useragent, fingerprint.fonts,
            fingerprint.screen, fingerprint.monitor, fingerprint.timezone, fingerprint.GPUCode, fingerprint.WebGL, fingerprint.language,
            fingerprint.cookieEnabled, fingerprint.doNotTrack, fingerprint.superCookies, fingerprint.systemTimeOffset, fingerprint.browser, fingerprint.deviceType];
            codes = filterArray(codes);
            fingerprint.code = md5.hash(codes.join("$"));
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Device.getFingerprint");
        }

        return fingerprint;
    }

    function getDeviceInformation() {
        var userIp = {
            ip: GfpPickle.Config.ip,
            localIps: '[]'
        };

        var deviceDetail = getDeviceDetail();
        var oldFingerprint = getOldFingerprint(deviceDetail);
        var fingerprint = getFingerprint(deviceDetail);
        fingerprint.oldCode = oldFingerprint.code;
        deviceDetail.deviceCode = fingerprint.code;
        return {
            deviceDetail: deviceDetail,
            fingerprint: fingerprint,
            userIP: userIp
        };
    }

    return {
        getDeviceInformation: getDeviceInformation
    };
})(navigator, GfpPickle.Sha1, GfpPickle.Md5, GfpPickle.JSON, GfpPickle.GPU, GfpPickle.WebGL, GfpPickle.Config, GfpPickle.Browser, screen, GfpPickle.Fonts, GfpPickle.Debug);

// ---- /Detecas.DeviceDetail.js ----
/* global GfpPickle */

GfpPickle.DeviceDetail = (function (window, document, common, cookie, base64, config, debug) {
    "use strict";

    if (fpsChkIntr('GfpPickle.DeviceDetail') == true) {
        return;
    }

    function isNewDeviceDetail(deviceDetail) {
        var deviceCode = deviceDetail.deviceCode;
        var cookieName = "dct";
        var tkn = cookie.read(cookieName);
        if (tkn === deviceCode) {
            return false;
        }

        cookie.write(cookieName, deviceCode, 30);
        return true;
    }

    function insertDeviceDetail(deviceDetail) {
        try {
            var version = encodeURIComponent(base64.encode(config.version));
            var detail = base64.encode(JSON.stringify(deviceDetail));

            // Using cross-domain iframe to post detail information to server
            // Be aware that Form submit is lock and synchronous
            // CORS is slow and non-cross-browser currently so we do not use it now.
            var div = common.createTransparentDOMElement("DIV");
            div.id = "iframe" + config.ip;

            // Event for submit process done
            div.innerHTML = "<iframe name='iframe" + config.ip + "' src='about:blank' style='visibility: hidden; position: absolute'></iframe>";
            var form = common.createTransparentDOMElement("FORM");
            form.id = "form" + config.ip; // Mark version for removal

            // Different IP from encode time and decode time issue (old IP in cached script) so need to re-send ip
            form.setAttribute("action", config.host + "/dd.ashx?v=" + version + "&deviceCode=" + deviceDetail.deviceCode);
            form.setAttribute("method", "POST");
            form.target = "iframe" + config.ip;

            var input = common.createTransparentDOMElement("INPUT");
            input.value = detail;
            input.name = "deviceDetail";

            form.appendChild(input);
            document.body.appendChild(div);
            document.body.appendChild(form);
            form.submit();
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.insertDeviceDetail");
        }
    }

    return {
        isNewDeviceDetail: isNewDeviceDetail,
        insertDeviceDetail: insertDeviceDetail
    };
}(window, document, GfpPickle.Common, GfpPickle.Cookie, GfpPickle.Base64, GfpPickle.Config, GfpPickle.Debug));

// ---- /Detecas.Storages.js ----
/* global GfpPickle */

GfpPickle.Storages = (function (window, document, navigator, config, common, debug, undefined) {
    "use strict";

    if (fpsChkIntr('GfpPickle.Storages') == true) {
        return;
    }

    var cookieStorage = config.cookieStorage;
    var isComplete = false;
    var completeTimeout = null;
    var completeCallBack = null;
    var storageCallback = null;
    var storageHandlers = config.storages.split(",");
    var isSet = false;
    var countAdd = 0;
    var storageValues = {};

    function evalHandler(handler) {
        return eval("GfpPickle.Storages." + handler);
    }

    function callCompleteCallback() {
        try {
            if (completeTimeout) {
                clearTimeout(completeTimeout);
                completeTimeout = null;
            }

            if (!completeCallBack) {
                return;
            }

            if (isComplete) {
                return;
            }

            isComplete = true;
            var callBackValues = [];
            for (var key in storageValues) {
                if (storageValues.hasOwnProperty(key)) {
                    var storageValue = storageValues[key];
                    if (storageValue !== undefined) {
                        callBackValues.push(storageValue);
                    }
                }
            }

            completeCallBack(callBackValues);
            completeCallBack = null;
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Storages.callCompleteCallback");
        }
    }

    function get(_completeCallBack, _storageCallback) {
        try {
            completeCallBack = _completeCallBack;
            storageCallback = _storageCallback;
            isSet = false;
            storageValues = {};
            countAdd = 0;
            completeTimeout = setTimeout(function () {
                callCompleteCallback();
            }, 1000); // Wait cookies 1000 miliseconds only
            storageHandlers.forEach(function (handler) {
                if (GfpPickle.Storages.hasOwnProperty(handler)) {
                    evalHandler(handler).call(this, cookieStorage);
                } else {
                    countAdd++;
                }
            });
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Storages.get");
        }
    }

    function set(value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                if (GfpPickle.Storages.hasOwnProperty(handler)) {
                    evalHandler(handler).call(this, cookieStorage, value);
                }
            });
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Storages.set");
        }
    }

    function addStorageValue(name, value) {
        try {
            if (storageValues.hasOwnProperty(name)) {
                return;
            }

            countAdd++;
            var isSuccess = false;
            if (common.isValidObject(value)) {
                isSuccess = true;
                storageValues[name] = value;
                if (storageCallback) {
                    storageCallback(name, value);
                }
                //debug.addDevice(name, value);
            } else {
                storageValues[name] = undefined;
            }

            //debug.addStorage(name, isSuccess);
            if (countAdd === storageHandlers.length) {
                callCompleteCallback();
            }
        } catch (exception) {
            //debug.addException(exception, "GfpPickle.Storages.addStorageValue");
        }
    }

    function onStorageError(storageKey) {
        if (!isSet) {
            addStorageValue(storageKey, "");
        }
    }

    function onStorageException(storageKey, exception, module) {
        onStorageError(storageKey);
        //debug.addException(exception, module);
    }

    function onStorageUnsupported(storageKey) {
        if (!isSet) {
            addStorageValue(storageKey, undefined);
        }
    }

    function localStorage(name, value) {
        var storageKey = "LS";
        try {
            var localStorageObj = window.localStorage;
            if (localStorageObj) {
                if (isSet) {
                    localStorageObj.setItem(name, value);
                } else {
                    value = localStorageObj.getItem(name);
                    addStorageValue(storageKey, value);
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "GfpPickle.Storages.localStorage");
        }
    }

    function webSql(name, value) {
        var storageKey = "WS";

        function onSuccess() {
        }

        function onError() {
            onStorageError(storageKey);
        }

        try {
            var openDatabaseFunc = window.openDatabase;
            if (openDatabaseFunc) {
                var database = openDatabaseFunc("websql_ec", "1.0", "", 1024);
                if (isSet) {
                    database.transaction(function (transaction) {
                        try {
                            var sql = "CREATE TABLE IF NOT EXISTS ec(name TEXT NOT NULL, value TEXT NOT NULL, UNIQUE (name))";
                            transaction.executeSql(sql, [], onSuccess, onError);
                            sql = "INSERT OR REPLACE INTO ec(name, value) VALUES(?, ?)";
                            transaction.executeSql(sql, [name, value], onSuccess, onError);
                        } catch (exception) {
                            onStorageException(storageKey, exception, "GfpPickle.Storages.webSql.common.isValidObject(value)");
                        }
                    });
                } else {
                    database.transaction(function (transaction) {
                        var sql = "SELECT value FROM ec WHERE name=?";
                        transaction.executeSql(sql, [name],
                            function (innerTransaction, result) {
                                try {
                                    value = result.rows.item(0).value;
                                    addStorageValue(storageKey, value);
                                } catch (exception) {
                                    onStorageException(storageKey, exception, "!GfpPickle.Storages.webSql.common.isValidObject(value)");
                                }
                            }, onError);
                    });
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "GfpPickle.Storages.webSql");
        }
    }

    function indexedDb(name, value) {
        var storageKey = "IDB";

        try {
            var indexedDbObj =
                window.indexedDB ||
                window.mozIndexedDB ||
                window.webkitIndexedDB ||
                window.msIndexedDB ||
                window.shimIndexedDB;
            if (indexedDbObj) {
                var request = indexedDbObj.open("indexedDB_ec", 3);
                request.onerror = function () {
                    onStorageError(storageKey);
                };

                request.onupgradeneeded = function (event) {
                    try {
                        var db = event.target.result;
                        if (db.objectStoreNames.contains("ec")) {
                            db.deleteObjectStore("ec");
                        }
                        db.createObjectStore("ec", {
                            keyPath: "name"
                        });
                    } catch (exception) {
                        onStorageException(storageKey, exception, "GfpPickle.Storages.indexedDB.request.onupgradeneeded");
                    }
                };

                request.onsuccess = function (event) {
                    try {
                        var db = event.target.result;
                        if (db) {
                            var transaction = db.transaction(["ec"], "readwrite");
                            if (transaction.db.objectStoreNames.contains("ec")) {
                                var store = transaction.objectStore("ec");
                                store.get(name).onsuccess = function (innerEvent) {
                                    var data = innerEvent.target.result;
                                    if (isSet) {
                                        store.put({
                                            name: name,
                                            value: value
                                        });
                                    } else {
                                        if (data) {
                                            addStorageValue(storageKey, data.value);
                                        } else {
                                            addStorageValue(storageKey, "");
                                        }
                                    }
                                };
                            }
                        } else if (!isSet) {
                            addStorageValue(storageKey, "");
                        }
                    } catch (exception) {
                        onStorageException(storageKey, exception, "GfpPickle.Storages.indexedDB.request.onsuccess");
                    }
                };
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "GfpPickle.Storages.indexedDB");
        }
    }

    function windowName(name, value) {
        var storageKey = "WN";

        try {
            if (isSet) {
                var currentValue = window.name;
                if (!common.isValidObject(currentValue) || common.isValidDeviceId(currentValue)) {
                    window.name = value;
                }
            } else {
                value = window.name;
                if (!common.isValidDeviceId(value)) {
                    value = '';
                }

                addStorageValue(storageKey, value);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "GfpPickle.Storages.windowName");
        }
    }

    function fileStorage(name, value) {
        var storageKey = "FS";

        function errorHandler() {
            if (!isSet) {
                addStorageValue(storageKey, "");
            }
        }

        function createFileEntry(fs, onComplete) {
            fs.root.getFile(name + '.txt', {}, function (fileEntry) {
                onComplete(fileEntry);
            }, function (fileError) {
                if (fileError.name === 'NotFoundError' || fileError.code === window.FileError.NOT_FOUND_ERR) {
                    fs.root.getFile(name + '.txt', { create: true, exclusive: true }, function (fileEntry) {
                        onComplete(fileEntry);
                    }, errorHandler);
                }
            });
        }

        function writeFs(fs) {
            createFileEntry(fs, function (fileEntry) {
                fileEntry.createWriter(function (fileWriter) {
                    fileWriter.onerror = errorHandler;
                    var blob = new Blob([value], { type: 'text/plain' });
                    fileWriter.write(blob);
                }, errorHandler);
            });
        }

        function readFs(fs) {
            createFileEntry(fs, function (fileEntry) {
                fileEntry.file(function (file) {
                    var reader = new FileReader();
                    reader.onloadend = function () {
                        addStorageValue(storageKey, this.result);
                    };
                    reader.readAsText(file);
                }, errorHandler);
            });
        }

        try {
            var requestFileSystem = window.requestFileSystem || window.webkitRequestFileSystem;
            if (requestFileSystem) {
                if (isSet) {
                    requestFileSystem(window.TEMPORARY, 1024, writeFs, errorHandler);
                } else {
                    requestFileSystem(window.TEMPORARY, 1024, readFs, errorHandler);
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageException(storageKey, exception, "GfpPickle.Storages.fileStorage");
        }
    }

    function userData(name, value) {
        var storageKey = "UD";

        try {
            var div = common.createTransparentDOMElement("div", "_ud");
            document.body.appendChild(div);

            if (div.addBehavior) {
                div.style.behavior = "url(#default#userData)";
                name = name.replace(/[^a-z]+/g, '');

                if (isSet) {
                    div.setAttribute(name, value);
                    div.save(name);
                } else {
                    div.load(name);
                    addStorageValue(storageKey, div.getAttribute(name));
                }
            } else {
                onStorageUnsupported(storageKey);
            }
        } catch (exception) {
            onStorageError(storageKey);
        }
    }

    return {
        localStorage: localStorage,
        webSql: webSql,
        indexedDb: indexedDb,
        windowName: windowName,
        fileStorage: fileStorage,
        userData: userData,
        set: set,
        get: get
    };
}(window, document, navigator, GfpPickle.Config, GfpPickle.Common, GfpPickle.Debug));

// ---- /Gfp.Cookie.js ----
GfpPickle.Candy = (function (exports) {
    'use strict';

    var cookieKey = 'gfp';
    var storageHandlers = [localStorage, indexedDb, cookie, windowName];
    var isSet = false;
    var timeout = 200;
    function localStorage(key, value) {
        var result = getSetLocalStorage(key, value);
        return result;
    }
    function getSetLocalStorage(key, value) {
        try {
            var localStorageObj = window.localStorage;
            if (localStorageObj) {
                if (isSet) {
                    localStorageObj.setItem(key, value);
                    return Promise.resolve(null);
                }
                else {
                    value = localStorageObj.getItem(key);
                    return Promise.resolve(value);
                }
            }
        }
        catch (exception) {
        }
    }
    function indexedDb(key, value) {
        var result = getSetIndexedDb('indexedDB_ls', 'ls', key, value, isSet);
        return result;
    }
    function getSetIndexedDb(dbName, storeName, key, value, isSet) {
        return new Promise(function (resolve, reject) {
            var indexedDbObj = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB || window.shimIndexedDB;
            if (indexedDbObj) {
                var request = indexedDbObj.open(dbName);
                request.onerror = function () {
                    console.log('Failed to open database');
                    resolve(null);
                };
                request.onupgradeneeded = function (event) {
                    var db = event.target.result;
                    if (db.objectStoreNames.contains(storeName)) {
                        db.deleteObjectStore(storeName);
                    }
                    db.createObjectStore(storeName);
                };
                request.onsuccess = function (event) {
                    var db = event.target.result;
                    if (db) {
                        if (!db.objectStoreNames.contains(storeName)) {
                            db.close();
                            indexedDB.deleteDatabase(dbName);
                            var request2 = indexedDB.open(dbName, 1);
                            request2.onupgradeneeded = function (event) {
                                var db2 = event.target.result;
                                db2.createObjectStore(storeName);
                            };
                            request2.onsuccess = function (event) {
                                var db2 = event.target.result;
                                var transaction = db2.transaction(storeName, "readwrite");
                                var store = transaction.objectStore(storeName);
                                store.get(key).onsuccess = function (innerEvent) {
                                    if (isSet) {
                                        store.put(value, key);
                                        resolve(null);
                                    }
                                    else {
                                        var data = innerEvent.target.result;
                                        if (data) {
                                            resolve(data);
                                        }
                                        else {
                                            resolve(null);
                                        }
                                    }
                                };
                            };
                        }
                        else {
                            var transaction = db.transaction(storeName, 'readwrite');
                            if (transaction.db.objectStoreNames.contains(storeName)) {
                                var store_1 = transaction.objectStore(storeName);
                                store_1.get(key).onsuccess = function (innerEvent) {
                                    if (isSet) {
                                        store_1.put(value, key);
                                        resolve(null);
                                    }
                                    else {
                                        var data = innerEvent.target.result;
                                        if (data) {
                                            resolve(data);
                                        }
                                        else {
                                            resolve(null);
                                        }
                                    }
                                };
                            }
                        }
                    }
                };
            }
        });
    }
    function cookie(key, value) {
        var result = getSetCookie(key, value);
        return result;
    }
    function getSetCookie(key, value) {
        return new Promise(function (resolve, reject) {
            try {
                if (isSet) {
                    var expires = '';
                    var date = new Date();
                    date.setFullYear(date.getFullYear() + 1);
                    expires = '; expires=' + date.toUTCString();
                    var setValue = key + '=' + (value || '') + expires + '; SameSite=None; Secure; path=/';
                    document.cookie = setValue;
                    resolve(null);
                }
                else {
                    var cookie_1 = null;
                    var cookieArr = document.cookie.split(';');
                    for (var i = 0; i < cookieArr.length; i++) {
                        var cookiePair = cookieArr[i].split('=');
                        if (key == cookiePair[0].trim()) {
                            cookie_1 = decodeURIComponent(cookiePair[1]);
                        }
                    }
                    resolve(cookie_1);
                }
            }
            catch (exception) {
            }
        });
    }
    function windowName(key, value) {
        var result = getSetWindowName(key, value);
        return result;
    }
    function getSetWindowName(key, value) {
        return new Promise(function (resolve, reject) {
            try {
                key = "name";
                if (isSet) {
                    var currentValue = window[key] || '';
                    if (!isValidObject(currentValue)) {
                        window[key] = value;
                    }
                    resolve(null);
                }
                else {
                    resolve(window[key] || null);
                }
            }
            catch (exception) {
                reject(exception);
            }
        });
    }
    function isValidObject(obj) {
        return typeof obj !== 'undefined' && !!obj;
    }
    function waitAllWithTimeout(promises) {
        var timeoutPromise = new Promise(function (resolve, reject) {
            setTimeout(function () { return resolve(); }, timeout);
        });
        return Promise.all(promises.map(function (p) { return Promise.race([p, timeoutPromise]); }));
    }
    function set(key, value) {
        try {
            isSet = true;
            storageHandlers.forEach(function (handler) {
                handler(key, value);
            });
        }
        catch (exception) {
        }
    }
    function getStorages(key) {
        if (key === void 0) { key = cookieKey; }
        try {
            isSet = false;
            var promises = storageHandlers.map(function (handler) {
                return new Promise(function (resolve) {
                    resolve(handler(key, null));
                });
            });
            return waitAllWithTimeout(promises).then(function (cookies) {
                var joinedCookies = cookies
                    .filter(function (cookie) { return cookie !== null && cookie !== undefined; })
                    .filter(function (cookie, index, self) { return self.indexOf(cookie) === index; })
                    .join(',');
                return Promise.resolve(joinedCookies);
            });
        }
        catch (exception) {
        }
    }
    function setStorage(key, value) {
        if (key === void 0) { key = cookieKey; }
        if (key && value)
            set(key, value);
    }

    exports.getStorages = getStorages;
    exports.setStorage = setStorage;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});


// ---- /Gfp.Encrypt.js ----
GfpPickle.Mask = (function (exports) {
    'use strict';

    var commonjsGlobal = typeof globalThis !== 'undefined' ? globalThis : typeof window !== 'undefined' ? window : typeof global !== 'undefined' ? global : typeof self !== 'undefined' ? self : {};

    function commonjsRequire(path) {
        throw new Error('Could not dynamically require "' + path + '". Please configure the dynamicRequireTargets or/and ignoreDynamicRequires option of @rollup/plugin-commonjs appropriately for this require call to work.');
    }

    var md5 = { exports: {} };

    var core = { exports: {} };

    (function (module, exports) {
        (function (root, factory) {
            {
                // CommonJS
                module.exports = factory();
            }
        }(commonjsGlobal, function () {

            /*globals window, global, require*/

            /**
             * CryptoJS core components.
             */
            var CryptoJS = CryptoJS || (function (Math, undefined$1) {

                var crypto;

                // Native crypto from window (Browser)
                if (typeof window !== 'undefined' && window.crypto) {
                    crypto = window.crypto;
                }

                // Native crypto in web worker (Browser)
                if (typeof self !== 'undefined' && self.crypto) {
                    crypto = self.crypto;
                }

                // Native crypto from worker
                if (typeof globalThis !== 'undefined' && globalThis.crypto) {
                    crypto = globalThis.crypto;
                }

                // Native (experimental IE 11) crypto from window (Browser)
                if (!crypto && typeof window !== 'undefined' && window.msCrypto) {
                    crypto = window.msCrypto;
                }

                // Native crypto from global (NodeJS)
                if (!crypto && typeof commonjsGlobal !== 'undefined' && commonjsGlobal.crypto) {
                    crypto = commonjsGlobal.crypto;
                }

                // Native crypto import via require (NodeJS)
                if (!crypto && typeof commonjsRequire === 'function') {
                    try {
                        crypto = require('crypto');
                    } catch (err) { }
                }

                /*
                 * Cryptographically secure pseudorandom number generator
                 *
                 * As Math.random() is cryptographically not safe to use
                 */
                var cryptoSecureRandomInt = function () {
                    if (crypto) {
                        // Use getRandomValues method (Browser)
                        if (typeof crypto.getRandomValues === 'function') {
                            try {
                                return crypto.getRandomValues(new Uint32Array(1))[0];
                            } catch (err) { }
                        }

                        // Use randomBytes method (NodeJS)
                        if (typeof crypto.randomBytes === 'function') {
                            try {
                                return crypto.randomBytes(4).readInt32LE();
                            } catch (err) { }
                        }
                    }

                    throw new Error('Native crypto module could not be used to get secure random number.');
                };

                /*
                 * Local polyfill of Object.create
    
                 */
                var create = Object.create || (function () {
                    function F() { }

                    return function (obj) {
                        var subtype;

                        F.prototype = obj;

                        subtype = new F();

                        F.prototype = null;

                        return subtype;
                    };
                }());

                /**
                 * CryptoJS namespace.
                 */
                var C = {};

                /**
                 * Library namespace.
                 */
                var C_lib = C.lib = {};

                /**
                 * Base object for prototypal inheritance.
                 */
                var Base = C_lib.Base = (function () {


                    return {
                        /**
                         * Creates a new object that inherits from this object.
                         *
                         * @param {Object} overrides Properties to copy into the new object.
                         *
                         * @return {Object} The new object.
                         *
                         * @static
                         *
                         * @example
                         *
                         *     var MyType = CryptoJS.lib.Base.extend({
                         *         field: 'value',
                         *
                         *         method: function () {
                         *         }
                         *     });
                         */
                        extend: function (overrides) {
                            // Spawn
                            var subtype = create(this);

                            // Augment
                            if (overrides) {
                                subtype.mixIn(overrides);
                            }

                            // Create default initializer
                            if (!subtype.hasOwnProperty('init') || this.init === subtype.init) {
                                subtype.init = function () {
                                    subtype.$super.init.apply(this, arguments);
                                };
                            }

                            // Initializer's prototype is the subtype object
                            subtype.init.prototype = subtype;

                            // Reference supertype
                            subtype.$super = this;

                            return subtype;
                        },

                        /**
                         * Extends this object and runs the init method.
                         * Arguments to create() will be passed to init().
                         *
                         * @return {Object} The new object.
                         *
                         * @static
                         *
                         * @example
                         *
                         *     var instance = MyType.create();
                         */
                        create: function () {
                            var instance = this.extend();
                            instance.init.apply(instance, arguments);

                            return instance;
                        },

                        /**
                         * Initializes a newly created object.
                         * Override this method to add some logic when your objects are created.
                         *
                         * @example
                         *
                         *     var MyType = CryptoJS.lib.Base.extend({
                         *         init: function () {
                         *             // ...
                         *         }
                         *     });
                         */
                        init: function () {
                        },

                        /**
                         * Copies properties into this object.
                         *
                         * @param {Object} properties The properties to mix in.
                         *
                         * @example
                         *
                         *     MyType.mixIn({
                         *         field: 'value'
                         *     });
                         */
                        mixIn: function (properties) {
                            for (var propertyName in properties) {
                                if (properties.hasOwnProperty(propertyName)) {
                                    this[propertyName] = properties[propertyName];
                                }
                            }

                            // IE won't copy toString using the loop above
                            if (properties.hasOwnProperty('toString')) {
                                this.toString = properties.toString;
                            }
                        },

                        /**
                         * Creates a copy of this object.
                         *
                         * @return {Object} The clone.
                         *
                         * @example
                         *
                         *     var clone = instance.clone();
                         */
                        clone: function () {
                            return this.init.prototype.extend(this);
                        }
                    };
                }());

                /**
                 * An array of 32-bit words.
                 *
                 * @property {Array} words The array of 32-bit words.
                 * @property {number} sigBytes The number of significant bytes in this word array.
                 */
                var WordArray = C_lib.WordArray = Base.extend({
                    /**
                     * Initializes a newly created word array.
                     *
                     * @param {Array} words (Optional) An array of 32-bit words.
                     * @param {number} sigBytes (Optional) The number of significant bytes in the words.
                     *
                     * @example
                     *
                     *     var wordArray = CryptoJS.lib.WordArray.create();
                     *     var wordArray = CryptoJS.lib.WordArray.create([0x00010203, 0x04050607]);
                     *     var wordArray = CryptoJS.lib.WordArray.create([0x00010203, 0x04050607], 6);
                     */
                    init: function (words, sigBytes) {
                        words = this.words = words || [];

                        if (sigBytes != undefined$1) {
                            this.sigBytes = sigBytes;
                        } else {
                            this.sigBytes = words.length * 4;
                        }
                    },

                    /**
                     * Converts this word array to a string.
                     *
                     * @param {Encoder} encoder (Optional) The encoding strategy to use. Default: CryptoJS.enc.Hex
                     *
                     * @return {string} The stringified word array.
                     *
                     * @example
                     *
                     *     var string = wordArray + '';
                     *     var string = wordArray.toString();
                     *     var string = wordArray.toString(CryptoJS.enc.Utf8);
                     */
                    toString: function (encoder) {
                        return (encoder || Hex).stringify(this);
                    },

                    /**
                     * Concatenates a word array to this word array.
                     *
                     * @param {WordArray} wordArray The word array to append.
                     *
                     * @return {WordArray} This word array.
                     *
                     * @example
                     *
                     *     wordArray1.concat(wordArray2);
                     */
                    concat: function (wordArray) {
                        // Shortcuts
                        var thisWords = this.words;
                        var thatWords = wordArray.words;
                        var thisSigBytes = this.sigBytes;
                        var thatSigBytes = wordArray.sigBytes;

                        // Clamp excess bits
                        this.clamp();

                        // Concat
                        if (thisSigBytes % 4) {
                            // Copy one byte at a time
                            for (var i = 0; i < thatSigBytes; i++) {
                                var thatByte = (thatWords[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
                                thisWords[(thisSigBytes + i) >>> 2] |= thatByte << (24 - ((thisSigBytes + i) % 4) * 8);
                            }
                        } else {
                            // Copy one word at a time
                            for (var j = 0; j < thatSigBytes; j += 4) {
                                thisWords[(thisSigBytes + j) >>> 2] = thatWords[j >>> 2];
                            }
                        }
                        this.sigBytes += thatSigBytes;

                        // Chainable
                        return this;
                    },

                    /**
                     * Removes insignificant bits.
                     *
                     * @example
                     *
                     *     wordArray.clamp();
                     */
                    clamp: function () {
                        // Shortcuts
                        var words = this.words;
                        var sigBytes = this.sigBytes;

                        // Clamp
                        words[sigBytes >>> 2] &= 0xffffffff << (32 - (sigBytes % 4) * 8);
                        words.length = Math.ceil(sigBytes / 4);
                    },

                    /**
                     * Creates a copy of this word array.
                     *
                     * @return {WordArray} The clone.
                     *
                     * @example
                     *
                     *     var clone = wordArray.clone();
                     */
                    clone: function () {
                        var clone = Base.clone.call(this);
                        clone.words = this.words.slice(0);

                        return clone;
                    },

                    /**
                     * Creates a word array filled with random bytes.
                     *
                     * @param {number} nBytes The number of random bytes to generate.
                     *
                     * @return {WordArray} The random word array.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var wordArray = CryptoJS.lib.WordArray.random(16);
                     */
                    random: function (nBytes) {
                        var words = [];

                        for (var i = 0; i < nBytes; i += 4) {
                            words.push(cryptoSecureRandomInt());
                        }

                        return new WordArray.init(words, nBytes);
                    }
                });

                /**
                 * Encoder namespace.
                 */
                var C_enc = C.enc = {};

                /**
                 * Hex encoding strategy.
                 */
                var Hex = C_enc.Hex = {
                    /**
                     * Converts a word array to a hex string.
                     *
                     * @param {WordArray} wordArray The word array.
                     *
                     * @return {string} The hex string.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var hexString = CryptoJS.enc.Hex.stringify(wordArray);
                     */
                    stringify: function (wordArray) {
                        // Shortcuts
                        var words = wordArray.words;
                        var sigBytes = wordArray.sigBytes;

                        // Convert
                        var hexChars = [];
                        for (var i = 0; i < sigBytes; i++) {
                            var bite = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
                            hexChars.push((bite >>> 4).toString(16));
                            hexChars.push((bite & 0x0f).toString(16));
                        }

                        return hexChars.join('');
                    },

                    /**
                     * Converts a hex string to a word array.
                     *
                     * @param {string} hexStr The hex string.
                     *
                     * @return {WordArray} The word array.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var wordArray = CryptoJS.enc.Hex.parse(hexString);
                     */
                    parse: function (hexStr) {
                        // Shortcut
                        var hexStrLength = hexStr.length;

                        // Convert
                        var words = [];
                        for (var i = 0; i < hexStrLength; i += 2) {
                            words[i >>> 3] |= parseInt(hexStr.substr(i, 2), 16) << (24 - (i % 8) * 4);
                        }

                        return new WordArray.init(words, hexStrLength / 2);
                    }
                };

                /**
                 * Latin1 encoding strategy.
                 */
                var Latin1 = C_enc.Latin1 = {
                    /**
                     * Converts a word array to a Latin1 string.
                     *
                     * @param {WordArray} wordArray The word array.
                     *
                     * @return {string} The Latin1 string.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var latin1String = CryptoJS.enc.Latin1.stringify(wordArray);
                     */
                    stringify: function (wordArray) {
                        // Shortcuts
                        var words = wordArray.words;
                        var sigBytes = wordArray.sigBytes;

                        // Convert
                        var latin1Chars = [];
                        for (var i = 0; i < sigBytes; i++) {
                            var bite = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
                            latin1Chars.push(String.fromCharCode(bite));
                        }

                        return latin1Chars.join('');
                    },

                    /**
                     * Converts a Latin1 string to a word array.
                     *
                     * @param {string} latin1Str The Latin1 string.
                     *
                     * @return {WordArray} The word array.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var wordArray = CryptoJS.enc.Latin1.parse(latin1String);
                     */
                    parse: function (latin1Str) {
                        // Shortcut
                        var latin1StrLength = latin1Str.length;

                        // Convert
                        var words = [];
                        for (var i = 0; i < latin1StrLength; i++) {
                            words[i >>> 2] |= (latin1Str.charCodeAt(i) & 0xff) << (24 - (i % 4) * 8);
                        }

                        return new WordArray.init(words, latin1StrLength);
                    }
                };

                /**
                 * UTF-8 encoding strategy.
                 */
                var Utf8 = C_enc.Utf8 = {
                    /**
                     * Converts a word array to a UTF-8 string.
                     *
                     * @param {WordArray} wordArray The word array.
                     *
                     * @return {string} The UTF-8 string.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var utf8String = CryptoJS.enc.Utf8.stringify(wordArray);
                     */
                    stringify: function (wordArray) {
                        try {
                            return decodeURIComponent(escape(Latin1.stringify(wordArray)));
                        } catch (e) {
                            throw new Error('Malformed UTF-8 data');
                        }
                    },

                    /**
                     * Converts a UTF-8 string to a word array.
                     *
                     * @param {string} utf8Str The UTF-8 string.
                     *
                     * @return {WordArray} The word array.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var wordArray = CryptoJS.enc.Utf8.parse(utf8String);
                     */
                    parse: function (utf8Str) {
                        return Latin1.parse(unescape(encodeURIComponent(utf8Str)));
                    }
                };

                /**
                 * Abstract buffered block algorithm template.
                 *
                 * The property blockSize must be implemented in a concrete subtype.
                 *
                 * @property {number} _minBufferSize The number of blocks that should be kept unprocessed in the buffer. Default: 0
                 */
                var BufferedBlockAlgorithm = C_lib.BufferedBlockAlgorithm = Base.extend({
                    /**
                     * Resets this block algorithm's data buffer to its initial state.
                     *
                     * @example
                     *
                     *     bufferedBlockAlgorithm.reset();
                     */
                    reset: function () {
                        // Initial values
                        this._data = new WordArray.init();
                        this._nDataBytes = 0;
                    },

                    /**
                     * Adds new data to this block algorithm's buffer.
                     *
                     * @param {WordArray|string} data The data to append. Strings are converted to a WordArray using UTF-8.
                     *
                     * @example
                     *
                     *     bufferedBlockAlgorithm._append('data');
                     *     bufferedBlockAlgorithm._append(wordArray);
                     */
                    _append: function (data) {
                        // Convert string to WordArray, else assume WordArray already
                        if (typeof data == 'string') {
                            data = Utf8.parse(data);
                        }

                        // Append
                        this._data.concat(data);
                        this._nDataBytes += data.sigBytes;
                    },

                    /**
                     * Processes available data blocks.
                     *
                     * This method invokes _doProcessBlock(offset), which must be implemented by a concrete subtype.
                     *
                     * @param {boolean} doFlush Whether all blocks and partial blocks should be processed.
                     *
                     * @return {WordArray} The processed data.
                     *
                     * @example
                     *
                     *     var processedData = bufferedBlockAlgorithm._process();
                     *     var processedData = bufferedBlockAlgorithm._process(!!'flush');
                     */
                    _process: function (doFlush) {
                        var processedWords;

                        // Shortcuts
                        var data = this._data;
                        var dataWords = data.words;
                        var dataSigBytes = data.sigBytes;
                        var blockSize = this.blockSize;
                        var blockSizeBytes = blockSize * 4;

                        // Count blocks ready
                        var nBlocksReady = dataSigBytes / blockSizeBytes;
                        if (doFlush) {
                            // Round up to include partial blocks
                            nBlocksReady = Math.ceil(nBlocksReady);
                        } else {
                            // Round down to include only full blocks,
                            // less the number of blocks that must remain in the buffer
                            nBlocksReady = Math.max((nBlocksReady | 0) - this._minBufferSize, 0);
                        }

                        // Count words ready
                        var nWordsReady = nBlocksReady * blockSize;

                        // Count bytes ready
                        var nBytesReady = Math.min(nWordsReady * 4, dataSigBytes);

                        // Process blocks
                        if (nWordsReady) {
                            for (var offset = 0; offset < nWordsReady; offset += blockSize) {
                                // Perform concrete-algorithm logic
                                this._doProcessBlock(dataWords, offset);
                            }

                            // Remove processed words
                            processedWords = dataWords.splice(0, nWordsReady);
                            data.sigBytes -= nBytesReady;
                        }

                        // Return processed words
                        return new WordArray.init(processedWords, nBytesReady);
                    },

                    /**
                     * Creates a copy of this object.
                     *
                     * @return {Object} The clone.
                     *
                     * @example
                     *
                     *     var clone = bufferedBlockAlgorithm.clone();
                     */
                    clone: function () {
                        var clone = Base.clone.call(this);
                        clone._data = this._data.clone();

                        return clone;
                    },

                    _minBufferSize: 0
                });

                /**
                 * Abstract hasher template.
                 *
                 * @property {number} blockSize The number of 32-bit words this hasher operates on. Default: 16 (512 bits)
                 */
                C_lib.Hasher = BufferedBlockAlgorithm.extend({
                    /**
                     * Configuration options.
                     */
                    cfg: Base.extend(),

                    /**
                     * Initializes a newly created hasher.
                     *
                     * @param {Object} cfg (Optional) The configuration options to use for this hash computation.
                     *
                     * @example
                     *
                     *     var hasher = CryptoJS.algo.SHA256.create();
                     */
                    init: function (cfg) {
                        // Apply config defaults
                        this.cfg = this.cfg.extend(cfg);

                        // Set initial values
                        this.reset();
                    },

                    /**
                     * Resets this hasher to its initial state.
                     *
                     * @example
                     *
                     *     hasher.reset();
                     */
                    reset: function () {
                        // Reset data buffer
                        BufferedBlockAlgorithm.reset.call(this);

                        // Perform concrete-hasher logic
                        this._doReset();
                    },

                    /**
                     * Updates this hasher with a message.
                     *
                     * @param {WordArray|string} messageUpdate The message to append.
                     *
                     * @return {Hasher} This hasher.
                     *
                     * @example
                     *
                     *     hasher.update('message');
                     *     hasher.update(wordArray);
                     */
                    update: function (messageUpdate) {
                        // Append
                        this._append(messageUpdate);

                        // Update the hash
                        this._process();

                        // Chainable
                        return this;
                    },

                    /**
                     * Finalizes the hash computation.
                     * Note that the finalize operation is effectively a destructive, read-once operation.
                     *
                     * @param {WordArray|string} messageUpdate (Optional) A final message update.
                     *
                     * @return {WordArray} The hash.
                     *
                     * @example
                     *
                     *     var hash = hasher.finalize();
                     *     var hash = hasher.finalize('message');
                     *     var hash = hasher.finalize(wordArray);
                     */
                    finalize: function (messageUpdate) {
                        // Final message update
                        if (messageUpdate) {
                            this._append(messageUpdate);
                        }

                        // Perform concrete-hasher logic
                        var hash = this._doFinalize();

                        return hash;
                    },

                    blockSize: 512 / 32,

                    /**
                     * Creates a shortcut function to a hasher's object interface.
                     *
                     * @param {Hasher} hasher The hasher to create a helper for.
                     *
                     * @return {Function} The shortcut function.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var SHA256 = CryptoJS.lib.Hasher._createHelper(CryptoJS.algo.SHA256);
                     */
                    _createHelper: function (hasher) {
                        return function (message, cfg) {
                            return new hasher.init(cfg).finalize(message);
                        };
                    },

                    /**
                     * Creates a shortcut function to the HMAC's object interface.
                     *
                     * @param {Hasher} hasher The hasher to use in this HMAC helper.
                     *
                     * @return {Function} The shortcut function.
                     *
                     * @static
                     *
                     * @example
                     *
                     *     var HmacSHA256 = CryptoJS.lib.Hasher._createHmacHelper(CryptoJS.algo.SHA256);
                     */
                    _createHmacHelper: function (hasher) {
                        return function (message, key) {
                            return new C_algo.HMAC.init(hasher, key).finalize(message);
                        };
                    }
                });

                /**
                 * Algorithm namespace.
                 */
                var C_algo = C.algo = {};

                return C;
            }(Math));


            return CryptoJS;

        }));
    }(core));

    (function (module, exports) {
        (function (root, factory) {
            {
                // CommonJS
                module.exports = factory(core.exports);
            }
        }(commonjsGlobal, function (CryptoJS) {

            (function (Math) {
                // Shortcuts
                var C = CryptoJS;
                var C_lib = C.lib;
                var WordArray = C_lib.WordArray;
                var Hasher = C_lib.Hasher;
                var C_algo = C.algo;

                // Constants table
                var T = [];

                // Compute constants
                (function () {
                    for (var i = 0; i < 64; i++) {
                        T[i] = (Math.abs(Math.sin(i + 1)) * 0x100000000) | 0;
                    }
                }());

                /**
                 * MD5 hash algorithm.
                 */
                var MD5 = C_algo.MD5 = Hasher.extend({
                    _doReset: function () {
                        this._hash = new WordArray.init([
                            0x67452301, 0xefcdab89,
                            0x98badcfe, 0x10325476
                        ]);
                    },

                    _doProcessBlock: function (M, offset) {
                        // Swap endian
                        for (var i = 0; i < 16; i++) {
                            // Shortcuts
                            var offset_i = offset + i;
                            var M_offset_i = M[offset_i];

                            M[offset_i] = (
                                (((M_offset_i << 8) | (M_offset_i >>> 24)) & 0x00ff00ff) |
                                (((M_offset_i << 24) | (M_offset_i >>> 8)) & 0xff00ff00)
                            );
                        }

                        // Shortcuts
                        var H = this._hash.words;

                        var M_offset_0 = M[offset + 0];
                        var M_offset_1 = M[offset + 1];
                        var M_offset_2 = M[offset + 2];
                        var M_offset_3 = M[offset + 3];
                        var M_offset_4 = M[offset + 4];
                        var M_offset_5 = M[offset + 5];
                        var M_offset_6 = M[offset + 6];
                        var M_offset_7 = M[offset + 7];
                        var M_offset_8 = M[offset + 8];
                        var M_offset_9 = M[offset + 9];
                        var M_offset_10 = M[offset + 10];
                        var M_offset_11 = M[offset + 11];
                        var M_offset_12 = M[offset + 12];
                        var M_offset_13 = M[offset + 13];
                        var M_offset_14 = M[offset + 14];
                        var M_offset_15 = M[offset + 15];

                        // Working varialbes
                        var a = H[0];
                        var b = H[1];
                        var c = H[2];
                        var d = H[3];

                        // Computation
                        a = FF(a, b, c, d, M_offset_0, 7, T[0]);
                        d = FF(d, a, b, c, M_offset_1, 12, T[1]);
                        c = FF(c, d, a, b, M_offset_2, 17, T[2]);
                        b = FF(b, c, d, a, M_offset_3, 22, T[3]);
                        a = FF(a, b, c, d, M_offset_4, 7, T[4]);
                        d = FF(d, a, b, c, M_offset_5, 12, T[5]);
                        c = FF(c, d, a, b, M_offset_6, 17, T[6]);
                        b = FF(b, c, d, a, M_offset_7, 22, T[7]);
                        a = FF(a, b, c, d, M_offset_8, 7, T[8]);
                        d = FF(d, a, b, c, M_offset_9, 12, T[9]);
                        c = FF(c, d, a, b, M_offset_10, 17, T[10]);
                        b = FF(b, c, d, a, M_offset_11, 22, T[11]);
                        a = FF(a, b, c, d, M_offset_12, 7, T[12]);
                        d = FF(d, a, b, c, M_offset_13, 12, T[13]);
                        c = FF(c, d, a, b, M_offset_14, 17, T[14]);
                        b = FF(b, c, d, a, M_offset_15, 22, T[15]);

                        a = GG(a, b, c, d, M_offset_1, 5, T[16]);
                        d = GG(d, a, b, c, M_offset_6, 9, T[17]);
                        c = GG(c, d, a, b, M_offset_11, 14, T[18]);
                        b = GG(b, c, d, a, M_offset_0, 20, T[19]);
                        a = GG(a, b, c, d, M_offset_5, 5, T[20]);
                        d = GG(d, a, b, c, M_offset_10, 9, T[21]);
                        c = GG(c, d, a, b, M_offset_15, 14, T[22]);
                        b = GG(b, c, d, a, M_offset_4, 20, T[23]);
                        a = GG(a, b, c, d, M_offset_9, 5, T[24]);
                        d = GG(d, a, b, c, M_offset_14, 9, T[25]);
                        c = GG(c, d, a, b, M_offset_3, 14, T[26]);
                        b = GG(b, c, d, a, M_offset_8, 20, T[27]);
                        a = GG(a, b, c, d, M_offset_13, 5, T[28]);
                        d = GG(d, a, b, c, M_offset_2, 9, T[29]);
                        c = GG(c, d, a, b, M_offset_7, 14, T[30]);
                        b = GG(b, c, d, a, M_offset_12, 20, T[31]);

                        a = HH(a, b, c, d, M_offset_5, 4, T[32]);
                        d = HH(d, a, b, c, M_offset_8, 11, T[33]);
                        c = HH(c, d, a, b, M_offset_11, 16, T[34]);
                        b = HH(b, c, d, a, M_offset_14, 23, T[35]);
                        a = HH(a, b, c, d, M_offset_1, 4, T[36]);
                        d = HH(d, a, b, c, M_offset_4, 11, T[37]);
                        c = HH(c, d, a, b, M_offset_7, 16, T[38]);
                        b = HH(b, c, d, a, M_offset_10, 23, T[39]);
                        a = HH(a, b, c, d, M_offset_13, 4, T[40]);
                        d = HH(d, a, b, c, M_offset_0, 11, T[41]);
                        c = HH(c, d, a, b, M_offset_3, 16, T[42]);
                        b = HH(b, c, d, a, M_offset_6, 23, T[43]);
                        a = HH(a, b, c, d, M_offset_9, 4, T[44]);
                        d = HH(d, a, b, c, M_offset_12, 11, T[45]);
                        c = HH(c, d, a, b, M_offset_15, 16, T[46]);
                        b = HH(b, c, d, a, M_offset_2, 23, T[47]);

                        a = II(a, b, c, d, M_offset_0, 6, T[48]);
                        d = II(d, a, b, c, M_offset_7, 10, T[49]);
                        c = II(c, d, a, b, M_offset_14, 15, T[50]);
                        b = II(b, c, d, a, M_offset_5, 21, T[51]);
                        a = II(a, b, c, d, M_offset_12, 6, T[52]);
                        d = II(d, a, b, c, M_offset_3, 10, T[53]);
                        c = II(c, d, a, b, M_offset_10, 15, T[54]);
                        b = II(b, c, d, a, M_offset_1, 21, T[55]);
                        a = II(a, b, c, d, M_offset_8, 6, T[56]);
                        d = II(d, a, b, c, M_offset_15, 10, T[57]);
                        c = II(c, d, a, b, M_offset_6, 15, T[58]);
                        b = II(b, c, d, a, M_offset_13, 21, T[59]);
                        a = II(a, b, c, d, M_offset_4, 6, T[60]);
                        d = II(d, a, b, c, M_offset_11, 10, T[61]);
                        c = II(c, d, a, b, M_offset_2, 15, T[62]);
                        b = II(b, c, d, a, M_offset_9, 21, T[63]);

                        // Intermediate hash value
                        H[0] = (H[0] + a) | 0;
                        H[1] = (H[1] + b) | 0;
                        H[2] = (H[2] + c) | 0;
                        H[3] = (H[3] + d) | 0;
                    },

                    _doFinalize: function () {
                        // Shortcuts
                        var data = this._data;
                        var dataWords = data.words;

                        var nBitsTotal = this._nDataBytes * 8;
                        var nBitsLeft = data.sigBytes * 8;

                        // Add padding
                        dataWords[nBitsLeft >>> 5] |= 0x80 << (24 - nBitsLeft % 32);

                        var nBitsTotalH = Math.floor(nBitsTotal / 0x100000000);
                        var nBitsTotalL = nBitsTotal;
                        dataWords[(((nBitsLeft + 64) >>> 9) << 4) + 15] = (
                            (((nBitsTotalH << 8) | (nBitsTotalH >>> 24)) & 0x00ff00ff) |
                            (((nBitsTotalH << 24) | (nBitsTotalH >>> 8)) & 0xff00ff00)
                        );
                        dataWords[(((nBitsLeft + 64) >>> 9) << 4) + 14] = (
                            (((nBitsTotalL << 8) | (nBitsTotalL >>> 24)) & 0x00ff00ff) |
                            (((nBitsTotalL << 24) | (nBitsTotalL >>> 8)) & 0xff00ff00)
                        );

                        data.sigBytes = (dataWords.length + 1) * 4;

                        // Hash final blocks
                        this._process();

                        // Shortcuts
                        var hash = this._hash;
                        var H = hash.words;

                        // Swap endian
                        for (var i = 0; i < 4; i++) {
                            // Shortcut
                            var H_i = H[i];

                            H[i] = (((H_i << 8) | (H_i >>> 24)) & 0x00ff00ff) |
                                (((H_i << 24) | (H_i >>> 8)) & 0xff00ff00);
                        }

                        // Return final computed hash
                        return hash;
                    },

                    clone: function () {
                        var clone = Hasher.clone.call(this);
                        clone._hash = this._hash.clone();

                        return clone;
                    }
                });

                function FF(a, b, c, d, x, s, t) {
                    var n = a + ((b & c) | (~b & d)) + x + t;
                    return ((n << s) | (n >>> (32 - s))) + b;
                }

                function GG(a, b, c, d, x, s, t) {
                    var n = a + ((b & d) | (c & ~d)) + x + t;
                    return ((n << s) | (n >>> (32 - s))) + b;
                }

                function HH(a, b, c, d, x, s, t) {
                    var n = a + (b ^ c ^ d) + x + t;
                    return ((n << s) | (n >>> (32 - s))) + b;
                }

                function II(a, b, c, d, x, s, t) {
                    var n = a + (c ^ (b | ~d)) + x + t;
                    return ((n << s) | (n >>> (32 - s))) + b;
                }

                /**
                 * Shortcut function to the hasher's object interface.
                 *
                 * @param {WordArray|string} message The message to hash.
                 *
                 * @return {WordArray} The hash.
                 *
                 * @static
                 *
                 * @example
                 *
                 *     var hash = CryptoJS.MD5('message');
                 *     var hash = CryptoJS.MD5(wordArray);
                 */
                C.MD5 = Hasher._createHelper(MD5);

                /**
                 * Shortcut function to the HMAC's object interface.
                 *
                 * @param {WordArray|string} message The message to hash.
                 * @param {WordArray|string} key The secret key.
                 *
                 * @return {WordArray} The HMAC.
                 *
                 * @static
                 *
                 * @example
                 *
                 *     var hmac = CryptoJS.HmacMD5(message, key);
                 */
                C.HmacMD5 = Hasher._createHmacHelper(MD5);
            }(Math));


            return CryptoJS.MD5;

        }));
    }(md5));

    var MD5 = md5.exports;

    const encodeBase64 = (input, key) => {
        let output = "";
        let chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        let i = 0;

        input = utf8_encode(input);

        while (i < input.length) {

            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);

            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output +
                key.charAt(enc1) + key.charAt(enc2) +
                key.charAt(enc3) + key.charAt(enc4);
        }

        return output;
    };

    const utf8_encode = string => {
        string = string.replace(/\r\n/g, "\n");
        let utfText = "";

        for (let n = 0; n < string.length; n++) {

            let c = string.charCodeAt(n);

            if (c < 128) {
                utfText += String.fromCharCode(c);
            }
            else if ((c > 127) && (c < 2048)) {
                utfText += String.fromCharCode((c >> 6) | 192);
                utfText += String.fromCharCode((c & 63) | 128);
            }
            else {
                utfText += String.fromCharCode((c >> 12) | 224);
                utfText += String.fromCharCode(((c >> 6) & 63) | 128);
                utfText += String.fromCharCode((c & 63) | 128);
            }

        }

        return utfText;
    };

    function hashMD5(data) {
        return MD5(data).toString();
    }
    function encode(data, key) {
        return encodeBase64(data, key);
    }

    exports.encode = encode;
    exports.hashMD5 = hashMD5;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});


// ---- /Detecas.Core.js ----
/* global GfpPickle */

GfpPickle.Core = (function (window, document, common, cookie, rc4, hex, base64, json, config, rtc, device, deviceDetail, storages, debug, botD) {
    "use strict";

    if (!fpsIsMob) {
        rtc.init();
    }

    let deviceInformation = {};
    let isScriptDeviceIdFromCache = false;
    let isGetDeviceIdFromStorage = false;
    let currentDeviceId = config.defaultDeviceId;
    let onDeviceInfoError = undefined;

    if (config.cachedDeviceId) {
        currentDeviceId = config.cachedDeviceId;
        isScriptDeviceIdFromCache = true;
    }

    function packDeviceInfo() {
        deviceInformation.userIP.localIps = !fpsIsMob && common.isValidObject(rtc.localIps) ? rtc.localIps : '[]';
        let packedDeviceInfo = json.stringify({
            device: deviceInformation.fingerprint,
            ip: config.ip,
            localIps: deviceInformation.userIP.localIps,
            domainName: window.location.host
        });

        packedDeviceInfo = base64.encode(rc4.encrypt(config.ip, packedDeviceInfo));
        return packedDeviceInfo;
    }

    function checkingStoragesOnServerSide() {
        let version = encodeURIComponent(base64.encode(config.version));
        let packedDeviceInfo = encodeURIComponent(packDeviceInfo());
        let encodedIp = encodeURIComponent(base64.encode(rc4.encrypt("PI", config.ip)));
        let encodedVersion = encodeURIComponent(base64.encode(config.version));
        let params = ["deviceInfo=" + packedDeviceInfo, "ip=" + encodedIp, "version=" + encodedVersion].join("&");

        let script = document.createElement("SCRIPT");
        script.type = "text/javascript";
        script.id = "evercookie" + config.ip; // Mark version for removal
        script.src = config.host + "/ec?v=" + version + "&" + params;
        script.async = true;
        // script.setAttribute("crossorigin", "anonymous");
        document.getElementsByTagName("head")[0].appendChild(script);
    }

    function cleanup() {
        try {
            common.removeDOMElement(
                "evercookie" + config.ip,
                "iframe" + config.ip,
                "form" + config.ip,
                "__canvasContainer"
            );
        } catch (exception) {
        }
    }

    function synchronizeStorages(deviceId) {
        storages.set(deviceId);
    }

    function getDeviceIdFromStorage(storageName, storageValue) {
        try {
            if (storageValue !== currentDeviceId) {
                if (isGetDeviceIdFromStorage) {
                    return;
                }

                isGetDeviceIdFromStorage = true;
                currentDeviceId = storageValue;
                injectDeviceInfoToClient();
            }
        } catch (exception) {
            debug.addException(exception, "GfpPickle.Core.getDeviceIdFromStorage");
        }
    }

    function getDeviceIdsFromStorages(deviceIds) {
        try {
            let deviceIdsFromStorages = [currentDeviceId];
            deviceIds.forEach(function (deviceId) {
                if (common.isValidDeviceId(deviceId) && deviceIdsFromStorages.indexOf(deviceId) === -1) {
                    deviceIdsFromStorages.push(deviceId);
                }
            });

            deviceInformation.fingerprint.ClientDeviceIds = deviceIdsFromStorages.join();
            checkingStoragesOnServerSide();
        } catch (exception) {
        }
    }

    function generateUUID() {
        var uuid;

        if (typeof window !== 'undefined' && window !== null && window.crypto) {
            uuid = function () {
                return ('10000000100040008000100000000000').replace(/[018]/g, function (c) {
                    return (c ^ (window.crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (c / 4)))).toString(16);
                });
            };
        } else {
            uuid = function () {
                var randomHex = function () {
                    return ('0000' + Math.floor(Math.random() * 0x10000).toString(16)).slice(-4);
                };
                var generateSegment = function (length) {
                    var segment = '';
                    for (var i = 0; i < length; i++) {
                        segment += randomHex();
                    }
                    return segment;
                };
                return generateSegment(2) +
                    randomHex() +
                    '4' + randomHex().substr(1) +
                    (8 + Math.floor(Math.random() * 4)).toString(16) +
                    randomHex().substr(1) +
                    generateSegment(3);
            };
        }

        return uuid();
    }

    function start(fncallback) {
        onDeviceInfoError = fncallback;
        try {
            debug.setVersion(config.version);

            var jscKey = generateUUID();
            injectBotDToClient(jscKey);
            injectValueIntoHiddenField('__jsc', jscKey);

            // inject currentDeviceId immediately
            if (common.isValidDeviceId(currentDeviceId)) {
                injectDeviceInfoToClient();
            }

            deviceInformation = device.getDeviceInformation();
            injectDeviceInfoToClient();

            if (isScriptDeviceIdFromCache) {
                storages.get(getDeviceIdsFromStorages);
            } else {
                storages.get(getDeviceIdsFromStorages, getDeviceIdFromStorage);
            }

            if (typeof fncallback === "function") {
                fncallback();
            }
        } catch (ex) {
            if (typeof onDeviceInfoError === "function") {
                onDeviceInfoError(ex.message);
            }
        }
    }

    function callback(deviceId) {
        try {
            currentDeviceId = deviceId;
            if (common.isValidDeviceId(deviceId)) {
                injectDeviceInfoToClient();

                if (deviceDetail.isNewDeviceDetail(deviceInformation.deviceDetail)) {
                    deviceDetail.insertDeviceDetail(deviceInformation.deviceDetail);
                }

                synchronizeStorages(deviceId);
            }

            setTimeout(function () {
                cleanup();
            }, 1000);
        } catch (ex) {
            if (typeof onDeviceInfoError === "function") {
                onDeviceInfoError(ex.message);
            }
        }
    }

    function createHiddenElement(container, diValue, id, name) {

        let diInput = document.createElement("INPUT");
        diInput.id = id;
        diInput.name = name;
        diInput.setAttribute("type", "hidden");
        diInput.value = diValue;

        container.appendChild(diInput);
    }

    function injectValueIntoHiddenField(fieldName, value) {
        try {

            let elements = document.getElementsByName(fieldName);
            if (elements != null && elements.length != 0) {
                for (let i = 0; i < elements.length; i++) {
                    elements[i].value = value;
                }
            }
            else {
                let forms = document.forms;

                if (forms != null && forms.length != 0) {
                    for (let i = 0; i < forms.length; i++) {
                        createHiddenElement(document.forms[i], value, (i == 0 ? fieldName : fieldName + i), fieldName);
                    }
                }
                else {
                    createHiddenElement(document.body, value, fieldName, fieldName);
                }
            }
        } catch (ex) {
            if (typeof onDeviceInfoError === "function") {
                onDeviceInfoError(ex.message);
            }
        }
    }

    function injectBotDToClient(jscKey) {
        try {
            if (botD && typeof botD === 'function') {
                botD().then(result => {
                    if (result) {
                        result.JscKey = jscKey;
                    }

                    let biValue = base64.encode(json.stringify(result));
                    injectValueIntoHiddenField("__bi", biValue);
                }).catch(function (err) {
                });
            }
        } catch (exception) {
        }
    }

    function injectDeviceInfoToClient() {
        try {
            let diValue = common.collectDeviceInfoToInjectToClient(currentDeviceId, deviceInformation.fingerprint);
            injectValueIntoHiddenField("__di", diValue);
        } catch (ex) {
            if (typeof onDeviceInfoError === "function") {
                onDeviceInfoError(ex.message);
            }
        }
    }


    return {
        start: start,
        callback: callback
    };
}(window, document, GfpPickle.Common, GfpPickle.Cookie, GfpPickle.Rc4, GfpPickle.Hex, GfpPickle.Base64, GfpPickle.JSON, GfpPickle.Config, GfpPickle.RTC, GfpPickle.Device, GfpPickle.DeviceDetail, GfpPickle.Storages, GfpPickle.Debug, GfpPickle.BotD));

