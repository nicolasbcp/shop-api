using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{

    [Route("products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get(
            [FromServices] DataContext context)
        {
            // Get com AsNoTracking para fazer rápida consulta ao BD
            // e não trazer informações adicionadas providas pelo EF Core
            // Apesar do custo de performance, Include foi utilizado pois
            // selecionaremos a categoria em sua totalidade
            var products = await context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
            return products;
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(
            int id,
            [FromServices] DataContext context)
        {
            // Get com AsNoTracking para fazer rápida consulta ao BD
            // e não trazer informações adicionadas providas pelo EF Core
            // Apesar do custo de performance, Include foi utilizado pois
            // selecionaremos a categoria em sua totalidade
            var product = await context.Products
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            return product;
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(
            int id,
            [FromServices] DataContext context)
        {
            // Get dos produtos filtrados por suas Categorias   
            var products = await context.Products
                .Include(x => x.Category)
                .AsNoTracking()
                .Where(x => x.CategoryId == id)
                .ToListAsync();

            return products;
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post(
            [FromServices] DataContext context,
            [FromBody] Product model)
        {
            if(ModelState.IsValid)
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return model;
            }
            else 
            {
                return BadRequest(ModelState);  
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<Product>> Put(
            int id,
            [FromServices] DataContext context,
            [FromBody] Product model)
        {
            // Verifica se os dados são válidos
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se o ID informado é o mesmo da model
            if(id != model.Id)
            {
                return NotFound(new { message = "Produto não encontrado" });
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
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<Product>>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            // Verifica no banco se o usuário requisitada existe
            var product = await context.Products
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if(product == null)
                {
                    return NotFound(new { message = "Produto não encontrado" });
                }

            try
            {
                // Caso exista, prossegue com a exclusão
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new { message = "Produto removido com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover o produto" });
            }
        }
    }
}