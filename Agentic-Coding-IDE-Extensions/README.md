# Todo App — AI-Assisted Development Exercise

A task management web application built as part of the SoftUni "AI for Developers" course, demonstrating AI-assisted development with agentic coding IDE extensions.

## Features

- **Todo management** — create, edit, delete, and toggle completion of tasks
- **Project management** — organize todos into projects with full CRUD support
- **View modes** — All Tasks, Today, Upcoming, Completed
- **Filtering** — filter by status (pending/completed), priority, and project
- **Sorting** — sort by due date, priority, or created date in ascending or descending order
- **AJAX interactions** — create, edit, delete, and toggle without full page reloads
- **Statistics dashboard** — daily and weekly completion charts, per-project progress bars
- **Responsive UI** — Bootstrap 5 layout with toast notifications and priority color indicators

## Tech Stack

**Backend**
- ASP.NET Core (.NET 10), C#
- Entity Framework Core 9 with SQLite

**Frontend**
- Bootstrap 5
- jQuery + jQuery Validation
- Chart.js 4.4

## Project Structure

```
TodoApp/
├── Controllers/        # HomeController, TodosController, ProjectsController, StatisticsController
├── Models/             # Todo, Project, Priority enum, ViewMode/filter/sort enums, StatisticsViewModel
├── Data/               # AppDbContext (EF Core)
├── Migrations/         # 3 EF Core migrations
├── Views/              # Razor templates for each controller
└── wwwroot/            # Static assets (CSS, JS, Bootstrap, jQuery)
```

## Getting Started

**Prerequisites:** .NET 10 SDK

```bash
dotnet restore
dotnet ef database update
dotnet run
```

App runs at `http://localhost:5237` (HTTP) or `https://localhost:7070` (HTTPS).
