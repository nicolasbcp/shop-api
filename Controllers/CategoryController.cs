using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("categories")]
    public class CategoryController : ControllerBase
    {

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<Category>>> Get(
            [FromServices] DataContext context)
        {
            // Get com AsNoTracking para fazer rápida consulta ao BD
            // e não trazer informações adicionadas providas pelo EF Core
            var categories = await context.Categories.AsNoTracking().ToListAsync();
            return categories;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Category>> GetById(
            int id,
            [FromServices] DataContext context)
        {
            // Get com AsNoTracking para fazer rápida consulta ao BD
            // e não trazer informações adicionadas providas pelo EF Core
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return new Category();
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<List<Category>>> Post(
            [FromBody] Category model,
            [FromServices] DataContext context
        )
        {
            // Verifica se os dados são válidos
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Cria uma nova categoria e em seguida persiste no BD
                context.Categories.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);    
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar a categoria" });
            }
            
        }
        
        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult<List<Category>>> Put(
            int id, 
            [FromBody] Category model,
            [FromServices] DataContext context)
        {
            // Verifica se o ID informado é o mesmo da model
            if(id != model.Id)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }

            // Verifica se os dados são válidos
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                context.Entry<Category>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado" });
            }
            catch 
            {
                return BadRequest(new { message = "Não foi possível atualizar a categoria" });
            }
        }
        
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult<List<Category>>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            // Verifica no banco se a categoria requisitada existe
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
                if(category == null)
                {
                    return NotFound(new { message = "Categoria não encontrada" });
                }

            try
            {
                // Caso exista, prossegue com a exclusão
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover a categoria" });
            }
        }
    }
}