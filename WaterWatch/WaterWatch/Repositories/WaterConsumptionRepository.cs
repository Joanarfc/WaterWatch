using Microsoft.EntityFrameworkCore;
using WaterWatch.Data;
using WaterWatch.Models;

namespace WaterWatch.Repositories
{
    public class WaterConsumptionRepository : IWaterConsumptionRepository
    {
        private readonly IDataContext _context;

        public WaterConsumptionRepository(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WaterConsumption>> GetAll()
        {
            return await _context.Consumptions.ToListAsync();
        }

        public async Task<IEnumerable<WaterConsumption>> GetTopTenConsumers()
        {
            var q = _context.Consumptions
            .OrderByDescending(avgKL => avgKL.averageMonthlyKL)
            .Take(10)
            .ToListAsync();

            return await q;
        }
    }
}