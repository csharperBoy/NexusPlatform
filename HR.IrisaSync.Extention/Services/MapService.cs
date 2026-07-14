using Core.Application.Abstractions;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Data;
using HR.IrisaSync.Extention.Contexts;
using HR.IrisaSync.Extention.Data;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Services
{
    public class MapService : IMapService
    {

        private readonly IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> _irisaRepo;
        private readonly IIrisaSyncUnitOfWork<IrisaExtentionDbContext> _uow;
        private readonly IHRUnitOfWork<HRDbContext> _hrUow;
        public MapService(IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> irisaRepo,
            IIrisaSyncUnitOfWork<IrisaExtentionDbContext> uow, IHRUnitOfWork<HRDbContext> hrUow
        )
        {
            _uow = uow;
            _hrUow = hrUow;
            _irisaRepo = irisaRepo;
        }

        /// <summary>
        /// پر کردن جدول مپ با داده های موجود در ویو ایریسا
        /// </summary>
        /// <returns></returns>
        public async Task FillJobTitleMap()
        {
            IEnumerable<PdsIdeaInformationViw> irisaList = await _irisaRepo.GetAllAsync();
            var lst = irisaList
                     .Where(e => e.CodEmtyp == true)
                     .GroupBy(a => a.CodJobpo)
                     .Select(group => new JobTitleMap(group.Key, group.First().DesJobpo, group.Count()))//$"{group.Count().ToString()} - {group.First().DesJobpo}"))                    
                     .ToList();
            var existlist = await _uow.JobTitleMapRepository.GetAllAsync();
            foreach (var item in lst)
            {
                var existEntity = existlist.Where(a => a.IrisaJobTitleId == item.IrisaJobTitleId).SingleOrDefault();
                if (existEntity != null)
                {
                    if (existEntity.IrisaJobTitle.Trim() != item.IrisaJobTitle.Trim())
                    {
                        existEntity.IrisaJobTitle =item.IrisaJobTitle;
                        await _uow.JobTitleMapRepository.UpdateAsync(existEntity);
                    }
                }
                else
                {
                    await _uow.JobTitleMapRepository.AddAsync(item);

                }
            }
            await _uow.SaveChangesAsync();

        }

        /// <summary>
        /// پر کردن جدول مپ با داده های موجود در ویو ایریسا
        /// </summary>
        /// <returns></returns>
        public async Task FillJobLevelMap()
        {
            IEnumerable<PdsIdeaInformationViw> irisaList = await _irisaRepo.GetAllAsync();
            var lst = irisaList
                     .Where(e => e.CodEmtyp == true)
                     .GroupBy(a => a.CodPosit)
                     .Select(group => new JobLevelMap(group.Key, group.First().DesPosit))
                     .ToList();
            var existlist = await _uow.JobLevelMapRepository.GetAllAsync();
            foreach (var item in lst)
            {
                var existEntity = existlist.Where(a => a.IrisaJobLevelId == item.IrisaJobLevelId).SingleOrDefault();
                if (existEntity != null)
                {
                    if (existEntity.IrisaJobLevel.Trim() != item.IrisaJobLevel.Trim())
                    {
                        existEntity.IrisaJobLevel = item.IrisaJobLevel;
                        await _uow.JobLevelMapRepository.UpdateAsync(existEntity);
                    }
                }
                else
                {
                    await _uow.JobLevelMapRepository.AddAsync(item);

                }
            }
            await _uow.SaveChangesAsync();

        }

        /// <summary>
        /// پر کردن جدول مپ با داده های موجود در ویو ایریسا
        /// </summary>
        /// <returns></returns>
        public async Task FillOrganizationUnitMap()
        {
            IEnumerable<PdsIdeaInformationViw> irisaList = await _irisaRepo.GetAllAsync();
            var lst = irisaList
                     .Where(e => e.CodEmtyp == true)
                     .GroupBy(a => a.CodBusun)
                     .Select(group => new OrganizationUnitMap(group.Key, group.First().DesBusun))
                     .ToList();
            var existlist = await _uow.OrganizationUnitMapRepository.GetAllAsync();
            foreach (var item in lst)
            {
                var existEntity = existlist.Where(a => a.IrisaOrganizationUnitId == item.IrisaOrganizationUnitId).SingleOrDefault();
                if (existEntity != null)
                {
                    if (existEntity.IrisaOrganizationUnit.Trim() != item.IrisaOrganizationUnit.Trim())
                    {
                        existEntity.IrisaOrganizationUnit = item.IrisaOrganizationUnit;
                        await _uow.OrganizationUnitMapRepository.UpdateAsync(existEntity);
                    }
                }
                else
                {
                    await _uow.OrganizationUnitMapRepository.AddAsync(item);

                }
            }
            await _uow.SaveChangesAsync();

        }

    }
}
