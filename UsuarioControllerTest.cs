using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using GestorDeTareas.Models;
using GestorDeTareas.Controllers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GestorDeTareas.Tests
{
    public class UsuarioControllerTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly UsuarioController _controller;

        public UsuarioControllerTest()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            _controller = new UsuarioController(_userManagerMock.Object, _roleManagerMock.Object, null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Role, "Administrador"),
                    new Claim(ClaimTypes.Name, "admin")
                    }, "TestAuth"))
                }
            };
        }

        [Fact]
        public async Task GetAll()
        {
            var list_usuarios = new List<ApplicationUser>
            {
            new ApplicationUser { UserName = "admin", Email = "admin@gmail.com" },
            new ApplicationUser { UserName = "user1", Email = "user1@gmail.com" }
            };

            _userManagerMock.Setup(um => um.Users).Returns(list_usuarios.AsQueryable());

            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "Administrador" });

            var result = await _controller.GetAll();

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var usuarios = Assert.IsType<List<Usuario>>(ok_result.Value);

            Assert.NotNull(usuarios);
            Assert.NotEmpty(usuarios);
            Assert.Equal(2, usuarios.Count);

            Assert.Contains(usuarios, u => u.Nombre == "admin" && u.Email == "admin@gmail.com");
            Assert.Contains(usuarios, u => u.Nombre == "user1" && u.Email == "user1@gmail.com");
        }

        [Fact]
        public async Task Insert()
        {
            var model = new RegisterUser
            {
                Nombre = "user2",
                Email = "user2@gmail.com",
                Password = "User2784@",
                Rol = new Rol { Name = "Empleado" }
            };

            _roleManagerMock.Setup(rm => rm.RoleExistsAsync(model.Rol.Name))
                .ReturnsAsync(true);

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), model.Rol.Name))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Insert(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result).Value;
            Assert.Equal("Usuario creado exitosamente!", response.GetType().GetProperty("message").GetValue(response));
        }

        [Fact]
        public async Task Update()
        {
            // Arrange
            var model = new Usuario
            {
                Nombre = "user3",
                Email = "user3@gmail.com",
                Rol = new Rol { Name = "Supervisor" }
            };

            _roleManagerMock.Setup(rm => rm.RoleExistsAsync(model.Rol.Name))
                .ReturnsAsync(true);

            var user = new ApplicationUser { UserName = model.Nombre, Email = "user3nuevo@gmail.com" };
            _userManagerMock.Setup(um => um.FindByNameAsync(model.Nombre))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.AddToRoleAsync(user, model.Rol.Name))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Update(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result).Value;
            Assert.Equal("Usuario actualizado exitosamente!", response.GetType().GetProperty("message").GetValue(response));
        }

        [Fact]
        public async Task Delete()
        {
            var model = new Usuario { Nombre = "user4" };

            var user = new ApplicationUser { UserName = model.Nombre };
            _userManagerMock.Setup(um => um.FindByNameAsync(model.Nombre))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Delete(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result).Value;
            Assert.Equal("Usuario eliminado exitosamente!", response.GetType().GetProperty("message").GetValue(response));
        }

        [Fact]
        public async Task Login()
        {
            var user = new ApplicationUser { UserName = "admin", Id = "1" };

            _userManagerMock.Setup(um => um.FindByNameAsync("admin"))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, "Admin052@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                            .ReturnsAsync(new List<string> { "Administrador" });

            var _controller = new UsuarioController(_userManagerMock.Object, null, null);

            var result = await _controller.Login(new Login { Name = "admin", Password = "Admin052@" });

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var response = ok_result.Value;
            Assert.NotNull(response);
        }
    }
}


