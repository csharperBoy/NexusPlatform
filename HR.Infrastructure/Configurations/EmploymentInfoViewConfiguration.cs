using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class EmploymentInfoViewConfiguration : BaseConfiguration<EmploymentInfoView>
    {
        public override void Configure(EntityTypeBuilder<EmploymentInfoView> builder)
        {
            //base.Configure(builder);


            builder
               .HasNoKey()
               .ToView("Employment_Info_View", "hr");

            builder.Property(e => e.AssignmentsAssigneeType).HasColumnName("Assignments_AssigneeType");
            builder.Property(e => e.AssignmentsEffectiveFrom).HasColumnName("Assignments_EffectiveFrom");
            builder.Property(e => e.AssignmentsEffectiveTo).HasColumnName("Assignments_EffectiveTo");
            builder.Property(e => e.CostCenterName).HasColumnName("CostCenter_Name");
            builder.Property(e => e.EmploymentEffectiveFrom).HasColumnName("Employment_EffectiveFrom");
            builder.Property(e => e.EmploymentEffectiveTo).HasColumnName("Employment_EffectiveTo");
            builder.Property(e => e.EmploymentStatusName).HasColumnName("Employment_Status_Name");
            builder.Property(e => e.EmploymentTypeName).HasColumnName("Employment_Type_Name");
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.GradeTitle).HasColumnName("Grade_Title");
            builder.Property(e => e.JobLevelTitle).HasColumnName("JobLevel_Title");
            builder.Property(e => e.JobTitleName).HasColumnName("JobTitle_Name");
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.NationalCode).HasMaxLength(10);
            builder.Property(e => e.OrganizationUnitsName)
                    .HasMaxLength(200)
                    .HasColumnName("OrganizationUnits_Name");
            builder.Property(e => e.PartyId).HasColumnName("Party_Id");
            builder.Property(e => e.PostCode).HasColumnName("Post_Code");
        }
    }
}
