using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.IrisaSync.Extention.Entities;

public partial class PdsIdeaInformationViw 
{
    /// <summary>
    /// کلید اصلی که با کد ملی پر میشود
    /// </summary>
    [Column("COD_NAT_EMPLY")]
    [Key]
    public string Id { get; set; }
    /// <summary>
    /// کد ملی
    /// کلید اصلی برای ما حساب میشه
    /// </summary>
    public string? CodNatEmply => Id;
    /// <summary>
    /// کد پرسنلی
    /// </summary>
    public int NumPrsnEmply { get; set; }
    /// <summary>
    /// نام
    /// </summary>
    public string? NamFirstEmply { get; set; }
    /// <summary>
    /// نام خانوادگی
    /// </summary>
    public string? NamLastEmply { get; set; }
    /// <summary>
    /// نام پدر
    /// </summary>
    public string? NamFathrEmply { get; set; }
    /// <summary>
    /// تاریخ تولد میلادی
    /// </summary>
    public string? DatBirthEmplyEn { get; set; }
    /// <summary>
    /// تاریخ تولد شمسی
    /// </summary>
    public string? DatBirthEmplyPr { get; set; }
    /// <summary>
    /// محل تولد
    /// </summary>
    public string? BirthPlace { get; set; }
    /// <summary>
    /// شماره شناسنامه
    /// </summary>
    public long? NumCrtEmply { get; set; }
    /// <summary>
    /// محل صدور
    /// </summary>
    public string? IssuancePlace { get; set; }
    /// <summary>
    /// جنسیت
    /// </summary>
    public string? DesSexEmply { get; set; }
    /// <summary>
    /// وضعیت تاهل
    /// </summary>
    public string? DesMarriedEmply { get; set; }
    /// <summary>
    /// مذهب
    /// </summary>
    public string? DesReligionEmply { get; set; }
    /// <summary>
    ///کد مدرک تحصیلی
    /// </summary>
    public decimal? CodEducation { get; set; }
    /// <summary>
    /// مدرک تخحصیلی
    /// </summary>
    public string? DesEducation { get; set; }
    /// <summary>
    /// کد رشته تحصیلی
    /// </summary>
    public decimal? CodBranch { get; set; }
    /// <summary>
    /// رشته تحصیلی 
    /// مثال : فناوری اطلاعات
    /// </summary>
    public string? DesBranch { get; set; }
    /// <summary>
    /// کد گرایش تحصیلی
    /// </summary>
    public decimal? CodTndcy { get; set; }
    /// <summary>
    /// گرایش تحصیلی
    /// مثال: مدیریت منابع اطلاعاتی
    /// </summary>
    public string? DesTndcy { get; set; }
    /// <summary>
    /// کد سمت - اشتباه است
    /// </summary>
    public string? CodClassJobpo { get; set; }
    /// <summary>
    /// سمت - اشتباه است
    /// مثال : رئیس
    /// </summary>
    public string? DesClassJobpo { get; set; }

    public decimal? CodFactoryBranch { get; set; }
    /// <summary>
    ///کد عنوان پست
    ///استفاده شده بعنوان گره نهایی در چارت
    /// </summary>
    public decimal? CodJobpo { get; set; }
    /// <summary>
    /// عنوان پست
    /// مثال: رئیس فاوا و امنیت
    /// استفاده شده بعنوان گره نهایی در چارت
    /// </summary>
    public string? DesJobpo { get; set; }
    /// <summary>
    /// کد واحد
    /// </summary>
    public decimal? CodBusun { get; set; }
    /// <summary>
    /// واحد
    /// مثال: خرید
    /// </summary>
    public string? DesBusun { get; set; }
    /// <summary>
    /// کد معاونت
    /// Cod_Moa_Busun
    /// </summary>
    public decimal? CodMoaBusun { get; set; }
    /// <summary>
    /// توضیح معاونت
    /// DES_MOA_BUSUN
    /// مثال: بازرگانی
    /// </summary>
    public decimal? DESMOABUSUN { get; set; }
    
    /// <summary>
    /// کد رده یا شغل
    /// </summary>
    public decimal? CodPosit { get; set; }
    /// <summary>
    /// رده یا شغل
    /// مثال: رئیس
    /// </summary>
    public string? DesPosit { get; set; }

    public string? CodCateJobpo { get; set; }
    /// <summary>
    /// 
    /// مثال : null
    /// </summary>
    public string? DesCodCateJobpo { get; set; }
    /// <summary>
    /// کد نوع استخدام
    /// </summary>
    public bool? CodEmtyp { get; set; }
    /// <summary>
    /// نوع استخدام
    /// مثال: قراردادی
    /// </summary>
    public string? DesEmtyp { get; set; }
    /// <summary>
    /// تاریخ استخدام میلادی
    /// </summary>
    public string? DatEmpltEmplyEn { get; set; }
    /// <summary>
    /// تاریخ استخدام شمسی
    /// </summary>
    public string? DatEmpltEmplyPr { get; set; }
    /// <summary>
    /// کد حالت اشتغال
    /// </summary>
    public byte? CodStaPcond { get; set; }
    /// <summary>
    /// حالت اشتغال
    /// مثال : کارمند جاری
    /// </summary>
    public string? DesStaPcond { get; set; }

    public byte? LevelJobEmply { get; set; }

    public byte? LevelEmply { get; set; }

    public long? NumMobilEmply { get; set; }

    public long? NumTelEmply { get; set; }
    /// <summary>
    /// آدرس ایمیل سازمانی
    /// </summary>
    public string? DesEmailAddresEmply { get; set; }
    /// <summary>
    /// آدرس
    /// </summary>
    public string? DesAdrEmply { get; set; }
    /// <summary>
    /// نوع تقویم
    /// مثال : T
    /// </summary>
    public string? CodCalCalnr { get; set; }
}
