using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoneBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class PostInfoViewConfiguration : BaseConfiguration<PostInfoView>
    {
        public override void Configure(EntityTypeBuilder<PostInfoView> builder)
        {
            //base.Configure(builder);


            builder
                .HasNoKey()
                    .ToView("Post_Info_View", "hr");

                builder.Property(e => e.AssignmentsAssigneeType).HasColumnName("Assignments_AssigneeType");
                builder.Property(e => e.CostCenterName).HasColumnName("CostCenter_Name");
                builder.Property(e => e.FirstName).HasMaxLength(100);
                builder.Property(e => e.GradeTitle).HasColumnName("Grade_Title");
                builder.Property(e => e.JobLevelTitle).HasColumnName("JobLevel_Title");
                builder.Property(e => e.JobTitleName).HasColumnName("JobTitle_Name");
                builder.Property(e => e.LastName).HasMaxLength(100);
                builder.Property(e => e.NationalCode).HasMaxLength(10);
                builder.Property(e => e.OrganizationUnitsName)
                    .HasMaxLength(200)
                    .HasColumnName("OrganizationUnits_Name");
            builder.Property(e => e.PostCode).HasColumnName("Post_Code");
           
            /*
            builder
                 .HasNoKey()
               .ToView("Post_Info_View", "hr")
              ;

            builder.Property(e => e.AssignmentsAssigneeType).HasColumnName("Assignments_AssigneeType");
            builder.Property(e => e.CostCenterName).HasColumnName("CostCenter_Name");
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.GradeTitle).HasColumnName("Grade_Title");
            builder.Property(e => e.JobLevelTitle).HasColumnName("JobLevel_Title");
            builder.Property(e => e.JobTitleName).HasColumnName("JobTitle_Name");
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.NationalCode).HasMaxLength(10);
            builder.Property(e => e.OrganizationUnitsName)
                    .HasMaxLength(200)
                    .HasColumnName("OrganizationUnits_Name");
            builder.Property(e => e.PostCode).HasColumnName("Post_Code");
            builder.Property(e => e.OrgEmail).HasColumnName("OrgEmail");
            builder.Property(e => e.OrgMobile).HasColumnName("OrgMobile");
            builder.Property(e => e.OfficePhone).HasColumnName("OfficePhone");*/
        }
    }

}
