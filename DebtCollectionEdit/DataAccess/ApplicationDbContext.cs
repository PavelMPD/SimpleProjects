//using DebtCollection.Common;
using DebtCollection.Model;
using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using DebtCollection.Model.Payment;
using DebtCollection.Model.ReportFilesProcessing;
using DebtCollection.Model.Reports;
//using DebtCollection.Reminders;
//using DebtCollection.Reminders.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;

namespace DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        #region Db sets

        public virtual IDbSet<Debtor> Debtors { get; set; }

        //public DbSet<ProcessedFile> ProcessedFiles { get; set; }

        //public DbSet<ProcessedFileDetails> ProcessedFileDetails { get; set; }

        public DbSet<Endorsement> Endorsements { get; set; }

        //public DbSet<Jurisdiction> Jurisdictions { get; set; }

        //public DbSet<FileEntity> FilesEntities { get; set; }

        //public DbSet<Operation> DocumentOperations { get; set; }

        //public DbSet<EndorsementOperationInfo> EndorsementOperationInfo { get; set; }

        //public DbSet<ComplaintEndorsement> ComplaintsEndorsements { get; set; }

        //public DbSet<DebtorData> DebtorsData { get; set; }

        //public virtual IDbSet<Claim> Claims { get; set; }

        //public DbSet<OperationDebtor> OperationDebtors { get; set; }

        //public DbSet<LuceneStatus> LuceneStatuses { get; set; }

        //public DbSet<ReportZeroItem> ReportZeroItems { get; set; }

        //public DbSet<ReportOneItem> ReportOneItems { get; set; }

        //public DbSet<ReportFourItem> ReportFourItems { get; set; }

        //public DbSet<DocumentPayment> DocumentPayments { get; set; }

        //public DbSet<RemindersConfiguration> RemindersConfiguration { get; set; }

        //public DbSet<OperationSubdocument> OperationSubdocument { get; set; }

        //public DbSet<PaymentInformation> PaymentInformations { get; set; }

        //public DbSet<PaymentAlert> PaymentAlerts { get; set; }

        //public DbSet<EarchiveFileUploadHistory> EArchiveFilesUploadHistory { get; set; }

        //public DbSet<Audit> Audit { get; set; }

        //public DbSet<EripReport> EripReports { get; set; }

        //public DbSet<Mail> Mails { get; set; }

        //public DbSet<Complaint> Complaints { get; set; }

        //public DbSet<SubdocumentOperationInfo> SubdocumentOperationInfos { get; set; }

        //public DbSet<ExludedDebtor> ExcludedDebtors { get; set; }

        //public DbSet<DebtAmountHistory> DebtAmountHistory { get; set; }

        //public DbSet<ComplaintSubdocument> ComplaintSubdocuments { get; set; }

        //public DbSet<EndorsementSubdocument> EndorsementSubdocuments { get; set; }

        #endregion

        public ApplicationDbContext()
            : base("ApplicationServices")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 120;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //TODO: investigate EdmModelDiffer to know more about data change tracking.
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            modelBuilder.Entity<Debtor>()
                .HasOptional(d => d.Endorsement)
                .WithRequired(c => c.Debtor);

            modelBuilder.Entity<Endorsement>()
                .HasOptional(e => e.EndorsementFile)
                .WithMany()
                .Map(map => map.MapKey("FileId"));
            //.WillCascadeOnDelete();

            modelBuilder.Entity<Audit>()
                .HasRequired(x => x.Debtor)
                .WithMany()
                .Map(map => map.MapKey("DebtorId"));

            modelBuilder.Entity<Endorsement>()
                .HasOptional(e => e.CoverLetter)
                .WithMany()
                .Map(map => map.MapKey("CoverLetterId"))
                .WillCascadeOnDelete();

            modelBuilder.Entity<Debtor>()
                .HasMany(x => x.Complaints)
                .WithRequired(x => x.Debtor);

            modelBuilder.Entity<Complaint>()
                .HasMany(c => c.Files)
                .WithMany()
                .Map(m =>
                {
                    m.MapLeftKey("ComplaintId");
                    m.MapRightKey("FileId");
                    m.ToTable("Complaints_Files");
                }
            );

            modelBuilder.Entity<Complaint>().Property(x => x.DebtorId).HasColumnName("Debtor_Id");

            modelBuilder.Entity<Debtor>()
                .HasOptional(d => d.AgreementScan)
                .WithOptionalDependent(f => f.Debtor)
                .Map(m => m.MapKey("AgreementScanId"));

            modelBuilder.Entity<Claim>()
                .HasRequired(c => c.Debtor)
                .WithMany(d => d.Claims);

            modelBuilder.Entity<Claim>().Property(x => x.DebtorId).HasColumnName("Debtor_Id");

            modelBuilder.Entity<Debtor>()
                .HasMany(a => a.Claims)
                .WithRequired(b => b.Debtor);

            modelBuilder.Entity<Claim>()
                .HasOptional(c => c.File);

            modelBuilder.Entity<DebtAmountHistory>().ToTable("DebtsAmountHistory");
            modelBuilder.Entity<DebtAmountHistory>().HasRequired(x => x.Debtor);

            modelBuilder.Entity<RegistrationAddressHistory>().ToTable("RegistrationAddressHistory");
            modelBuilder.Entity<RegistrationAddressHistory>().HasRequired(x => x.Debtor);

            modelBuilder.Entity<SubscriberNameHistory>().ToTable("SubscriberNameHistory");
            modelBuilder.Entity<SubscriberNameHistory>().HasRequired(x => x.Debtor);

            modelBuilder.Entity<PaymentInformation>().ToTable("PaymentsInformation");
            modelBuilder.Entity<PaymentInformation>().HasOptional(x => x.Debtor);

            modelBuilder.Entity<PaymentInformation>().Property(x => x.DebtorId).HasColumnName("Debtor_Id");

            modelBuilder.Entity<DocumentPayment>()
                .HasRequired(dp => dp.PaymentInformation)
                .WithMany(p => p.DocumentPayments)
                .Map(m => m.MapKey("PaymentId"));

            modelBuilder.Entity<DocumentPayment>()
                .HasOptional(dp => dp.Endorsement)
                .WithMany(p => p.DocumentPayments)
                .Map(m => m.MapKey("EndorsementId"));

            modelBuilder.Entity<DocumentPayment>()
              .HasOptional(dp => dp.Complaint)
              .WithMany(p => p.DocumentPayments)
              .Map(m => m.MapKey("ComplaintId"));

            modelBuilder.Entity<FileServer>().ToTable("FileServers");
            modelBuilder.Entity<FileServer>()
                .Property(a => a.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<FileEntity>().ToTable("Files");
            modelBuilder.Entity<FileEntity>()
                .Property(prop => prop.FileServerId)
                .HasColumnName("FileServer");
            modelBuilder.Entity<FileEntity>()
                .HasOptional(x => x.Debtor).WithOptionalPrincipal(x => x.AgreementScan);

            modelBuilder.Entity<OperationDebtor>().ToTable("OperationDebtors");
            modelBuilder.Entity<OperationDebtor>()
                .HasRequired(a => a.Operation)
                .WithMany(b => b.OperationDebtors)
                .Map(m => m.MapKey("OperationId"));

            modelBuilder.Entity<OperationDebtor>()
                .HasRequired(b => b.Debtor)
                .WithMany(a => a.DocumentOperationDebtors)
                .Map(m => m.MapKey("DebtorId"));

            modelBuilder.Entity<Operation>().ToTable("Operations");
            modelBuilder.Entity<Operation>()
                .HasOptional(x => x.EndorsementOperationInfo)
                .WithRequired(x => x.Operation);

            modelBuilder.Entity<EndorsementOperationInfo>().ToTable("EndorsementOperationInfo");

            modelBuilder.Entity<DebtorsClaimOperationInfo>().ToTable("DebtorsClaimOperationInfo");
            modelBuilder.Entity<ClaimOperationInfo>().ToTable("ClaimOperationInfo");

            modelBuilder.Entity<Operation>()
                .HasOptional(x => x.ClaimOperationInfo)
                .WithRequired(x => x.Operation);

            modelBuilder.Entity<DebtorsClaimOperationInfo>()
                .Property(d => d.Id)
                .HasColumnName("OperationDebtorId");

            modelBuilder.Entity<DebtorsClaimOperationInfo>()
                .HasOptional(d => d.Claim)
                .WithMany()
                .Map(m => m.MapKey("ClaimId"));

            modelBuilder.Entity<PaymentAlert>()
                .HasOptional(pa => pa.PymentInformation)
                .WithOptionalDependent()
                .Map(m => m.MapKey("PaymentId"));

            modelBuilder.Entity<PaymentAlert>().Property(x => x.DebtorId).HasColumnName("Debtor_Id");

            modelBuilder.Entity<OperationDebtor>()
                .HasOptional(d => d.DebtorsClaimOperationInfo)
                .WithRequired(m => m.OperationDebtor);

            modelBuilder.Entity<Debtor>()
                .HasOptional(debtor => debtor.Jurisdiction);

            modelBuilder.Entity<ExludedDebtor>().ToTable("DebtorsExcludeList");

            modelBuilder.Entity<ProcessedFile>()
                .HasMany(x => x.FileItems)
                .WithRequired(x => x.ProcessedFile);

            modelBuilder.Entity<EarchiveFileUploadHistory>()
                .HasRequired(c => c.File).WithOptional().Map(m => m.MapKey("FileId"));

            InitializeSubdocumentEntities(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void InitializeSubdocumentEntities(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubdocumentOperationInfo>().ToTable("SubdocumentOperationInfo");

            modelBuilder.Entity<Operation>()
                .HasOptional(x => x.SubdocumentOperationInfo)
                .WithRequired(x => x.Operation);


            modelBuilder.Entity<OperationSubdocument>().ToTable("OperationSubdocuments");

            modelBuilder.Entity<OperationSubdocument>()
                .HasRequired(a => a.Operation)
                .WithMany(b => b.OperationSubdocuments)
                .Map(m => m.MapKey("OperationId"));

            modelBuilder.Entity<OperationSubdocument>()
                .HasOptional(b => b.Complaint)
                .WithMany()
                .Map(m => m.MapKey("ComplaintId"));

            modelBuilder.Entity<OperationSubdocument>()
                .HasOptional(b => b.Endorsement)
                .WithMany()
                .Map(m => m.MapKey("EndorsementId"));

            modelBuilder.Entity<OperationSubdocument>()
                .HasOptional(d => d.ComplaintSubdocument)
                .WithOptionalDependent(a => a.OperationSubdocument)
                .Map(m => m.MapKey("ComplaintSubdocumentId"));

            modelBuilder.Entity<OperationSubdocument>()
                .HasOptional(d => d.EndorsementSubdocument)
                .WithOptionalDependent(a => a.OperationSubdocument)
                .Map(m => m.MapKey("EndorsementSubdocumentId"));

            modelBuilder.Entity<ComplaintSubdocument>().ToTable("ComplaintSubdocuments");

            modelBuilder.Entity<ComplaintSubdocument>()
                .HasRequired(d => d.Complaint)
                .WithMany(a => a.Subdocuments)
                .Map(map => map.MapKey("ComplaintId"));

            modelBuilder.Entity<ComplaintSubdocument>()
                .HasOptional(e => e.File)
                .WithMany()
                .Map(map => map.MapKey("FileId"))
                .WillCascadeOnDelete();


            modelBuilder.Entity<EndorsementSubdocument>().ToTable("EndorsementSubdocuments");

            modelBuilder.Entity<EndorsementSubdocument>()
                .HasRequired(d => d.Endorsement)
                .WithMany(d => d.Subdocuments)
                .Map(map => map.MapKey("EndorsementId"));

            modelBuilder.Entity<EndorsementSubdocument>()
                .HasOptional(e => e.File)
                .WithMany()
                .Map(map => map.MapKey("FileId"));
            //.WillCascadeOnDelete();
        }
    }
}
