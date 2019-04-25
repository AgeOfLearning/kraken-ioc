# Kraken Inversion of Control
[travis-ci-status]: https://img.shields.io/travis/AgeOfLearning/kraken-ioc.svg
[nuget-status]: https://img.shields.io/nuget/vpre/KrakenIoc.svg
[nuget-status-editor]: https://img.shields.io/nuget/vpre/KrakenIoc.Editor.svg
[nuget-status-unity]: https://img.shields.io/nuget/vpre/KrakenIoc.Unity.svg

[![travis][travis-ci-status]](https://travis-ci.org/AgeOfLearning/kraken-ioc)

**KrakenIoc** 

[![nuget][nuget-status]](https://www.nuget.org/packages/KrakenIoc/)

**KrakenIoc.Editor** 

[![nuget][nuget-status-editor]](https://www.nuget.org/packages/KrakenIoc.Editor/)

**KrakenIoc.Unity** 

[![nuget][nuget-status-unity]](https://www.nuget.org/packages/KrakenIoc.Unity/)

## About
Inversion of Control is a design principle in modern software applications. Kraken is a dependency injection framework with extensibility built-in.

### Table of Contents
1. [Why use a D.I. Framework?](#why-use-a-d-i-framework)
2. [The Container](#the-container)
    - [Binding](#binding)
    - [Resolving](#resolving)
    - [Binding Abstractions to Implementations](#binding-abstractions-to-implementations)
    - [Binding & Resolving with Categories](#binding-resolving-with-categories)
    - [Binding Type Factories](#binding-type-factories)
    - [Bootstraps](#bootstraps)
    - [Binding Middleware](#binding-middleware)
    - [Inheritance](#inheritance)
3. [Injection](#injection)
4. [Injector](#injector)
    - [IInjectContext](#iinjectcontext)
5. [Unity Integration, Kraken.Unity](#krakenunity)
    - [Resolve a Component onto a GameObject](#resolving-a-component-onto-a-gameobject)
    - [Context](#context)

## Why use a D.I. Framework?
**Without A D.I. Framework**
Dependency Injection can be achieved without a framework. Take the following example:
```csharp
public class MyClass
{
    public MyClass(WebService webService) {...}
}
```
By specifying all the dependencies that a class expects within its constructor, you explicitly state what your class needs to run. This pattern allows your classes to be unit-tested easier because the developer can replace which services your classes use into the constructor.

**D.I. Framework**
By using a D.I. framework, you simplify injections and refactoring by allowing the framework to create you objects. The framework auto-injects the dependencies, thus freeing you up to develop features instead of boilerplate.

## The Container
A container contains the mappings of bound abstract types to implementation types. The container is the entry point where types are bound and resolved.

### Binding
The `Bind` method returns an object of type `IBinding`; of which contains fluent api for adding additional details to the set.
```csharp
var container = new Container(...);
container.Bind<WebService>().AsSingleton();
```

### Resolving
`Resolve` has a few overrides, but the method returns an instance of the specified binding. Depending on your `IBinding` settings, your instance can be `Transient` or a `Singleton`.
```csharp
var service = container.Resolve<IWebService>();
```

**Transient:** A new instance every time it is resolved (default).
**Singleton:** A new instance if null, otherwise return existing instance.

### Binding Abstractions to Implementations
When abstractions are required, you can pass the implementation type to the `To` fluent method of `IBinding`.
```csharp
container.Bind<IWebService>().To<WebService>().AsSingleton();
```

### Binding Multiple Interfaces to a single Implementation
You can bind multiple interfaces to the same implementation. That way, you can have multiple interfaces referring to the same singleton:
```csharp
container.Bind(typeof(IWebService), typeof(IProductWebService), typeof(IWebConnectionService)).To<WebService>().AsSingleton();
```

### Binding & Resolving with Categories
It is often useful to map different types of implementations. You can do this with categories.
```csharp
container.Bind<IDashboard>().To<EmailDashboard>().WithCategory(ProductCategories.Email);
container.Bind<IDashboard>().To<HomeDashboard>().WithCategory(ProductCategories.Home);

// Resolve the `Home` category...
var homeDashboard = container.ResolveWithCategory<IDashboard>(ProductCategories.Home);
```

### Binding Type Factories
You can pass a `IFactory<T>` or a delegate matching `FactoryMethod<T>` to handle how an object is resolved.
```csharp
class WebServiceFactory : IFactory<WebService>
{
	object Create(IInjectContext injectionContext)
    {
        return Create(injectionContext);
    }
}
// From a factory type...
container.Bind<WebService>().FromFactory<WebServiceFactory, WebService>();

// From a factory method...
container.Bind<WebService>().FromFactoryMethod((injectContext) => {
    return new WebService();
});
```

As an alternative, you can decide to bind a non-generic factory. This is useful when you bind multiple interfaces to one implementations.

```csharp
// From a factory type...
container.Bind(typeof(IWebService), typeof(IProductWebService), typeof(IWebConnectionService)).To<WebService>().FromFactory<WebServiceFactory>().AsSingleton();
```


### Bootstraps
`Bootstrap`s provide a modular way to bind your services and features.
```csharp
public class MyBootstrap : IBootstrap
{
    public void SetupBindings(IContainer container)
    {
        ...
    }
}

container.Bootstrap<MyBootstrap>();
```

### Binding Middleware
You can extend the `Resolve` functionality by implementing an `IBindingMiddleware`.

```csharp
public class MyMiddleware : IBindingMiddleware
{
	  public object Resolve(IBinding binding, object target = null)
      {
          // Every type resolved is now MyCoolClass()...
          return new MyCoolClass();
      }

      public object Resolve(IBinding binding, IInjectContext injectContext, object target = null)
      {
          // Default binding resolve will handle....
          return null;
      }
}

// Add middleware to all bindings...
Binding.AddBindingMiddleware(new MyMiddleware());
```

### Inheritance
If you have more than one container, and wish to use bindings from one container into another, you can inherit the container as follows:
```csharp
var containerA = new Container();
containerA.Bind<WebService>().AsSingleton();

var containerB = new Container();
containerB.Inherit(containerA);

var webService = containerB.Resolve<WebService>();
```
#### Order of Inheritance
If containerB has a type bound that is found within containerA, then containerB's binding will be used. 
```csharp
var containerA = new Container();
containerA.Bind<WebService>().AsSingleton();

var containerB = new Container();
containerB.Bind<WebService>().AsSingleton();
containerB.Inherit(containerA);

// Resolves containerB WebService...
var webService = containerB.Resolve<WebService>();
```

#### Inheritance - Inversed Resolving
If containerA resolves a type that depends on a type within containerB, containerB binding will be used. However, this only works with **transient** bindings:
```csharp
public class ApiService
{
    public WebService WebService { get; private set; }
    public ApiService(WebService webService)
    {
        _webService = webService;
    }
}


var containerA = new Container();
containerA.Bind<ApiService>().AsTransient();

var containerB = new Container();
containerB.Bind<WebService>().AsTransient();

containerB.Inherit(containerA);

var apiService = containerB.Resolve<ApiService>(); // apiService.WebService is new instance of WebService
```

Singleton bindings will not resolve this way (in fact, they will be resolved from containerA and can not be overridden):

```csharp
var containerA = new Container();
containerA.Bind<ApiService>().AsSingleton();

var containerB = new Container();
containerB.Bind<WebService>().AsTransient();

containerB.Inherit(containerA);

// apiService resolves and caches, but apiService.WebSerivce is null.
// This happens because singleton instances resolved from ContainerA have no knowledge about ContainerB 
var apiService = containerB.Resolve<ApiService>();
```



## Injection
When a type is resolved, injection will occur on that type. Parameters in a constructor are automatically injected if the type is bound in the container. For instance, in the following example IWebService will be injected, however ILogService will be null because it was not bound.
```csharp
class MyClass
{
    public MyClass(WebService webService, LogService logService)
    {
        Assert.IsNotNull(webService);
        Assert.IsNull(logService);
    }
}

container.Bind<WebService>().AsSingleton();
container.Bind<MyClass>();

var myClass = container.Resolve<MyClass>();
```
### Circular Dependencies
You cannot have circular dependencies. The inject should flow top to bottom; never circular.

## Injector

The injector is the class that does the work of finding the parameters, constructors for injection during resolving. The default implementation is provided by the Kraken framework. However you can override the default by feeding your custom implementation into the constructor.
```csharp
var myInjector = new MyInjector();
var container = new Container(myInjector);
```

### IInjectContext
`IInjectContext` is a special type that can be injected into your classes. This type carries meta information about the request to resolve your type. See the following example:
```csharp
public class MyConsumingClass
{
    public MyConsumingClass(MyProvidingClass myClass) { ... }
}

public class MyProvidingClass
{
    public MyClass(IInjectContext injectContext)
    {
	    // The container resolving this...
		Assert.NotNull(injectContext.Container);
		
        // The class declaring this instance...
	    Assert.IsTrue(typeof(MyConsumingClass), injectContext.DeclaringType);
	    
	    // The parent IInjectContext, for the object resolved before this...
	    Assert.NotNull(injectContext.ParentContext);
	}
}
```

## Kraken.Unity

Kraken.Unity provides a `UnityInjector` to replace the default `Injector` along with several new concepts.

### Resolving a Component onto a GameObject
You can pass a `target` into the `Resolve` method. In the `UnityInjector` this will effectively attempt to `AddComponent<T>` onto the target.

```csharp
container.Bind<ISomeComponent>().To<SomeComponent>();
// Adds the component onto an existing gameObject...
container.Resolve<ISomeComponent>(myGameObject);

// creates a new GameObject since none were passed..
container.Resolve<ISomeComponent>();
```

### Context
`Context` is a MonoBehaviour that contains a `Container` that is pre-configured to use the `UnityInjector`. A `Context` serves as the entry point for your bindings and application. In this best-practices example, we bind our services and a standard c# class to act as our main application entry point.

```csharp
public class MyProductSetup
{
	private WebService _webService;
	
	public MyProductSetup(WebService webService)
	{
		_webService = webService;
	}
		
    public void Setup()
    {
	    // Kick off the application...
	    _webService.Request(...);
	}
}
public class MyProductContext : Context
{
    protected override void OnSetupBindings()
    {
	    Container.Bind<WebService>().AsSingleton();
	    Container.Bind<MyProductSetup>().AsSingleton();
	}
	
	 protected virtual void OnStart()
     {
		var product = Container.Resolve<MyProductSetup>();
		product.Setup();
     }
}
```