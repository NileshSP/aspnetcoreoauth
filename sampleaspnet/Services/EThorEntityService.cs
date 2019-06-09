using Microsoft.EntityFrameworkCore;
using sampleaspnet.Data;
using sampleaspnet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sampleaspnet.Services
{
    public class EThorEntityService : IEThorEntityService
    {
        private IApplicationDBContext _dbContext;
        public EThorEntityService(IApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EThorTestEntity>> GetEThorTestEntityList(Func<EThorTestEntity, bool> whereClause)
            => await _dbContext.EThorTestEntity.Where(whereClause).ToAsyncEnumerable().ToList<EThorTestEntity>();

        public async Task<EThorTestEntity> GetEThorTestEntity(int? id) 
            => await _dbContext.EThorTestEntity.FirstOrDefaultAsync(m => m.Id == id);

        public async Task<int> AddEThorTestEntity(EThorTestEntity entity) 
            => await _dbContext.DatabaseContext.Add(entity).Context.SaveChangesAsync();

        public async Task<int> UpdateEThorTestEntity(EThorTestEntity entity) 
            => await _dbContext.DatabaseContext.Update(entity).Context.SaveChangesAsync();

        public async Task<int> DeleteEThorTestEntity(int? id) 
            => await _dbContext.DatabaseContext.Remove(await GetEThorTestEntity(id)).Context.SaveChangesAsync();
    }
}
