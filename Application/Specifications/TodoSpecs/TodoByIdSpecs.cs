using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.TodoSpecs
{
    public class TodoByIdSpecs : BaseSpecification<Todo>

    {
        public TodoByIdSpecs(int id, int userId, bool isAdmin)
            : base(x => x.Id == id && (isAdmin || x.UserId == userId))
        {
            AddInclude(x => x.Category);
            AddInclude(x => x.User);
            AddInclude(x => x.Attachments);
        }
    }
}
