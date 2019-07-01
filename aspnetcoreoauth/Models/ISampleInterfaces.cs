using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcoreoauth.Models
{
    public interface IApplicationDBContext
    {
        DbSet<SampleTestEntity> SampleTestEntity { get; set; }
        DbContext DatabaseContext { get; }
        void Dispose();
    }

    public interface ISampleEntityService
    {
        Task<int> AddSampleTestEntity(SampleTestEntity entity);
        Task<int> DeleteSampleTestEntity(int? id);
        Task<SampleTestEntity> GetSampleTestEntity(int? id);
        Task<List<SampleTestEntity>> GetSampleTestEntityList(Func<SampleTestEntity, bool> whereClause);
        Task<int> UpdateSampleTestEntity(SampleTestEntity entity);
    }
}
