using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Contact.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Infrastructure.Configurations
{
    public class PhoneBookInfoViewConfiguration : BaseConfiguration<PhoneBookInfoView>
    {
        public override void Configure(EntityTypeBuilder<PhoneBookInfoView> builder)
        {
            //base.Configure(builder);


            builder
               .HasNoKey()
               .ToView("PhoneBook_Info_View", "contact");

            builder.Property(e => e.EmpLocationContactEmail).HasColumnName("EmpLocationContact_Email");
            builder.Property(e => e.EmpLocationContactFax).HasColumnName("EmpLocationContact_Fax");
            builder.Property(e => e.EmpLocationContactMobile).HasColumnName("EmpLocationContact_Mobile");
            builder.Property(e => e.EmpLocationContactPhone).HasColumnName("EmpLocationContact_Phone");
            builder.Property(e => e.EmploymentContactEmail).HasColumnName("EmploymentContact_Email");
            builder.Property(e => e.EmploymentContactFax).HasColumnName("EmploymentContact_Fax");
            builder.Property(e => e.EmploymentContactMobile).HasColumnName("EmploymentContact_Mobile");
            builder.Property(e => e.EmploymentContactPhone).HasColumnName("EmploymentContact_Phone");
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.NationalCode).HasMaxLength(10);
            builder.Property(e => e.OrganizationUnitsName).HasMaxLength(200);
            builder.Property(e => e.PartyAddress).HasColumnName("Party_Address");
            builder.Property(e => e.PartyEmail).HasColumnName("Party_Email");
            builder.Property(e => e.PartyMobile).HasColumnName("Party_Mobile");
            builder.Property(e => e.PartyPhone).HasColumnName("Party_Phone");
            builder.Property(e => e.PostContactEmail).HasColumnName("PostContact_Email");
            builder.Property(e => e.PostContactFax).HasColumnName("PostContact_Fax");
            builder.Property(e => e.PostContactMobile).HasColumnName("PostContact_Mobile");
            builder.Property(e => e.PostContactPhone).HasColumnName("PostContact_Phone");
            builder.Property(e => e.PostLocationContactEmail).HasColumnName("PostLocationContact_Email");
            builder.Property(e => e.PostLocationContactFax).HasColumnName("PostLocationContact_Fax");
            builder.Property(e => e.PostLocationContactMobile).HasColumnName("PostLocationContact_Mobile");
            builder.Property(e => e.PostLocationContactPhone).HasColumnName("PostLocationContact_Phone");

        }
    }
}
