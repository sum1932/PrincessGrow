using System.Collections.Generic;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// 활동 데이터 저장소 인터페이스
    /// </summary>
    public interface IActivityRepository
    {
        List<Activity> GetAll();
        Activity GetById(string id);
        List<Activity> GetByType(ActivityType type);
        List<Activity> GetAvailable(int age);
    }
    
    /// <summary>
    /// 이벤트 데이터 저장소 인터페이스
    /// </summary>
    public interface IEventRepository
    {
        List<GameEvent> GetAll();
        GameEvent GetById(string id);
        List<GameEvent> GetByType(EventType type);
    }
    
    /// <summary>
    /// 엔딩 데이터 저장소 인터페이스
    /// </summary>
    public interface IEndingRepository
    {
        List<EndingCondition> GetAll();
        EndingCondition GetById(string id);
        List<EndingCondition> GetByType(bool isHidden);
    }
}
