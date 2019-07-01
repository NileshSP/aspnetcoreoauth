using Microsoft.EntityFrameworkCore;
using aspnetcoreoauth.Data;
using aspnetcoreoauth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcoreoauth.Services
{
    public class SampleEntityService : ISampleEntityService
    {
        private IApplicationDBContext _dbContext;
        public SampleEntityService(IApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SampleTestEntity>> GetSampleTestEntityList(Func<SampleTestEntity, bool> whereClause)
            => await _dbContext.SampleTestEntity.Where(whereClause).ToAsyncEnumerable().ToList<SampleTestEntity>();

        public async Task<SampleTestEntity> GetSampleTestEntity(int? id) 
            => await _dbContext.SampleTestEntity.FirstOrDefaultAsync(m => m.Id == id);

        public async Task<int> AddSampleTestEntity(SampleTestEntity entity) 
            => await _dbContext.DatabaseContext.Add(entity).Context.SaveChangesAsync();

        public async Task<int> UpdateSampleTestEntity(SampleTestEntity entity) 
            => await _dbContext.DatabaseContext.Update(entity).Context.SaveChangesAsync();

        public async Task<int> DeleteSampleTestEntity(int? id) 
            => await _dbContext.DatabaseContext.Remove(await GetSampleTestEntity(id)).Context.SaveChangesAsync();
    }
}
