using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class StatisticsController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            var allTodos = await _context.Todos.ToListAsync();
            int totalTodos = allTodos.Count;
            int totalCompleted = allTodos.Count(t => t.IsDone);

            // Last 7 days (today inclusive)
            var sevenDaysAgo = today.AddDays(-6);
            var dailyDict = allTodos
                .Where(t => t.IsDone && t.CreatedAt.Date >= sevenDaysAgo)
                .GroupBy(t => t.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var last7DayStats = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var date = sevenDaysAgo.AddDays(i);
                    return new DailyCompletionStat { Date = date, Count = dailyDict.GetValueOrDefault(date, 0) };
                })
                .ToList();

            // Last 4 weeks (each = 7-day window ending on today, last week, etc.)
            var last4WeeksStats = Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var weekEnd = today.AddDays(-i * 7);
                    var weekStart = weekEnd.AddDays(-6);
                    var count = allTodos.Count(t =>
                        t.IsDone && t.CreatedAt.Date >= weekStart && t.CreatedAt.Date <= weekEnd);
                    var label = i switch { 0 => "This week", 1 => "Last week", _ => $"{i + 1} weeks ago" };
                    return new WeeklyCompletionStat { WeekLabel = label, Count = count };
                })
                .Reverse()
                .ToList();

            // Per-project completion rates
            var projects = await _context.Projects
                .Include(p => p.Todos)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var projectStats = projects
                .Select(p => new ProjectCompletionStat
                {
                    ProjectName = p.Name ?? "Unnamed",
                    Total = p.Todos.Count,
                    Completed = p.Todos.Count(t => t.IsDone)
                })
                .ToList();

            var unassigned = allTodos.Where(t => t.ProjectId == null).ToList();
            if (unassigned.Count > 0)
            {
                projectStats.Add(new ProjectCompletionStat
                {
                    ProjectName = "Unassigned",
                    Total = unassigned.Count,
                    Completed = unassigned.Count(t => t.IsDone)
                });
            }

            ViewData["Title"] = "Statistics";
            return View(new StatisticsViewModel
            {
                Last7DayStats = last7DayStats,
                Last4WeeksStats = last4WeeksStats,
                ProjectStats = projectStats,
                TotalCompleted = totalCompleted,
                TotalTodos = totalTodos
            });
        }
    }
}
