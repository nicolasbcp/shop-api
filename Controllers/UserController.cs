using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{

    [Route("users")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get(
            [FromServices] DataContext context)
        {
            var users = await context
                .Users
                .AsNoTracking()
                .ToListAsync();
            return users;
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post(
            [FromServices] DataContext context,
            [FromBody] User model)
        {
            // Verifica se os dados são válidos
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Força o usuário a ser sempre "funcionário"
                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                // Esconde a senha
                model.Password = "";
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível cadastrar o usuário" });
            }
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromServices] DataContext context,
            [FromBody] User model)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();

            // Verifica se os dados são válidos
            if(user == null)
            {
                return NotFound(new { message = "Usuário ou senha inválidos" });
            }

            var token = TokenService.GenerateToken(user);
            // Esconde a senha
            user.Password = "";
            return new
            {
                user = user,
                token = token
            };
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles="manager")]
        public async Task<ActionResult<User>> Put(
            int id,
            [FromServices] DataContext context,
            [FromBody] User model)
        {
            // Verifica se os dados são válidos
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se o ID informado é o mesmo da model
            if(id != model.Id)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível criar usuário" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles="manager")]
        public async Task<ActionResult<List<User>>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            // Verifica no banco se o usuário requisitada existe
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if(user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

            try
            {
                // Caso exista, prossegue com a exclusão
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return Ok(new { message = "Usuário removido com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover o usuário" });
            }
        } 
    }
}