using System.Collections.Generic;
using Common.Application.Messaging;
using PCL.Modules.Session.Application.LSessions.GetLearningSession;

namespace PCL.Modules.Session.Application.LSessions.ListLearningSessions
{
    public record ListLearningSessionsQuery() : IQuery<IEnumerable<LSessionDto>>;
}