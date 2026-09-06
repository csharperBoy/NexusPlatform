using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            if (result.Data == null)
            {
                return NoContent();
            }

            return Ok(result.Data);
        }

        protected IActionResult HandleResult(Result result)
        {
            return result.Succeeded ? Ok() : BadRequest(result.Error);
        }


        // ===== متدهای جدید برای BatchResult =====

        /// <summary>
        /// مدیریت نتیجهٔ دسته‌ای بدون داده (غیرجنریک)
        /// </summary>
        protected IActionResult HandleBatchResult(BatchResult result)
        {
            if (!result.Succeeded)
            {
                // استخراج پیام خطای کلی (اگر موجود نبود، پیام پیش‌فرض)
                var errorMessage = result.Errors?.FirstOrDefault() ?? "Batch operation failed.";
                return BadRequest(errorMessage);
            }

            // در حالت موفقیت، کل شیء را برمی‌گردانیم (شامل پیام‌ها و خطاهای جزئی)
            return Ok(result);
        }

        /// <summary>
        /// مدیریت نتیجهٔ دسته‌ای با داده (جنریک)
        /// </summary>
        protected IActionResult HandleBatchResult<T>(BatchResult<T> result)
        {
            if (!result.Succeeded)
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "Batch operation failed.";
                return BadRequest(errorMessage);
            }

            // گزینه ۱: همیشه کل شیء را برگردان (برای مشاهده پیام‌ها)
            return Ok(result);

            // گزینه ۲: اگر داده وجود دارد، داده را برگردان، در غیر این صورت NoContent
            // (اگر می‌خواهید مشابه Result<T> رفتار کنید، این بخش را فعال کنید)
            // return result.Data == null ? NoContent() : Ok(result);
        }

    }
}
