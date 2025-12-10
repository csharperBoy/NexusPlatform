using Authorization.Application.Commands;
using Authorization.Application.Commands.Resource;
using Authorization.Application.DTOs.Resource;
using Authorization.Application.Queries.Resource;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
namespace Authorization.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/resources")]
    [Authorize(Policy = "RequireAdminRole")]
    public class ResourcesController : BaseController
    {
        /// <summary>
        /// 📋 دریافت درخت کامل منابع
        /// </summary>
        [HttpGet("tree")]
        [AuthorizeResource("authorization.resources", "View")]
        public async Task<IActionResult> GetResourceTree([FromQuery] Guid? rootId = null)
        {
            var query = new GetResourceTreeQuery(rootId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 🔍 دریافت منبع براساس ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [AuthorizeResource("authorization.resources", "View")]
        public async Task<IActionResult> GetResourceById(Guid id)
        {
            var query = new GetResourceByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 🔑 دریافت منبع براساس کلید
        /// </summary>
        [HttpGet("key/{key}")]
        [AuthorizeResource("authorization.resources", "View")]
        public async Task<IActionResult> GetResourceByKey(string key)
        {
            var query = new GetResourceByKeyQuery(key);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// 🆕 ایجاد منبع جدید
        /// </summary>
        [HttpPost]
        [AuthorizeResource("authorization.resources", "Create")]
        public async Task<IActionResult> CreateResource([FromBody] CreateResourceCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// ✏️ به‌روزرسانی منبع
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizeResource("authorization.resources", "Edit")]
        public async Task<IActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }

        /// <summary>
        /// 🗑️ حذف منبع
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("authorization.resources", "Delete")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var command = new DeleteResourceCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// 📊 دریافت منابع براساس دسته‌بندی
        /// </summary>
        [HttpGet("category/{category}")]
        [AuthorizeResource("authorization.resources", "View")]
        public async Task<IActionResult> GetResourcesByCategory(
            string category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // ⚠️ نیاز به کوئری جدید - فعلاً از GetResourceTreeQuery استفاده می‌کنیم
            var query = new GetResourceTreeQuery();
            var result = await Mediator.Send(query);

            if (result.Succeeded && result.Data != null)
            {
                // فیلتر براساس دسته‌بندی
                var filteredResources = FilterResourcesByCategory(result.Data, category);
                // صفحه‌بندی
                var pagedResult = PaginateResources(filteredResources, page, pageSize);
                return Ok(pagedResult);
            }

            return HandleResult(result);
        }

        /// <summary>
        /// 📱 دریافت منابع براساس نوع
        /// </summary>
        [HttpGet("type/{type}")]
        [AuthorizeResource("authorization.resources", "View")]
        public async Task<IActionResult> GetResourcesByType(
            string type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // ⚠️ نیاز به کوئری جدید - فعلاً از GetResourceTreeQuery استفاده می‌کنیم
            var query = new GetResourceTreeQuery();
            var result = await Mediator.Send(query);

            if (result.Succeeded && result.Data != null)
            {
                // فیلتر براساس نوع
                var filteredResources = FilterResourcesByType(result.Data, type);
                // صفحه‌بندی
                var pagedResult = PaginateResources(filteredResources, page, pageSize);
                return Ok(pagedResult);
            }

            return HandleResult(result);
        }

        // ========== APIهای نیازمند توسعه (کامنت شده) ==========

        /*
        /// <summary>
        /// 🌳 تغییر والد یک منبع
        /// </summary>
        [HttpPatch("{id:guid}/parent")]
        [AuthorizeResource("authorization.resources", "Edit")]
        public async Task<IActionResult> ChangeParent(Guid id, [FromBody] ChangeParentRequest request)
        {
            // ⚠️ نیاز به کامند جدید: ChangeResourceParentCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// ⚙️ فعال/غیرفعال کردن منبع
        /// </summary>
        [HttpPatch("{id:guid}/active")]
        [AuthorizeResource("authorization.resources", "Edit")]
        public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveRequest request)
        {
            // ⚠️ نیاز به کامند جدید: ToggleResourceActiveCommand
            return BadRequest("این API در حال توسعه است");
        }

        /// <summary>
        /// 🔄 بازسازی درخت منابع
        /// </summary>
        [HttpPost("rebuild-tree")]
        [AuthorizeResource("authorization.resources", "Admin")]
        public async Task<IActionResult> RebuildTree()
        {
            // ⚠️ نیاز به کامند جدید: RebuildResourceTreeCommand
            return BadRequest("این API در حال توسعه است");
        }
        */

        // ========== متدهای کمکی ==========

        private List<ResourceDto> FilterResourcesByCategory(IReadOnlyList<ResourceTreeDto> forest, string category)
        {
            var all = FlattenForest(forest);
            return all.Where(r => r.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<ResourceDto> FilterResourcesByType(IReadOnlyList<ResourceTreeDto> forest, string type)
        {
            var all = FlattenForest(forest);
            return all.Where(r => r.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        private List<ResourceDto> FlattenForest(IReadOnlyList<ResourceTreeDto> forest)
        {
            var result = new List<ResourceDto>();
            foreach (var root in forest)
            {
                FlattenTree(root, result);
            }
            return result;
        }

        private void FlattenTree(ResourceTreeDto node, List<ResourceDto> result)
        {
            result.Add(new ResourceDto
            {
                Id = node.Id,
                Key = node.Key,
                Name = node.Name,
                Description = node.Description,
                Type = node.Type,
                Category = node.Category,
                ParentId = node.ParentId,
                ParentKey = node.ParentKey,
                IsActive = node.IsActive,
                DisplayOrder = node.DisplayOrder,
                Icon = node.Icon,
                Route = node.Route,
                Path = node.Path,
                CreatedAt = node.CreatedAt,
                CreatedBy = node.CreatedBy,
                ModifiedAt = node.ModifiedAt,
                ModifiedBy = node.ModifiedBy
            });

            if (node.Children is { Count: > 0 })
            {
                foreach (var child in node.Children)
                {
                    FlattenTree(child, result);
                }
            }
        }

        private object PaginateResources(List<ResourceDto> resources, int page, int pageSize)
        {
            var totalCount = resources.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedResources = resources
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new
            {
                Data = pagedResources,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            };
        }
    }

    // ========== DTOهای درخواست (برای APIهای آینده) ==========

    /*
    public class ChangeParentRequest
    {
        public Guid? NewParentId { get; set; }
    }

    public class ToggleActiveRequest
    {
        public bool IsActive { get; set; }
    }
    */
}
