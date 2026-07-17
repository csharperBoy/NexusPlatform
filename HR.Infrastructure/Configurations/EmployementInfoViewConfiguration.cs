using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoneBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class EmployementInfoViewConfiguration : BaseConfiguration<EmployementInfoView>
    {
        public void Configure(EntityTypeBuilder<EmployementInfoView> builder)
        {
            //base.Configure(builder);


            builder
               .HasNoKey()
               .ToView("Employement_Info_View","hr");

            builder.Property(e => e.AssignmentsAssigneeType).HasColumnName("Assignments_AssigneeType");
            builder.Property(e => e.AssignmentsEffectiveFrom).HasColumnName("Assignments_EffectiveFrom");
            builder.Property(e => e.AssignmentsEffectiveTo).HasColumnName("Assignments_EffectiveTo");
            builder.Property(e => e.CostCenterName).HasColumnName("CostCenter_Name");
            builder.Property(e => e.EmployeeEffectiveFrom).HasColumnName("Employee_EffectiveFrom");
            builder.Property(e => e.EmployeeEffectiveTo).HasColumnName("Employee_EffectiveTo");
            builder.Property(e => e.EmployeeStatusName).HasColumnName("Employee_Status_Name");
            builder.Property(e => e.EmployeeTypeName).HasColumnName("Employee_Type_Name");
            builder.Property(e => e.EmploymentLocationsEffectiveFrom).HasColumnName("EmploymentLocations_EffectiveFrom");
            builder.Property(e => e.EmploymentLocationsEffectiveTo).HasColumnName("EmploymentLocations_EffectiveTo");
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.GradeTitle).HasColumnName("Grade_Title");
            builder.Property(e => e.JobLevelTitle).HasColumnName("JobLevel_Title");
            builder.Property(e => e.JobTitleName).HasColumnName("JobTitle_Name");
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.LocationTitle).HasColumnName("Location_Title");
            builder.Property(e => e.NationalCode).HasMaxLength(10);
            builder.Property(e => e.OrganizationUnitsName)
                    .HasMaxLength(200)
                    .HasColumnName("OrganizationUnits_Name");
            builder.Property(e => e.PartyAddress).HasColumnName("Party_Address");
            builder.Property(e => e.PartyEmail).HasColumnName("Party_Email");
            builder.Property(e => e.PartyId).HasColumnName("Party_Id");
            builder.Property(e => e.PartyMobile).HasColumnName("Party_Mobile");
            builder.Property(e => e.PartyPhone).HasColumnName("Party_Phone");
            builder.Property(e => e.PostCode).HasColumnName("Post_Code");
            builder.Property(e => e.PostContactEmail).HasColumnName("PostContact_Email");
            builder.Property(e => e.PostContactFax).HasColumnName("PostContact_Fax");
            builder.Property(e => e.PostContactMobile).HasColumnName("PostContact_Mobile");
            builder.Property(e => e.PostContactPhone).HasColumnName("PostContact_Phone");
        }
    }

    internal class EmployementInfoViewConfiguration
    {

        modelBuilder.Entity<EmployementInfoView>(entity =>
        {
           
    });

    }
}
