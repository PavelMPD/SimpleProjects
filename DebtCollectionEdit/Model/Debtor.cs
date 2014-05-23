using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DebtCollection.Model
{
    [Table("Debtors")]
    public class Debtor : Entity//, IAuditable
    {
        /// <summary>
        /// ���
        /// </summary>
        [Auditable("���")]
        public string FirstName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [Auditable("�������")]
        public string LastName { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [Auditable("��������")]
        public string MiddleName { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return string.Format("{0} {1} {2}", LastName, FirstName, MiddleName); }
        }

        /// <summary>
        /// ����������� ����� (MSISDN)
        /// </summary>
        [Auditable("����������� �����")]
        public long? SubscriberId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Msisdn { get; set; }

        /// <summary>
        /// �������� ����
        /// </summary>
        [Auditable("�������� ����")]
        public string TariffPlan { get; set; }

        /// <summary>
        /// ��� ������������
        /// </summary>
        [Auditable("��� ������������")]
        public string DeviceType { get; set; }

        /// <summary>
        /// IMEI ������
        /// </summary>
        [Auditable("IMEI ������������")]
        public long? Imei { get; set; }

        /// <summary>
        /// �������� �������������� ��������
        /// </summary>
        [Auditable("��������, �������������� ��������")]
        public string CertificateType { get; set; }

        /// <summary>
        /// ����� ��������� (��������������� ��������)
        /// </summary>
        [Auditable("����� ���������")]
        public string CertificateId { get; set; }

        /// <summary>
        /// ���� ������ ��������� (��������������� ��������)
        /// </summary>
        [Auditable("���� ������ ���������")]
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// �����, �������� �������� (�������������� ��������)
        /// </summary>
        [Auditable("�����, �������� ��������")]
        public string Authority { get; set; }

        /// <summary>
        /// ���� �������� 
        /// </summary>
        [Auditable("���� ��������")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// ����� �����������
        /// </summary>
        [Auditable("����� �����������")]
        public String RegistrationAddress { get; set; }

        /// <summary>
        /// �������� ������
        /// </summary>
        [Auditable("�������� ������")]
        public String PostalCode { get; set; }

        /// <summary>
        /// ����� �������� (=SIM number, ���������� ���� ��� ������ � E-archive)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("����� ��������")]
        public long? AgreementNumber { get; set; }

        /// <summary>
        /// ���� �������� (=���� ������� ������������)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("���� ��������")]
        public DateTime? AgreementDate { get; set; }

        /// <summary>
        /// ���������� �����
        /// </summary>
        [Auditable("���������� �����")]
        public string Phone1 { get; set; }

        /// <summary>
        /// ���������� ����� 2
        /// </summary>
        [Auditable("���������� ����� 2")]
        public string Phone2 { get; set; }

        /// <summary>
        /// ���� ����������� �������������
        /// </summary>
        public DateTime DebtDate { get; set; }

        /// <summary>
        /// ����� �������������
        /// </summary>
        [Auditable("����� �������������, ���")]
        public decimal DebtAmount { get; set; }

        /// <summary>
        /// ���������� �������� ������������� ������ life:) �������� (=���������� ��������� ��������)
        /// </summary>
        public int? PeriodsCount { get; set; }

        /// <summary>
        /// ����� �����������
        /// </summary>
        public string ConnectionPoint { get; set; }

        /// <summary>
        /// ��� ��������� (=CO_ID, ���������� ���� ��� ������ � ��������)
        /// </summary>
        [Required]
        public long ContractCode { get; set; }

        /// <summary>
        /// ���� ��������� (=���� ��������� ���-�����)
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        /// ���� �������� �� �������� ����� (=���� ���������� �������������)
        /// </summary>
        public DateTime? DebtIncreaseDate { get; set; }

        /// <summary>
        /// ������������� �����
        /// </summary>
        [Auditable("������������� �����")]
        public string MarketingAction { get; set; }

        /// <summary>
        /// ���� ������������� (= ���-�� ���� �� ���� ������������� �������������)
        /// </summary>
        public int? DebtTerm { get; set; }

        /// <summary>
        /// ���������� ���������� ������������ ��������
        /// </summary>
        [Auditable("���������� ���������� ������������ ��������")]
        public int? LeftSubscriptions { get; set; }

        /// <summary>
        /// �������� ������ life:) ��������
        /// </summary>
        [Auditable("�������� ������ life:) ��������")]
        public string ActiveService { get; set; }
 

        /// <summary>
        /// ���������� �� ��
        /// </summary>
        public virtual Endorsement Endorsement { get; set; }

        /// <summary>
        /// ���������� �� �����
        /// </summary>
        public virtual ICollection<Complaint> Complaints { get; set; }

        /// <summary>
        /// The history of debt amount changes
        /// </summary>
        public virtual ICollection<DebtAmountHistory> DebtAmountHistory { get; set; }

        /// <summary>
        /// The history of registration address changes
        /// </summary>
        public virtual ICollection<RegistrationAddressHistory> RegistrationAddressHistory { get; set; }

        /// <summary>
        /// The history of subscriber name changes
        /// </summary>
        public virtual ICollection<SubscriberNameHistory> SubscriberNameHistory { get; set; }

        //-RollCall tab

        /// <summary>
        /// ���� �������
        /// </summary>
        public DateTime? RollCallDate { get; set; }

        /// <summary>
        /// �������������
        /// </summary>
        public string RollCallResponsible { get; set; }

        /// <summary>
        /// ��������� �������
        /// </summary>
        public string RollCallSubStatus { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        public string RollCallComments { get; set; }

        /// <summary>
        /// ���������� � ����������
        /// </summary>
        public virtual ICollection<Claim> Claims { get; set; }

        /// <summary>
        /// Notes from payment info tab
        /// </summary>
        public virtual string PaymentInfoNote { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [Auditable("������")]
        public SubscriberStatus SubscriberStatus { get; set; }

        /// <summary>
        /// ���� ��������� �������
        /// </summary>
        public DateTime? SubscriberStatusChangeDate { get; set; }

        /// <summary>
        /// Notes from payment info tab
        /// </summary>
        public virtual ICollection<PaymentInformation> PaymentsInformation { get; set; }

        public virtual FileEntity AgreementScan { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        public virtual Jurisdiction Jurisdiction { get; set; }

        //todo: rename
        public virtual ICollection<OperationDebtor> DocumentOperationDebtors { get; set; }

        /// <summary>
        /// ����������������� ����� (�� ��������)
        /// </summary>
        [Auditable("����������������� �����")]
        public string IdentificationNumber { get; set; }


        /// <summary>
        /// ��� ������������� ������� ����� ������ �������
        /// </summary>
        public SubscriberStatus? PreviousSubscriberStatus { get; set; }

        /// <summary>
        /// ��� ������������� ������� ����� ������ �������
        /// </summary>
        public DateTime? PreviousSubscriberStatusChangeDate { get; set; }


        /// <summary>
        ///  (����� ����� ������, ���) TotalFineWithFee in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> TotalFineWithFreeExpression =
             DefaultTranslationOf<Debtor>.Property(d => d.TotalFineWithFee)
                .Is(d => d.PaymentsInformation.Select(x => x.FinePaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal TotalFineWithFee
        {
            get
            {
                return TotalFineWithFreeExpression.Evaluate(this);
            }
        }

        /// <summary>
        /// (����� ����� ����������, ���) TotalDutyWithFee in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> TotalDutyWithFreeExpression =
            DefaultTranslationOf<Debtor>.Property(d => d.TotalDutyWithFee)
                .Is(d => d.PaymentsInformation.Select(p => p.DutyPaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal TotalDutyWithFee
        {
            get
            {
                return TotalDutyWithFreeExpression.Evaluate(this);
            }
        }

        /// <summary>
        ///  �������, ��� (�� ����� ���� ����� �������� ��������)  PaymentsRest in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> PaymentsRestExpression =
            DefaultTranslationOf<Debtor>.Property(d => d.PaymentsRest)
                .Is(d => d.PaymentsInformation.Select(p => p.RestTotal).Union(new decimal[] { 0 }).Sum());

        public decimal PaymentsRest
        {
            get
            {
                return PaymentsRestExpression.Evaluate(this);
            }
        }

        /// <summary>
        /// "����� ����������� ��������, ���
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> PaymentsTotalExpression =
              DefaultTranslationOf<Debtor>.Property(d => d.PaymentsTotal)
                 .Is(d => d.PaymentsInformation.Select(p => p.PaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal PaymentsTotal
        {
            get
            {
                return PaymentsTotalExpression.Evaluate(this);
            }
        }


        /// <summary>
        /// ����� � ���������, ���.:
        /// </summary>
        [NotMapped]
        public decimal FullAmountOfDebt
        {
            get
            {
                decimal fullAmountOfDebt = 0;
                if (Endorsement != null)
                {
                    fullAmountOfDebt += Endorsement.DemandedAmount;
                }

                if (Complaints != null)
                {
                    fullAmountOfDebt += Complaints.Where(c => c.DemandedAmount != null).Select(c => c.DemandedAmount).Sum(c => c.Value);
                }

                return fullAmountOfDebt;
            }
        }

        /// <summary>
        /// �������� ��������, ���.
        /// </summary>
        [NotMapped]
        public decimal DebtRemaining
        {
            get
            {
                decimal debtRemaining = 0;

                if (Endorsement != null)
                {
                    debtRemaining += Endorsement.FullAmountOfDebt ?? 0;
                }

                if (Complaints != null)
                {
                    debtRemaining += Complaints.Where(c => c.FullAmountOfDebt != null).Select(c => c.FullAmountOfDebt).Sum(c => c.Value);
                }

                return debtRemaining;
            }
        }
 
        [NotMapped] //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId
        {
            get { return Id; }
            set { }
        }

        [NotMapped]
        public Debtor AuditDebtor { get { return this; } set { } }
 
     }
}