# Creating the simplest possible ASP. NET Core form

Recently I needed to create a simple page for someone to submit an HTML form. The rest of the Azure aplication is running on serverless functions, Logic apps and Cognitive services, but for that last part I wanted something where the user can click on a link, open the page in a web browser (probably on a mobile device), enter a passphrase in a form and the submit through a POST to an SSL encrypted page. I thought of writing a small Xamarin app and submitting through POST to an Azure Function. Another option would be to use a static HTML page and to use Javascript to submit the Form through a POST to that Azure Function. I don't exclude these two options for the future.

But in the meantime I wanted to experiment with a simple Razor page that would present an HTML Form to the user, and submit this Form to itself with a POST over HTTPS.

## Razor pages with models are really cool

I love super simple ASP.NET Core sites without MVC. Don't get me wrong, MVC is awesome for enterprise web applications, where testability and maintainability are primordial. But they also come with a lot of overhead. If you go ahead and create an "empty" ASP.NET Core MVC website with the red-circled template below, you will end up with a lot of files (CSHTML pages, controllers, setup classes, Javascript, CSS etc). Even an empty ASP.NET Core MVC website contains 38 files (!).

![ASP.NET Core Razor and ASP.NET Core MVC templates in Visual Studio](./simplest-form-with-aspnetcore/002.png)

*ASP.NET Core Razor and ASP.NET Core MVC templates in Visual Studio*

Another option is to create an ASP.NET Core web application with Razor pages only. In such an app, you eliminate the controllers, and you handle the code in a PageModel instance which is attached to the CSHTML Razor page. This is definitely less complex and in fact my private website is implemented in this manner. However even if you create an "empty" web application in this manner using the orange-circled template shown above, you still end up with a lot of files and need to delete most of them. That's annoying and I prefer to start from a "truly empty" template like the one selected in blue in the image above.

> Even though I use Visual Studio in this example, you can create the ASP.NET Core apps using a command line with the `dotnet new` [syntax shown here](TODO).

## Starting from scratch

Let's start from scratch using the `Empty` web app template shown above. To do this, in Visual Studio 2017, start by selecting File, New, Project.

> Before you start, you must make sure to have installed the .NET Core workload in the Visual Studio installer. You can always run the Visual Studio Installer from your Start menu, and check that the following workload is checked.

![.NET Core workload](./simplest-form-with-aspnetcore/003.png)

*.NET Core workload*

In the next step, you will have to select the Empty web application as shown below. You can enable HTTPS by default, which is usually a good idea. In our example we won't use authentication so you can leave this option to `No Authentication`. Then click OK.

![Creating an empty ASP.NET Core application](./simplest-form-with-aspnetcore/002.png)

## Configuring Razor pages 

As the next steps, we will configure the application to serve Razor pages. To do this, and even though we are not going to use the full MVC capabilities here, we still need to ask the application to use MVC services.

1. Open [Startup.cs](TODO) in Visual Studio.
2. Modify the `ConfigureServices` method as follows:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
}
```

3. Modify the `Configure` method as shown below:

```cs
public void Configure(
    IApplicationBuilder app, 
    IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseMvc();
}
```

## Creating the Razor page

With this configuration, the application will look for Razor (CSHTML) pages in a folder named `Pages` and will direct the request to the page corresponding to the route (URL). In this example, since we want to keep it as simple as possible, we will use the default route and create a page named Index.cshtml.

1. Right click on the project and select `Add` and then `New Folder` from the context menu.
2. Name this new folder `Pages`.
3. Right click on the `Pages` folder and select `Add` and then `Razor Page` from the context menu.
4. in the `Add scaffold` dialog, press on `Add`. 
5. In the `Add Razor Page` dialog, enter the name `Index` and make sure that `Generate PageModel class` is selected. 
6. Uncheck `Use a layout page` and then press `Add`.

![Creating the Razor page](./simplest-form-with-aspnetcore/004.png)

> This step can take a moment because a few Nuget packages need to be installed in order for the ASP.NET Core application, the routing and the Razor pages to work.

## Testing the GET method

By default, the page is configured to receive GET requests. We will test this now.

- Open the Index.cshtml page (in the Pages folder).
- Note the presence of the `@model` directive. This is instructing ASP.NET to use the `IndexModel` class (in Index.cshtml.cs) to handle the calls.

> You can also add code directly inside the Index.cshtml page, inline with the HTML markup. This is super convenient but it can also lead to some "spaghetti" code which is very hard to test. It is not recommended to do so, except for layout code (for example `for` loops to create lists, etc...).

- Open the Index.cshtml.cs file. This file is nested within the Index.cshtml page in the Solution explorer.
- Place a breakpoint within the OnGet method.
- Run the application in debug mode. This will start IIS Express and open a `localhost` URL in your favorite web browser, for example `https://localhost:44367/`.
- Notice that the breakpoint is hit.

