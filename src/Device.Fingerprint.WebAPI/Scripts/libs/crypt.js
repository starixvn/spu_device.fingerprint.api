var crypt = (function (exports) {
    'use strict';

    let publicKey = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkgKhpNAuxe5b2XuaM3skyK6CV61Xjf3Q8ayvNs6kdJgR3aAkKDs5jNbP1v6Ywa1GCrq/vhNstz/7Tv07xTXd4op7IIZ7sj6gIGCMovWo4fPwIfPxbf4halYfWx2eyAwvUqGgT1gf+j03i7q3/nhGatJAM9af/877P+O3xINdXkXwoXwk2Rb9Toqu0UP1bO4Pq3sAZg3JCxr02EfHOp6Ha+rUHNbY8BEc2oUDcdhOpwGVFxXlOh/8EdzavZXCmv5aJVozSu9tok8wiVJ5EhDJgDXBFNpcmNTYEnbSwysf7uyHNGVYq/rWzRLxHP83uT2AeFH/P3HGkviqoipdQw0dHwIDAQAB
-----END PUBLIC KEY-----`;
    let privateKey = `-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCSAqGk0C7F7lvZe5ozeyTIroJXrVeN/dDxrK82zqR0mBHdoCQoOzmM1s/W/pjBrUYKur++E2y3P/tO/TvFNd3iinsghnuyPqAgYIyi9ajh8/Ah8/Ft/iFqVh9bHZ7IDC9SoaBPWB/6PTeLurf+eEZq0kAz1p//zvs/47fEg11eRfChfCTZFv1Oiq7RQ/Vs7g+rewBmDckLGvTYR8c6nodr6tQc1tjwERzahQNx2E6nAZUXFeU6H/wR3Nq9lcKa/lolWjNK722iTzCJUnkSEMmANcEU2lyY1NgSdtLDKx/u7Ic0ZVir+tbNEvEc/ze5PYB4Uf8/ccaS+KqiKl1DDR0fAgMBAAECggEADFxdAIhqpUawD5csr0C0v6kyZKd1RZKKw/hECbubEDnS2AnXdcZ0cU1ljARtZzFllWvzkUJbY59u2uQ0/AA67Wj0J9qVZzobsTyrO0fZHm6EfO4GGYiNnfnLhBmxAwDbvWmXGjsnfeD9LlRGK6yqqB7Di6qW91TiT/7VubULTFCLudEx9SUaBmMyj2FYFou62vf0FZz4rut4Vn9wqExi+BPg+vouwkXvTLEEzUld2vhxCUvS6xyycbhE6PvKk53WaSeHwfM/WsT3FvN6u0Jksuiy2v88q3ex6iQzkT2KSmnuV6GCt8sO+ynxlBycWbOONkZDtRs/3P0CZP7pZlWgJQKBgQDCKA1tBGFLpL1E+2qAkVFizWLj1tbQyabeSuzDL/M9wXQqFbUpjuYMeJ2i7NLqSFY/KXFeYfWZATPbOkpcChhciT9b1iF0va4yD8EdCwgudUaM8QUdyUenUDQ7Tg9Z1B5EMwVPZpT53bBbTCzCGie48m2LSzw6iBFXk22V5eUC3QKBgQDAhKITfYvESCY4+8JNezVBArlPyeVyhRrii22G1XFoDTaMTdfo1dXeSE+53bB2W2kJsxCiPWzX5COmt6+7dnKun7LrgW6Dxx9COsoPDXredOkT5rTwTqGPQ47qSqVR4HEo4vGqVDJPen1KNeTLcgJhhR3kYCqkyVW9C0cvQ00KKwKBgQCjNa4LjVoIj2Q3P61IhF/I0WQmqKuNeWXXiV47uewcZqmy0uqmE6SYqS/CDJhELyipEbpmH6ynwNYJnJdzhVzzNMBY/8jB7M0o8gtg0aw/+X+j7NtjLZcZM5eknrrSxTfoFoeJNm9XF58vL/qT9A6eLtQc+tKXVVhWqOy+zDFV1QKBgGqfCkRRG78xVjno9MuEmRHQOGGAJ5stLJ6bKaWrQIuoBbXext0k0LWyxrThmoFyTtIYYfX4nvdSWoxS3viruQgNWkIkBNjmxuEfPhjoFkxt40gqVbthUQjPGa+Yd8bIzuMeDUmQJ+zsoccf4f7qvXbVCqEYwYVSHDH+tQXv+BLRAoGAS869HWTv8HSicZx66HqZ/hXBS5qQWJIdQZwQhkawS1/nSnwJEtrzyalzXPs0/rbuXYZZ+m+djlaaS0cLxPAzTUnkNSPhj9WSJEA7EEihfSlzYbfo8Vu9bqjEU03LveuwBiZoQyDhsMVhT1AaQMu2rT/pToK8A2WsoysQcYU4eZQ=
-----END PRIVATE KEY-----`;
    const chunkSize = 344;
    const algorithm = { name: "RSA-OAEP" };
    function ab2str(buf) {
        return String.fromCharCode.apply(null, new Uint8Array(buf));
    }
    function str2ab(str) {
        const buf = new ArrayBuffer(str.length);
        const bufView = new Uint8Array(buf);
        for (let i = 0, strLen = str.length; i < strLen; i++) {
            bufView[i] = str.charCodeAt(i);
        }
        return buf;
    }
    async function importPrivateKey(privateKeyPem) {
        // Remove the PEM header and footer
        const pemBody = privateKeyPem
            .replace("-----BEGIN PRIVATE KEY-----", "")
            .replace("-----END PRIVATE KEY-----", "")
            .replace(/\s/g, "");
        // Convert the base64-encoded PEM body to a Uint8Array
        const pemBytes = base64ToArrayBuffer(pemBody);
        // Parse the DER-encoded private key
        const privateKey = await window.crypto.subtle.importKey("pkcs8", pemBytes, {
            name: "RSA-OAEP",
            hash: { name: "SHA-256" }
        }, true, ["decrypt"]);
        return privateKey;
    }
    function base64ToArrayBuffer(base64) {
        const binaryString = atob(base64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes.buffer;
    }
    function importPublicKey(pem) {
        // fetch the part of the PEM string between header and footer
        const pemHeader = "-----BEGIN PUBLIC KEY-----";
        const pemFooter = "-----END PUBLIC KEY-----";
        const pemContents = pem.substring(pemHeader.length, pem.length - pemFooter.length);
        // base64 decode the string to get the binary data
        const binaryDerString = window.atob(pemContents);
        // convert from a binary string to an ArrayBuffer
        const binaryDer = str2ab(binaryDerString);
        return window.crypto.subtle.importKey("spki", binaryDer, {
            name: "RSA-OAEP",
            hash: "SHA-256"
        }, true, ["encrypt"]);
    }
    async function encryptData(obj) {
        const message = JSON.stringify(obj);
        const key = await importPublicKey(publicKey);
        const enc = new TextEncoder();
        const encodedChunks = chunkString(message, 100).map(str => enc.encode(str));
        const ciphertexts = await Promise.all(encodedChunks.map(chunk => window.crypto.subtle.encrypt(algorithm, key, chunk)));
        const exportedCiphertexts = ciphertexts.map(ab2str).map((str) => {
            var btoa = window.btoa(str);
            return btoa;
        });
        return exportedCiphertexts.join('');
    }
    function chunkString(str, size) {
        const numChunks = Math.ceil(str.length / size);
        const chunks = new Array(numChunks);
        for (let i = 0, o = 0; i < numChunks; ++i, o += size) {
            chunks[i] = str.substr(o, size);
        }
        return chunks;
    }
    async function decryptData(ciphertext) {
        const key = await importPrivateKey(privateKey);
        const chunks = chunkString(ciphertext, chunkSize);
        const decodedChunks = chunks.map(window.atob).map(str2ab);
        const plaintexts = await Promise.all(decodedChunks.map(chunk => window.crypto.subtle.decrypt(algorithm, key, chunk)));
        const arrayBuffers = concatArrayBuffers(plaintexts);
        return new TextDecoder().decode(arrayBuffers);
    }
    function concatArrayBuffers(arrayBuffers) {
        let totalLength = 0;
        for (let i = 0; i < arrayBuffers.length; i++) {
            totalLength += arrayBuffers[i].byteLength;
        }
        let result = new Uint8Array(totalLength);
        let offset = 0;
        for (let i = 0; i < arrayBuffers.length; i++) {
            result.set(new Uint8Array(arrayBuffers[i]), offset);
            offset += arrayBuffers[i].byteLength;
        }
        return result.buffer;
    }
    // Generate RSA key pair
    function exportKey() {
        window.crypto.subtle.generateKey({
            name: "RSA-OAEP",
            modulusLength: 2048,
            publicExponent: new Uint8Array([0x01, 0x00, 0x01]),
            hash: "SHA-256"
        }, true, ["encrypt", "decrypt"]).then(keyPair => {
            // Export public key
            window.crypto.subtle.exportKey("spki", keyPair.publicKey).then(exportedPublicKey => {
                const exportedAsString = ab2str(exportedPublicKey);
                const exportedAsBase64 = window.btoa(exportedAsString);
                const pemExported = `-----BEGIN PUBLIC KEY-----\n${exportedAsBase64}\n-----END PUBLIC KEY-----`;
                console.log("Public key:", pemExported);
            });
            // Export private key
            window.crypto.subtle.exportKey("pkcs8", keyPair.privateKey).then(exportedPrivateKey => {
                const exportedAsString = ab2str(exportedPrivateKey);
                const exportedAsBase64 = window.btoa(exportedAsString);
                const pemExported = `-----BEGIN PRIVATE KEY-----\n${exportedAsBase64}\n-----END PRIVATE KEY-----`;
                console.log("Private key:", pemExported);
            });
        });
    }
    async function digestMessage(message) {
        const msgUint8 = new TextEncoder().encode(message); // encode as (utf-8) Uint8Array
        const hashBuffer = await crypto.subtle.digest("SHA-256", msgUint8); // hash the message
        const hashArray = Array.from(new Uint8Array(hashBuffer)); // convert buffer to byte array
        const hashHex = hashArray
            .map((b) => b.toString(16).padStart(2, "0"))
            .join(""); // convert bytes to hex string
        return hashHex;
    }

    exports.decryptData = decryptData;
    exports.digestMessage = digestMessage;
    exports.encryptData = encryptData;
    exports.exportKey = exportKey;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

})({});
