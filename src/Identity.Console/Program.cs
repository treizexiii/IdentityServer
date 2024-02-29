// See https://aka.ms/new-console-template for more information

using Identity.Sdk;
using Identity.Wrappers.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddRestClient("http://localhost:9090/");
    })
    .Build();

var client = host.Services.GetRequiredService<IIdentityClient>();

// await TestUserOk(client);
// await TestUserFalse(client);

Console.ReadKey();

// async Task TestUserOk(IIdentityClient identityHttpClient)
// {
//     const string user = "superadmin";
//     const string password = "superadmin";
//
//     var response = await identityHttpClient.LoginAsync(new LoginDto(user, password, null));
//     if (response.Success)
//     {
//         Console.WriteLine("Login Ok");
//         Console.WriteLine(response.Data?.Token);
//     }
//     else
//     {
//         Console.WriteLine("Login Error");
//     }
// }

// async Task TestUserFalse(IIdentityClient identityHttpClient)
// {
//     const string user = "superadmi";
//     const string password = "superadmin";
//
//     var response = await identityHttpClient.LoginAsync(new LoginDto(user, password, null));
//     if (response.Code == 400)
//     {
//         foreach (var error in response.Errors)
//         {
//             Console.WriteLine(error);
//         }
//     }
// }