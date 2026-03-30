using System.Collections.Generic;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    public interface IActivityRepository
    {
        Activity GetById(string id);
        List<Activity> GetAll();
        List<Activity> GetAvailable(int age, CharacterStats stats);
        List<Activity> GetByType(ActivityType type);
    }

    public interface IEventRepository
    {
        GameEvent GetById(string id);
        List<GameEvent> GetAll();
        List<GameEvent> GetByType(EventType type);
    }

    public interface IEndingRepository
    {
        EndingCondition GetById(string id);
        List<EndingCondition> GetAll();
    }
}
