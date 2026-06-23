using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// گرید کارمند
    /// 
    /// (طبقه / رتبه) - برای «ارزش‌گذاری و حقوق/مزایا»
    /// مثال: طبقه ۱۰ (ویژه مدیران)، طبقه ۷ (ویژه کارشناسان ارشد)، طبقه ۵ (ویژه کارشناسان عادی).
    /// </summary>
    public class Grade : BaseEntity
    {
        public string Code { get; private set; }
        public string Title { get; private set; }
        public bool IsActive { get; private set; }
        protected Grade()
        {
            
        }
    }
}
