// $ANTLR 3.3 Nov 30, 2010 12:50:56 PomonaQueryJs.g 2013-05-03 11:58:56

var PomonaQueryJsLexer = function(input, state) {
// alternate constructor @todo
// public PomonaQueryJsLexer(CharStream input)
// public PomonaQueryJsLexer(CharStream input, RecognizerSharedState state) {
    if (!state) {
        state = new org.antlr.runtime.RecognizerSharedState();
    }

    (function(){
    }).call(this);

    this.dfa12 = new PomonaQueryJsLexer.DFA12(this);
    PomonaQueryJsLexer.superclass.constructor.call(this, input, state);


};

org.antlr.lang.augmentObject(PomonaQueryJsLexer, {
    EOF: -1,
    T__37: 37,
    T__38: 38,
    T__39: 39,
    T__40: 40,
    T__41: 41,
    T__42: 42,
    T__43: 43,
    T__44: 44,
    T__45: 45,
    T__46: 46,
    T__47: 47,
    T__48: 48,
    T__49: 49,
    T__50: 50,
    T__51: 51,
    T__52: 52,
    T__53: 53,
    T__54: 54,
    T__55: 55,
    T__56: 56,
    T__57: 57,
    T__58: 58,
    T__59: 59,
    T__60: 60,
    T__61: 61,
    T__62: 62,
    T__63: 63,
    T__64: 64,
    T__65: 65,
    T__66: 66,
    T__67: 67,
    T__68: 68,
    T__69: 69,
    T__70: 70,
    T__71: 71,
    ROOT: 4,
    AND_OP: 5,
    OR_OP: 6,
    LT_OP: 7,
    GT_OP: 8,
    LE_OP: 9,
    GE_OP: 10,
    EQ_OP: 11,
    MUL_OP: 12,
    DIV_OP: 13,
    NE_OP: 14,
    ADD_OP: 15,
    SUB_OP: 16,
    MOD_OP: 17,
    DOT_OP: 18,
    AS_OP: 19,
    IN_OP: 20,
    NOT_OP: 21,
    DATETIME_LITERAL: 22,
    GUID_LITERAL: 23,
    METHOD_CALL: 24,
    INDEXER_ACCESS: 25,
    LAMBDA_OP: 26,
    ARRAY_LITERAL: 27,
    PREFIXED_STRING: 28,
    ID: 29,
    INT: 30,
    WS: 31,
    STRING: 32,
    HEX_DIGIT: 33,
    UNICODE_ESC: 34,
    OCTAL_ESC: 35,
    ESC_SEQ: 36
});

