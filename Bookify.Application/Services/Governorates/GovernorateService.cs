using Bookify.Domain.Consts;

namespace Bookify.Application.Services
{
    internal class GovernorateService : IGovernorateService
    {
        private readonly IUnitOFWork _unitOFWork;

        public GovernorateService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public IEnumerable<Governorate> GetActiveGovernorates()
        {
            return _unitOFWork.Governorates.FindAll(predicate: g => !g.IsDeleted, orderBy: g => g.Name, orderByDirection: OrderBy.Ascending);
        }
    }
}
