using System.Net;
using Example.Web.Models.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Example.Web.Tests.Controllers;

public class HomeControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly Db _db;

    public HomeControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.Init();
        _scope = _factory.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<Db>(); // most tests need to check database, so provide it here already and easier to use in specific tests
    }

    public void Dispose()
    {
        _db.Dispose();
        _scope.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Post_Will_Store_Registration_In_Database()
    {
        // send a raw POST here for true integration test

        using var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsync("/", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("FirstName", "Some first name"), new("LastName", "Some last name"), new("EmailAddress", "Some email")
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // nothing in database
        Assert.Single(await _db.Registrations.ToListAsync());
        var registration = await _db.Registrations.SingleAsync();
        Assert.Equal("Some first name", registration.FirstName);
        Assert.Equal("Some last name", registration.LastName);
    }

    [Fact]
    public async Task Post_Will_Not_Store_Registration_If_Email_Already_Used()
    {
        using var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsync("/", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("FirstName", "Some first name"), new("LastName", "Some last name"), new("EmailAddress", "Some email")
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseError = await httpClient.PostAsync("/", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("FirstName", "Some first name2"), new("LastName", "Some last name2"), new("EmailAddress", "Some email")
        }));

        Assert.Single(await _db.Registrations.ToListAsync());
    }
}
