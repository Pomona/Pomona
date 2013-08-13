/*App = Ember.Application.create();

App.Router.map(function() {
  // put your routes here
});

App.IndexRoute = Ember.Route.extend({
  model: function() {
    return ['red', 'yellow', 'blue'];
  }
});
*/
App = Ember.Application.create();

App.Router.map(function() {
  var types = PomonaSchemas.types;
    
  for (var i = 0; i < types.length; i++)
  {
    var t = types[i];
    var f =function (type) {
    if (type.uri === undefined || type["extends"] !== undefined)
      return;
    console.log('Registering route ' + type.name + ' at ' + type.uri);
    this.route(type.name.toLowerCase(), { path: '/' + type.uri });
    
    App[type.name + 'Route'] = Ember.Route.extend({
      model: function() {
          return $.getJSON('/' + type.uri + '.json');
        },
        renderTemplate: function() {
          this.render('special');
        }
    });
    };
    f.call(this, t);
  }
});

App.Router.reopen({location: 'history'});

App.Person = Ember.Object.extend({
  firstName: null,
  lastName: null,

  fullName: function() {
    return this.get('firstName') +
           " " + this.get('lastName');
  }.property('firstName', 'lastName')
});

App.Special = Ember.Object.extend({
  duty: 'heavy'
});

/*
App.CritterRoute = Ember.Route.extend({
  model: function() {
    return $.getJSON("/critters", function(data)
      {
        return data;
      });
    return App.Special.create();
  },
  renderTemplate: function() {
    this.render('special');
  }
});
*/
App.IndexRoute = Ember.Route.extend({
  model: function(params) {
    var people = [
      App.Person.create({
        firstName: params.id,
        lastName: "Dale"
      }),
      App.Person.create({
        firstName: "Yehuda",
        lastName: "Katz"
      })
    ];
    return people;
  }
});
