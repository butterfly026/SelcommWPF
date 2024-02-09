using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.global
{
    class Constants
    {
        public static string Environment = "ua-api.selcomm.com";
        public static string BusinessUnit = "MN";
        public static string SiteId = "Demo3";

        public static bool DEBUG_MODE = true;
        public static string BaseURL = string.Format("https://{0}/", Environment);

        public static string ConfigURL = BaseURL + "Configurations/";
        public static string SitesURL = ConfigURL + "Sites";
        public static string DefaultSiteURL = ConfigURL + "Sites/Default";

        public static string MenuURL = BaseURL + "Menus/";
        public static string MenuContactURL = MenuURL + "DisplayAttributes/ContactCode/{ContactCode}";
        public static string MenuSerivcesURL = MenuURL + "DisplayAttributes/ServiceReference/{ServiceReference}";

        public static string AuthURL = BaseURL + "Authentication/";
        public static string AccessTokenURL = AuthURL + "{SiteId}/Anonymous/Token/Access";
        public static string RefreshTokenURL = AuthURL + "{SiteId}/SelcommUser/{identifier}/Token/Refresh";
        public static string CustomerTokenURL = AuthURL + "SelcommUser/Token/Access";
        public static string ChangePasswordURL = AuthURL + "{SiteId}/SelcommUser/{identifier}/Password";
        public static string ResetPasswordURL = AuthURL + "{SiteId}/SelcommUser/{identifier}/Password/Reset";

        public static string OTTURL = BaseURL + "OTT/";
        public static string AddressForPINURL = OTTURL + "Contacts/{ContactCode}/PIN/Addresses";
        public static string GeneratePINURL = OTTURL + "Contacts/{ContactCode}/PIN";
        public static string ConsumePINURL = OTTURL + "Contacts/{ContactCode}/Consume/PIN/{PIN}";
        public static string SendPINURL = OTTURL + "Contacts/{ContactCode}/PIN/Send";

        public static string DocumentURL = BaseURL + "Documents/";
        public static string DocumentCreateURL = DocumentURL + "Contacts/{ContactCode}";
        public static string DocumentsGetURL = DocumentURL + "Information/Contacts/{ContactCode}";
        public static string DocumentDownloadURL = DocumentURL + "{DocumentId}";
        public static string DocumentDeleteURL = DocumentURL + "{DocumentId}";
        public static string ServiceDocumentsGetURL = DocumentURL + "Information/Services/{ServiceReference}";
        public static string ServiceDocumentCreateURL = DocumentURL + "Services/{ServiceReference}";

        public static string ContactURL = BaseURL + "Contacts/";
        public static string DisplayDetailsURL = ContactURL + "{ContactCode}/DisplayDetails";
        public static string NotesListURL = ContactURL + "{code}/Notes";
        public static string NoteEditURL = ContactURL + "Notes/{noteId}";
        public static string ContactPhoneURL = ContactURL + "{ContactCode}/ContactPhones/Usage";
        public static string PhoneHistoryURL = ContactURL + "{ContactCode}/ContactPhones/UsageHistory";
        public static string PhoneTypeURL = ContactURL + "ContactPhones/Types";
        public static string ContactAddressURL = ContactURL + "{ContactCode}/Addresses/Usage";
        public static string AddressCountriesURL = ContactURL + "Addresses/countries";
        public static string AddressUpdateURL = ContactURL + "{ContactCode}/Addresses/UsageUpdate";
        public static string AddressStateURL = ContactURL + "Addresses/Country/{CountryCode}/States";
        public static string PostCodeURL = ContactURL + "Addresses/postallocalities/Country/{CountryCode}/PostCode/{PostCode}";
        public static string AddressHistoryURL = ContactURL + "{ContactCode}/Addresses/UsageHistory";
        public static string AddressTypeURL = ContactURL + "Addresses/Types";
        public static string ContactNameURL = ContactURL + "{ContactCode}/Names";
        public static string ContactTitlesURL = ContactURL + "Titles";
        public static string AliasTypeURL = ContactURL + "Aliases/Types";
        public static string AliasesHistoryURL = ContactURL + "{ContactCode}/Aliases/History";
        public static string NamesHistoryURL = ContactURL + "{ContactCode}/Names/History";
        public static string ContactEmailURL = BaseURL + "contactemails/Usage/ContactCode:{ContactCode}";
        public static string EmailHistoryURL = BaseURL + "contactemails/UsageHistory/ContactCode:{ContactCode}";
        public static string DocumentListURL = ContactURL + "{ContactCode}/Documents";
        public static string DocumentListAllURL = ContactURL + "{ContactCode}/Documents/All";
        public static string DocumentDetailURL = ContactURL + "{ContactCode}/Document/{DocumentName}";
        public static string DocumentUploadURL = ContactURL + "{ContactCode}/Document/Upload";
        public static string UserDefinedURL = ContactURL + "UserDefinedDataDefinitions";
        public static string UserDefinedListURL = ContactURL + "{ContactCode}/UserDefinedDataDefinitions";
        public static string UserDefinedDetailURL = ContactURL + "{ContactCode}/UserDefinedDataDefinitions/{Id}";
        public static string RelationShipURL = ContactURL + "RelationshipTypes";
        public static string RelatedContactListURL = ContactURL + "{ContactCode}/RelatedContacts";
        public static string RelatedContactDetailURL = ContactURL + "{ContactCode}/RelatedContacts/{Id}";
        public static string RelatedContactTypeURL = ContactURL + "{ContactCode}/RelatedContacts/{Id}/Type/{Type}";
        public static string TimeZoneListURL = ContactURL + "TimeZones/Search";
        public static string TimeZoneDefaultURL = ContactURL + "TimeZones/Default";
        public static string IdentificationTypeURL = ContactURL + "IdentificationTypes";
        //public static string IdentificationRuleURL = ContactURL + "Identifications/ContactTypes/{ContactType}/IdentificationMandatoryRules";
        public static string IdentificationRuleURL = "https://virtserver.swaggerhub.com/Selcomm/Contacts/1.0/IdentificationMandatoryRules/ContactType/{ContactType}";
        public static string QuestionAnswerURL = ContactURL + "{ContactCode}/Questions";
        public static string EnquiryPasswordURL = ContactURL + "{ContactCode}/EnquiryPassword";

        public static string AddressURL = BaseURL + "AddressDatabases/";
        public static string AutoCompleteURL = AddressURL + "AutocompleteAustralian";
        public static string ParseAddressURL = AddressURL + "ParseAustralian";

        public static string UserURL = BaseURL + "Users/";
        public static string SimpleUserURL = UserURL + "Id/LoginId/{userId}";
        public static string FullUserURL = UserURL + "ServiceProviderUsers/Id/{userId}";
        public static string ComplexURL = UserURL + "Passwords/CheckComplexity";
        public static string SuggestopnURL = UserURL + "Passwords/Suggestion";
        public static string ResetPasswordConfigURL = UserURL + "Passwords/Reset/Configuration";
        public static string ResetPasswordSMSURL = UserURL + "Passwords/Reset/SMS";
        public static string CheckLoginIdURL = UserURL + "Authentication/LoginId/{Parameter}/Unique";
        public static string CheckEmailURL = UserURL + "Authentication/Email/{Parameter}/Unique";
        public static string CheckMobileURL = UserURL + "Authentication/Mobile/{Parameter}/Unique";
        public static string NextLoginIdURL = UserURL + "ServiceProviderUsers/NextId";
        public static string PasswordInformationURL = UserURL + "PasswordInformation/Contact/{ContactCode}";
        public static string LoginDeniedURL = UserURL + "Login/Denied";
        public static string LoginApprovedURL = UserURL + "Login/Approved/Contact/{UserId}";
        public static string LoginHistoryURL = UserURL + "Login/History/Contact/{UserId}";
        public static string LoginMakeSuspeck = UserURL + "Login/History/ToggleSuspect/{HistoryId}";
        public static string RegisterEmailOrMobileURL = UserURL + "Register/{Param}/Contact/{UserId}";
        public static string ServiceProviderUsersURL = UserURL + "ServiceProviderUsers";
        public static string BusinessUnitsURL = UserURL + "BusinessUnits";
        public static string ServiceProviderUserConfigURL = UserURL + "ServiceProviderUsers/Configuration";
        public static string UserTeamsURL = UserURL + "Teams";
        public static string ServiceProviderUserRolesURL = UserURL + "ServiceProviderUsers/Roles";
        public static string AuthenticationAccountURL = UserURL + "Authentication/{Id}";
        public static string AuthenticationCreateURL = UserURL + "{Id}/Authentication/{Category}";
        public static string DeleteUserURL = UserURL + "Contact/{ContactCode}";
        public static string ResetPassowrdURL = UserURL + "PasswordChanged/Contact/{UserId}";

        public static string BillURL = BaseURL + "Bills/";
        public static string BillHistoryURL = BillURL + "ContactCode/{contactCode}";
        public static string HotBillsPeriodsURL = BillURL + "HotBillPeriods/ContactCode/{ContactCode}";
        public static string BillServicesURL = BillURL + "ServiceSummaries/BillId/{BillId}";
        public static string BillTransURL = BillURL + "Transactions/BillId/{BillId}";
        public static string BillChargesURL = BillURL + "Charges/BillId/{BillId}";
        public static string BillPDFURL = BillURL + "BillImage/Id/{BillId}";
        public static string BillExcelURL = BillURL + "BillExcel/Id/{BillId}";
        public static string BillEmailAddressURL = BillURL + "Emails/Addreses/BillId/{BillId}";
        public static string BillEmailSendURL = BillURL + "Emails/{BillId}";
        public static string BillDisputesListURL = BillURL + "Disputes/BillId/{BillId}";
        public static string BillDisputesDetailURL = BillURL + "Disputes/Id/{DisputeId}";
        public static string BillDeleteURL = BillURL + "Id/{BillId}";

        public static string CostCentersURL = BaseURL + "CostCenters/";
        public static string CostCenterCreateURL = CostCentersURL + "Contacts/{contactCode}";
        public static string CostCentersGetURL = CostCentersURL + "Contacts/{contactCode}";
        public static string CostCenterDeleteURL = CostCentersURL + "{Id}";
        public static string CostCenterDetailURL = CostCentersURL + "{Id}";
        public static string CostCenterUpdateURL = CostCentersURL + "{Id}";

        public static string ServicesURL = BaseURL + "ServiceLists/";
        public static string ServicesListURL = ServicesURL + "Contacts/{contactCode}";
        public static string ServicesDetailURL = BaseURL + "Services/{serviceRef}/DisplayDetails";
        public static string ServicesTypeURL = ServicesURL + "Contacts/{contactCode}/ServiceTypes";
        public static string ServicesTypeDetailURL = ServicesURL + "Contacts/{contactCode}/ServiceTypes/{serviceCode}";
        public static string ServicesPlansURL = ServicesURL + "Contacts/{contactCode}/Plans";
        public static string ServicesPlansDetailURL = ServicesURL + "Contacts/{contactCode}/Plans/{serviceCode}";
        public static string ServicesChangesURL = ServicesURL + "Contacts/{contactCode}/Changes";
        public static string ServicesChangesDetailURL = ServicesURL + "Contacts/{contactCode}/ChangeTypes/{serviceCode}";
        public static string ServicesStatusURL = ServicesURL + "Contacts/{contactCode}/Statuses";
        public static string ServicesStatusDetailURL = ServicesURL + "Contacts/{contactCode}/Statuses/{serviceCode}";
        public static string ServicesCostURL = ServicesURL + "Contacts/{contactCode}/CostCenters";
        public static string ServicesCostDetailURL = ServicesURL + "Contacts/{contactCode}/CostCenters/{serviceCode}";
        public static string ServicesGroupURL = ServicesURL + "Contacts/{contactCode}/ServiceGroups";
        public static string ServicesGroupDetailURL = ServicesURL + "Contacts/{contactCode}/ServiceGroups/{serviceCode}";
        public static string ServicesSitesURL = ServicesURL + "Contacts/{contactCode}/Sites";
        public static string ServicesSitesDetailURL = ServicesURL + "Contacts/{contactCode}/Sites/{serviceCode}";
        public static string ServicesNoteURL = BaseURL + "Services/{code}/Notes";
        public static string ServicesNoteEditURL = BaseURL + "Services/Notes/{noteId}";
        public static string ServicesPlanHistoryURL = BaseURL + "Services/{ServiceReference}/Plans/History";
        public static string ServicesPlanURL = BaseURL + "Services/{ServiceReference}/Plans/History";
        public static string ServicesPlanScheduledURL = BaseURL + "Services/{ServiceReference}/PlanChanges/Scheduled";
        public static string ServicesPlanAvailableURL = BaseURL + "Services/{ServiceReference}/Plans/Available";
        public static string ServicesDocumentListURL = BaseURL + "Services/{ServiceReference}/Documents";
        public static string ServicesDocumentDetailURL = BaseURL + "Services/{ServiceReference}/Document/{DocumentName}";
        public static string ServicesDocumentUploadURL = BaseURL + "Services/{ServiceReference}/Document/Upload";
        public static string ServicesPlanDetailURL = BaseURL + "Services/{ServiceReference}/Plans/Definitions/{Id}";
        public static string ServicesRatesURL = BaseURL + "Services/Plans/{ServiceReference}/TransactionRates";
        public static string ServicesAttributesURL = BaseURL + "Services/{ServiceReference}/Attributes/Instances";
        public static string ServicesAttributeDetailURL = BaseURL + "Services/Attributes/{Id}";
        public static string ServicesDefinitionsURL = BaseURL + "Services/ServiceTypes/{ServiceTypeId}/Attributes/Definitions";
        public static string ServicesBasicURL = BaseURL + "Services/{ServiceReference}/Basic";
        public static string ServiceTypesURL = BaseURL + "Services/ServiceTypes/Select";
        public static string ServiceAddressURL = BaseURL + "Services/{ServiceReference}/Addresses/{Type}";
        public static string ServiceAttrDefinitionsURL = BaseURL + "Services/ServiceTypes/{ServiceTypeId}/Attributes/Definitions";
        public static string ServiceStatusURL = BaseURL + "Services/Statuses/Select";
        public static string ServiceQualificationURL = BaseURL + "ServiceQualification";
        public static string ServicePlansURL = BaseURL + "Services/ServiceTypes/{ServiceTypeId}/Contacts/{ContactCode}/Plans/Available";
        public static string ServiceConfigURL = BaseURL + "Services/ServiceTypes/{ServiceTypeId}/OnBoarding/Configuration";
        public static string ServiceConfigDefaultURL = BaseURL + "Services/OnBoarding/Configuration/Default";
        public static string ServiceAvailableIdURL = "https://virtserver.swaggerhub.com/Selcomm/Services/1.2/ServiceTypes/{ServiceTypeId}/AvailableServiceIds";
        public static string ServiceReserveURL = "https://virtserver.swaggerhub.com/Selcomm/Services/1.2/ServiceIds/Reserve/{ServiceId}/Release";
        public static string ServiceReserveDetailURL = "https://virtserver.swaggerhub.com/Selcomm/Services/1.2/ServiceIds/Reservations/{ServiceTypeId}";
        public static string ServicePasswordURL = BaseURL + "Services/{ServiceReference}/EnquiryPassword";
        public static string ConfigurationsURL = BaseURL + "Services/ServiceTypes/{ServiceTypeId}/{Type}/Configurations";
        public static string TerminationsURL = BaseURL + "Services/{ServiceReference}/Terminations";
        public static string TerminateInfoURL = BaseURL + "Services/{ServiceReference}/Terminations/{Type}";
        public static string TerminateReasonURL = BaseURL + "Services/Terminations/Reasons";

        public static string TransactionsURL = BaseURL + "FinancialTransactions/";
        public static string FinancialsListURL = TransactionsURL + "Statements/List/Accounts/{code}";
        public static string TransactionsListURL = TransactionsURL + "Accounts/{code}";
        public static string TransactionDetilURL = TransactionsURL + "{TransactionId}";
        public static string TransactionNumberURL = TransactionsURL + "Numbers/Types/{Type}";
        public static string TransactionCategoryURL = TransactionsURL + "Categories/Types/{Type}";
        public static string AllocationListURL = TransactionsURL + "Allocations/AllocatableTransactions/Accounts/{AccountCode}";
        public static string AllocationClearURL = TransactionsURL + "Allocations/Accounts/{AccountCode}";
        public static string ReceiptCreateURL = TransactionsURL + "Receipts/Accounts/{AccountCode}";
        public static string TransactionReasonURL = TransactionsURL + "Reasons";
        public static string InvoiceCreateURL = TransactionsURL + "Invoices/Accounts/{AccountCode}";

        public static string PayMethodURL = BaseURL + "PaymentMethods/";
        public static string PayMethodListURL = PayMethodURL + "Contacts/{ContactCode}";
        public static string MakeDetaultURL = PayMethodURL + "{PayMethodId}/MakeDefault";
        public static string ValidateURL = PayMethodURL + "CreditCardNumber/{CardNumber}/Validate";
        public static string DeleteCardURL = PayMethodURL + "{PayMethodId}/StatusChange";
        public static string AddCreditCardURL = PayMethodURL + "Contacts/{ContactCode}/CreditCards";
        public static string AddBankAccountURL = PayMethodURL + "Contacts/{ContactCode}/Banks";
        public static string SurchargeURL = PayMethodURL + "CalculateSurcharge/PaymentMethodId/{PaymentMethodId}/Amount/{Amount}/Source/{Source}";
        public static string AvailablePaymentMethodURL = "https://virtserver.swaggerhub.com/Selcomm/PaymentMethods/1.1/";

        public static string EventURL = BaseURL + "Events/Instances/";
        public static string EventListURL = EventURL + "Contacts/{ContactCode}";
        public static string EventServiceListURL = EventURL + "Services/{ServiceReference}";
        public static string EventDefinitionURL = BaseURL + "Events/Definitions/Services/{ServiceReference}";
        public static string EventDefDetailURL = BaseURL + "Events/Definitions/{Id}";
        public static string EventReasonURL = BaseURL + "Events/Reasons/EventDefinitions/{Id}";
        public static string EventCreateURL = BaseURL + "Events/Instances/Services/{ServiceReference}";

        public static string ChargesURL = BaseURL + "Charges/";
        public static string ChargesHistoryURL = ChargesURL + "Profiles/Accounts/{ContactCode}";
        public static string ChargesEndURL = ChargesURL + "Profiles/{ProfileId}/End";
        public static string ChargesDefinedURL = ChargesURL + "Definitions/Search/Accounts/{ContactCode}";
        public static string ChargesDetailDefURL = ChargesURL + "Definitions/{DefinitionId}";
        public static string ChargesDefinitionURL = ChargesURL + "Definitions";
        public static string ChargesInstanceDetailURL = ChargesURL + "Instances/Profiles/{ProfileId}";
        public static string ChargesServiceURL = ChargesURL + "Profiles/Services/{ServiceReference}";
        public static string ChargesTaxURL = ChargesURL + "Tax/Accounts/{ContactCode}/DefinitionId/{DefinitionId}/Amount/{Amount}";

        public static string TaskURL = BaseURL + "Tasks/";
        public static string TaskUpdateURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/{Id}";
        public static string TaskListURL = TaskURL + "Contacts/{ContactCode}";
        public static string TaskNewURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Contacts/{ContactCode}";
        public static string ServiceTaskListURL = TaskURL + "Services/{ServiceReference}";
        public static string TaskTypeListURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Types";
        public static string RequestorListURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Requestors/Contacts/{ContactCode}";
        public static string TaskStatusListURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Statuses";
        public static string PriorityListURL = TaskURL + "Priorities";
        public static string EmailListURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Emails/Contacts/{ContactCode}";
        public static string CommentsURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/{Id}/Comments";
        public static string CommentsDetailURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Comments/{Id}";
        public static string ResolutionURL = TaskURL + "Resolutions";
        public static string TimeLogsURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/{Id}/TimeLogs";
        public static string DependenciesURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/{Id}/Dependencies";
        public static string ResourcesURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/{Id}/Resources/Assignments";
        public static string TaskNumberURL = "https://virtserver.swaggerhub.com/Selcomm/Tasks/1.0/Numbers/Next";

        public static string UsagesURL = BaseURL + "UsageTransactions/";
        public static string AccountUsagesURL = UsagesURL + "Accounts/{ContactCode}";
        public static string ServiceUsagesURL = UsagesURL + "Services/{ServiceReference}";

        public static string ReportURL = BaseURL + "Reports/";
        public static string ReportListURL = ReportURL + "Definitions";
        public static string ReportDetailURL = ReportURL + "Definitions/{DefinitionId}";
        public static string ReportInstancePostURL = ReportURL + "Schedules";
        public static string ReportSchedulePostURL = ReportURL + "Schedules";
        public static string ReportInstanceURL = ReportURL + "Instances/DefinitionId/{DefinitionId}";
        public static string ReportScheduleURL = ReportURL + "Schedules/DefinitionId/{DefinitionId}";
        public static string ReportDownloadURL = ReportURL + "Download/{Id}";
        public static string ReportInstanceDeleteURL = ReportURL + "Instances/{Id}";
        public static string ReportScheduleDeleteURL = ReportURL + "Schedules/{Id}";
        public static string ReportSchedultEndURL = ReportURL + "Schedules/End/{Id}";

        public static string MessageURL = BaseURL + "Messages/";
        public static string ValidatePhone = MessageURL + "ValidatePhoneNumber/{Param}";
        public static string ValidateEmail = MessageURL + "Emails/ValidateFormat/{Param}";
        public static string ValidateSMS = MessageURL + "SMSs/ValidateNumber/{Param}";
        public static string MessagesListURL = MessageURL + "Contacts/{ContactCode}";
        public static string SMSNumberListURL = MessageURL + "SMSs/Numbers/ContactCode/{ContactCode}";
        public static string EmailAddressListURL = MessageURL + "Emails/Addresses/Contacts/{ContactCode}";
        public static string AvailableDocumentsURL = MessageURL + "AvailableDocuments/Contacts/{ContactCode}";
        public static string SendEmailOrSMSURL = MessageURL + "{Type}/{Contacts}/{ContactCode}";
        public static string TaskMessagesURL = MessageURL + "Tasks/{Id}";
        public static string NotificationConfigURL = "https://virtserver.swaggerhub.com/Selcomm/Messages/1.0/Notifications/Configuration";

        public static string InventoryURL = BaseURL + "Inventory/";
        public static string ProductListURL = InventoryURL + "Available/Products";

        public static string SearchURL = BaseURL + "search/";
        public static string SimpleSearchURL = SearchURL + "Contacts";
        public static string AdvancedSearchURL = SimpleSearchURL + "/Advanced";
        public static string SearchTypeURL = SearchURL + "Services/Types";
        public static string BillingCycleURL = SearchURL + "Billing/Cycles";
        public static string SubTypeURL = SearchURL + "Contacts/SubTypes";
        public static string BussinessUnitURL = SearchURL + "BusinessUnits";
        public static string SearchPlanURL = SearchURL + "Plans";
        public static string SearchStatusURL = SearchURL + "Contacts/Status";

        public static string AccountsURL = BaseURL + "Accounts/";
        public static string BillOptionsURL = AccountsURL + "BillingOptions/AccountCode/{ContactCode}";
        public static string BillFormatsURL = AccountsURL + "BillFormats";
        public static string BillFormatsAccountURL = AccountsURL + "BillFormats/AccountCode/{ContactCode}";
        public static string BillDeliveryOptionURL = AccountsURL + "BillDeliveryOptions/AccountCode/{ContactCode}";
        public static string BillMediaOptionURL = AccountsURL + "BillMediaOptions/AccountCode/{ContactCode}";
        public static string BillIntervalURL = AccountsURL + "InvoiceIntervals/AccountCode/{ContactCode}";
        public static string InvoiceIntervalsURL = AccountsURL + "InvoiceIntervals";
        public static string AvailableCurrenciesURL = AccountsURL + "Currencies/Available/AccountCode/{ContactCode}";
        public static string CurrencyList = AccountsURL + "Currencies";
        public static string BillTaxesURL = AccountsURL + "Taxes";
        public static string TaxExemptionsURL = AccountsURL + "TaxExemptions/AccountCode/{ContactCode}";
        public static string ExemptionsUpdateURL = AccountsURL + "TaxExemptions/AccountCode/{ContactCode}/Id/{Id}";
        public static string TaxRatesURL = AccountsURL + "TaxRates";
        public static string TaxTransactionTypesURL = AccountsURL + "TaxTransactionTypes";
        public static string BillTermsURL = AccountsURL + "Terms";
        public static string TermsAccountURL = AccountsURL + "Terms/AccountCode/{ContactCode}";
        public static string BillReturnReasonURL = AccountsURL + "ReturnReasons";
        public static string BillReturnReasonListURL = AccountsURL + "ReturnReasons/AccountCode/{ContactCode}";
        public static string BillReturnReasonDeleteURL = AccountsURL + "ReturnReasons/AccountCode/{ContactCode}/Id/{Id}";
        public static string BillProofingURL = AccountsURL + "BillProofs/AccountCode/{ContactCode}";
        public static string BillAvailableCycleURL = AccountsURL + "BillCycles/Available/AccountCode/{ContactCode}";
        public static string BillCycleURL = AccountsURL + "BillCycles/AccountCode/{ContactCode}";
        public static string BillCycleBussinessURL = AccountsURL + "BillCycles/Available/BusinessUnitCode/{BusinessUnitCode}";
        public static string BillRunExclusionsURL = AccountsURL + "BillRunExclusions/AccountCode/{ContactCode}";
        public static string BillExclusionsDetailURL = AccountsURL + "BillRunExclusions/AccountCode/{ContactCode}/Id/{Id}";
        public static string BillPeriodsURL = AccountsURL + "BillPeriods";
        public static string AccountNextIdURL = AccountsURL + "NextId";
        public static string AccountConfiguURL = AccountsURL + "OnBoarding/Configuration/{AccountType}";

        public static string PlanURL = BaseURL + "Accounts/Plans/";
        public static string PlanHistoryURL = PlanURL + "History/{ContactCode}";
        public static string PlanAvailableURL = AccountsURL + "{AccountCode}/Plans/Available";
        public static string PlanDefinitionURL = BaseURL + "Plans/Definitions/{Id}";
        public static string PlanScheduledURL = AccountsURL + "{AccountCode}/PlanChanges/Scheduled";

        public static string DocumentsURL = BaseURL + "Documents/";
        public static string DocumentsInfoURL = "https://virtserver.swaggerhub.com/Selcomm/Documents/2.0/Documents/Information";

        public static string AuthrisationURL = BaseURL + "Authorisations";

        public static string AuthenticationURL = BaseURL + "Authentication";
        public static string SelcommUserLockURL = AuthenticationURL + "SelcommUser/Lockout/{Identifier}";



    }
}
