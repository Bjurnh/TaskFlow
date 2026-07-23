namespace TaskFlow.Domain.Enums;

// NOTE: This type intentionally shares a name with System.Threading.Tasks.TaskStatus.
// Because every async method implicitly pulls in System.Threading.Tasks, a bare
// "TaskStatus" reference inside a file that also has `using System.Threading.Tasks;`
// will resolve to the WRONG type and often fail to compile or bind silently to the BCL enum.
// This is exactly why the entity itself is named `TaskItem` rather than `Task` throughout
// this codebase - it sidesteps the much worse collision (Task vs System.Threading.Tasks.Task)
// and keeps this enum reference unambiguous in practice, since Domain/Application code
// rarely needs to `using System.Threading.Tasks;` explicitly (it's brought in implicitly
// for async method signatures without an explicit using directive in SDK-style projects).
// If you ever DO see a CS0104 ambiguous reference error here, alias it:
//   using DomainTaskStatus = TaskFlow.Domain.Enums.TaskStatus;
public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}
