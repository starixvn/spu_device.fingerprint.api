var subtleCrypto = (function (exports) {
    'use strict';

    function sha512(string) {
        return new Promise((resolve, reject) => {
            let buffer = (new TextEncoder).encode(string);
            crypto.subtle.digest('SHA-512', buffer.buffer).then(result => {
                resolve(Array.from(new Uint8Array(result)).map(c => c.toString(16).padStart(2, '0')).join(''));
            }, reject);
        });
    }
    function encryptMessage(publicKey, message) {
        let enc = new TextEncoder();
        let encoded = enc.encode(message);
        return window.crypto.subtle.encrypt({
            name: "RSA-OAEP"
        }, publicKey, encoded);
    }
    function decryptMessage(privateKey, ciphertext) {
        return window.crypto.subtle.decrypt({
            name: "RSA-OAEP"
        }, privateKey, ciphertext);
    }
    function GenerateKey() {
        window.crypto.subtle.generateKey({
            name: "RSA-OAEP",
            // Consider using a 4096-bit key for systems that require long-term security
            modulusLength: 2048,
            publicExponent: new Uint8Array([1, 0, 1]),
            hash: "SHA-256",
        }, true, ["encrypt", "decrypt"]).then((keyPair) => {
            console.log('public key:' + keyPair.publicKey);
            console.log('private key:' + keyPair.privateKey);
        });
    }

    exports.GenerateKey = GenerateKey;
    exports.decryptMessage = decryptMessage;
    exports.encryptMessage = encryptMessage;
    exports.sha512 = sha512;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
