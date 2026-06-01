namespace TodoApp.Models
{
    public enum SortField
    {
        DueDate,
        Priority,
        CreatedAt
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum StatusFilter
    {
        All,
        Pending,
        Completed
    }

    public enum PriorityFilter
    {
        All = -1,
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum ViewMode
    {
        All,
        Today,
        Upcoming,
        Completed
    }
}