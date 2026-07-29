using Microsoft.EntityFrameworkCore;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PCL.Modules.TaskPlanning.Infrastructure.Database;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Infrastructure.Tasks
{
    public sealed class TaskRepository(TaskPlanningDbContext context) : ITaskRepository
    {
        public void Add(PlanningTask task)
        {
            context.Tasks.Add(task);
        }

        public async System.Threading.Tasks.Task<PlanningTask?> GetAsync(
            TaskId id,
            CancellationToken cancellationToken = default)
        {
            return await context.Tasks.SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
        }
    }
}
