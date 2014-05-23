using DebtCollection.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Dtos
{
    public class EarchiveDto : TrackableTuple, IHousable
    {
        [Display(Name = "���������� ����� ��������")]
        public long? SubscriberId { get; set; }

        [Display(Name = "��� ���������")]
        public string CertificateType { get; set; }

        [Display(Name = "����  ������ ��������")]
        public DateTime? CertificateIssueDate { get; set; }

        [Display(Name = "����� ��������")]
        public string CertificateId { get; set; }

        [Display(Name = "����� �������� �������")]
        public string Authority { get; set; }

        [Display(Name = "����������������� �����")]
        public string IdentificationNumber { get; set; }

        [Display(Name = "���� ��������")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "��� ��������")]
        public string Fullname { get; set; }

        [Display(Name = "����� ��������")]
        public string RegistrationAddress { get; set; }

        [Display(Name = "���")]
        public string JurisdictionName { get; set; }

        [Display(Name = "����� ����")]
        public string JurisdictionAddress { get; set; }

        [Display(Name = "���������� ��� 1")]
        public string Phone { get; set; }

        [Display(Name = "����� ���������")]
        public string AgreementNumber { get; set; }

        [Display(Name = "���� ���������")]
        public DateTime? ContractDate { get; set; }

        [Display(Name = "����")]
        public decimal AmountOfPenalty { get; set; }

        [Display(Name = "��� ������������")]
        public string DeviceType { get; set; }

        [Display(Name = "������ ���������")]//���������, ������
        public string ProcessingStatus { get; set; }

        [Display(Name = "���������� ���������")]
        public string ProcessingNotes { get; set; }
    }
}