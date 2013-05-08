String.prototype.replaceAll = function (str1, str2, ignore) {
    return this.replace(new RegExp(str1.replace(/([\,\!\\\^\$\{\}\[\]\(\)\.\*\+\?\|\<\>\-\&])/g, function (c) {
        return "\\" + c;
    }), "g" + (ignore ? "i" : "")), str2);
};


function encodeHtmlChars(text) {
    return text.replaceAll(" ", "&nbsp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
}


function isLetterOrNumber(c) {
    if (c.length != 1)
        return false;
    
    var cc = c.charCodeAt(0);
    return (cc >= 48 /*0*/ && cc <= 57 /*9*/) || (cc >= 65 && cc <= 90) || (cc >= 97 && cc <= 122) || cc == 46 /* . */;
}


function QueryEditor(box, text) {
    if (!text) text = "";

    this.box = box;
    this.textContainer = $('<div class="textContainer" />');
    this.box.append(this.textContainer);
    this.text = text;
    this.pos = 0;

    var self = this;
    // var keyTarget = $("body");
    var keyTarget = box;
    keyTarget.keydown(function (ev) {
        self.shiftDown = ev.shiftKey;
        
        if (ev.keyCode == 16 && !self.selectStartPos) { // Shift down, start selection
            self.selectStartPos = self.pos;
        }
        
        if (ev.keyCode == 36) { // home
            self.setPos(0);
        } else if (ev.keyCode == 35) { // end
            self.setPos(self.text.length);
        } else if (ev.keyCode == 39) { // right
            if (ev.ctrlKey)
                self.movePosToNextWord(1);
            else
                self.setPos(self.pos + 1);
        } else if (ev.keyCode == 38) { // up
            self.arrowUp();
        } else if (ev.keyCode == 37) { // left
            if (ev.ctrlKey)
                self.movePosToNextWord(-1);
            else
                self.setPos(self.pos - 1);
        } else if (ev.keyCode == 40) { // down
            self.arrowDown();
        } else if (ev.keyCode == 8) { // backspace
            self.backspace(ev.ctrlKey);
        } else if (ev.keyCode == 32 && ev.ctrlKey) { // ctrl-space 
            self.startAutoComplete(false);
        } else if (ev.keyCode == 13) { // cr
            if (self.inAutoComplete)
                self.closeAutoComplete(true);
            else
                window.setTimeout(function () { self.onSubmit(self.text); });
        } else {
            return true;
        }
        return false;
    });
    
    function charCodeIsValidForSymbol(cc) {
        return (cc >= 48 /*0*/ && cc <= 57 /*9*/) || (cc >= 65 && cc <= 90) || (cc >= 97 && cc <= 122) || cc == 46 /* . */;
    }
    
    keyTarget.keypress(function (ev) {
        var c = String.fromCharCode(ev.which);


        if (self.inAutoComplete && c == '.')
            self.closeAutoComplete(true);

        var left = self.text.substring(0, self.pos);
        var right = self.text.substring(self.pos, self.text.length);
        self.text = left + c + right;
        self.pos++;

        if (self.selectStartPos)
            self.selectStartPos = self.pos;

        if (!self.inAutoComplete && charCodeIsValidForSymbol(c.charCodeAt(0)))
            self.startAutoComplete(true);

        self.refresh();
        return false;
    });


    this.getAutoCompleteStartPos = function () {
        var t = this.text;
        var p = this.pos;
        while (p > 0 && t.charAt(p - 1) != ' ' && t.charAt(p - 1) != '.')
            p--;
        return p;
    };

    this.getAutocompleteAlternatives = function () {
        return ['Foo', 'Bar', 'SubscriptionOrder', 'FunkyBusiness'];
    };

    this.onSubmit = function(text) {
    };
    
    this.checkAutoCompleteStart = function (ev) { return false; }

    this.refresh();
}

QueryEditor.prototype.getCharacterClass = function(c) {
    if (c == ' ' || c == "\t")
        return 1;
    if (isLetterOrNumber(c))
        return 2;
    return c.charCodeAt(c) + 10;
};

QueryEditor.prototype.findWordPos = function(text, curpos, direction) {
    if (direction == -1 && curpos == 0)
        return curpos;
    if (direction == 1 && curpos >= text.length)
        return text.length;
    if (Math.abs(direction) != 1)
        return null; // Fail should throw exception

    var startPos = direction == -1 ? curpos - 1 : curpos;
    var matchClass = this.getCharacterClass(text.charAt(startPos));

    while (curpos > 0 && curpos <= text.length && this.getCharacterClass(text.charAt(curpos + direction)) == matchClass)
        curpos += direction;

    if (direction == 1)
        curpos++;

    return curpos;
};

QueryEditor.prototype.movePosToNextWord = function(direction) {
    this.setPos(this.findWordPos(this.text, this.pos, direction));
};

QueryEditor.prototype.areaIsSelected = function() {
    return this.selectStartPos && this.selectStartPos != this.pos;
};

QueryEditor.prototype.backspace = function (ctrlDown) {
    var backUntil = this.pos - 1;
    if (this.areaIsSelected()) {
        var pos = Math.max(this.selectStartPos, this.pos);
        backUntil = Math.min(this.selectStartPos, this.pos);
        this.pos = pos;
        this.selectStartPos = null;
    } else {
        if (ctrlDown)
            backUntil = this.findWordPos(this.text, this.pos, -1);
    }

    if (this.pos < 1) return;


    var left = this.text.substring(0, backUntil);
    var right = this.text.substring(this.pos, this.text.length);
    this.text = left + right;
    this.pos = backUntil;

    if (this.inAutoComplete)
        this.closeAutoComplete(false);
    
    this.refresh();
};

QueryEditor.prototype.arrowUp = function () {
    if (this.inAutoComplete)
        this.setAutocompleteAlt(this.acSelected - 1);
};

QueryEditor.prototype.arrowDown = function () {
    if (this.inAutoComplete)
        this.setAutocompleteAlt(this.acSelected + 1);
};

QueryEditor.prototype.setAutocompleteAlt = function (newIndex) {
    if (newIndex < 0)
        return;
    if (newIndex >= this.acAlts.length)
        return;

    this.acAlts[this.acSelected].el.removeClass('acSelected');
    this.acSelected = newIndex;
    this.acAlts[this.acSelected].el.addClass('acSelected');
};

QueryEditor.prototype.filterStrings = function (arr, start) {
    if (start == "")
        return arr;
    var newArr = [];
    start = start.toLowerCase();
    for (var i = 0; i < arr.length; i++) {
        var v = arr[i];
        if (start.length > v.length)
            continue;
        if (v.substring(0, start.length).toLowerCase() != start)
            continue;
        newArr.push(v);
    }
    return newArr;
}

QueryEditor.prototype.updateAutoComplete = function (closeOnNoAlternatives) {
    if (this.pos < this.acStartPos) {
        this.closeAutoComplete(false);
    }
    var starting = true;
    if (this.acUl) starting = false;

    var acAlts = [];
    if (starting)
        this.acStartPos = this.getAutoCompleteStartPos();
    var hint = this.text.substring(this.acStartPos, this.pos);
    var acStrings = this.filterStrings(this.getAutocompleteAlternatives(), hint);
    if (closeOnNoAlternatives && acStrings.length == 0) {
        this.closeAutoComplete(false);
        return;
    }

    if (!starting) {
        this.acUl.empty();
    }
    else {
        this.acUl = $('<ul class="acAlts" />');
    }
    var acUl = this.acUl;
    var selected = 0;
    $.each(acStrings, function (i, v) {
        var li = $("<li />").text(v);
        if (i == selected)
            li.addClass("acSelected");
        acUl.append(li);
        acAlts.push({ index: i, value: v, el: li });
    });
    if (acStrings.length == 0)
        acUl.append($('<li>(no alternatives)</li>'));

    this.acAlts = acAlts;
    this.acSelected = selected;
    this.acUl = acUl;
    acUl.css('top', '24px' /* TODO: make dynamic */);
    acUl.css('left', this.cursorPos.left + 'px');
    this.box.append(acUl);
};

QueryEditor.prototype.closeAutoComplete = function (insertSelected) {
    this.inAutoComplete = false;
    if (this.acUl) {
        this.acUl.detach();
        this.acUl = null;
    }

    if (insertSelected && this.acAlts.length > 0) {
        var selectedWord = this.acAlts[this.acSelected].value;
        var left = this.text.substring(0, this.acStartPos);
        var right = this.text.substring(this.pos, this.text.length);
        this.text = left + selectedWord + right;
        this.pos = this.acStartPos + selectedWord.length;
        if (this.selectStartPos)
            this.selectStartPos = this.pos;
        // TODO: Stop autocomplete here
        this.refresh();
    }
};

QueryEditor.prototype.startAutoComplete = function (closeOnNoAlternatives) {
    if (!this.inAutoComplete) {
        this.inAutoComplete = true;
        this.updateAutoComplete(closeOnNoAlternatives);
    } else {
        // Insert alternative
        this.closeAutoComplete(true);
    }
};

QueryEditor.prototype.refresh = function () {
    this.textContainer.empty();
    if (this.onEmpty)
        this.onEmpty();

    var tlen = this.text.length;
    var selectStart = -1;
    var selectEnd = -1;
    if (this.selectStartPos) {
        selectStart = Math.min(this.pos, this.selectStartPos);
        selectEnd = Math.max(this.pos, this.selectStartPos);
    }

    var htmlStr = "";
    for (var i = 0; i <= tlen; i++) {
        if (i == selectStart)
            htmlStr += '<span class="textSelected">';
        if (i == selectEnd)
            htmlStr += '</span>';
        if (i == this.pos)
            htmlStr += '<span class="textCursor" />';
        if (i < tlen)
            htmlStr += encodeHtmlChars(this.text.charAt(i));
    }

    this.textContainer.append(htmlStr);
    //this.textContainer.append(encodeHtmlChars(left));
    var textMarker = $('<span class="textCursorVisible">');
    var textCursor = this.textContainer.find(".textCursor");
    //this.textContainer.append(textCursor);
    //this.textContainer.append(encodeHtmlChars(right));
    
    this.textContainer.append(textMarker);
    this.cursorPos = textCursor.position();
    textMarker.css('top', this.cursorPos.top + 'px');
    textMarker.css('left', this.cursorPos.left + 'px');
    var timerHandle = window.setInterval(function () {
        if (textMarker.is(":visible"))
            textMarker.hide();
        else
            textMarker.show();
    }, 600);
    this.onEmpty = function () {
        window.clearInterval(timerHandle);
    };

    if (this.inAutoComplete)
        this.updateAutoComplete();
};

QueryEditor.prototype.setPos = function (newPos) {
    if (this.inAutoComplete)
        this.closeAutoComplete(false);

    if (this.selectStartPos && !this.shiftDown)
        this.selectStartPos = null;

    this.pos = Math.max(0, Math.min(this.text.length, newPos));
    this.refresh();
};
