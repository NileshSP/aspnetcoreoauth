using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sampleaspnet.Models
{
    public interface IApplicationDBContext
    {
        DbSet<EThorTestEntity> EThorTestEntity { get; set; }
        DbContext DatabaseContext { get; }
        void Dispose();
    }

    public interface IEThorEntityService
    {
        Task<int> AddEThorTestEntity(EThorTestEntity entity);
        Task<int> DeleteEThorTestEntity(int? id);
        Task<EThorTestEntity> GetEThorTestEntity(int? id);
        Task<List<EThorTestEntity>> GetEThorTestEntityList(Func<EThorTestEntity, bool> whereClause);
        Task<int> UpdateEThorTestEntity(EThorTestEntity entity);
    }
}