(function(){
var HIDDEN = org.antlr.runtime.Token.HIDDEN_CHANNEL,
    EOF = org.antlr.runtime.Token.EOF;
org.antlr.lang.extend(PomonaQueryJsLexer, org.antlr.runtime.Lexer, {
    EOF : -1,
    T__37 : 37,
    T__38 : 38,
    T__39 : 39,
    T__40 : 40,
    T__41 : 41,
    T__42 : 42,
    T__43 : 43,
    T__44 : 44,
    T__45 : 45,
    T__46 : 46,
    T__47 : 47,
    T__48 : 48,
    T__49 : 49,
    T__50 : 50,
    T__51 : 51,
    T__52 : 52,
    T__53 : 53,
    T__54 : 54,
    T__55 : 55,
    T__56 : 56,
    T__57 : 57,
    T__58 : 58,
    T__59 : 59,
    T__60 : 60,
    T__61 : 61,
    T__62 : 62,
    T__63 : 63,
    T__64 : 64,
    T__65 : 65,
    T__66 : 66,
    T__67 : 67,
    T__68 : 68,
    T__69 : 69,
    T__70 : 70,
    T__71 : 71,
    ROOT : 4,
    AND_OP : 5,
    OR_OP : 6,
    LT_OP : 7,
    GT_OP : 8,
    LE_OP : 9,
    GE_OP : 10,
    EQ_OP : 11,
    MUL_OP : 12,
    DIV_OP : 13,
    NE_OP : 14,
    ADD_OP : 15,
    SUB_OP : 16,
    MOD_OP : 17,
    DOT_OP : 18,
    AS_OP : 19,
    IN_OP : 20,
    NOT_OP : 21,
    DATETIME_LITERAL : 22,
    GUID_LITERAL : 23,
    METHOD_CALL : 24,
    INDEXER_ACCESS : 25,
    LAMBDA_OP : 26,
    ARRAY_LITERAL : 27,
    PREFIXED_STRING : 28,
    ID : 29,
    INT : 30,
    WS : 31,
    STRING : 32,
    HEX_DIGIT : 33,
    UNICODE_ESC : 34,
    OCTAL_ESC : 35,
    ESC_SEQ : 36,
    getGrammarFileName: function() { return "PomonaQueryJs.g"; }
});
org.antlr.lang.augmentObject(PomonaQueryJsLexer.prototype, {
    // $ANTLR start T__37
    mT__37: function()  {
        try {
            var _type = this.T__37;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:7:7: ( 'as' )
            // PomonaQueryJs.g:7:9: 'as'
            this.match("as"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__37",

    // $ANTLR start T__38
    mT__38: function()  {
        try {
            var _type = this.T__38;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:8:7: ( ':' )
            // PomonaQueryJs.g:8:9: ':'
            this.match(':'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__38",

    // $ANTLR start T__39
    mT__39: function()  {
        try {
            var _type = this.T__39;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:9:7: ( 'or' )
            // PomonaQueryJs.g:9:9: 'or'
            this.match("or"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__39",

    // $ANTLR start T__40
    mT__40: function()  {
        try {
            var _type = this.T__40;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:10:7: ( 'and' )
            // PomonaQueryJs.g:10:9: 'and'
            this.match("and"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__40",

    // $ANTLR start T__41
    mT__41: function()  {
        try {
            var _type = this.T__41;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:11:7: ( 'gt' )
            // PomonaQueryJs.g:11:9: 'gt'
            this.match("gt"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__41",

    // $ANTLR start T__42
    mT__42: function()  {
        try {
            var _type = this.T__42;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:12:7: ( '>' )
            // PomonaQueryJs.g:12:9: '>'
            this.match('>'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__42",

    // $ANTLR start T__43
    mT__43: function()  {
        try {
            var _type = this.T__43;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:13:7: ( 'lt' )
            // PomonaQueryJs.g:13:9: 'lt'
            this.match("lt"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__43",

    // $ANTLR start T__44
    mT__44: function()  {
        try {
            var _type = this.T__44;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:14:7: ( '<' )
            // PomonaQueryJs.g:14:9: '<'
            this.match('<'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__44",

    // $ANTLR start T__45
    mT__45: function()  {
        try {
            var _type = this.T__45;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:15:7: ( 'eq' )
            // PomonaQueryJs.g:15:9: 'eq'
            this.match("eq"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__45",

    // $ANTLR start T__46
    mT__46: function()  {
        try {
            var _type = this.T__46;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:16:7: ( '==' )
            // PomonaQueryJs.g:16:9: '=='
            this.match("=="); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__46",

    // $ANTLR start T__47
    mT__47: function()  {
        try {
            var _type = this.T__47;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:17:7: ( 'ge' )
            // PomonaQueryJs.g:17:9: 'ge'
            this.match("ge"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__47",

    // $ANTLR start T__48
    mT__48: function()  {
        try {
            var _type = this.T__48;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:18:7: ( '>=' )
            // PomonaQueryJs.g:18:9: '>='
            this.match(">="); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__48",

    // $ANTLR start T__49
    mT__49: function()  {
        try {
            var _type = this.T__49;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:19:7: ( 'le' )
            // PomonaQueryJs.g:19:9: 'le'
            this.match("le"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__49",

    // $ANTLR start T__50
    mT__50: function()  {
        try {
            var _type = this.T__50;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:20:7: ( '<=' )
            // PomonaQueryJs.g:20:9: '<='
            this.match("<="); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__50",

    // $ANTLR start T__51
    mT__51: function()  {
        try {
            var _type = this.T__51;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:21:7: ( 'ne' )
            // PomonaQueryJs.g:21:9: 'ne'
            this.match("ne"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__51",

    // $ANTLR start T__52
    mT__52: function()  {
        try {
            var _type = this.T__52;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:22:7: ( '!=' )
            // PomonaQueryJs.g:22:9: '!='
            this.match("!="); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__52",

    // $ANTLR start T__53
    mT__53: function()  {
        try {
            var _type = this.T__53;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:23:7: ( 'in' )
            // PomonaQueryJs.g:23:9: 'in'
            this.match("in"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__53",

    // $ANTLR start T__54
    mT__54: function()  {
        try {
            var _type = this.T__54;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:24:7: ( 'add' )
            // PomonaQueryJs.g:24:9: 'add'
            this.match("add"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__54",

    // $ANTLR start T__55
    mT__55: function()  {
        try {
            var _type = this.T__55;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:25:7: ( '+' )
            // PomonaQueryJs.g:25:9: '+'
            this.match('+'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__55",

    // $ANTLR start T__56
    mT__56: function()  {
        try {
            var _type = this.T__56;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:26:7: ( 'sub' )
            // PomonaQueryJs.g:26:9: 'sub'
            this.match("sub"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__56",

    // $ANTLR start T__57
    mT__57: function()  {
        try {
            var _type = this.T__57;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:27:7: ( '-' )
            // PomonaQueryJs.g:27:9: '-'
            this.match('-'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__57",

    // $ANTLR start T__58
    mT__58: function()  {
        try {
            var _type = this.T__58;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:28:7: ( 'mul' )
            // PomonaQueryJs.g:28:9: 'mul'
            this.match("mul"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__58",

    // $ANTLR start T__59
    mT__59: function()  {
        try {
            var _type = this.T__59;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:29:7: ( '*' )
            // PomonaQueryJs.g:29:9: '*'
            this.match('*'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__59",

    // $ANTLR start T__60
    mT__60: function()  {
        try {
            var _type = this.T__60;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:30:7: ( 'div' )
            // PomonaQueryJs.g:30:9: 'div'
            this.match("div"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__60",

    // $ANTLR start T__61
    mT__61: function()  {
        try {
            var _type = this.T__61;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:31:7: ( '/' )
            // PomonaQueryJs.g:31:9: '/'
            this.match('/'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__61",

    // $ANTLR start T__62
    mT__62: function()  {
        try {
            var _type = this.T__62;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:32:7: ( 'mod' )
            // PomonaQueryJs.g:32:9: 'mod'
            this.match("mod"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__62",

    // $ANTLR start T__63
    mT__63: function()  {
        try {
            var _type = this.T__63;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:33:7: ( '%' )
            // PomonaQueryJs.g:33:9: '%'
            this.match('%'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__63",

    // $ANTLR start T__64
    mT__64: function()  {
        try {
            var _type = this.T__64;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:34:7: ( 'not' )
            // PomonaQueryJs.g:34:9: 'not'
            this.match("not"); 




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__64",

    // $ANTLR start T__65
    mT__65: function()  {
        try {
            var _type = this.T__65;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:35:7: ( '!' )
            // PomonaQueryJs.g:35:9: '!'
            this.match('!'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__65",

    // $ANTLR start T__66
    mT__66: function()  {
        try {
            var _type = this.T__66;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:36:7: ( '.' )
            // PomonaQueryJs.g:36:9: '.'
            this.match('.'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__66",

    // $ANTLR start T__67
    mT__67: function()  {
        try {
            var _type = this.T__67;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:37:7: ( '(' )
            // PomonaQueryJs.g:37:9: '('
            this.match('('); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__67",

    // $ANTLR start T__68
    mT__68: function()  {
        try {
            var _type = this.T__68;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:38:7: ( ')' )
            // PomonaQueryJs.g:38:9: ')'
            this.match(')'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__68",

    // $ANTLR start T__69
    mT__69: function()  {
        try {
            var _type = this.T__69;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:39:7: ( '[' )
            // PomonaQueryJs.g:39:9: '['
            this.match('['); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__69",

    // $ANTLR start T__70
    mT__70: function()  {
        try {
            var _type = this.T__70;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:40:7: ( ']' )
            // PomonaQueryJs.g:40:9: ']'
            this.match(']'); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__70",

    // $ANTLR start T__71
    mT__71: function()  {
        try {
            var _type = this.T__71;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:41:7: ( ',' )
            // PomonaQueryJs.g:41:9: ','
            this.match(','); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "T__71",

    // $ANTLR start PREFIXED_STRING
    mPREFIXED_STRING: function()  {
        try {
            var _type = this.PREFIXED_STRING;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:38:2: ( ( 'a' .. 'z' | 'A' .. 'Z' ) ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' )* '\\'' (~ ( '\\\\' | '\\'' ) )* '\\'' )
            // PomonaQueryJs.g:38:4: ( 'a' .. 'z' | 'A' .. 'Z' ) ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' )* '\\'' (~ ( '\\\\' | '\\'' ) )* '\\''
            if ( (this.input.LA(1)>='A' && this.input.LA(1)<='Z')||(this.input.LA(1)>='a' && this.input.LA(1)<='z') ) {
                this.input.consume();

            }
            else {
                var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                this.recover(mse);
                throw mse;}

            // PomonaQueryJs.g:38:24: ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' )*
            loop1:
            do {
                var alt1=2;
                var LA1_0 = this.input.LA(1);

                if ( ((LA1_0>='0' && LA1_0<='9')||(LA1_0>='A' && LA1_0<='Z')||(LA1_0>='a' && LA1_0<='z')) ) {
                    alt1=1;
                }


                switch (alt1) {
                case 1 :
                    // PomonaQueryJs.g:
                    if ( (this.input.LA(1)>='0' && this.input.LA(1)<='9')||(this.input.LA(1)>='A' && this.input.LA(1)<='Z')||(this.input.LA(1)>='a' && this.input.LA(1)<='z') ) {
                        this.input.consume();

                    }
                    else {
                        var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                        this.recover(mse);
                        throw mse;}



                    break;

                default :
                    break loop1;
                }
            } while (true);

            this.match('\''); 
            // PomonaQueryJs.g:38:60: (~ ( '\\\\' | '\\'' ) )*
            loop2:
            do {
                var alt2=2;
                var LA2_0 = this.input.LA(1);

                if ( ((LA2_0>='\u0000' && LA2_0<='&')||(LA2_0>='(' && LA2_0<='[')||(LA2_0>=']' && LA2_0<='\uFFFF')) ) {
                    alt2=1;
                }


                switch (alt2) {
                case 1 :
                    // PomonaQueryJs.g:38:62: ~ ( '\\\\' | '\\'' )
                    if ( (this.input.LA(1)>='\u0000' && this.input.LA(1)<='&')||(this.input.LA(1)>='(' && this.input.LA(1)<='[')||(this.input.LA(1)>=']' && this.input.LA(1)<='\uFFFF') ) {
                        this.input.consume();

                    }
                    else {
                        var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                        this.recover(mse);
                        throw mse;}



                    break;

                default :
                    break loop2;
                }
            } while (true);

            this.match('\''); 



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "PREFIXED_STRING",

    // $ANTLR start ID
    mID: function()  {
        try {
            var _type = this.ID;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:41:5: ( ( '@' | 'a' .. 'z' | 'A' .. 'Z' | '_' ) ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' | '_' )* )
            // PomonaQueryJs.g:41:7: ( '@' | 'a' .. 'z' | 'A' .. 'Z' | '_' ) ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' | '_' )*
            if ( (this.input.LA(1)>='@' && this.input.LA(1)<='Z')||this.input.LA(1)=='_'||(this.input.LA(1)>='a' && this.input.LA(1)<='z') ) {
                this.input.consume();

            }
            else {
                var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                this.recover(mse);
                throw mse;}

            // PomonaQueryJs.g:41:35: ( 'a' .. 'z' | 'A' .. 'Z' | '0' .. '9' | '_' )*
            loop3:
            do {
                var alt3=2;
                var LA3_0 = this.input.LA(1);

                if ( ((LA3_0>='0' && LA3_0<='9')||(LA3_0>='A' && LA3_0<='Z')||LA3_0=='_'||(LA3_0>='a' && LA3_0<='z')) ) {
                    alt3=1;
                }


                switch (alt3) {
                case 1 :
                    // PomonaQueryJs.g:
                    if ( (this.input.LA(1)>='0' && this.input.LA(1)<='9')||(this.input.LA(1)>='A' && this.input.LA(1)<='Z')||this.input.LA(1)=='_'||(this.input.LA(1)>='a' && this.input.LA(1)<='z') ) {
                        this.input.consume();

                    }
                    else {
                        var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                        this.recover(mse);
                        throw mse;}



                    break;

                default :
                    break loop3;
                }
            } while (true);




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "ID",

    // $ANTLR start INT
    mINT: function()  {
        try {
            var _type = this.INT;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:44:5: ( ( '0' .. '9' )+ ( '.' ( '0' .. '9' )+ )? ( 'm' | 'M' | 'f' | 'F' )? )
            // PomonaQueryJs.g:44:7: ( '0' .. '9' )+ ( '.' ( '0' .. '9' )+ )? ( 'm' | 'M' | 'f' | 'F' )?
            // PomonaQueryJs.g:44:7: ( '0' .. '9' )+
            var cnt4=0;
            loop4:
            do {
                var alt4=2;
                var LA4_0 = this.input.LA(1);

                if ( ((LA4_0>='0' && LA4_0<='9')) ) {
                    alt4=1;
                }


                switch (alt4) {
                case 1 :
                    // PomonaQueryJs.g:44:7: '0' .. '9'
                    this.matchRange('0','9'); 


                    break;

                default :
                    if ( cnt4 >= 1 ) {
                        break loop4;
                    }
                        var eee = new org.antlr.runtime.EarlyExitException(4, this.input);
                        throw eee;
                }
                cnt4++;
            } while (true);

            // PomonaQueryJs.g:44:17: ( '.' ( '0' .. '9' )+ )?
            var alt6=2;
            var LA6_0 = this.input.LA(1);

            if ( (LA6_0=='.') ) {
                alt6=1;
            }
            switch (alt6) {
                case 1 :
                    // PomonaQueryJs.g:44:18: '.' ( '0' .. '9' )+
                    this.match('.'); 
                    // PomonaQueryJs.g:44:22: ( '0' .. '9' )+
                    var cnt5=0;
                    loop5:
                    do {
                        var alt5=2;
                        var LA5_0 = this.input.LA(1);

                        if ( ((LA5_0>='0' && LA5_0<='9')) ) {
                            alt5=1;
                        }


                        switch (alt5) {
                        case 1 :
                            // PomonaQueryJs.g:44:23: '0' .. '9'
                            this.matchRange('0','9'); 


                            break;

                        default :
                            if ( cnt5 >= 1 ) {
                                break loop5;
                            }
                                var eee = new org.antlr.runtime.EarlyExitException(5, this.input);
                                throw eee;
                        }
                        cnt5++;
                    } while (true);



                    break;

            }

            // PomonaQueryJs.g:44:36: ( 'm' | 'M' | 'f' | 'F' )?
            var alt7=2;
            var LA7_0 = this.input.LA(1);

            if ( (LA7_0=='F'||LA7_0=='M'||LA7_0=='f'||LA7_0=='m') ) {
                alt7=1;
            }
            switch (alt7) {
                case 1 :
                    // PomonaQueryJs.g:
                    if ( this.input.LA(1)=='F'||this.input.LA(1)=='M'||this.input.LA(1)=='f'||this.input.LA(1)=='m' ) {
                        this.input.consume();

                    }
                    else {
                        var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                        this.recover(mse);
                        throw mse;}



                    break;

            }




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "INT",

    // $ANTLR start WS
    mWS: function()  {
        try {
            var _type = this.WS;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:49:5: ( ( ' ' | '\\t' | '\\r' | '\\n' ) )
            // PomonaQueryJs.g:49:9: ( ' ' | '\\t' | '\\r' | '\\n' )
            if ( (this.input.LA(1)>='\t' && this.input.LA(1)<='\n')||this.input.LA(1)=='\r'||this.input.LA(1)==' ' ) {
                this.input.consume();

            }
            else {
                var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                this.recover(mse);
                throw mse;}

            _channel=HIDDEN;



            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "WS",

    // $ANTLR start STRING
    mSTRING: function()  {
        try {
            var _type = this.STRING;
            var _channel = org.antlr.runtime.BaseRecognizer.DEFAULT_TOKEN_CHANNEL;
            // PomonaQueryJs.g:62:5: ( ( '\\'' (~ ( '\\\\' | '\\'' ) )* '\\'' )+ )
            // PomonaQueryJs.g:62:8: ( '\\'' (~ ( '\\\\' | '\\'' ) )* '\\'' )+
            // PomonaQueryJs.g:62:8: ( '\\'' (~ ( '\\\\' | '\\'' ) )* '\\'' )+
            var cnt9=0;
            loop9:
            do {
                var alt9=2;
                var LA9_0 = this.input.LA(1);

                if ( (LA9_0=='\'') ) {
                    alt9=1;
                }


                switch (alt9) {
                case 1 :
                    // PomonaQueryJs.g:62:10: '\\'' (~ ( '\\\\' | '\\'' ) )* '\\''
                    this.match('\''); 
                    // PomonaQueryJs.g:62:15: (~ ( '\\\\' | '\\'' ) )*
                    loop8:
                    do {
                        var alt8=2;
                        var LA8_0 = this.input.LA(1);

                        if ( ((LA8_0>='\u0000' && LA8_0<='&')||(LA8_0>='(' && LA8_0<='[')||(LA8_0>=']' && LA8_0<='\uFFFF')) ) {
                            alt8=1;
                        }


                        switch (alt8) {
                        case 1 :
                            // PomonaQueryJs.g:62:17: ~ ( '\\\\' | '\\'' )
                            if ( (this.input.LA(1)>='\u0000' && this.input.LA(1)<='&')||(this.input.LA(1)>='(' && this.input.LA(1)<='[')||(this.input.LA(1)>=']' && this.input.LA(1)<='\uFFFF') ) {
                                this.input.consume();

                            }
                            else {
                                var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                                this.recover(mse);
                                throw mse;}



                            break;

                        default :
                            break loop8;
                        }
                    } while (true);

                    this.match('\''); 


                    break;

                default :
                    if ( cnt9 >= 1 ) {
                        break loop9;
                    }
                        var eee = new org.antlr.runtime.EarlyExitException(9, this.input);
                        throw eee;
                }
                cnt9++;
            } while (true);




            this.state.type = _type;
            this.state.channel = _channel;
        }
        finally {
        }
    },
    // $ANTLR end "STRING",

    // $ANTLR start HEX_DIGIT
    mHEX_DIGIT: function()  {
        try {
            // PomonaQueryJs.g:179:11: ( ( '0' .. '9' | 'a' .. 'f' | 'A' .. 'F' ) )
            // PomonaQueryJs.g:179:13: ( '0' .. '9' | 'a' .. 'f' | 'A' .. 'F' )
            if ( (this.input.LA(1)>='0' && this.input.LA(1)<='9')||(this.input.LA(1)>='A' && this.input.LA(1)<='F')||(this.input.LA(1)>='a' && this.input.LA(1)<='f') ) {
                this.input.consume();

            }
            else {
                var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                this.recover(mse);
                throw mse;}




        }
        finally {
        }
    },
    // $ANTLR end "HEX_DIGIT",

    // $ANTLR start ESC_SEQ
    mESC_SEQ: function()  {
        try {
            // PomonaQueryJs.g:183:5: ( '\\\\' ( 'b' | 't' | 'n' | 'f' | 'r' | '\\\"' | '\\'' | '\\\\' ) | UNICODE_ESC | OCTAL_ESC )
            var alt10=3;
            var LA10_0 = this.input.LA(1);

            if ( (LA10_0=='\\') ) {
                switch ( this.input.LA(2) ) {
                case '\"':
                case '\'':
                case '\\':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                    alt10=1;
                    break;
                case 'u':
                    alt10=2;
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    alt10=3;
                    break;
                default:
                    var nvae =
                        new org.antlr.runtime.NoViableAltException("", 10, 1, this.input);

                    throw nvae;
                }

            }
            else {
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 10, 0, this.input);

                throw nvae;
            }
            switch (alt10) {
                case 1 :
                    // PomonaQueryJs.g:183:9: '\\\\' ( 'b' | 't' | 'n' | 'f' | 'r' | '\\\"' | '\\'' | '\\\\' )
                    this.match('\\'); 
                    if ( this.input.LA(1)=='\"'||this.input.LA(1)=='\''||this.input.LA(1)=='\\'||this.input.LA(1)=='b'||this.input.LA(1)=='f'||this.input.LA(1)=='n'||this.input.LA(1)=='r'||this.input.LA(1)=='t' ) {
                        this.input.consume();

                    }
                    else {
                        var mse = new org.antlr.runtime.MismatchedSetException(null,this.input);
                        this.recover(mse);
                        throw mse;}



                    break;
                case 2 :
                    // PomonaQueryJs.g:184:9: UNICODE_ESC
                    this.mUNICODE_ESC(); 


                    break;
                case 3 :
                    // PomonaQueryJs.g:185:9: OCTAL_ESC
                    this.mOCTAL_ESC(); 


                    break;

            }
        }
        finally {
        }
    },
    // $ANTLR end "ESC_SEQ",

    // $ANTLR start OCTAL_ESC
    mOCTAL_ESC: function()  {
        try {
            // PomonaQueryJs.g:190:5: ( '\\\\' ( '0' .. '3' ) ( '0' .. '7' ) ( '0' .. '7' ) | '\\\\' ( '0' .. '7' ) ( '0' .. '7' ) | '\\\\' ( '0' .. '7' ) )
            var alt11=3;
            var LA11_0 = this.input.LA(1);

            if ( (LA11_0=='\\') ) {
                var LA11_1 = this.input.LA(2);

                if ( ((LA11_1>='0' && LA11_1<='3')) ) {
                    var LA11_2 = this.input.LA(3);

                    if ( ((LA11_2>='0' && LA11_2<='7')) ) {
                        var LA11_4 = this.input.LA(4);

                        if ( ((LA11_4>='0' && LA11_4<='7')) ) {
                            alt11=1;
                        }
                        else {
                            alt11=2;}
                    }
                    else {
                        alt11=3;}
                }
                else if ( ((LA11_1>='4' && LA11_1<='7')) ) {
                    var LA11_3 = this.input.LA(3);

                    if ( ((LA11_3>='0' && LA11_3<='7')) ) {
                        alt11=2;
                    }
                    else {
                        alt11=3;}
                }
                else {
                    var nvae =
                        new org.antlr.runtime.NoViableAltException("", 11, 1, this.input);

                    throw nvae;
                }
            }
            else {
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 11, 0, this.input);

                throw nvae;
            }
            switch (alt11) {
                case 1 :
                    // PomonaQueryJs.g:190:9: '\\\\' ( '0' .. '3' ) ( '0' .. '7' ) ( '0' .. '7' )
                    this.match('\\'); 
                    // PomonaQueryJs.g:190:14: ( '0' .. '3' )
                    // PomonaQueryJs.g:190:15: '0' .. '3'
                    this.matchRange('0','3'); 



                    // PomonaQueryJs.g:190:25: ( '0' .. '7' )
                    // PomonaQueryJs.g:190:26: '0' .. '7'
                    this.matchRange('0','7'); 



                    // PomonaQueryJs.g:190:36: ( '0' .. '7' )
                    // PomonaQueryJs.g:190:37: '0' .. '7'
                    this.matchRange('0','7'); 





                    break;
                case 2 :
                    // PomonaQueryJs.g:191:9: '\\\\' ( '0' .. '7' ) ( '0' .. '7' )
                    this.match('\\'); 
                    // PomonaQueryJs.g:191:14: ( '0' .. '7' )
                    // PomonaQueryJs.g:191:15: '0' .. '7'
                    this.matchRange('0','7'); 



                    // PomonaQueryJs.g:191:25: ( '0' .. '7' )
                    // PomonaQueryJs.g:191:26: '0' .. '7'
                    this.matchRange('0','7'); 





                    break;
                case 3 :
                    // PomonaQueryJs.g:192:9: '\\\\' ( '0' .. '7' )
                    this.match('\\'); 
                    // PomonaQueryJs.g:192:14: ( '0' .. '7' )
                    // PomonaQueryJs.g:192:15: '0' .. '7'
                    this.matchRange('0','7'); 





                    break;

            }
        }
        finally {
        }
    },
    // $ANTLR end "OCTAL_ESC",

    // $ANTLR start UNICODE_ESC
    mUNICODE_ESC: function()  {
        try {
            // PomonaQueryJs.g:197:5: ( '\\\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT )
            // PomonaQueryJs.g:197:9: '\\\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
            this.match('\\'); 
            this.match('u'); 
            this.mHEX_DIGIT(); 
            this.mHEX_DIGIT(); 
            this.mHEX_DIGIT(); 
            this.mHEX_DIGIT(); 



        }
        finally {
        }
    },
    // $ANTLR end "UNICODE_ESC",

    mTokens: function() {
        // PomonaQueryJs.g:1:8: ( T__37 | T__38 | T__39 | T__40 | T__41 | T__42 | T__43 | T__44 | T__45 | T__46 | T__47 | T__48 | T__49 | T__50 | T__51 | T__52 | T__53 | T__54 | T__55 | T__56 | T__57 | T__58 | T__59 | T__60 | T__61 | T__62 | T__63 | T__64 | T__65 | T__66 | T__67 | T__68 | T__69 | T__70 | T__71 | PREFIXED_STRING | ID | INT | WS | STRING )
        var alt12=40;
        alt12 = this.dfa12.predict(this.input);
        switch (alt12) {
            case 1 :
                // PomonaQueryJs.g:1:10: T__37
                this.mT__37(); 


                break;
            case 2 :
                // PomonaQueryJs.g:1:16: T__38
                this.mT__38(); 


                break;
            case 3 :
                // PomonaQueryJs.g:1:22: T__39
                this.mT__39(); 


                break;
            case 4 :
                // PomonaQueryJs.g:1:28: T__40
                this.mT__40(); 


                break;
            case 5 :
                // PomonaQueryJs.g:1:34: T__41
                this.mT__41(); 


                break;
            case 6 :
                // PomonaQueryJs.g:1:40: T__42
                this.mT__42(); 


                break;
            case 7 :
                // PomonaQueryJs.g:1:46: T__43
                this.mT__43(); 


                break;
            case 8 :
                // PomonaQueryJs.g:1:52: T__44
                this.mT__44(); 


                break;
            case 9 :
                // PomonaQueryJs.g:1:58: T__45
                this.mT__45(); 


                break;
            case 10 :
                // PomonaQueryJs.g:1:64: T__46
                this.mT__46(); 


                break;
            case 11 :
                // PomonaQueryJs.g:1:70: T__47
                this.mT__47(); 


                break;
            case 12 :
                // PomonaQueryJs.g:1:76: T__48
                this.mT__48(); 


                break;
            case 13 :
                // PomonaQueryJs.g:1:82: T__49
                this.mT__49(); 


                break;
            case 14 :
                // PomonaQueryJs.g:1:88: T__50
                this.mT__50(); 


                break;
            case 15 :
                // PomonaQueryJs.g:1:94: T__51
                this.mT__51(); 


                break;
            case 16 :
                // PomonaQueryJs.g:1:100: T__52
                this.mT__52(); 


                break;
            case 17 :
                // PomonaQueryJs.g:1:106: T__53
                this.mT__53(); 


                break;
            case 18 :
                // PomonaQueryJs.g:1:112: T__54
                this.mT__54(); 


                break;
            case 19 :
                // PomonaQueryJs.g:1:118: T__55
                this.mT__55(); 


                break;
            case 20 :
                // PomonaQueryJs.g:1:124: T__56
                this.mT__56(); 


                break;
            case 21 :
                // PomonaQueryJs.g:1:130: T__57
                this.mT__57(); 


                break;
            case 22 :
                // PomonaQueryJs.g:1:136: T__58
                this.mT__58(); 


                break;
            case 23 :
                // PomonaQueryJs.g:1:142: T__59
                this.mT__59(); 


                break;
            case 24 :
                // PomonaQueryJs.g:1:148: T__60
                this.mT__60(); 


                break;
            case 25 :
                // PomonaQueryJs.g:1:154: T__61
                this.mT__61(); 


                break;
            case 26 :
                // PomonaQueryJs.g:1:160: T__62
                this.mT__62(); 


                break;
            case 27 :
                // PomonaQueryJs.g:1:166: T__63
                this.mT__63(); 


                break;
            case 28 :
                // PomonaQueryJs.g:1:172: T__64
                this.mT__64(); 


                break;
            case 29 :
                // PomonaQueryJs.g:1:178: T__65
                this.mT__65(); 


                break;
            case 30 :
                // PomonaQueryJs.g:1:184: T__66
                this.mT__66(); 


                break;
            case 31 :
                // PomonaQueryJs.g:1:190: T__67
                this.mT__67(); 


                break;
            case 32 :
                // PomonaQueryJs.g:1:196: T__68
                this.mT__68(); 


                break;
            case 33 :
                // PomonaQueryJs.g:1:202: T__69
                this.mT__69(); 


                break;
            case 34 :
                // PomonaQueryJs.g:1:208: T__70
                this.mT__70(); 


                break;
            case 35 :
                // PomonaQueryJs.g:1:214: T__71
                this.mT__71(); 


                break;
            case 36 :
                // PomonaQueryJs.g:1:220: PREFIXED_STRING
                this.mPREFIXED_STRING(); 


                break;
            case 37 :
                // PomonaQueryJs.g:1:236: ID
                this.mID(); 


                break;
            case 38 :
                // PomonaQueryJs.g:1:239: INT
                this.mINT(); 


                break;
            case 39 :
                // PomonaQueryJs.g:1:243: WS
                this.mWS(); 


                break;
            case 40 :
                // PomonaQueryJs.g:1:246: STRING
                this.mSTRING(); 


                break;

        }

    }

}, true); // important to pass true to overwrite default implementations

org.antlr.lang.augmentObject(PomonaQueryJsLexer, {
    DFA12_eotS:
        "\u0001\uffff\u0001\u001c\u0001\uffff\u0002\u001c\u0001\u0029\u0001"+
    "\u001c\u0001\u002d\u0001\u001c\u0001\uffff\u0001\u001c\u0001\u0032\u0001"+
    "\u001c\u0001\uffff\u0001\u001c\u0001\uffff\u0001\u001c\u0001\uffff\u0001"+
    "\u001c\u0008\uffff\u0001\u001c\u0004\uffff\u0001\u0038\u0003\u001c\u0001"+
    "\uffff\u0001\u003b\u0001\u003c\u0001\u003d\u0002\uffff\u0001\u003e\u0001"+
    "\u003f\u0002\uffff\u0001\u0040\u0001\u0041\u0001\u001c\u0002\uffff\u0001"+
    "\u0043\u0004\u001c\u0001\uffff\u0001\u0048\u0001\u0049\u0007\uffff\u0001"+
    "\u004a\u0001\uffff\u0001\u004b\u0001\u004c\u0001\u004d\u0001\u004e\u0007"+
    "\uffff",
    DFA12_eofS:
        "\u004f\uffff",
    DFA12_minS:
        "\u0001\u0009\u0001\u0027\u0001\uffff\u0002\u0027\u0001\u003d\u0001"+
    "\u0027\u0001\u003d\u0001\u0027\u0001\uffff\u0001\u0027\u0001\u003d\u0001"+
    "\u0027\u0001\uffff\u0001\u0027\u0001\uffff\u0001\u0027\u0001\uffff\u0001"+
    "\u0027\u0008\uffff\u0001\u0027\u0004\uffff\u0004\u0027\u0001\uffff\u0003"+
    "\u0027\u0002\uffff\u0002\u0027\u0002\uffff\u0003\u0027\u0002\uffff\u0005"+
    "\u0027\u0001\uffff\u0002\u0027\u0007\uffff\u0001\u0027\u0001\uffff\u0004"+
    "\u0027\u0007\uffff",
    DFA12_maxS:
        "\u0002\u007a\u0001\uffff\u0002\u007a\u0001\u003d\u0001\u007a\u0001"+
    "\u003d\u0001\u007a\u0001\uffff\u0001\u007a\u0001\u003d\u0001\u007a\u0001"+
    "\uffff\u0001\u007a\u0001\uffff\u0001\u007a\u0001\uffff\u0001\u007a\u0008"+
    "\uffff\u0001\u007a\u0004\uffff\u0004\u007a\u0001\uffff\u0003\u007a\u0002"+
    "\uffff\u0002\u007a\u0002\uffff\u0003\u007a\u0002\uffff\u0005\u007a\u0001"+
    "\uffff\u0002\u007a\u0007\uffff\u0001\u007a\u0001\uffff\u0004\u007a\u0007"+
    "\uffff",
    DFA12_acceptS:
        "\u0002\uffff\u0001\u0002\u0006\uffff\u0001\u000a\u0003\uffff\u0001"+
    "\u0013\u0001\uffff\u0001\u0015\u0001\uffff\u0001\u0017\u0001\uffff\u0001"+
    "\u0019\u0001\u001b\u0001\u001e\u0001\u001f\u0001\u0020\u0001\u0021\u0001"+
    "\u0022\u0001\u0023\u0001\uffff\u0001\u0025\u0001\u0026\u0001\u0027\u0001"+
    "\u0028\u0004\uffff\u0001\u0024\u0003\uffff\u0001\u000c\u0001\u0006\u0002"+
    "\uffff\u0001\u000e\u0001\u0008\u0003\uffff\u0001\u0010\u0001\u001d\u0005"+
    "\uffff\u0001\u0001\u0002\uffff\u0001\u0003\u0001\u0005\u0001\u000b\u0001"+
    "\u0007\u0001\u000d\u0001\u0009\u0001\u000f\u0001\uffff\u0001\u0011\u0004"+
    "\uffff\u0001\u0004\u0001\u0012\u0001\u001c\u0001\u0014\u0001\u0016\u0001"+
    "\u001a\u0001\u0018",
    DFA12_specialS:
        "\u004f\uffff}>",
    DFA12_transitionS: [
            "\u0002\u001e\u0002\uffff\u0001\u001e\u0012\uffff\u0001\u001e"+
            "\u0001\u000b\u0003\uffff\u0001\u0014\u0001\uffff\u0001\u001f"+
            "\u0001\u0016\u0001\u0017\u0001\u0011\u0001\u000d\u0001\u001a"+
            "\u0001\u000f\u0001\u0015\u0001\u0013\u000a\u001d\u0001\u0002"+
            "\u0001\uffff\u0001\u0007\u0001\u0009\u0001\u0005\u0001\uffff"+
            "\u0001\u001c\u001a\u001b\u0001\u0018\u0001\uffff\u0001\u0019"+
            "\u0001\uffff\u0001\u001c\u0001\uffff\u0001\u0001\u0002\u001b"+
            "\u0001\u0012\u0001\u0008\u0001\u001b\u0001\u0004\u0001\u001b"+
            "\u0001\u000c\u0002\u001b\u0001\u0006\u0001\u0010\u0001\u000a"+
            "\u0001\u0003\u0003\u001b\u0001\u000e\u0007\u001b",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0003\u0023\u0001\u0022\u0009\u0023\u0001\u0021"+
            "\u0004\u0023\u0001\u0020\u0007\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0011\u0023\u0001\u0025\u0008\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0004\u0023\u0001\u0027\u000e\u0023\u0001\u0026"+
            "\u0006\u0023",
            "\u0001\u0028",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0004\u0023\u0001\u002b\u000e\u0023\u0001\u002a"+
            "\u0006\u0023",
            "\u0001\u002c",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0010\u0023\u0001\u002e\u0009\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0004\u0023\u0001\u002f\u0009\u0023\u0001\u0030"+
            "\u000b\u0023",
            "\u0001\u0031",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u000d\u0023\u0001\u0033\u000c\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0014\u0023\u0001\u0034\u0005\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u000e\u0023\u0001\u0036\u0005\u0023\u0001\u0035"+
            "\u0005\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0008\u0023\u0001\u0037\u0011\u0023",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u001a\u0023",
            "",
            "",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0003\u0023\u0001\u0039\u0016\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0003\u0023\u0001\u003a\u0016\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u001a\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0013\u0023\u0001\u0042\u0006\u0023",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0001\u0023\u0001\u0044\u0018\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u000b\u0023\u0001\u0045\u000e\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0003\u0023\u0001\u0046\u0016\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0006\uffff\u0015\u0023\u0001\u0047\u0004\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "\u0001\u0024\u0008\uffff\u000a\u0023\u0007\uffff\u001a\u0023"+
            "\u0004\uffff\u0001\u001c\u0001\uffff\u001a\u0023",
            "",
            "",
            "",
            "",
            "",
            "",
            ""
    ]
});

org.antlr.lang.augmentObject(PomonaQueryJsLexer, {
    DFA12_eot:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsLexer.DFA12_eotS),
    DFA12_eof:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsLexer.DFA12_eofS),
    DFA12_min:
        org.antlr.runtime.DFA.unpackEncodedStringToUnsignedChars(PomonaQueryJsLexer.DFA12_minS),
    DFA12_max:
        org.antlr.runtime.DFA.unpackEncodedStringToUnsignedChars(PomonaQueryJsLexer.DFA12_maxS),
    DFA12_accept:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsLexer.DFA12_acceptS),
    DFA12_special:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsLexer.DFA12_specialS),
    DFA12_transition: (function() {
        var a = [],
            i,
            numStates = PomonaQueryJsLexer.DFA12_transitionS.length;
        for (i=0; i<numStates; i++) {
            a.push(org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsLexer.DFA12_transitionS[i]));
        }
        return a;
    })()
});

PomonaQueryJsLexer.DFA12 = function(recognizer) {
    this.recognizer = recognizer;
    this.decisionNumber = 12;
    this.eot = PomonaQueryJsLexer.DFA12_eot;
    this.eof = PomonaQueryJsLexer.DFA12_eof;
    this.min = PomonaQueryJsLexer.DFA12_min;
    this.max = PomonaQueryJsLexer.DFA12_max;
    this.accept = PomonaQueryJsLexer.DFA12_accept;
    this.special = PomonaQueryJsLexer.DFA12_special;
    this.transition = PomonaQueryJsLexer.DFA12_transition;
};

org.antlr.lang.extend(PomonaQueryJsLexer.DFA12, org.antlr.runtime.DFA, {
    getDescription: function() {
        return "1:1: Tokens : ( T__37 | T__38 | T__39 | T__40 | T__41 | T__42 | T__43 | T__44 | T__45 | T__46 | T__47 | T__48 | T__49 | T__50 | T__51 | T__52 | T__53 | T__54 | T__55 | T__56 | T__57 | T__58 | T__59 | T__60 | T__61 | T__62 | T__63 | T__64 | T__65 | T__66 | T__67 | T__68 | T__69 | T__70 | T__71 | PREFIXED_STRING | ID | INT | WS | STRING );";
    },
    dummy: null
});
 
})();