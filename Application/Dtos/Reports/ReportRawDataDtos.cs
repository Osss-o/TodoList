namespace Application.Dtos.Reports
{
    public class UserTodosData
    {
        public string UserName { get; set; }
        public IEnumerable<Domain.Entities.Todo> Todos { get; set; }
    }
    public class CategoryTodosData
    {
        public string CategoryName { get; set; }
        public string CategoryOwner { get; set; }
        public IEnumerable<Domain.Entities.Todo> Todos { get; set; }
    }
}
