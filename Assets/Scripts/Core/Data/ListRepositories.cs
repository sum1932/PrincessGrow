using System.Collections.Generic;
using System.Linq;
using DessertKingdom.Core.Domain;

namespace DessertKingdom.Core.Data
{
    /// <summary>
    /// 메모리 기반 활동 저장소 (샘플 데이터용)
    /// </summary>
    public class ListActivityRepository : IActivityRepository
    {
        private readonly List<Activity> _activities;
        
        public ListActivityRepository(List<Activity> activities)
        {
            _activities = activities ?? new List<Activity>();
        }
        
        public List<Activity> GetAll() => _activities;
        
        public Activity GetById(string id) => 
            _activities.FirstOrDefault(a => a.Id == id);
        
        public List<Activity> GetByType(ActivityType type) => 
            _activities.Where(a => a.Type == type).ToList();
        
        public List<Activity> GetAvailable(int age) => 
            _activities.Where(a => a.MinAge <= age).ToList();
    }
    
    /// <summary>
    /// 메모리 기반 이벤트 저장소 (샘플 데이터용)
    /// </summary>
    public class ListEventRepository : IEventRepository
    {
        private readonly List<GameEvent> _events;
        
        public ListEventRepository(List<GameEvent> events)
        {
            _events = events ?? new List<GameEvent>();
        }
        
        public List<GameEvent> GetAll() => _events;
        
        public GameEvent GetById(string id) => 
            _events.FirstOrDefault(e => e.Id == id);
        
        public List<GameEvent> GetByType(EventType type) => 
            _events.Where(e => e.Type == type).ToList();
    }
    
    /// <summary>
    /// 메모리 기반 엔딩 저장소 (샘플 데이터용)
    /// </summary>
    public class ListEndingRepository : IEndingRepository
    {
        private readonly List<EndingCondition> _endings;
        
        public ListEndingRepository(List<EndingCondition> endings)
        {
            _endings = endings ?? new List<EndingCondition>();
        }
        
        public List<EndingCondition> GetAll() => _endings;
        
        public EndingCondition GetById(string id) => 
            _endings.FirstOrDefault(e => e.Id == id);
        
        public List<EndingCondition> GetByType(bool isHidden) => 
            _endings.Where(e => e.IsHidden == isHidden).ToList();
    }
}
