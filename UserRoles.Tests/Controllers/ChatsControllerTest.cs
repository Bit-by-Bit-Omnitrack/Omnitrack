using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Controllers;
using UserRoles.Models;
using Xunit;

public class ChatsControllerTests
{
    private async Task<AppDbContext> GetDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Optional: Pre-populate for tests
        if (!context.Chats.Any())
        {
            context.Chats.Add(new Chats
            {
                TicketId = 99,
                Sender = "TestUser",
                Message = "Hello from test",
                SentAt = DateTime.Now
            });
            await context.SaveChangesAsync();
        }

        return context;
    }

    private ChatsController GetControllerWithUser(AppDbContext context)
    {
        var controller = new ChatsController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "UnitTester")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithChatList()
    {
        var context = await GetDbContextAsync();
        var controller = new ChatsController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Chats>>(viewResult.Model);

        Assert.NotEmpty(model); // Ensures seeding worked
    }

    [Fact]
    public async Task Create_ValidChat_RedirectsToIndex()
    {
        var context = await GetDbContextAsync();
        var controller = GetControllerWithUser(context);

        var newChat = new Chats
        {
            TicketId = 100,
            Message = "Testing Create action"
        };

        var result = await controller.Create(newChat);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var savedChat = await context.Chats.FirstOrDefaultAsync(c => c.Message == newChat.Message);
        Assert.NotNull(savedChat);
        Assert.Equal("UnitTester", savedChat.Sender);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsViewWithModel()
    {
        var context = await GetDbContextAsync();
        var controller = GetControllerWithUser(context);

        controller.ModelState.AddModelError("Message", "Required");

        var chat = new Chats { TicketId = 101 };

        var result = await controller.Create(chat);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsAssignableFrom<Chats>(viewResult.Model);
        Assert.Equal(chat.TicketId, returnedModel.TicketId);
    }
}
