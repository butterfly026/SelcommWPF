using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class PlanModel
    {
        public long Id { get; set; }

        public string Status { get; set; }

        public long? PlanId { get; set; }

        public string Plan { get; set; }

        public long? OptionId { get; set; }

        public string Option { get; set; }

        public string Scheduled { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public long? EventId { get; set; }

        public string PlanTypeId { get; set; }

        public string PlanType { get; set; }

        public string BackDate { get; set; }

        public bool CanClose { get; set; }

        public bool CanCancel { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }

        public class Detail
        {
            public long Id { get; set; }

            public int SalesRank { get; set; }

            public int ValueRank { get; set; }

            public string AdditionalInformation1 { get; set; }

            public string AdditionalInformation2 { get; set; }

            public string AdditionalInformation3 { get; set; }

            public string URL { get; set; }

            public bool CycleLocked { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

            public UserModel.BusinessUnitModel ParentPlan { get; set; }

            public UserModel.BusinessUnitModel ChildPlan { get; set; }

            public List<UserModel.BusinessUnitModel> ServiceTypes { get; set; }

            public string CreatedBy { get; set; }

            public List<PlanVersion> TransactionPlanVersions { get; set; }

            public string Requestor { get; set; }

            public string Availability { get; set; }

            public string Name { get; set; }

            public string DisplayName { get; set; }

            public string GroupId { get; set; }

            public string Group { get; set; }

            public string TypeId { get; set; }

            public string Type { get; set; }

            public string Comment { get; set; }

            public bool TypeDefault { get; set; }

            public string Contract { get; set; }

            public long TransactionPlanId { get; set; }

            public string TransactionPlan { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public string BillingInterval { get; set; }

            public string ContractId { get; set; }

            public List<PlanOption> Options { get; set; }
        }

        public class PlanVersion
        {
            public long Id { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class PlanOption
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public bool Default { get; set; }

            public int Order { get; set; }

            public string AdditionalInformation1 { get; set; }

            public string AdditionalInformation2 { get; set; }

            public string AdditionalInformation3 { get; set; }

            public string AdditionalInformation4 { get; set; }

            public string ContractId { get; set; }

            public string Contract { get; set; }

            public int? MinimumScore { get; set; }

            public List<AccountCharge.History> Charges { get; set; }

            public List<PlanDiscount> Discounts { get; set; }

            public List<AttrCharge> AttributeCharges { get; set; }

            public List<HardwareOption> HardwareOptions { get; set; }

            public string ContractAction { get; set; }
        }

        public class PlanDiscount
        {
            public long Id { get; set; }

            public long DiscountId { get; set; }

            public string ContractAction { get; set; }

            public string Discount { get; set; }

            public string DiscountLongDescription { get; set; }

            public string DiscountShortDescription { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public bool AutoApplied { get; set; }

            public bool Used { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class AttrCharge
        {
            public long Id { get; set; }

            public string AttributeCharge { get; set; }

            public string ChargeDefinitionId { get; set; }

            public string Charge { get; set; }

            public string DisplayName { get; set; }

            public long AttributeDefintion1Id { get; set; }

            public string AttributeDefintion1 { get; set; }

            public long AttributeDefintion2Id { get; set; }

            public string AttributeDefintion2 { get; set; }

            public string AttributeBand { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class HardwareOption
        {
            public long Id { get; set; }

            public int DisplyOrder { get; set; }

            public string Name { get; set; }

            public string Note { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public bool PartShipmentAllowed { get; set; }

            public string URL { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

    }
}
