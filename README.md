# Autofac.Extensions.DependencyInjection.AzureFunctions

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](https://autofac.readthedocs.io/en/latest/glossary.html)*.

[![Build status](https://ci.appveyor.com/api/projects/status/1mhkjcqr1ug80lra/branch/develop?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-extensions-dependencyinjection/branch/develop)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation - .NET Core Integration](https://autofac.readthedocs.io/en/latest/integration/netcore.html)
- [Documentation - ASP.NET Core Integration](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection.AzureFunctions)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)

## Get Started in Azure Functions

This quick start shows how to use the `UseAutofacServiceProviderFactory` integration to help automatically build the root service provider for you. 

- Reference the `Autofac.Extensions.DependencyInjection.AzureFunctions` package from NuGet.
- Create your `Startup` class, following the documentation on how to [Use dependency injection in .NET Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)
- In your `Configure` method, where you configure the `IFunctionsHostBuilder`, call `UseAutofacServiceProviderFactory(ConfigureContainer)` to hook Autofac into the startup pipeline.
- In the `ConfigureContainer` method of your `Startup` class register things directly into an Autofac `ContainerBuilder`.
- If you want to use functions declared in referenced projects, add `    <FunctionsInDependencies>true</FunctionsInDependencies>` to the `<PropertyGroup>` of your main Azure Functions project.

The `IServiceProvider` will automatically be created for you, so there's nothing you have to do but *register things*.


```C#
[assembly: FunctionsStartup(typeof(Startup))]

public class Startup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder
            .UseAppSettings() // this is optional, this will bind IConfiguration in the container.
            .UseAutofacServiceProviderFactory(ConfigureContainer);
    }

    private void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .Register(activator =>
            {
                // Example on how to bind settings from appsettings.json
                // to a class instance
                var mySettings = new MySettings();

                var config = activator.Resolve<IConfiguration>();
                config.GetSection(nameof(MySettings)).Bind(mySettings);

                return mySettings;
            })
            .AsSelf()
            .SingleInstance();

        // Register all functions that resides in a given namespace
        builder
            .RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespace("MyNamespace.Functions")
            .AsSelf()
            .InstancePerTriggerRequest();

        builder
            .RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespace("MyNamespace.Services")
            .AsImplementedInterfaces()
            .InstancePerTriggerRequest();

    }

}

```

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).