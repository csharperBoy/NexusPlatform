using Contact.Application.Commands.ContactResource;
using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Domain.Enums;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Domain.Common;
using Core.Domain.ValueObjects;

using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using HR.Domain.Entities;
using HR.Domain.Events.Employment;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Shared.Results;
using Core.Shared.DTOs;
using Contact.Application.Queries;
using Contact.Application.DTOs;

namespace Contact.Infrastructure.Services
{
    public class ContactResourceService : IContactResourceCommandService , IContactResourceQueryService
    {

        private readonly IRepository<ContactDbContext, ContactResource, Guid> _contactResourceRepository;
        private readonly IUnitOfWork<ContactDbContext> _uow;
        public ContactResourceService(IRepository<ContactDbContext, ContactResource, Guid> contactResourceRepository, IUnitOfWork<ContactDbContext> uow)
        {
            _contactResourceRepository = contactResourceRepository;
            _uow = uow;
        }
        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
       
        public async Task<bool> UpdateContactResourceAsync(UpdateContactResourceCommand request)
        {
            bool hasChange = await UpdateAsync(
                  request.Id,
                  request.Value,
                  request.Label,
                  request.ContactType,
                  request.IsPrimary,
                  request.SortOrder,
                  request.RelationType,
                  request.ParentId
                  );
            return hasChange;
        }

        public async Task<Guid> CreateContactResourceAsync(CreateContactResourceCommand request)
        {
            Guid id = await CreateAsync(
                 request.Value,
                 request.Label,
                 request.ContactType,
                 request.IsPrimary,
                 request.SortOrder,
                 request.RelationType,
                 request.ParentId
                );

            return id;

        }

        public async Task DeleteContactResourceAsync(DeleteContactResourceCommand request)
        {
            await DeleteAsync(request.Id);

        }
        private async Task<bool> UpdateAsync(Guid id, Optional<string> value, Optional<string?> label, Optional<ContactTypeEnum> contactType, Optional<bool> isPrimary, Optional<int?> sortOrder,  Optional<ContactRelationTypeEnum?> relationType, Optional<Guid?> parentId)
        {
            ContactResource? entity = await _contactResourceRepository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("can not found contactResource!!!");

            bool hasChange = entity.ApplyChange(value, label,contactType,   isPrimary,  sortOrder,   relationType, parentId);
            if (hasChange)
            {
                await _contactResourceRepository.UpdateAsync(entity);
            }

            return hasChange;
        }
        private async Task<Guid> CreateAsync(string value, string? label, ContactTypeEnum contactType,  bool isPrimary, int? sortOrder,  ContactRelationTypeEnum? relationType, Guid? parentId)
        {
            ContactResource? existEntity = (await _contactResourceRepository.GetAllAsync(queryOptions: q => q.Where(a => a.Value.Trim() == value.Trim()))).FirstOrDefault();
            ContactResource entity;
            if (existEntity == null)
            {
                entity = new ContactResource(contactType,value,label,isPrimary,sortOrder,parentId,relationType);

                await _contactResourceRepository.AddAsync(entity);

                return entity.Id;
            }
            else
            {
                entity = new ContactResource(contactType, value,  label, isPrimary, sortOrder, parentId, relationType);

                existEntity.ApplyChange(entity,
                    new List<string> {
                    "ContactResource.contactType",
                    "ContactResource.value",
                    "ContactResource.label",
                    "ContactResource.isPrimary",
                    "ContactResource.sortOrder",
                    "ContactResource.parentId",
                    "ContactResource.relationType"
                });


                await existEntity.SetIsRemove(false);
                await _contactResourceRepository.UpdateAsync(existEntity);
                return existEntity.Id;
            }
        }
        private async Task DeleteAsync(Guid id)
        {
            ContactResource? model = await _contactResourceRepository.GetByIdAsync(id);
            if (model == null)
                throw new Exception("can not found contactResource!!!");

            await model.SoftRemove();

        }

        public async Task<Result<IList<TreeSelectionListDto>>> GetContactResourcesSelectionListAsync(GetContactResourcesSelectionListQuery request)
        {
            var list = await   _contactResourceRepository.GetAllAsync();
            var result =  list.Select(a => new TreeSelectionListDto(a.Id.ToString(), a.Value));
            return Result<IList<TreeSelectionListDto>>.Ok(result.ToList());
        }

        public async Task<Result<IList<ContactResourceInfoDto>>> GetContactResourceListAsync(GetContactResourceListQuery request)
        {
            var list = await _contactResourceRepository.GetAllAsync();
            var result = list.Select(a => new ContactResourceInfoDto
            {
                Id = a.Id,
                Value = a.Value,
                ContactType = a.ContactType,
                IsPrimary = a.IsPrimary,
                Label = a.Label,
                ParentId = a.ParentId,
                RelationType = a.RelationType,
                SortOrder = a.SortOrder

            });
            return Result<IList<ContactResourceInfoDto>>.Ok(result.ToList());
        }
    }
}
