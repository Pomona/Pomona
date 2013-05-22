// $ANTLR 3.3 Nov 30, 2010 12:50:56 PomonaQueryJs.g 2013-05-03 11:58:56

var PomonaQueryJsParser = function(input, state) {
    if (!state) {
        state = new org.antlr.runtime.RecognizerSharedState();
    }

    (function(){
    }).call(this);

    PomonaQueryJsParser.superclass.constructor.call(this, input, state);

    this.dfa25 = new PomonaQueryJsParser.DFA25(this);

         

    /* @todo only create adaptor if output=AST */
    this.adaptor = new org.antlr.runtime.tree.CommonTreeAdaptor();

};

org.antlr.lang.augmentObject(PomonaQueryJsParser, {
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
// public class variables
var EOF= -1,
    T__37= 37,
    T__38= 38,
    T__39= 39,
    T__40= 40,
    T__41= 41,
    T__42= 42,
    T__43= 43,
    T__44= 44,
    T__45= 45,
    T__46= 46,
    T__47= 47,
    T__48= 48,
    T__49= 49,
    T__50= 50,
    T__51= 51,
    T__52= 52,
    T__53= 53,
    T__54= 54,
    T__55= 55,
    T__56= 56,
    T__57= 57,
    T__58= 58,
    T__59= 59,
    T__60= 60,
    T__61= 61,
    T__62= 62,
    T__63= 63,
    T__64= 64,
    T__65= 65,
    T__66= 66,
    T__67= 67,
    T__68= 68,
    T__69= 69,
    T__70= 70,
    T__71= 71,
    ROOT= 4,
    AND_OP= 5,
    OR_OP= 6,
    LT_OP= 7,
    GT_OP= 8,
    LE_OP= 9,
    GE_OP= 10,
    EQ_OP= 11,
    MUL_OP= 12,
    DIV_OP= 13,
    NE_OP= 14,
    ADD_OP= 15,
    SUB_OP= 16,
    MOD_OP= 17,
    DOT_OP= 18,
    AS_OP= 19,
    IN_OP= 20,
    NOT_OP= 21,
    DATETIME_LITERAL= 22,
    GUID_LITERAL= 23,
    METHOD_CALL= 24,
    INDEXER_ACCESS= 25,
    LAMBDA_OP= 26,
    ARRAY_LITERAL= 27,
    PREFIXED_STRING= 28,
    ID= 29,
    INT= 30,
    WS= 31,
    STRING= 32,
    HEX_DIGIT= 33,
    UNICODE_ESC= 34,
    OCTAL_ESC= 35,
    ESC_SEQ= 36;

// public instance methods/vars
org.antlr.lang.extend(PomonaQueryJsParser, org.antlr.runtime.Parser, {
        
    setTreeAdaptor: function(adaptor) {
        this.adaptor = adaptor;
    },
    getTreeAdaptor: function() {
        return this.adaptor;
    },

    getTokenNames: function() { return PomonaQueryJsParser.tokenNames; },
    getGrammarFileName: function() { return "PomonaQueryJs.g"; }
});
org.antlr.lang.augmentObject(PomonaQueryJsParser.prototype, {

    // inline static return class
    parse_return: (function() {
        PomonaQueryJsParser.parse_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.parse_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:66:8: public parse : exp EOF -> ^( ROOT exp ) ;
    // $ANTLR start "parse"
    parse: function() {
        var retval = new PomonaQueryJsParser.parse_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var EOF2 = null;
         var exp1 = null;

        var EOF2_tree=null;
        var stream_EOF=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token EOF");
        var stream_exp=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule exp");
        try {
            // PomonaQueryJs.g:67:2: ( exp EOF -> ^( ROOT exp ) )
            // PomonaQueryJs.g:67:4: exp EOF
            this.pushFollow(PomonaQueryJsParser.FOLLOW_exp_in_parse450);
            exp1=this.exp();

            this.state._fsp--;

            stream_exp.add(exp1.getTree());
            EOF2=this.match(this.input,EOF,PomonaQueryJsParser.FOLLOW_EOF_in_parse452);  
            stream_EOF.add(EOF2);



            // AST REWRITE
            // elements: exp
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 67:12: -> ^( ROOT exp )
            {
                // PomonaQueryJs.g:67:15: ^( ROOT exp )
                {
                var root_1 = this.adaptor.nil();
                root_1 = this.adaptor.becomeRoot(this.adaptor.create(ROOT, "ROOT"), root_1);

                this.adaptor.addChild(root_1, stream_exp.nextTree());

                this.adaptor.addChild(root_0, root_1);
                }

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    exp_return: (function() {
        PomonaQueryJsParser.exp_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.exp_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:70:1: exp : as_expression ;
    // $ANTLR start "exp"
    exp: function() {
        var retval = new PomonaQueryJsParser.exp_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var as_expression3 = null;


        try {
            // PomonaQueryJs.g:71:2: ( as_expression )
            // PomonaQueryJs.g:71:4: as_expression
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_as_expression_in_exp471);
            as_expression3=this.as_expression();

            this.state._fsp--;

            this.adaptor.addChild(root_0, as_expression3.getTree());



            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    as_expression_return: (function() {
        PomonaQueryJsParser.as_expression_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.as_expression_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:74:1: as_expression : lambda_expression ( 'as' lambda_expression )? -> ^( AS_OP ( lambda_expression )+ ) ;
    // $ANTLR start "as_expression"
    as_expression: function() {
        var retval = new PomonaQueryJsParser.as_expression_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal5 = null;
         var lambda_expression4 = null;
         var lambda_expression6 = null;

        var string_literal5_tree=null;
        var stream_37=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 37");
        var stream_lambda_expression=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule lambda_expression");
        try {
            // PomonaQueryJs.g:75:2: ( lambda_expression ( 'as' lambda_expression )? -> ^( AS_OP ( lambda_expression )+ ) )
            // PomonaQueryJs.g:75:4: lambda_expression ( 'as' lambda_expression )?
            this.pushFollow(PomonaQueryJsParser.FOLLOW_lambda_expression_in_as_expression482);
            lambda_expression4=this.lambda_expression();

            this.state._fsp--;

            stream_lambda_expression.add(lambda_expression4.getTree());
            // PomonaQueryJs.g:75:22: ( 'as' lambda_expression )?
            var alt1=2;
            var LA1_0 = this.input.LA(1);

            if ( (LA1_0==37) ) {
                alt1=1;
            }
            switch (alt1) {
                case 1 :
                    // PomonaQueryJs.g:75:24: 'as' lambda_expression
                    string_literal5=this.match(this.input,37,PomonaQueryJsParser.FOLLOW_37_in_as_expression486);  
                    stream_37.add(string_literal5);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_lambda_expression_in_as_expression488);
                    lambda_expression6=this.lambda_expression();

                    this.state._fsp--;

                    stream_lambda_expression.add(lambda_expression6.getTree());


                    break;

            }



            // AST REWRITE
            // elements: lambda_expression
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 75:50: -> ^( AS_OP ( lambda_expression )+ )
            {
                // PomonaQueryJs.g:75:53: ^( AS_OP ( lambda_expression )+ )
                {
                var root_1 = this.adaptor.nil();
                root_1 = this.adaptor.becomeRoot(this.adaptor.create(AS_OP, "AS_OP"), root_1);

                if ( !(stream_lambda_expression.hasNext()) ) {
                    throw new org.antlr.runtime.tree.RewriteEarlyExitException();
                }
                while ( stream_lambda_expression.hasNext() ) {
                    this.adaptor.addChild(root_1, stream_lambda_expression.nextTree());

                }
                stream_lambda_expression.reset();

                this.adaptor.addChild(root_0, root_1);
                }

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    lambda_expression_return: (function() {
        PomonaQueryJsParser.lambda_expression_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.lambda_expression_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:78:1: lambda_expression : or_expression ( ':' or_expression )? -> ^( LAMBDA_OP ( or_expression )+ ) ;
    // $ANTLR start "lambda_expression"
    lambda_expression: function() {
        var retval = new PomonaQueryJsParser.lambda_expression_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var char_literal8 = null;
         var or_expression7 = null;
         var or_expression9 = null;

        var char_literal8_tree=null;
        var stream_38=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 38");
        var stream_or_expression=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule or_expression");
        try {
            // PomonaQueryJs.g:79:2: ( or_expression ( ':' or_expression )? -> ^( LAMBDA_OP ( or_expression )+ ) )
            // PomonaQueryJs.g:79:4: or_expression ( ':' or_expression )?
            this.pushFollow(PomonaQueryJsParser.FOLLOW_or_expression_in_lambda_expression511);
            or_expression7=this.or_expression();

            this.state._fsp--;

            stream_or_expression.add(or_expression7.getTree());
            // PomonaQueryJs.g:79:18: ( ':' or_expression )?
            var alt2=2;
            var LA2_0 = this.input.LA(1);

            if ( (LA2_0==38) ) {
                alt2=1;
            }
            switch (alt2) {
                case 1 :
                    // PomonaQueryJs.g:79:20: ':' or_expression
                    char_literal8=this.match(this.input,38,PomonaQueryJsParser.FOLLOW_38_in_lambda_expression515);  
                    stream_38.add(char_literal8);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_or_expression_in_lambda_expression517);
                    or_expression9=this.or_expression();

                    this.state._fsp--;

                    stream_or_expression.add(or_expression9.getTree());


                    break;

            }



            // AST REWRITE
            // elements: or_expression
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 79:41: -> ^( LAMBDA_OP ( or_expression )+ )
            {
                // PomonaQueryJs.g:79:44: ^( LAMBDA_OP ( or_expression )+ )
                {
                var root_1 = this.adaptor.nil();
                root_1 = this.adaptor.becomeRoot(this.adaptor.create(LAMBDA_OP, "LAMBDA_OP"), root_1);

                if ( !(stream_or_expression.hasNext()) ) {
                    throw new org.antlr.runtime.tree.RewriteEarlyExitException();
                }
                while ( stream_or_expression.hasNext() ) {
                    this.adaptor.addChild(root_1, stream_or_expression.nextTree());

                }
                stream_or_expression.reset();

                this.adaptor.addChild(root_0, root_1);
                }

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    or_expression_return: (function() {
        PomonaQueryJsParser.or_expression_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.or_expression_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:82:1: or_expression : and_expression ( 'or' and_expression )* -> ^( OR_OP ( and_expression )+ ) ;
    // $ANTLR start "or_expression"
    or_expression: function() {
        var retval = new PomonaQueryJsParser.or_expression_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal11 = null;
         var and_expression10 = null;
         var and_expression12 = null;

        var string_literal11_tree=null;
        var stream_39=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 39");
        var stream_and_expression=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule and_expression");
        try {
            // PomonaQueryJs.g:83:2: ( and_expression ( 'or' and_expression )* -> ^( OR_OP ( and_expression )+ ) )
            // PomonaQueryJs.g:83:4: and_expression ( 'or' and_expression )*
            this.pushFollow(PomonaQueryJsParser.FOLLOW_and_expression_in_or_expression540);
            and_expression10=this.and_expression();

            this.state._fsp--;

            stream_and_expression.add(and_expression10.getTree());
            // PomonaQueryJs.g:83:19: ( 'or' and_expression )*
            loop3:
            do {
                var alt3=2;
                var LA3_0 = this.input.LA(1);

                if ( (LA3_0==39) ) {
                    alt3=1;
                }


                switch (alt3) {
                case 1 :
                    // PomonaQueryJs.g:83:21: 'or' and_expression
                    string_literal11=this.match(this.input,39,PomonaQueryJsParser.FOLLOW_39_in_or_expression544);  
                    stream_39.add(string_literal11);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_and_expression_in_or_expression546);
                    and_expression12=this.and_expression();

                    this.state._fsp--;

                    stream_and_expression.add(and_expression12.getTree());


                    break;

                default :
                    break loop3;
                }
            } while (true);



            // AST REWRITE
            // elements: and_expression
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 83:44: -> ^( OR_OP ( and_expression )+ )
            {
                // PomonaQueryJs.g:83:47: ^( OR_OP ( and_expression )+ )
                {
                var root_1 = this.adaptor.nil();
                root_1 = this.adaptor.becomeRoot(this.adaptor.create(OR_OP, "OR_OP"), root_1);

                if ( !(stream_and_expression.hasNext()) ) {
                    throw new org.antlr.runtime.tree.RewriteEarlyExitException();
                }
                while ( stream_and_expression.hasNext() ) {
                    this.adaptor.addChild(root_1, stream_and_expression.nextTree());

                }
                stream_and_expression.reset();

                this.adaptor.addChild(root_0, root_1);
                }

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    and_operator_return: (function() {
        PomonaQueryJsParser.and_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.and_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:86:1: and_operator : 'and' ;
    // $ANTLR start "and_operator"
    and_operator: function() {
        var retval = new PomonaQueryJsParser.and_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal13 = null;

        var string_literal13_tree=null;

        try {
            // PomonaQueryJs.g:87:2: ( 'and' )
            // PomonaQueryJs.g:87:4: 'and'
            root_0 = this.adaptor.nil();

            string_literal13=this.match(this.input,40,PomonaQueryJsParser.FOLLOW_40_in_and_operator569); 
            string_literal13_tree = this.adaptor.create(string_literal13);
            this.adaptor.addChild(root_0, string_literal13_tree);




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    and_expression_return: (function() {
        PomonaQueryJsParser.and_expression_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.and_expression_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:90:1: and_expression : relational_expr ( and_operator relational_expr )* -> ^( AND_OP ( relational_expr )+ ) ;
    // $ANTLR start "and_expression"
    and_expression: function() {
        var retval = new PomonaQueryJsParser.and_expression_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var relational_expr14 = null;
         var and_operator15 = null;
         var relational_expr16 = null;

        var stream_and_operator=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule and_operator");
        var stream_relational_expr=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule relational_expr");
        try {
            // PomonaQueryJs.g:91:2: ( relational_expr ( and_operator relational_expr )* -> ^( AND_OP ( relational_expr )+ ) )
            // PomonaQueryJs.g:91:4: relational_expr ( and_operator relational_expr )*
            this.pushFollow(PomonaQueryJsParser.FOLLOW_relational_expr_in_and_expression581);
            relational_expr14=this.relational_expr();

            this.state._fsp--;

            stream_relational_expr.add(relational_expr14.getTree());
            // PomonaQueryJs.g:91:20: ( and_operator relational_expr )*
            loop4:
            do {
                var alt4=2;
                var LA4_0 = this.input.LA(1);

                if ( (LA4_0==40) ) {
                    alt4=1;
                }


                switch (alt4) {
                case 1 :
                    // PomonaQueryJs.g:91:22: and_operator relational_expr
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_and_operator_in_and_expression585);
                    and_operator15=this.and_operator();

                    this.state._fsp--;

                    stream_and_operator.add(and_operator15.getTree());
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_relational_expr_in_and_expression587);
                    relational_expr16=this.relational_expr();

                    this.state._fsp--;

                    stream_relational_expr.add(relational_expr16.getTree());


                    break;

                default :
                    break loop4;
                }
            } while (true);



            // AST REWRITE
            // elements: relational_expr
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 91:54: -> ^( AND_OP ( relational_expr )+ )
            {
                // PomonaQueryJs.g:91:57: ^( AND_OP ( relational_expr )+ )
                {
                var root_1 = this.adaptor.nil();
                root_1 = this.adaptor.becomeRoot(this.adaptor.create(AND_OP, "AND_OP"), root_1);

                if ( !(stream_relational_expr.hasNext()) ) {
                    throw new org.antlr.runtime.tree.RewriteEarlyExitException();
                }
                while ( stream_relational_expr.hasNext() ) {
                    this.adaptor.addChild(root_1, stream_relational_expr.nextTree());

                }
                stream_relational_expr.reset();

                this.adaptor.addChild(root_0, root_1);
                }

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    relational_operator_return: (function() {
        PomonaQueryJsParser.relational_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.relational_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:96:1: relational_operator : ( ( 'gt' | '>' ) -> GT_OP | ( 'lt' | '<' ) -> LT_OP | ( 'eq' | '==' ) -> EQ_OP | ( 'ge' | '>=' ) -> GE_OP | ( 'le' | '<=' ) -> LE_OP | ( 'ne' | '!=' ) -> NE_OP | 'in' -> IN_OP );
    // $ANTLR start "relational_operator"
    relational_operator: function() {
        var retval = new PomonaQueryJsParser.relational_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal17 = null;
        var char_literal18 = null;
        var string_literal19 = null;
        var char_literal20 = null;
        var string_literal21 = null;
        var string_literal22 = null;
        var string_literal23 = null;
        var string_literal24 = null;
        var string_literal25 = null;
        var string_literal26 = null;
        var string_literal27 = null;
        var string_literal28 = null;
        var string_literal29 = null;

        var string_literal17_tree=null;
        var char_literal18_tree=null;
        var string_literal19_tree=null;
        var char_literal20_tree=null;
        var string_literal21_tree=null;
        var string_literal22_tree=null;
        var string_literal23_tree=null;
        var string_literal24_tree=null;
        var string_literal25_tree=null;
        var string_literal26_tree=null;
        var string_literal27_tree=null;
        var string_literal28_tree=null;
        var string_literal29_tree=null;
        var stream_49=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 49");
        var stream_48=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 48");
        var stream_45=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 45");
        var stream_44=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 44");
        var stream_47=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 47");
        var stream_46=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 46");
        var stream_43=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 43");
        var stream_42=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 42");
        var stream_41=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 41");
        var stream_51=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 51");
        var stream_52=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 52");
        var stream_53=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 53");
        var stream_50=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 50");

        try {
            // PomonaQueryJs.g:97:2: ( ( 'gt' | '>' ) -> GT_OP | ( 'lt' | '<' ) -> LT_OP | ( 'eq' | '==' ) -> EQ_OP | ( 'ge' | '>=' ) -> GE_OP | ( 'le' | '<=' ) -> LE_OP | ( 'ne' | '!=' ) -> NE_OP | 'in' -> IN_OP )
            var alt11=7;
            switch ( this.input.LA(1) ) {
            case 41:
            case 42:
                alt11=1;
                break;
            case 43:
            case 44:
                alt11=2;
                break;
            case 45:
            case 46:
                alt11=3;
                break;
            case 47:
            case 48:
                alt11=4;
                break;
            case 49:
            case 50:
                alt11=5;
                break;
            case 51:
            case 52:
                alt11=6;
                break;
            case 53:
                alt11=7;
                break;
            default:
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 11, 0, this.input);

                throw nvae;
            }

            switch (alt11) {
                case 1 :
                    // PomonaQueryJs.g:97:4: ( 'gt' | '>' )
                    // PomonaQueryJs.g:97:4: ( 'gt' | '>' )
                    var alt5=2;
                    var LA5_0 = this.input.LA(1);

                    if ( (LA5_0==41) ) {
                        alt5=1;
                    }
                    else if ( (LA5_0==42) ) {
                        alt5=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 5, 0, this.input);

                        throw nvae;
                    }
                    switch (alt5) {
                        case 1 :
                            // PomonaQueryJs.g:97:5: 'gt'
                            string_literal17=this.match(this.input,41,PomonaQueryJsParser.FOLLOW_41_in_relational_operator614);  
                            stream_41.add(string_literal17);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:97:10: '>'
                            char_literal18=this.match(this.input,42,PomonaQueryJsParser.FOLLOW_42_in_relational_operator616);  
                            stream_42.add(char_literal18);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 97:15: -> GT_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(GT_OP, "GT_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 2 :
                    // PomonaQueryJs.g:98:4: ( 'lt' | '<' )
                    // PomonaQueryJs.g:98:4: ( 'lt' | '<' )
                    var alt6=2;
                    var LA6_0 = this.input.LA(1);

                    if ( (LA6_0==43) ) {
                        alt6=1;
                    }
                    else if ( (LA6_0==44) ) {
                        alt6=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 6, 0, this.input);

                        throw nvae;
                    }
                    switch (alt6) {
                        case 1 :
                            // PomonaQueryJs.g:98:5: 'lt'
                            string_literal19=this.match(this.input,43,PomonaQueryJsParser.FOLLOW_43_in_relational_operator627);  
                            stream_43.add(string_literal19);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:98:10: '<'
                            char_literal20=this.match(this.input,44,PomonaQueryJsParser.FOLLOW_44_in_relational_operator629);  
                            stream_44.add(char_literal20);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 98:15: -> LT_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(LT_OP, "LT_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 3 :
                    // PomonaQueryJs.g:99:4: ( 'eq' | '==' )
                    // PomonaQueryJs.g:99:4: ( 'eq' | '==' )
                    var alt7=2;
                    var LA7_0 = this.input.LA(1);

                    if ( (LA7_0==45) ) {
                        alt7=1;
                    }
                    else if ( (LA7_0==46) ) {
                        alt7=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 7, 0, this.input);

                        throw nvae;
                    }
                    switch (alt7) {
                        case 1 :
                            // PomonaQueryJs.g:99:5: 'eq'
                            string_literal21=this.match(this.input,45,PomonaQueryJsParser.FOLLOW_45_in_relational_operator640);  
                            stream_45.add(string_literal21);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:99:10: '=='
                            string_literal22=this.match(this.input,46,PomonaQueryJsParser.FOLLOW_46_in_relational_operator642);  
                            stream_46.add(string_literal22);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 99:16: -> EQ_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(EQ_OP, "EQ_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 4 :
                    // PomonaQueryJs.g:100:4: ( 'ge' | '>=' )
                    // PomonaQueryJs.g:100:4: ( 'ge' | '>=' )
                    var alt8=2;
                    var LA8_0 = this.input.LA(1);

                    if ( (LA8_0==47) ) {
                        alt8=1;
                    }
                    else if ( (LA8_0==48) ) {
                        alt8=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 8, 0, this.input);

                        throw nvae;
                    }
                    switch (alt8) {
                        case 1 :
                            // PomonaQueryJs.g:100:5: 'ge'
                            string_literal23=this.match(this.input,47,PomonaQueryJsParser.FOLLOW_47_in_relational_operator653);  
                            stream_47.add(string_literal23);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:100:10: '>='
                            string_literal24=this.match(this.input,48,PomonaQueryJsParser.FOLLOW_48_in_relational_operator655);  
                            stream_48.add(string_literal24);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 100:16: -> GE_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(GE_OP, "GE_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 5 :
                    // PomonaQueryJs.g:101:4: ( 'le' | '<=' )
                    // PomonaQueryJs.g:101:4: ( 'le' | '<=' )
                    var alt9=2;
                    var LA9_0 = this.input.LA(1);

                    if ( (LA9_0==49) ) {
                        alt9=1;
                    }
                    else if ( (LA9_0==50) ) {
                        alt9=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 9, 0, this.input);

                        throw nvae;
                    }
                    switch (alt9) {
                        case 1 :
                            // PomonaQueryJs.g:101:5: 'le'
                            string_literal25=this.match(this.input,49,PomonaQueryJsParser.FOLLOW_49_in_relational_operator666);  
                            stream_49.add(string_literal25);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:101:10: '<='
                            string_literal26=this.match(this.input,50,PomonaQueryJsParser.FOLLOW_50_in_relational_operator668);  
                            stream_50.add(string_literal26);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 101:16: -> LE_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(LE_OP, "LE_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 6 :
                    // PomonaQueryJs.g:102:4: ( 'ne' | '!=' )
                    // PomonaQueryJs.g:102:4: ( 'ne' | '!=' )
                    var alt10=2;
                    var LA10_0 = this.input.LA(1);

                    if ( (LA10_0==51) ) {
                        alt10=1;
                    }
                    else if ( (LA10_0==52) ) {
                        alt10=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 10, 0, this.input);

                        throw nvae;
                    }
                    switch (alt10) {
                        case 1 :
                            // PomonaQueryJs.g:102:5: 'ne'
                            string_literal27=this.match(this.input,51,PomonaQueryJsParser.FOLLOW_51_in_relational_operator679);  
                            stream_51.add(string_literal27);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:102:10: '!='
                            string_literal28=this.match(this.input,52,PomonaQueryJsParser.FOLLOW_52_in_relational_operator681);  
                            stream_52.add(string_literal28);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 102:16: -> NE_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(NE_OP, "NE_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 7 :
                    // PomonaQueryJs.g:103:4: 'in'
                    string_literal29=this.match(this.input,53,PomonaQueryJsParser.FOLLOW_53_in_relational_operator691);  
                    stream_53.add(string_literal29);



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 103:9: -> IN_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(IN_OP, "IN_OP"));

                    }

                    retval.tree = root_0;

                    break;

            }
            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    relational_expr_return: (function() {
        PomonaQueryJsParser.relational_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.relational_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:114:1: relational_expr : additive_expr ( relational_operator additive_expr )? ;
    // $ANTLR start "relational_expr"
    relational_expr: function() {
        var retval = new PomonaQueryJsParser.relational_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var additive_expr30 = null;
         var relational_operator31 = null;
         var additive_expr32 = null;


        try {
            // PomonaQueryJs.g:115:2: ( additive_expr ( relational_operator additive_expr )? )
            // PomonaQueryJs.g:115:4: additive_expr ( relational_operator additive_expr )?
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_additive_expr_in_relational_expr708);
            additive_expr30=this.additive_expr();

            this.state._fsp--;

            this.adaptor.addChild(root_0, additive_expr30.getTree());
            // PomonaQueryJs.g:115:18: ( relational_operator additive_expr )?
            var alt12=2;
            var LA12_0 = this.input.LA(1);

            if ( ((LA12_0>=41 && LA12_0<=53)) ) {
                alt12=1;
            }
            switch (alt12) {
                case 1 :
                    // PomonaQueryJs.g:115:19: relational_operator additive_expr
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_relational_operator_in_relational_expr711);
                    relational_operator31=this.relational_operator();

                    this.state._fsp--;

                    root_0 = this.adaptor.becomeRoot(relational_operator31.getTree(), root_0);
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_additive_expr_in_relational_expr714);
                    additive_expr32=this.additive_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, additive_expr32.getTree());


                    break;

            }




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    additive_operator_return: (function() {
        PomonaQueryJsParser.additive_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.additive_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:118:1: additive_operator : ( ( 'add' | '+' ) -> ADD_OP | ( 'sub' | '-' ) -> SUB_OP );
    // $ANTLR start "additive_operator"
    additive_operator: function() {
        var retval = new PomonaQueryJsParser.additive_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal33 = null;
        var char_literal34 = null;
        var string_literal35 = null;
        var char_literal36 = null;

        var string_literal33_tree=null;
        var char_literal34_tree=null;
        var string_literal35_tree=null;
        var char_literal36_tree=null;
        var stream_57=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 57");
        var stream_56=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 56");
        var stream_55=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 55");
        var stream_54=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 54");

        try {
            // PomonaQueryJs.g:119:5: ( ( 'add' | '+' ) -> ADD_OP | ( 'sub' | '-' ) -> SUB_OP )
            var alt15=2;
            var LA15_0 = this.input.LA(1);

            if ( ((LA15_0>=54 && LA15_0<=55)) ) {
                alt15=1;
            }
            else if ( ((LA15_0>=56 && LA15_0<=57)) ) {
                alt15=2;
            }
            else {
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 15, 0, this.input);

                throw nvae;
            }
            switch (alt15) {
                case 1 :
                    // PomonaQueryJs.g:119:7: ( 'add' | '+' )
                    // PomonaQueryJs.g:119:7: ( 'add' | '+' )
                    var alt13=2;
                    var LA13_0 = this.input.LA(1);

                    if ( (LA13_0==54) ) {
                        alt13=1;
                    }
                    else if ( (LA13_0==55) ) {
                        alt13=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 13, 0, this.input);

                        throw nvae;
                    }
                    switch (alt13) {
                        case 1 :
                            // PomonaQueryJs.g:119:8: 'add'
                            string_literal33=this.match(this.input,54,PomonaQueryJsParser.FOLLOW_54_in_additive_operator731);  
                            stream_54.add(string_literal33);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:119:14: '+'
                            char_literal34=this.match(this.input,55,PomonaQueryJsParser.FOLLOW_55_in_additive_operator733);  
                            stream_55.add(char_literal34);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 119:19: -> ADD_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(ADD_OP, "ADD_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 2 :
                    // PomonaQueryJs.g:120:7: ( 'sub' | '-' )
                    // PomonaQueryJs.g:120:7: ( 'sub' | '-' )
                    var alt14=2;
                    var LA14_0 = this.input.LA(1);

                    if ( (LA14_0==56) ) {
                        alt14=1;
                    }
                    else if ( (LA14_0==57) ) {
                        alt14=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 14, 0, this.input);

                        throw nvae;
                    }
                    switch (alt14) {
                        case 1 :
                            // PomonaQueryJs.g:120:8: 'sub'
                            string_literal35=this.match(this.input,56,PomonaQueryJsParser.FOLLOW_56_in_additive_operator747);  
                            stream_56.add(string_literal35);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:120:14: '-'
                            char_literal36=this.match(this.input,57,PomonaQueryJsParser.FOLLOW_57_in_additive_operator749);  
                            stream_57.add(char_literal36);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 120:19: -> SUB_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(SUB_OP, "SUB_OP"));

                    }

                    retval.tree = root_0;

                    break;

            }
            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    additive_expr_return: (function() {
        PomonaQueryJsParser.additive_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.additive_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:123:1: additive_expr : multiplicative_expr ( additive_operator multiplicative_expr )* ;
    // $ANTLR start "additive_expr"
    additive_expr: function() {
        var retval = new PomonaQueryJsParser.additive_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var multiplicative_expr37 = null;
         var additive_operator38 = null;
         var multiplicative_expr39 = null;


        try {
            // PomonaQueryJs.g:124:2: ( multiplicative_expr ( additive_operator multiplicative_expr )* )
            // PomonaQueryJs.g:124:4: multiplicative_expr ( additive_operator multiplicative_expr )*
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_multiplicative_expr_in_additive_expr769);
            multiplicative_expr37=this.multiplicative_expr();

            this.state._fsp--;

            this.adaptor.addChild(root_0, multiplicative_expr37.getTree());
            // PomonaQueryJs.g:124:24: ( additive_operator multiplicative_expr )*
            loop16:
            do {
                var alt16=2;
                var LA16_0 = this.input.LA(1);

                if ( ((LA16_0>=54 && LA16_0<=57)) ) {
                    alt16=1;
                }


                switch (alt16) {
                case 1 :
                    // PomonaQueryJs.g:124:26: additive_operator multiplicative_expr
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_additive_operator_in_additive_expr773);
                    additive_operator38=this.additive_operator();

                    this.state._fsp--;

                    root_0 = this.adaptor.becomeRoot(additive_operator38.getTree(), root_0);
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_multiplicative_expr_in_additive_expr776);
                    multiplicative_expr39=this.multiplicative_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, multiplicative_expr39.getTree());


                    break;

                default :
                    break loop16;
                }
            } while (true);




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    multiplicative_operator_return: (function() {
        PomonaQueryJsParser.multiplicative_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.multiplicative_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:127:1: multiplicative_operator : ( ( 'mul' | '*' ) -> MUL_OP | ( 'div' | '/' ) -> DIV_OP | ( 'mod' | '%' ) -> MOD_OP );
    // $ANTLR start "multiplicative_operator"
    multiplicative_operator: function() {
        var retval = new PomonaQueryJsParser.multiplicative_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal40 = null;
        var char_literal41 = null;
        var string_literal42 = null;
        var char_literal43 = null;
        var string_literal44 = null;
        var char_literal45 = null;

        var string_literal40_tree=null;
        var char_literal41_tree=null;
        var string_literal42_tree=null;
        var char_literal43_tree=null;
        var string_literal44_tree=null;
        var char_literal45_tree=null;
        var stream_59=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 59");
        var stream_58=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 58");
        var stream_62=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 62");
        var stream_63=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 63");
        var stream_60=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 60");
        var stream_61=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 61");

        try {
            // PomonaQueryJs.g:128:5: ( ( 'mul' | '*' ) -> MUL_OP | ( 'div' | '/' ) -> DIV_OP | ( 'mod' | '%' ) -> MOD_OP )
            var alt20=3;
            switch ( this.input.LA(1) ) {
            case 58:
            case 59:
                alt20=1;
                break;
            case 60:
            case 61:
                alt20=2;
                break;
            case 62:
            case 63:
                alt20=3;
                break;
            default:
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 20, 0, this.input);

                throw nvae;
            }

            switch (alt20) {
                case 1 :
                    // PomonaQueryJs.g:128:9: ( 'mul' | '*' )
                    // PomonaQueryJs.g:128:9: ( 'mul' | '*' )
                    var alt17=2;
                    var LA17_0 = this.input.LA(1);

                    if ( (LA17_0==58) ) {
                        alt17=1;
                    }
                    else if ( (LA17_0==59) ) {
                        alt17=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 17, 0, this.input);

                        throw nvae;
                    }
                    switch (alt17) {
                        case 1 :
                            // PomonaQueryJs.g:128:10: 'mul'
                            string_literal40=this.match(this.input,58,PomonaQueryJsParser.FOLLOW_58_in_multiplicative_operator796);  
                            stream_58.add(string_literal40);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:128:16: '*'
                            char_literal41=this.match(this.input,59,PomonaQueryJsParser.FOLLOW_59_in_multiplicative_operator798);  
                            stream_59.add(char_literal41);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 128:21: -> MUL_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(MUL_OP, "MUL_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 2 :
                    // PomonaQueryJs.g:129:9: ( 'div' | '/' )
                    // PomonaQueryJs.g:129:9: ( 'div' | '/' )
                    var alt18=2;
                    var LA18_0 = this.input.LA(1);

                    if ( (LA18_0==60) ) {
                        alt18=1;
                    }
                    else if ( (LA18_0==61) ) {
                        alt18=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 18, 0, this.input);

                        throw nvae;
                    }
                    switch (alt18) {
                        case 1 :
                            // PomonaQueryJs.g:129:10: 'div'
                            string_literal42=this.match(this.input,60,PomonaQueryJsParser.FOLLOW_60_in_multiplicative_operator814);  
                            stream_60.add(string_literal42);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:129:16: '/'
                            char_literal43=this.match(this.input,61,PomonaQueryJsParser.FOLLOW_61_in_multiplicative_operator816);  
                            stream_61.add(char_literal43);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 129:21: -> DIV_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(DIV_OP, "DIV_OP"));

                    }

                    retval.tree = root_0;

                    break;
                case 3 :
                    // PomonaQueryJs.g:130:6: ( 'mod' | '%' )
                    // PomonaQueryJs.g:130:6: ( 'mod' | '%' )
                    var alt19=2;
                    var LA19_0 = this.input.LA(1);

                    if ( (LA19_0==62) ) {
                        alt19=1;
                    }
                    else if ( (LA19_0==63) ) {
                        alt19=2;
                    }
                    else {
                        var nvae =
                            new org.antlr.runtime.NoViableAltException("", 19, 0, this.input);

                        throw nvae;
                    }
                    switch (alt19) {
                        case 1 :
                            // PomonaQueryJs.g:130:7: 'mod'
                            string_literal44=this.match(this.input,62,PomonaQueryJsParser.FOLLOW_62_in_multiplicative_operator829);  
                            stream_62.add(string_literal44);



                            break;
                        case 2 :
                            // PomonaQueryJs.g:130:13: '%'
                            char_literal45=this.match(this.input,63,PomonaQueryJsParser.FOLLOW_63_in_multiplicative_operator831);  
                            stream_63.add(char_literal45);



                            break;

                    }



                    // AST REWRITE
                    // elements: 
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 130:18: -> MOD_OP
                    {
                        this.adaptor.addChild(root_0, this.adaptor.create(MOD_OP, "MOD_OP"));

                    }

                    retval.tree = root_0;

                    break;

            }
            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    multiplicative_expr_return: (function() {
        PomonaQueryJsParser.multiplicative_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.multiplicative_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:133:1: multiplicative_expr : unary_expr ( multiplicative_operator unary_expr )* ;
    // $ANTLR start "multiplicative_expr"
    multiplicative_expr: function() {
        var retval = new PomonaQueryJsParser.multiplicative_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var unary_expr46 = null;
         var multiplicative_operator47 = null;
         var unary_expr48 = null;


        try {
            // PomonaQueryJs.g:134:2: ( unary_expr ( multiplicative_operator unary_expr )* )
            // PomonaQueryJs.g:134:4: unary_expr ( multiplicative_operator unary_expr )*
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_unary_expr_in_multiplicative_expr851);
            unary_expr46=this.unary_expr();

            this.state._fsp--;

            this.adaptor.addChild(root_0, unary_expr46.getTree());
            // PomonaQueryJs.g:134:15: ( multiplicative_operator unary_expr )*
            loop21:
            do {
                var alt21=2;
                var LA21_0 = this.input.LA(1);

                if ( ((LA21_0>=58 && LA21_0<=63)) ) {
                    alt21=1;
                }


                switch (alt21) {
                case 1 :
                    // PomonaQueryJs.g:134:17: multiplicative_operator unary_expr
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_multiplicative_operator_in_multiplicative_expr855);
                    multiplicative_operator47=this.multiplicative_operator();

                    this.state._fsp--;

                    root_0 = this.adaptor.becomeRoot(multiplicative_operator47.getTree(), root_0);
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_unary_expr_in_multiplicative_expr858);
                    unary_expr48=this.unary_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, unary_expr48.getTree());


                    break;

                default :
                    break loop21;
                }
            } while (true);




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    unary_operator_return: (function() {
        PomonaQueryJsParser.unary_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.unary_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:137:1: unary_operator : ( 'not' | '!' ) -> NOT_OP ;
    // $ANTLR start "unary_operator"
    unary_operator: function() {
        var retval = new PomonaQueryJsParser.unary_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var string_literal49 = null;
        var char_literal50 = null;

        var string_literal49_tree=null;
        var char_literal50_tree=null;
        var stream_64=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 64");
        var stream_65=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 65");

        try {
            // PomonaQueryJs.g:138:2: ( ( 'not' | '!' ) -> NOT_OP )
            // PomonaQueryJs.g:138:4: ( 'not' | '!' )
            // PomonaQueryJs.g:138:4: ( 'not' | '!' )
            var alt22=2;
            var LA22_0 = this.input.LA(1);

            if ( (LA22_0==64) ) {
                alt22=1;
            }
            else if ( (LA22_0==65) ) {
                alt22=2;
            }
            else {
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 22, 0, this.input);

                throw nvae;
            }
            switch (alt22) {
                case 1 :
                    // PomonaQueryJs.g:138:5: 'not'
                    string_literal49=this.match(this.input,64,PomonaQueryJsParser.FOLLOW_64_in_unary_operator874);  
                    stream_64.add(string_literal49);



                    break;
                case 2 :
                    // PomonaQueryJs.g:138:11: '!'
                    char_literal50=this.match(this.input,65,PomonaQueryJsParser.FOLLOW_65_in_unary_operator876);  
                    stream_65.add(char_literal50);



                    break;

            }



            // AST REWRITE
            // elements: 
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 138:16: -> NOT_OP
            {
                this.adaptor.addChild(root_0, this.adaptor.create(NOT_OP, "NOT_OP"));

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    dot_operator_return: (function() {
        PomonaQueryJsParser.dot_operator_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.dot_operator_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:141:1: dot_operator : '.' -> DOT_OP ;
    // $ANTLR start "dot_operator"
    dot_operator: function() {
        var retval = new PomonaQueryJsParser.dot_operator_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var char_literal51 = null;

        var char_literal51_tree=null;
        var stream_66=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 66");

        try {
            // PomonaQueryJs.g:142:2: ( '.' -> DOT_OP )
            // PomonaQueryJs.g:142:4: '.'
            char_literal51=this.match(this.input,66,PomonaQueryJsParser.FOLLOW_66_in_dot_operator892);  
            stream_66.add(char_literal51);



            // AST REWRITE
            // elements: 
            // token labels: 
            // rule labels: retval
            // token list labels: 
            // rule list labels: 
            retval.tree = root_0;
            var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

            root_0 = this.adaptor.nil();
            // 142:8: -> DOT_OP
            {
                this.adaptor.addChild(root_0, this.adaptor.create(DOT_OP, "DOT_OP"));

            }

            retval.tree = root_0;


            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    unary_expr_return: (function() {
        PomonaQueryJsParser.unary_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.unary_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:146:1: unary_expr : ( unary_operator unary_expr | primary_expr );
    // $ANTLR start "unary_expr"
    unary_expr: function() {
        var retval = new PomonaQueryJsParser.unary_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var unary_operator52 = null;
         var unary_expr53 = null;
         var primary_expr54 = null;


        try {
            // PomonaQueryJs.g:147:2: ( unary_operator unary_expr | primary_expr )
            var alt23=2;
            var LA23_0 = this.input.LA(1);

            if ( ((LA23_0>=64 && LA23_0<=65)) ) {
                alt23=1;
            }
            else if ( ((LA23_0>=PREFIXED_STRING && LA23_0<=INT)||LA23_0==STRING||LA23_0==67||LA23_0==69) ) {
                alt23=2;
            }
            else {
                var nvae =
                    new org.antlr.runtime.NoViableAltException("", 23, 0, this.input);

                throw nvae;
            }
            switch (alt23) {
                case 1 :
                    // PomonaQueryJs.g:147:4: unary_operator unary_expr
                    root_0 = this.adaptor.nil();

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_unary_operator_in_unary_expr909);
                    unary_operator52=this.unary_operator();

                    this.state._fsp--;

                    root_0 = this.adaptor.becomeRoot(unary_operator52.getTree(), root_0);
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_unary_expr_in_unary_expr912);
                    unary_expr53=this.unary_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, unary_expr53.getTree());


                    break;
                case 2 :
                    // PomonaQueryJs.g:148:4: primary_expr
                    root_0 = this.adaptor.nil();

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_primary_expr_in_unary_expr917);
                    primary_expr54=this.primary_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, primary_expr54.getTree());


                    break;

            }
            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    primary_expr_return: (function() {
        PomonaQueryJsParser.primary_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.primary_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:151:1: primary_expr : postfix_expr ( dot_operator postfix_expr )* ;
    // $ANTLR start "primary_expr"
    primary_expr: function() {
        var retval = new PomonaQueryJsParser.primary_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

         var postfix_expr55 = null;
         var dot_operator56 = null;
         var postfix_expr57 = null;


        try {
            // PomonaQueryJs.g:152:2: ( postfix_expr ( dot_operator postfix_expr )* )
            // PomonaQueryJs.g:152:4: postfix_expr ( dot_operator postfix_expr )*
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_postfix_expr_in_primary_expr928);
            postfix_expr55=this.postfix_expr();

            this.state._fsp--;

            this.adaptor.addChild(root_0, postfix_expr55.getTree());
            // PomonaQueryJs.g:152:17: ( dot_operator postfix_expr )*
            loop24:
            do {
                var alt24=2;
                var LA24_0 = this.input.LA(1);

                if ( (LA24_0==66) ) {
                    alt24=1;
                }


                switch (alt24) {
                case 1 :
                    // PomonaQueryJs.g:152:19: dot_operator postfix_expr
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_dot_operator_in_primary_expr932);
                    dot_operator56=this.dot_operator();

                    this.state._fsp--;

                    root_0 = this.adaptor.becomeRoot(dot_operator56.getTree(), root_0);
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_postfix_expr_in_primary_expr935);
                    postfix_expr57=this.postfix_expr();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, postfix_expr57.getTree());


                    break;

                default :
                    break loop24;
                }
            } while (true);




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    postfix_expr_return: (function() {
        PomonaQueryJsParser.postfix_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.postfix_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:155:1: postfix_expr : ( ID ( '(' arglist_expr ')' ) -> ^( METHOD_CALL ID arglist_expr ) | ID ( '(' ')' ) -> ^( METHOD_CALL ID ) | ID ( '[' arglist_expr ']' ) -> ^( INDEXER_ACCESS ID arglist_expr ) | ID ( '[' ']' ) -> ^( INDEXER_ACCESS ID ) | ID | STRING | INT | '(' exp ')' | '[' arglist_expr ']' -> ^( ARRAY_LITERAL arglist_expr ) | PREFIXED_STRING );
    // $ANTLR start "postfix_expr"
    postfix_expr: function() {
        var retval = new PomonaQueryJsParser.postfix_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var ID58 = null;
        var char_literal59 = null;
        var char_literal61 = null;
        var ID62 = null;
        var char_literal63 = null;
        var char_literal64 = null;
        var ID65 = null;
        var char_literal66 = null;
        var char_literal68 = null;
        var ID69 = null;
        var char_literal70 = null;
        var char_literal71 = null;
        var ID72 = null;
        var STRING73 = null;
        var INT74 = null;
        var char_literal75 = null;
        var char_literal77 = null;
        var char_literal78 = null;
        var char_literal80 = null;
        var PREFIXED_STRING81 = null;
         var arglist_expr60 = null;
         var arglist_expr67 = null;
         var exp76 = null;
         var arglist_expr79 = null;

        var ID58_tree=null;
        var char_literal59_tree=null;
        var char_literal61_tree=null;
        var ID62_tree=null;
        var char_literal63_tree=null;
        var char_literal64_tree=null;
        var ID65_tree=null;
        var char_literal66_tree=null;
        var char_literal68_tree=null;
        var ID69_tree=null;
        var char_literal70_tree=null;
        var char_literal71_tree=null;
        var ID72_tree=null;
        var STRING73_tree=null;
        var INT74_tree=null;
        var char_literal75_tree=null;
        var char_literal77_tree=null;
        var char_literal78_tree=null;
        var char_literal80_tree=null;
        var PREFIXED_STRING81_tree=null;
        var stream_67=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 67");
        var stream_69=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 69");
        var stream_68=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 68");
        var stream_ID=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token ID");
        var stream_70=new org.antlr.runtime.tree.RewriteRuleTokenStream(this.adaptor,"token 70");
        var stream_arglist_expr=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"rule arglist_expr");
        try {
            // PomonaQueryJs.g:156:2: ( ID ( '(' arglist_expr ')' ) -> ^( METHOD_CALL ID arglist_expr ) | ID ( '(' ')' ) -> ^( METHOD_CALL ID ) | ID ( '[' arglist_expr ']' ) -> ^( INDEXER_ACCESS ID arglist_expr ) | ID ( '[' ']' ) -> ^( INDEXER_ACCESS ID ) | ID | STRING | INT | '(' exp ')' | '[' arglist_expr ']' -> ^( ARRAY_LITERAL arglist_expr ) | PREFIXED_STRING )
            var alt25=10;
            alt25 = this.dfa25.predict(this.input);
            switch (alt25) {
                case 1 :
                    // PomonaQueryJs.g:156:4: ID ( '(' arglist_expr ')' )
                    ID58=this.match(this.input,ID,PomonaQueryJsParser.FOLLOW_ID_in_postfix_expr949);  
                    stream_ID.add(ID58);

                    // PomonaQueryJs.g:156:7: ( '(' arglist_expr ')' )
                    // PomonaQueryJs.g:156:9: '(' arglist_expr ')'
                    char_literal59=this.match(this.input,67,PomonaQueryJsParser.FOLLOW_67_in_postfix_expr953);  
                    stream_67.add(char_literal59);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_arglist_expr_in_postfix_expr955);
                    arglist_expr60=this.arglist_expr();

                    this.state._fsp--;

                    stream_arglist_expr.add(arglist_expr60.getTree());
                    char_literal61=this.match(this.input,68,PomonaQueryJsParser.FOLLOW_68_in_postfix_expr957);  
                    stream_68.add(char_literal61);






                    // AST REWRITE
                    // elements: ID, arglist_expr
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 156:32: -> ^( METHOD_CALL ID arglist_expr )
                    {
                        // PomonaQueryJs.g:156:35: ^( METHOD_CALL ID arglist_expr )
                        {
                        var root_1 = this.adaptor.nil();
                        root_1 = this.adaptor.becomeRoot(this.adaptor.create(METHOD_CALL, "METHOD_CALL"), root_1);

                        this.adaptor.addChild(root_1, stream_ID.nextNode());
                        this.adaptor.addChild(root_1, stream_arglist_expr.nextTree());

                        this.adaptor.addChild(root_0, root_1);
                        }

                    }

                    retval.tree = root_0;

                    break;
                case 2 :
                    // PomonaQueryJs.g:157:4: ID ( '(' ')' )
                    ID62=this.match(this.input,ID,PomonaQueryJsParser.FOLLOW_ID_in_postfix_expr974);  
                    stream_ID.add(ID62);

                    // PomonaQueryJs.g:157:7: ( '(' ')' )
                    // PomonaQueryJs.g:157:9: '(' ')'
                    char_literal63=this.match(this.input,67,PomonaQueryJsParser.FOLLOW_67_in_postfix_expr978);  
                    stream_67.add(char_literal63);

                    char_literal64=this.match(this.input,68,PomonaQueryJsParser.FOLLOW_68_in_postfix_expr980);  
                    stream_68.add(char_literal64);






                    // AST REWRITE
                    // elements: ID
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 157:19: -> ^( METHOD_CALL ID )
                    {
                        // PomonaQueryJs.g:157:22: ^( METHOD_CALL ID )
                        {
                        var root_1 = this.adaptor.nil();
                        root_1 = this.adaptor.becomeRoot(this.adaptor.create(METHOD_CALL, "METHOD_CALL"), root_1);

                        this.adaptor.addChild(root_1, stream_ID.nextNode());

                        this.adaptor.addChild(root_0, root_1);
                        }

                    }

                    retval.tree = root_0;

                    break;
                case 3 :
                    // PomonaQueryJs.g:158:4: ID ( '[' arglist_expr ']' )
                    ID65=this.match(this.input,ID,PomonaQueryJsParser.FOLLOW_ID_in_postfix_expr995);  
                    stream_ID.add(ID65);

                    // PomonaQueryJs.g:158:7: ( '[' arglist_expr ']' )
                    // PomonaQueryJs.g:158:9: '[' arglist_expr ']'
                    char_literal66=this.match(this.input,69,PomonaQueryJsParser.FOLLOW_69_in_postfix_expr999);  
                    stream_69.add(char_literal66);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_arglist_expr_in_postfix_expr1001);
                    arglist_expr67=this.arglist_expr();

                    this.state._fsp--;

                    stream_arglist_expr.add(arglist_expr67.getTree());
                    char_literal68=this.match(this.input,70,PomonaQueryJsParser.FOLLOW_70_in_postfix_expr1003);  
                    stream_70.add(char_literal68);






                    // AST REWRITE
                    // elements: arglist_expr, ID
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 158:32: -> ^( INDEXER_ACCESS ID arglist_expr )
                    {
                        // PomonaQueryJs.g:158:35: ^( INDEXER_ACCESS ID arglist_expr )
                        {
                        var root_1 = this.adaptor.nil();
                        root_1 = this.adaptor.becomeRoot(this.adaptor.create(INDEXER_ACCESS, "INDEXER_ACCESS"), root_1);

                        this.adaptor.addChild(root_1, stream_ID.nextNode());
                        this.adaptor.addChild(root_1, stream_arglist_expr.nextTree());

                        this.adaptor.addChild(root_0, root_1);
                        }

                    }

                    retval.tree = root_0;

                    break;
                case 4 :
                    // PomonaQueryJs.g:159:4: ID ( '[' ']' )
                    ID69=this.match(this.input,ID,PomonaQueryJsParser.FOLLOW_ID_in_postfix_expr1020);  
                    stream_ID.add(ID69);

                    // PomonaQueryJs.g:159:7: ( '[' ']' )
                    // PomonaQueryJs.g:159:9: '[' ']'
                    char_literal70=this.match(this.input,69,PomonaQueryJsParser.FOLLOW_69_in_postfix_expr1024);  
                    stream_69.add(char_literal70);

                    char_literal71=this.match(this.input,70,PomonaQueryJsParser.FOLLOW_70_in_postfix_expr1026);  
                    stream_70.add(char_literal71);






                    // AST REWRITE
                    // elements: ID
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 159:19: -> ^( INDEXER_ACCESS ID )
                    {
                        // PomonaQueryJs.g:159:22: ^( INDEXER_ACCESS ID )
                        {
                        var root_1 = this.adaptor.nil();
                        root_1 = this.adaptor.becomeRoot(this.adaptor.create(INDEXER_ACCESS, "INDEXER_ACCESS"), root_1);

                        this.adaptor.addChild(root_1, stream_ID.nextNode());

                        this.adaptor.addChild(root_0, root_1);
                        }

                    }

                    retval.tree = root_0;

                    break;
                case 5 :
                    // PomonaQueryJs.g:160:4: ID
                    root_0 = this.adaptor.nil();

                    ID72=this.match(this.input,ID,PomonaQueryJsParser.FOLLOW_ID_in_postfix_expr1041); 
                    ID72_tree = this.adaptor.create(ID72);
                    this.adaptor.addChild(root_0, ID72_tree);



                    break;
                case 6 :
                    // PomonaQueryJs.g:161:4: STRING
                    root_0 = this.adaptor.nil();

                    STRING73=this.match(this.input,STRING,PomonaQueryJsParser.FOLLOW_STRING_in_postfix_expr1046); 
                    STRING73_tree = this.adaptor.create(STRING73);
                    this.adaptor.addChild(root_0, STRING73_tree);



                    break;
                case 7 :
                    // PomonaQueryJs.g:162:4: INT
                    root_0 = this.adaptor.nil();

                    INT74=this.match(this.input,INT,PomonaQueryJsParser.FOLLOW_INT_in_postfix_expr1051); 
                    INT74_tree = this.adaptor.create(INT74);
                    this.adaptor.addChild(root_0, INT74_tree);



                    break;
                case 8 :
                    // PomonaQueryJs.g:163:4: '(' exp ')'
                    root_0 = this.adaptor.nil();

                    char_literal75=this.match(this.input,67,PomonaQueryJsParser.FOLLOW_67_in_postfix_expr1056); 
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_exp_in_postfix_expr1059);
                    exp76=this.exp();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, exp76.getTree());
                    char_literal77=this.match(this.input,68,PomonaQueryJsParser.FOLLOW_68_in_postfix_expr1061); 


                    break;
                case 9 :
                    // PomonaQueryJs.g:164:4: '[' arglist_expr ']'
                    char_literal78=this.match(this.input,69,PomonaQueryJsParser.FOLLOW_69_in_postfix_expr1067);  
                    stream_69.add(char_literal78);

                    this.pushFollow(PomonaQueryJsParser.FOLLOW_arglist_expr_in_postfix_expr1069);
                    arglist_expr79=this.arglist_expr();

                    this.state._fsp--;

                    stream_arglist_expr.add(arglist_expr79.getTree());
                    char_literal80=this.match(this.input,70,PomonaQueryJsParser.FOLLOW_70_in_postfix_expr1071);  
                    stream_70.add(char_literal80);



                    // AST REWRITE
                    // elements: arglist_expr
                    // token labels: 
                    // rule labels: retval
                    // token list labels: 
                    // rule list labels: 
                    retval.tree = root_0;
                    var stream_retval=new org.antlr.runtime.tree.RewriteRuleSubtreeStream(this.adaptor,"token retval",retval!=null?retval.tree:null);

                    root_0 = this.adaptor.nil();
                    // 164:25: -> ^( ARRAY_LITERAL arglist_expr )
                    {
                        // PomonaQueryJs.g:164:28: ^( ARRAY_LITERAL arglist_expr )
                        {
                        var root_1 = this.adaptor.nil();
                        root_1 = this.adaptor.becomeRoot(this.adaptor.create(ARRAY_LITERAL, "ARRAY_LITERAL"), root_1);

                        this.adaptor.addChild(root_1, stream_arglist_expr.nextTree());

                        this.adaptor.addChild(root_0, root_1);
                        }

                    }

                    retval.tree = root_0;

                    break;
                case 10 :
                    // PomonaQueryJs.g:165:4: PREFIXED_STRING
                    root_0 = this.adaptor.nil();

                    PREFIXED_STRING81=this.match(this.input,PREFIXED_STRING,PomonaQueryJsParser.FOLLOW_PREFIXED_STRING_in_postfix_expr1084); 
                    PREFIXED_STRING81_tree = this.adaptor.create(PREFIXED_STRING81);
                    this.adaptor.addChild(root_0, PREFIXED_STRING81_tree);



                    break;

            }
            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    },

    // inline static return class
    arglist_expr_return: (function() {
        PomonaQueryJsParser.arglist_expr_return = function(){};
        org.antlr.lang.extend(PomonaQueryJsParser.arglist_expr_return,
                          org.antlr.runtime.ParserRuleReturnScope,
        {
            getTree: function() { return this.tree; }
        });
        return;
    })(),

    // PomonaQueryJs.g:168:1: arglist_expr : exp ( ',' exp )* ;
    // $ANTLR start "arglist_expr"
    arglist_expr: function() {
        var retval = new PomonaQueryJsParser.arglist_expr_return();
        retval.start = this.input.LT(1);

        var root_0 = null;

        var char_literal83 = null;
         var exp82 = null;
         var exp84 = null;

        var char_literal83_tree=null;

        try {
            // PomonaQueryJs.g:169:2: ( exp ( ',' exp )* )
            // PomonaQueryJs.g:169:4: exp ( ',' exp )*
            root_0 = this.adaptor.nil();

            this.pushFollow(PomonaQueryJsParser.FOLLOW_exp_in_arglist_expr1096);
            exp82=this.exp();

            this.state._fsp--;

            this.adaptor.addChild(root_0, exp82.getTree());
            // PomonaQueryJs.g:169:8: ( ',' exp )*
            loop26:
            do {
                var alt26=2;
                var LA26_0 = this.input.LA(1);

                if ( (LA26_0==71) ) {
                    alt26=1;
                }


                switch (alt26) {
                case 1 :
                    // PomonaQueryJs.g:169:10: ',' exp
                    char_literal83=this.match(this.input,71,PomonaQueryJsParser.FOLLOW_71_in_arglist_expr1100); 
                    this.pushFollow(PomonaQueryJsParser.FOLLOW_exp_in_arglist_expr1103);
                    exp84=this.exp();

                    this.state._fsp--;

                    this.adaptor.addChild(root_0, exp84.getTree());


                    break;

                default :
                    break loop26;
                }
            } while (true);




            retval.stop = this.input.LT(-1);

            retval.tree = this.adaptor.rulePostProcessing(root_0);
            this.adaptor.setTokenBoundaries(retval.tree, retval.start, retval.stop);

        }
        catch (re) {
            if (re instanceof org.antlr.runtime.RecognitionException) {
                this.reportError(re);
                this.recover(this.input,re);
                retval.tree = this.adaptor.errorNode(this.input, retval.start, this.input.LT(-1), re);
            } else {
                throw re;
            }
        }
        finally {
        }
        return retval;
    }

    // Delegated rules




}, true); // important to pass true to overwrite default implementations

org.antlr.lang.augmentObject(PomonaQueryJsParser, {
    DFA25_eotS:
        "\u000e\uffff",
    DFA25_eofS:
        "\u0001\uffff\u0001\u0009\u000c\uffff",
    DFA25_minS:
        "\u0001\u001c\u0001\u0025\u0005\uffff\u0002\u001c\u0005\uffff",
    DFA25_maxS:
        "\u0001\u0045\u0001\u0047\u0005\uffff\u0001\u0045\u0001\u0046\u0005"+
    "\uffff",
    DFA25_acceptS:
        "\u0002\uffff\u0001\u0006\u0001\u0007\u0001\u0008\u0001\u0009\u0001"+
    "\u000a\u0002\uffff\u0001\u0005\u0001\u0002\u0001\u0001\u0001\u0004\u0001"+
    "\u0003",
    DFA25_specialS:
        "\u000e\uffff}>",
    DFA25_transitionS: [
            "\u0001\u0006\u0001\u0001\u0001\u0003\u0001\uffff\u0001\u0002"+
            "\u0022\uffff\u0001\u0004\u0001\uffff\u0001\u0005",
            "\u001b\u0009\u0002\uffff\u0001\u0009\u0001\u0007\u0001\u0009"+
            "\u0001\u0008\u0002\u0009",
            "",
            "",
            "",
            "",
            "",
            "\u0003\u000b\u0001\uffff\u0001\u000b\u001f\uffff\u0002\u000b"+
            "\u0001\uffff\u0001\u000b\u0001\u000a\u0001\u000b",
            "\u0003\u000d\u0001\uffff\u0001\u000d\u001f\uffff\u0002\u000d"+
            "\u0001\uffff\u0001\u000d\u0001\uffff\u0001\u000d\u0001\u000c",
            "",
            "",
            "",
            "",
            ""
    ]
});

org.antlr.lang.augmentObject(PomonaQueryJsParser, {
    DFA25_eot:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsParser.DFA25_eotS),
    DFA25_eof:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsParser.DFA25_eofS),
    DFA25_min:
        org.antlr.runtime.DFA.unpackEncodedStringToUnsignedChars(PomonaQueryJsParser.DFA25_minS),
    DFA25_max:
        org.antlr.runtime.DFA.unpackEncodedStringToUnsignedChars(PomonaQueryJsParser.DFA25_maxS),
    DFA25_accept:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsParser.DFA25_acceptS),
    DFA25_special:
        org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsParser.DFA25_specialS),
    DFA25_transition: (function() {
        var a = [],
            i,
            numStates = PomonaQueryJsParser.DFA25_transitionS.length;
        for (i=0; i<numStates; i++) {
            a.push(org.antlr.runtime.DFA.unpackEncodedString(PomonaQueryJsParser.DFA25_transitionS[i]));
        }
        return a;
    })()
});

PomonaQueryJsParser.DFA25 = function(recognizer) {
    this.recognizer = recognizer;
    this.decisionNumber = 25;
    this.eot = PomonaQueryJsParser.DFA25_eot;
    this.eof = PomonaQueryJsParser.DFA25_eof;
    this.min = PomonaQueryJsParser.DFA25_min;
    this.max = PomonaQueryJsParser.DFA25_max;
    this.accept = PomonaQueryJsParser.DFA25_accept;
    this.special = PomonaQueryJsParser.DFA25_special;
    this.transition = PomonaQueryJsParser.DFA25_transition;
};

org.antlr.lang.extend(PomonaQueryJsParser.DFA25, org.antlr.runtime.DFA, {
    getDescription: function() {
        return "155:1: postfix_expr : ( ID ( '(' arglist_expr ')' ) -> ^( METHOD_CALL ID arglist_expr ) | ID ( '(' ')' ) -> ^( METHOD_CALL ID ) | ID ( '[' arglist_expr ']' ) -> ^( INDEXER_ACCESS ID arglist_expr ) | ID ( '[' ']' ) -> ^( INDEXER_ACCESS ID ) | ID | STRING | INT | '(' exp ')' | '[' arglist_expr ']' -> ^( ARRAY_LITERAL arglist_expr ) | PREFIXED_STRING );";
    },
    dummy: null
});
 

// public class variables
org.antlr.lang.augmentObject(PomonaQueryJsParser, {
    tokenNames: ["<invalid>", "<EOR>", "<DOWN>", "<UP>", "ROOT", "AND_OP", "OR_OP", "LT_OP", "GT_OP", "LE_OP", "GE_OP", "EQ_OP", "MUL_OP", "DIV_OP", "NE_OP", "ADD_OP", "SUB_OP", "MOD_OP", "DOT_OP", "AS_OP", "IN_OP", "NOT_OP", "DATETIME_LITERAL", "GUID_LITERAL", "METHOD_CALL", "INDEXER_ACCESS", "LAMBDA_OP", "ARRAY_LITERAL", "PREFIXED_STRING", "ID", "INT", "WS", "STRING", "HEX_DIGIT", "UNICODE_ESC", "OCTAL_ESC", "ESC_SEQ", "'as'", "':'", "'or'", "'and'", "'gt'", "'>'", "'lt'", "'<'", "'eq'", "'=='", "'ge'", "'>='", "'le'", "'<='", "'ne'", "'!='", "'in'", "'add'", "'+'", "'sub'", "'-'", "'mul'", "'*'", "'div'", "'/'", "'mod'", "'%'", "'not'", "'!'", "'.'", "'('", "')'", "'['", "']'", "','"],
    FOLLOW_exp_in_parse450: new org.antlr.runtime.BitSet([0x00000000, 0x00000000]),
    FOLLOW_EOF_in_parse452: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_as_expression_in_exp471: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_lambda_expression_in_as_expression482: new org.antlr.runtime.BitSet([0x00000002, 0x00000020]),
    FOLLOW_37_in_as_expression486: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_lambda_expression_in_as_expression488: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_or_expression_in_lambda_expression511: new org.antlr.runtime.BitSet([0x00000002, 0x00000040]),
    FOLLOW_38_in_lambda_expression515: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_or_expression_in_lambda_expression517: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_and_expression_in_or_expression540: new org.antlr.runtime.BitSet([0x00000002, 0x00000080]),
    FOLLOW_39_in_or_expression544: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_and_expression_in_or_expression546: new org.antlr.runtime.BitSet([0x00000002, 0x00000080]),
    FOLLOW_40_in_and_operator569: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_relational_expr_in_and_expression581: new org.antlr.runtime.BitSet([0x00000002, 0x00000100]),
    FOLLOW_and_operator_in_and_expression585: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_relational_expr_in_and_expression587: new org.antlr.runtime.BitSet([0x00000002, 0x00000100]),
    FOLLOW_41_in_relational_operator614: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_42_in_relational_operator616: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_43_in_relational_operator627: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_44_in_relational_operator629: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_45_in_relational_operator640: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_46_in_relational_operator642: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_47_in_relational_operator653: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_48_in_relational_operator655: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_49_in_relational_operator666: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_50_in_relational_operator668: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_51_in_relational_operator679: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_52_in_relational_operator681: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_53_in_relational_operator691: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_additive_expr_in_relational_expr708: new org.antlr.runtime.BitSet([0x00000002, 0x003FFE00]),
    FOLLOW_relational_operator_in_relational_expr711: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_additive_expr_in_relational_expr714: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_54_in_additive_operator731: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_55_in_additive_operator733: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_56_in_additive_operator747: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_57_in_additive_operator749: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_multiplicative_expr_in_additive_expr769: new org.antlr.runtime.BitSet([0x00000002, 0x03C00000]),
    FOLLOW_additive_operator_in_additive_expr773: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_multiplicative_expr_in_additive_expr776: new org.antlr.runtime.BitSet([0x00000002, 0x03C00000]),
    FOLLOW_58_in_multiplicative_operator796: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_59_in_multiplicative_operator798: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_60_in_multiplicative_operator814: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_61_in_multiplicative_operator816: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_62_in_multiplicative_operator829: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_63_in_multiplicative_operator831: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_unary_expr_in_multiplicative_expr851: new org.antlr.runtime.BitSet([0x00000002, 0xFC000000]),
    FOLLOW_multiplicative_operator_in_multiplicative_expr855: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_unary_expr_in_multiplicative_expr858: new org.antlr.runtime.BitSet([0x00000002, 0xFC000000]),
    FOLLOW_64_in_unary_operator874: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_65_in_unary_operator876: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_66_in_dot_operator892: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_unary_operator_in_unary_expr909: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_unary_expr_in_unary_expr912: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_primary_expr_in_unary_expr917: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_postfix_expr_in_primary_expr928: new org.antlr.runtime.BitSet([0x00000002, 0x00000000,0x00000004, 0x00000000]),
    FOLLOW_dot_operator_in_primary_expr932: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_postfix_expr_in_primary_expr935: new org.antlr.runtime.BitSet([0x00000002, 0x00000000,0x00000004, 0x00000000]),
    FOLLOW_ID_in_postfix_expr949: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000008, 0x00000000]),
    FOLLOW_67_in_postfix_expr953: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_arglist_expr_in_postfix_expr955: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000010, 0x00000000]),
    FOLLOW_68_in_postfix_expr957: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_ID_in_postfix_expr974: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000008, 0x00000000]),
    FOLLOW_67_in_postfix_expr978: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000010, 0x00000000]),
    FOLLOW_68_in_postfix_expr980: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_ID_in_postfix_expr995: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000020, 0x00000000]),
    FOLLOW_69_in_postfix_expr999: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_arglist_expr_in_postfix_expr1001: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000040, 0x00000000]),
    FOLLOW_70_in_postfix_expr1003: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_ID_in_postfix_expr1020: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000020, 0x00000000]),
    FOLLOW_69_in_postfix_expr1024: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000040, 0x00000000]),
    FOLLOW_70_in_postfix_expr1026: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_ID_in_postfix_expr1041: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_STRING_in_postfix_expr1046: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_INT_in_postfix_expr1051: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_67_in_postfix_expr1056: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_exp_in_postfix_expr1059: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000010, 0x00000000]),
    FOLLOW_68_in_postfix_expr1061: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_69_in_postfix_expr1067: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_arglist_expr_in_postfix_expr1069: new org.antlr.runtime.BitSet([0x00000000, 0x00000000,0x00000040, 0x00000000]),
    FOLLOW_70_in_postfix_expr1071: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_PREFIXED_STRING_in_postfix_expr1084: new org.antlr.runtime.BitSet([0x00000002, 0x00000000]),
    FOLLOW_exp_in_arglist_expr1096: new org.antlr.runtime.BitSet([0x00000002, 0x00000000,0x00000080, 0x00000000]),
    FOLLOW_71_in_arglist_expr1100: new org.antlr.runtime.BitSet([0x70000000, 0x00000001,0x0000002B, 0x00000000]),
    FOLLOW_exp_in_arglist_expr1103: new org.antlr.runtime.BitSet([0x00000002, 0x00000000,0x00000080, 0x00000000])
});

})();