using System.Linq;
using System.Threading.Tasks;
using Ardalis.Specification;
using Server.Core.ProjectAggregate;
using Server.Core.Services;
using Server.SharedKernel.Interfaces;
using Moq;
using Xunit;

namespace Server.UnitTests.Core.Services
{
    public class ToDoItemSearchService_GetAllIncompleteItems
    {
        private Mock<IRepository<ToDoItem>> _mockRepo = new Mock<IRepository<ToDoItem>>();
        private ToDoItemSearchService _searchService;

        public ToDoItemSearchService_GetAllIncompleteItems()
        {
            _searchService = new ToDoItemSearchService(_mockRepo.Object);
        }

        [Fact]
        public async Task ReturnsInvalidGivenNullSearchString()
        {
            var result = await _searchService.GetAllIncompleteItemsAsync(null);

            Assert.Equal(Server.SharedKernel.Result.ResultStatus.Invalid, result.Status);
            Assert.Equal("searchString is required.", result.ValidationErrors.First().ErrorMessage);
        }

        [Fact]
        public async Task ReturnsErrorGivenDataAccessException()
        {
            string expectedErrorMessage = "Database not there.";
            _mockRepo.Setup(r => r.ListAsync(It.IsAny<ISpecification<ToDoItem>>()))
                .ThrowsAsync(new System.Exception(expectedErrorMessage));

            var result = await _searchService.GetAllIncompleteItemsAsync("something");

            Assert.Equal(Server.SharedKernel.Result.ResultStatus.Error, result.Status);
            Assert.Equal(expectedErrorMessage, result.Errors.First());
        }

        [Fact]
        public async Task ReturnsListGivenSearchString()
        {
            var result = await _searchService.GetAllIncompleteItemsAsync("foo");

            Assert.Equal(Server.SharedKernel.Result.ResultStatus.Ok, result.Status);
        }
    }
}