Within the method, you have access to all the usual ASP.NET objects, such as the [HttpRequest](TODO) instance (in the `Request` property), etc.

## Setting up the POST feature

Just like we can handle `GET` calls in the `OnGet` method, we will handle `POST` calls in the `OnPost` method. However this needs a little addition configuration as we will see. First let's prepare an HTML Form which will post a single text field to the IndexModel class.

1. In the `IndexModel` class, add a property of type `string` named `Message`.

```cs
public string Message
{
    get;
    set;
}
```

2. Modify the `OnGet` method as follows:

```cs
public void OnGet()
{
    Message = "Enter your message here";
}
```

3. Still in the IndexModel class, add a method named `OnPost`:

```cs
public void OnPost()
{
    Message = Request.Form[nameof(Message)];
}
```

4. Place a breakpoint inside the `OnPost` method.
5. Open Index.cshtml in the editor.
6. Edit the `body` as follows:

```html
<body>
    <form method="post">
        <input asp-for="Message" />
        <br />
        <input type="submit" />
    </form>
</body>
```

7. Run the application again in Debug mode. The `OnGet` method will get called like before, and you should see the HTML Form with an empty input field.

![Empty HTML form](./simplest-form-with-aspnetcore/005.png)

At this point, you might be surprised if like me you had expected the input field to be initialized with the content of the `Message` property (because of the `asp-for` attribute). But let's test further to see another issue.

8. Enter any text in the field and press the Submit button.

At this point, you will get an HTTP error 400 in the browser (Bad request). 

## Fixing the Bad request error 400

A quick online search reveals that the issue has to do with a missing antiforgery tokens, which are a security measure put in place by ASP.NET to avoid cross-domain attacks. In essence, what the token does is prove that the request comes from the site which the form originated from.

So how do we get the token? This is where a useful namespace called `Microsoft.AspNetCore.Mvc.TagHelpers` comes to play. Adding this to the CSHTML page will automatically generate the antiforgery token in the HTML form, and will also create the HTML attributes corresponding to the `asp-for` attribute that we added into the form.

1. Open Index.cshtml.
2. On top of the file, but *below* the `@page` attribute, add the `@addTagHelper` attribute so that your file looks like the following code:

```cs
@page
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@model TestPostWithRazor.Pages.IndexModel
@{
    Layout = null;
}
```

3. Run the application again in Debug mode.
4. In your web browser, once the page is loaded, you should now see the form initialized as expected:

![Initialized form](./simplest-form-with-aspnetcore/006.png)

5. Right click anywhere on the page and select `View page source`. You should see the following HTML code that was generated by the ASP.NET application:

```html
<body>
    <form method="post">
        <input type="text" id="Message" name="Message" value="Enter your message here" />
        <br />
        <input type="submit" />
    <input name="__RequestVerificationToken" type="hidden" value="[Some token]" /></form>
</body>
```

6. Modify the content of the text field and click on the Submit button. At this point, the breakpoint in the `OnPost` method should get hit, and the content of the field will be assigned to the `Message` property.

## Conclusion

Sometimes, simple is better. In this example, we saw how to create an empty ASP.NET Core application with a Razor page (and the corresponding `PageModel`), and how to configure this application to handle simple `GET` and `POST` methods. This is an alternative to other mechanisms such as a pure client-side JavaScript powered page talking to a serverless Azure function. While the solution presented here is not serverless, you can take advantage of it if you already have an App Service plan in Azure, with Windows or Linux thanks to the cross-platform abilities of ASP.NET Core.

Hopefully this code will be useful to some.

Happy coding!

Laurent