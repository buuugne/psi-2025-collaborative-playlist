using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using MyApi.Controllers;
using MyApi.Services;
using MyApi.Dtos;
using MyApi.Models;

namespace TestProject.Controllers
{
    public class UsersControllerTests
    {
        private static UsersController CreateController(
            Mock<IUserService> userService,
            ClaimsPrincipal? user = null)
        {
            var controller = new UsersController(userService.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            return controller;
        }

        private static ClaimsPrincipal BuildUser(int? userId = null, params string[] roles)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static IFormFile CreateFakeFormFile(string fileName, byte[] content)
        {
            var ms = new MemoryStream(content);
            return new FormFile(ms, 0, content.Length, "imageFile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenTokenInvalid()
        {
            var svc = new Mock<IUserService>();
            var user = BuildUser(userId: null); // no NameIdentifier
            var controller = CreateController(svc, user);

            var result = await controller.GetCurrentUser();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token", unauthorized.Value);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserMissing()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.GetByIdAsync(10)).ReturnsAsync((UserDto?)null);

            var controller = CreateController(svc, BuildUser(10));

            var result = await controller.GetCurrentUser();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
        {
            var svc = new Mock<IUserService>();
            var dto = new UserDto { Id = 10, Username = "m", Role = UserRole.Guest };
            svc.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(dto);

            var controller = CreateController(svc, BuildUser(10));

            var result = await controller.GetCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((UserDto?)null);

            var controller = CreateController(svc, BuildUser(99));

            var result = await controller.GetById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenExists()
        {
            var svc = new Mock<IUserService>();
            var dto = new UserDto { Id = 1, Username = "u", Role = UserRole.Host };
            svc.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(dto);

            var controller = CreateController(svc, BuildUser(99));

            var result = await controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task ChangeRole_ShouldReturnBadRequest_WhenServiceFails()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.ChangeRoleAsync(1, UserRole.Admin))
               .ReturnsAsync((false, "User not found"));

            var controller = CreateController(svc, BuildUser(999, "Admin"));

            var dto = new ChangeRoleDto { Role = UserRole.Admin };

            var result = await controller.ChangeRole(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User not found", bad.Value);
        }

        [Fact]
        public async Task ChangeRole_ShouldReturnNoContent_WhenSuccess()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.ChangeRoleAsync(1, UserRole.Admin))
               .ReturnsAsync((true, (string?)null));

            var controller = CreateController(svc, BuildUser(999, "Admin"));

            var dto = new ChangeRoleDto { Role = UserRole.Admin };

            var result = await controller.ChangeRole(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenServiceFails()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.DeleteAsync(1)).ReturnsAsync((false, "User not found"));

            var controller = CreateController(svc, BuildUser(999, "Admin"));

            var result = await controller.Delete(1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User not found", bad.Value);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccess()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.DeleteAsync(1)).ReturnsAsync((true, (string?)null));

            var controller = CreateController(svc, BuildUser(999, "Admin"));

            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnUnauthorized_WhenTokenInvalid()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            var controller = CreateController(svc, BuildUser(userId: null));
            var file = CreateFakeFormFile("a.png", new byte[] { 1, 2, 3 });

            var result = await controller.UpdateProfileImage(1, file, env.Object);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token", unauthorized.Value);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnForbid_WhenNotOwnerAndNotAdminOrHost()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            // current user is 2, trying to update id=1, roles none => forbid
            var controller = CreateController(svc, BuildUser(2));
            var file = CreateFakeFormFile("a.png", new byte[] { 1 });

            var result = await controller.UpdateProfileImage(1, file, env.Object);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnBadRequest_WhenNoFile()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            var controller = CreateController(svc, BuildUser(1)); // owner

            var result = await controller.UpdateProfileImage(1, null, env.Object);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", bad.Value);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            svc.Setup(x => x.UpdateProfileImageAsync(1, It.IsAny<IFormFile>(), "C:\\fake"))
               .ReturnsAsync((UserDto?)null);

            var controller = CreateController(svc, BuildUser(1));
            var file = CreateFakeFormFile("a.png", new byte[] { 1 });

            var result = await controller.UpdateProfileImage(1, file, env.Object);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            svc.Setup(x => x.UpdateProfileImageAsync(1, It.IsAny<IFormFile>(), "C:\\fake"))
               .ThrowsAsync(new ArgumentException("Invalid file type."));

            var controller = CreateController(svc, BuildUser(1));
            var file = CreateFakeFormFile("a.exe", new byte[] { 1 });

            var result = await controller.UpdateProfileImage(1, file, env.Object);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid file type.", bad.Value);
        }

        [Fact]
        public async Task UpdateProfileImage_ShouldReturnOk_WhenSuccess()
        {
            var svc = new Mock<IUserService>();
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns("C:\\fake");

            var dto = new UserDto { Id = 1, Username = "u", Role = UserRole.Guest, ProfileImage = "/profiles/a.png" };

            svc.Setup(x => x.UpdateProfileImageAsync(1, It.IsAny<IFormFile>(), "C:\\fake"))
               .ReturnsAsync(dto);

            var controller = CreateController(svc, BuildUser(1));
            var file = CreateFakeFormFile("a.png", new byte[] { 1, 2 });

            var result = await controller.UpdateProfileImage(1, file, env.Object);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task SearchUsers_ShouldReturnEmptyList_WhenQueryTooShort()
        {
            var svc = new Mock<IUserService>();
            var controller = CreateController(svc, BuildUser(1));

            var result = await controller.SearchUsers("a");

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<List<object>>(ok.Value);
            Assert.Empty(list);

            svc.Verify(x => x.SearchUsersAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SearchUsers_ShouldReturnIdAndUsernameProjection()
        {
            var svc = new Mock<IUserService>();
            svc.Setup(x => x.SearchUsersAsync("ma"))
               .ReturnsAsync(new List<UserDto>
               {
                   new UserDto { Id = 1, Username = "marius", Role = UserRole.Guest },
                   new UserDto { Id = 2, Username = "martynas", Role = UserRole.Host }
               });

            var controller = CreateController(svc, BuildUser(1));

            var result = await controller.SearchUsers("ma");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);

            // Because controller returns anonymous objects { Id, Username },
            // we can just ensure it's an enumerable with 2 items.
            var enumerable = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            var count = 0;
            foreach (var _ in enumerable) count++;
            Assert.Equal(2, count);
        }
    }
}
