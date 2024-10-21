using GestorDeTareas.Controllers;
using GestorDeTareas.Models;
using GestorDeTareas.Models.Custom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace GestorDeTareas.Tests
{
    public class TareaControllerTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly ApplicationDbContext _db;
        private readonly TareaController _controller;
        
        public TareaControllerTest()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                  . UseInMemoryDatabase(databaseName: "TestDB")
                  .Options;

            _db = new ApplicationDbContext(options);            

            _controller = new TareaController(_userManagerMock.Object, _roleManagerMock.Object, _db, null);

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
            var usuario = new ApplicationUser { UserName = "admin", Id = "1" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "Admin052@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Administrador" });

            Tarea tarea = new Tarea { Id = 1, Titulo = "Tarea 1", Descripcion = "Descripción Tarea 1", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = "admin", Estado = EstadoTarea.PENDIENTE };
            _db.Tareas.Add(tarea);
            _db.SaveChanges();

            int pgnum = 1; 
            int pgsize = 5;

            var result = await _controller.GetAll(pgnum, pgsize);
            var ok_result = Assert.IsType<OkObjectResult>(result);
            var tareas = Assert.IsType<PgResponse>(ok_result.Value);
            Assert.Contains(tarea, tareas.Tareas);
        }

        
        [Fact]
        public async Task GetByUser()
        {
            var usuario = new ApplicationUser { UserName = "user1", Id = "2" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "User078@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Empleado" });
        
            Tarea tarea = new Tarea { Id = 2, Titulo = "Tarea 2", Descripcion = "Descripción Tarea para Empleado", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = usuario.UserName, Estado = EstadoTarea.PENDIENTE };
            _db.Tareas.Add(tarea);
            _db.SaveChanges();

            int pgnum = 1;
            int pgsize = 5;

            var result = await _controller.GetByUser(usuario.UserName, pgnum, pgsize);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            var tareas = Assert.IsType<PgResponse>(ok_result.Value);
            Assert.All(tareas.Tareas, tarea => Assert.Equal(usuario.UserName, tarea.UserName));
        }

        [Fact]
        public async Task Insert()
        {
            var usuario = new ApplicationUser { UserName = "user2", Id = "3" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "User452@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Empleado" });

            var model = new Tarea { Id = 3, Titulo = "Tarea 3", Descripcion = "Descripción Tarea 3", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = usuario.UserName, Estado = EstadoTarea.PENDIENTE };

            var result = await _controller.Insert(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Tarea creada exitosamente!", ok_result.Value.GetType().GetProperty("message").GetValue(ok_result.Value));
        }

        
        [Fact]
        public async Task Update()
        {
            var usuario = new ApplicationUser { UserName = "user2", Id = "3" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "User452@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Empleado" });

            var model = new Tarea { Id = 3, Titulo = "Tarea 3", Descripcion = "Descripción Tarea 3 Actualizada!", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = usuario.UserName, Estado = EstadoTarea.PENDIENTE };

            var result = await _controller.Update(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Tarea actualizada exitosamente!", ok_result.Value.GetType().GetProperty("message").GetValue(ok_result.Value));
        }

        
        [Fact]
        public async Task UpdateEstado()
        {
            var usuario = new ApplicationUser { UserName = "user2", Id = "3" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "User452@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Empleado" });

            var model = new Tarea { Id = 3, Titulo = "Tarea 3", Descripcion = "Descripción Tarea 3", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = usuario.UserName, Estado = EstadoTarea.ENPROCESO };

            var result = await _controller.UpdateEstado(model);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Estado de la Tarea actualizado exitosamente!", ok_result.Value.GetType().GetProperty("message").GetValue(ok_result.Value));
        }

        
        [Fact]
        public async Task Delete()
        {
            var usuario = new ApplicationUser { UserName = "user2", Id = "3" };

            _userManagerMock.Setup(um => um.FindByNameAsync(usuario.UserName))
                            .ReturnsAsync(usuario);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(usuario, "User452@"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(usuario))
                            .ReturnsAsync(new List<string> { "Empleado" });

            Tarea tarea = new Tarea { Id = 4, Titulo = "Tarea 4", Descripcion = "Descripción Tarea 4", FechaIni = new DateTime(), FechaFin = new DateTime(), UserName = usuario.UserName, Estado = EstadoTarea.PENDIENTE };
            _db.Tareas.Add(tarea);
            _db.SaveChanges(); ;

            var result = await _controller.Delete(tarea.Id);

            var ok_result = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Tarea eliminada exitosamente!", ok_result.Value.GetType().GetProperty("message").GetValue(ok_result.Value));
        }
        

    }
}
