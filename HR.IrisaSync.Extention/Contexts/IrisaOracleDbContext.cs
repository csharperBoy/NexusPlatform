using System;
using System.Collections.Generic;
using HR.IrisaSync.Extention.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.IrisaSync.Extention.Contexts;

public partial class IrisaOracleDbContext : DbContext
{
    public IrisaOracleDbContext()
    {
    }

    public IrisaOracleDbContext(DbContextOptions<IrisaOracleDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PdsIdeaInformationViw> PdsIdeaInformationViws { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("User Id=TPOUT_PDS;Password=irisatpout;Data Source=//192.168.7.5:1521/prod;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("TPOUT_PDS")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<PdsIdeaInformationViw>(entity =>
        {
            entity
                .ToView("PDS_IDEA_INFORMATION_VIW", "APPS")
                .HasKey(e => e.Id);

            entity.Property(e => e.BirthPlace)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("BIRTH_PLACE");
            entity.Property(e => e.CodBranch)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_BRANCH");
            entity.Property(e => e.CodBusun)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_BUSUN");
            entity.Property(e => e.CodCalCalnr)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("COD_CAL_CALNR");
            entity.Property(e => e.CodCateJobpo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("COD_CATE_JOBPO");
            entity.Property(e => e.CodClassJobpo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("COD_CLASS_JOBPO");
            entity.Property(e => e.CodEducation)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_EDUCATION");
            entity.Property(e => e.CodEmtyp)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("COD_EMTYP");
            entity.Property(e => e.CodFactoryBranch)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_FACTORY_BRANCH");
            entity.Property(e => e.CodJobpo)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_JOBPO");
            entity.Property(e => e.CodMoaBusun)
                .IsUnicode(false)
                .HasColumnName("COD_MOA_BUSUN");
            //entity.Property(e => e.CodNatEmply)
            //    .HasMaxLength(13)
            //    .IsUnicode(false)
            //    .HasColumnName("COD_NAT_EMPLY");
            //entity.Property(e => e.Id)
            //    .HasMaxLength(13)
            //    .IsUnicode(true)
            //    .HasColumnName("COD_NAT_EMPLY");
            entity.Property(e => e.CodPosit)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_POSIT");
            entity.Property(e => e.CodStaPcond)
                .HasPrecision(2)
                .HasColumnName("COD_STA_PCOND");
            entity.Property(e => e.CodTndcy)
                .HasColumnType("NUMBER")
                .HasColumnName("COD_TNDCY");
            entity.Property(e => e.DatBirthEmplyEn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DAT_BIRTH_EMPLY_EN");
            entity.Property(e => e.DatBirthEmplyPr)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DAT_BIRTH_EMPLY_PR");
            entity.Property(e => e.DatEmpltEmplyEn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DAT_EMPLT_EMPLY_EN");
            entity.Property(e => e.DatEmpltEmplyPr)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DAT_EMPLT_EMPLY_PR");
            entity.Property(e => e.DesAdrEmply)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("DES_ADR_EMPLY");
            entity.Property(e => e.DesBranch)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_BRANCH");
            entity.Property(e => e.DesBusun)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("DES_BUSUN");
            entity.Property(e => e.DesClassJobpo)
                .IsUnicode(false)
                .HasColumnName("DES_CLASS_JOBPO");
            entity.Property(e => e.DesCodCateJobpo)
                .IsUnicode(false)
                .HasColumnName("DES_COD_CATE_JOBPO");
            entity.Property(e => e.DesEducation)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_EDUCATION");
            entity.Property(e => e.DesEmailAddresEmply)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_EMAIL_ADDRES_EMPLY");
            entity.Property(e => e.DesEmtyp)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_EMTYP");
            entity.Property(e => e.DesJobpo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("DES_JOBPO");
            entity.Property(e => e.DesMarriedEmply)
                .IsUnicode(false)
                .HasColumnName("DES_MARRIED_EMPLY");
            entity.Property(e => e.DesMoaBusun)
                .IsUnicode(false)
                .HasColumnName("DES_MOA_BUSUN");
            entity.Property(e => e.DesPosit)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_POSIT");
            entity.Property(e => e.DesReligionEmply)
                .IsUnicode(false)
                .HasColumnName("DES_RELIGION_EMPLY");
            entity.Property(e => e.DesSexEmply)
                .IsUnicode(false)
                .HasColumnName("DES_SEX_EMPLY");
            entity.Property(e => e.DesStaPcond)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_STA_PCOND");
            entity.Property(e => e.DesTndcy)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("DES_TNDCY");
            entity.Property(e => e.IssuancePlace)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("ISSUANCE_PLACE");
            entity.Property(e => e.LevelEmply)
                .HasPrecision(2)
                .HasColumnName("LEVEL_EMPLY");
            entity.Property(e => e.LevelJobEmply)
                .HasPrecision(2)
                .HasColumnName("LEVEL_JOB_EMPLY");
            entity.Property(e => e.NamFathrEmply)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NAM_FATHR_EMPLY");
            entity.Property(e => e.NamFirstEmply)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NAM_FIRST_EMPLY");
            entity.Property(e => e.NamLastEmply)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NAM_LAST_EMPLY");
            entity.Property(e => e.NumCrtEmply)
                .HasPrecision(13)
                .HasColumnName("NUM_CRT_EMPLY");
            entity.Property(e => e.NumMobilEmply)
                .HasPrecision(11)
                .HasColumnName("NUM_MOBIL_EMPLY");
            entity.Property(e => e.NumPrsnEmply)
                .HasPrecision(10)
                .HasColumnName("NUM_PRSN_EMPLY");
            entity.Property(e => e.NumTelEmply)
                .HasPrecision(11)
                .HasColumnName("NUM_TEL_EMPLY");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
