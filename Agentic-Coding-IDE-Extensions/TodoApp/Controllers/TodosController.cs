using Microsoft.AspNetCore.Mvc;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class TodosController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // GET: Todos/Index
        public async Task<IActionResult> Index()
        {
            var todos = _context.Todos.OrderBy(t => t.CreatedAt).ToList();
            return View(todos);
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                todo.CreatedAt = DateTime.UtcNow;
                todo.IsDone = false;
                _context.Add(todo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Todo created successfully.";
                if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
                {
                    return Json(new { success = true, todo = new { id = todo.Id, title = todo.Title, description = todo.Description, isDone = todo.IsDone, createdAt = todo.CreatedAt, dueDate = todo.DueDate } });
                }
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return View(todo);
        }

        // POST: Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IsDone,CreatedAt,DueDate")] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the todo.");
                }
                TempData["Success"] = "Todo updated successfully.";
                if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
                {
                    return Json(new { success = true, todo = new { id = todo.Id, title = todo.Title, description = todo.Description, isDone = todo.IsDone, createdAt = todo.CreatedAt, dueDate = todo.DueDate } });
                }
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Todo deleted.";
            if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
            {
                return Json(new { success = true, id });
            }
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Todos/ToggleComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsDone = !todo.IsDone;
            _context.Update(todo);
            await _context.SaveChangesAsync();

            TempData["Success"] = todo.IsDone ? "Todo marked completed." : "Todo marked pending.";

            if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
            {
                return Json(new { success = true, isDone = todo.IsDone });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
