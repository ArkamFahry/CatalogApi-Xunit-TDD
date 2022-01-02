using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Catalog.UnitTest;

public class ItemsControllerTest
{
    private readonly Mock<IInMemItemsRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();

    [Fact]
    public async Task GetItemAsync_WithNotExistingItem_ReturnNotFoundAsync()
    {
        // Arrange

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync((item)null);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var result = await controller.GetItemAsync(Guid.NewGuid());

        // Assert

        result.Result.Should().BeOfType<NotFoundResult>();

    }

    [Fact]
    public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
    {
        // Arrange

        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(expectedItem);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var result = await controller.GetItemAsync(Guid.NewGuid());

        // Assert

        result.Value.Should().BeEquivalentTo(expectedItem);

    }

    [Fact]
    public async Task GetItemsAsync_WithExsistingItems_ReturnsAllItems()
    {
        //  Arrange 

        var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };
        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var actualItems = await controller.GetItemsAsync();

        // Assert

        actualItems.Should().BeEquivalentTo(expectedItems);

    }

    [Fact]
    public async Task GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
    {
        //  Arrange 

        var allItems = new[] 
        { 
            new item() { Name = "Potion" },
            new item() { Name = "Antidote" },
            new item() { Name = "Hi-Potion" }
        };

        var nameToMatch = "Potion";

        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(allItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

        // Assert

        foundItems.Should().OnlyContain(
            item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
        );

    }


    [Fact]
    public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
    {
        //  Arrange 

        var ItemToCreate = new CreateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), rand.Next(1000));

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var result = await controller.CreateItemAsync(ItemToCreate);

        // Assert

        var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
        ItemToCreate.Should().BeEquivalentTo(createdItem, options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());
        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, 2.Seconds());

    }

    [Fact]
    public async Task UpdateItemAsync_WithExsistingItem_ReturnsNoContent()
    {
        //  Arrange 

        var existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(existingItem);
        var itemId = existingItem.Id;
        var itemToUpdate = new UpdateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), existingItem.Price + 3);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var result = await controller.UpdateItem(itemId, itemToUpdate);

        // Assert

        result.Should().BeOfType<NoContentResult>();

    }

    [Fact]
    public async Task DeleteItemAsync_WithExsistingItem_ReturnsNoContent()
    {
        //  Arrange 

        var existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(existingItem);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        // Act

        var result = await controller.DeleteItemAsync(existingItem.Id);

        // Assert

        result.Should().BeOfType<NoContentResult>();

    }

    private item CreateRandomItem()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}