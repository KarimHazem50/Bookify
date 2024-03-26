using Bookify.Domain.Consts;

namespace Bookify.Application.Services
{
    internal class AreaService : IAreaService
    {
        private readonly IUnitOFWork _unitOFWork;

        public AreaService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public IEnumerable<Area> GetActiveAreasByGovernorateId(int governorateId)
        {
            return _unitOFWork.Areas.FindAll(predicate: a => !a.IsDeleted && a.GovernorateId == governorateId, orderBy: g => g.Name, orderByDirection: OrderBy.Ascending);
        }
    }
}
