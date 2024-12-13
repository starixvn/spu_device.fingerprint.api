// Ref: https: //github.com/KPEBETKA/BrowserFingerprint/blob/master/browser_fingerprint.html
// Done for reshaper

Detecas.Fonts = (function (window, document) {
    "use strict";

    var maxTimeFonts = 500; // maximum 0.5s for detecting fonts

    if (fpsChkIntr('Detecas.Fonts', maxTimeFonts) == true) {
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
            if (fpsChkIntr('Detecas.Fonts' + font, maxTimeFonts) == true) {
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