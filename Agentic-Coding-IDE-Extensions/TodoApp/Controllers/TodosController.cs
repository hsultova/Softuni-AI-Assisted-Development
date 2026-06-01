using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class TodosController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        private async Task PopulateProjectSelectListAsync(int? selectedProject = null)
        {
            var projects = await _context.Projects.OrderBy(p => p.Name).ToListAsync();
            ViewData["Projects"] = new SelectList(projects, "Id", "Name", selectedProject);
        }

        private async Task<object> BuildTodoJsonAsync(Todo todo)
        {
            Project? project = todo.Project;
            if (project == null && todo.ProjectId.HasValue)
            {
                project = await _context.Projects.FindAsync(todo.ProjectId.Value);
            }

            return new
            {
                success = true,
                todo = new
                {
                    id = todo.Id,
                    title = todo.Title,
                    description = todo.Description,
                    isDone = todo.IsDone,
                               priority = todo.Priority,
                    createdAt = todo.CreatedAt,
                    dueDate = todo.DueDate,
                    project = project is null ? null : new { id = project.Id, name = project.Name }
                }
            };
        }

        // GET: Todos/Index
        public async Task<IActionResult> Index(int? projectId, ViewMode? viewMode = null, StatusFilter? statusFilter = null, PriorityFilter? priorityFilter = null, SortField? sortBy = null, SortDirection? sortDirection = null)
        {
            var today = DateTime.Today;
            var todosQuery = _context.Todos.Include(t => t.Project).AsQueryable();

            if (projectId.HasValue)
            {
                todosQuery = todosQuery.Where(t => t.ProjectId == projectId.Value);
            }

            if (statusFilter.HasValue && statusFilter != StatusFilter.All)
            {
                bool isDoneFilter = statusFilter == StatusFilter.Completed;
                todosQuery = todosQuery.Where(t => t.IsDone == isDoneFilter);
            }

            if (priorityFilter.HasValue && priorityFilter != PriorityFilter.All)
            {
                var priorityValue = (Priority)priorityFilter.Value;
                todosQuery = todosQuery.Where(t => t.Priority == priorityValue);
            }

            switch (viewMode)
            {
                case ViewMode.Today:
                    todosQuery = todosQuery.Where(t => !t.IsDone && (t.DueDate.HasValue ? t.DueDate.Value.Date <= today : true));
                    break;
                case ViewMode.Upcoming:
                    todosQuery = todosQuery.Where(t => !t.IsDone && t.DueDate.HasValue && t.DueDate.Value.Date > today);
                    break;
                case ViewMode.Completed:
                    todosQuery = todosQuery.Where(t => t.IsDone);
                    break;
            }

            bool descending = !sortDirection.HasValue || sortDirection == SortDirection.Descending;

            todosQuery = sortBy switch
            {
                SortField.DueDate => descending
                    ? todosQuery.OrderBy(t => t.DueDate.HasValue ? 0 : 1).ThenByDescending(t => t.DueDate)
                    : todosQuery.OrderBy(t => t.DueDate.HasValue ? 0 : 1).ThenBy(t => t.DueDate),
                SortField.Priority => descending 
                    ? todosQuery.OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt) 
                    : todosQuery.OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt),
                SortField.CreatedAt => descending 
                    ? todosQuery.OrderByDescending(t => t.CreatedAt) 
                    : todosQuery.OrderBy(t => t.CreatedAt),
                _ => descending 
                    ? todosQuery.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ThenByDescending(t => t.CreatedAt) 
                    : todosQuery.OrderBy(t => t.Priority).ThenBy(t => t.DueDate).ThenBy(t => t.CreatedAt)
            };

            var todos = await todosQuery.ToListAsync();
            await PopulateProjectSelectListAsync(projectId);
            ViewData["SelectedProjectId"] = projectId;
            ViewData["ViewMode"] = viewMode;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["PriorityFilter"] = priorityFilter;
            ViewData["SortBy"] = sortBy;
            ViewData["SortDirection"] = sortDirection;
            ViewData["Title"] = GetViewModeTitle(viewMode);
            return View(todos);
        }

        private static string GetViewModeTitle(ViewMode? viewMode) => viewMode switch
        {
            ViewMode.Today => "Today's Tasks",
            ViewMode.Upcoming => "Upcoming Tasks",
            ViewMode.Completed => "Completed Tasks",
            _ => "All Tasks"
        };

        // GET: Todos/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProjectSelectListAsync();
            return View();
        }

        // POST: Todos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate,Priority,ProjectId")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                todo.CreatedAt = DateTime.UtcNow;
                todo.IsDone = false;
                _context.Add(todo);
                await _context.SaveChangesAsync();

                if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
                {
                    return Json(await BuildTodoJsonAsync(todo));
                }

                TempData["Success"] = "Todo created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateProjectSelectListAsync(todo.ProjectId);
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

            await PopulateProjectSelectListAsync(todo.ProjectId);
            ViewBag.Projects = ViewData["Projects"];
            return View(todo);
        }

        // POST: Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IsDone,CreatedAt,DueDate,Priority,ProjectId")] Todo todo)
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

                if (Request.Headers.TryGetValue("X-Requested-With", out var header) && header == "XMLHttpRequest")
                {
                    return Json(await BuildTodoJsonAsync(todo));
                }

                TempData["Success"] = "Todo updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateProjectSelectListAsync(todo.ProjectId);
            ViewBag.Projects = ViewData["Projects"];
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
