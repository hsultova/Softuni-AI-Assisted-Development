namespace TodoApp.Models
{
    public class StatisticsViewModel
    {
        public List<DailyCompletionStat> Last7DayStats { get; set; } = [];
        public List<WeeklyCompletionStat> Last4WeeksStats { get; set; } = [];
        public List<ProjectCompletionStat> ProjectStats { get; set; } = [];
        public int TotalCompleted { get; set; }
        public int TotalTodos { get; set; }
        public double OverallCompletionRate => TotalTodos == 0 ? 0 : (double)TotalCompleted / TotalTodos * 100;
    }

    public class DailyCompletionStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class WeeklyCompletionStat
    {
        public string WeekLabel { get; set; } = "";
        public int Count { get; set; }
    }

    public class ProjectCompletionStat
    {
        public string ProjectName { get; set; } = "";
        public int Total { get; set; }
        public int Completed { get; set; }
        public double CompletionRate => Total == 0 ? 0 : Math.Round((double)Completed / Total * 100, 1);
    }
}
