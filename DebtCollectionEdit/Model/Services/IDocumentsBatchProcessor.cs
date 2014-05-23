using DebtCollection.Model.Enums;
using DebtCollection.Model.Services.Dtos;
using System.Collections.Generic;
using System.ServiceModel;

namespace DebtCollection.Model.Services
{
    [ServiceContract(Namespace = Namespaces.ForDocumentsBatchProcessor)]
    public interface IDocumentsBatchProcessor
    {
        [OperationContract]
        OperationResult RunNewOperation(ActionParametersDto parameters,
                             DocumentType documentType, bool runAsync);

        [OperationContract]
        OperationResult RerunAction(ActionParametersDto parameters, bool runAsync);

        [OperationContract]
        OperationResult RunOperationAction(ActionParametersDto parameters,
            OperationAction operationAction, bool runAsync);

        [OperationContract]
        IList<OperationDto> GetRunningOperations(long debtorId);

        [OperationContract]
        DocumentOperations GetOperations(int pageSize, int page, OrderDto[] orders, DocumentType documentType,
            List<OperationAction> operationActions);

        [OperationContract]
        OperationDto GetOperation(long operationId);

        [OperationContract]
        OperationDebtorCollection GetClaimListDetails(long claimListId, int pageSize, int page, OrderDto[] orderings);

        [OperationContract]
        OperationDebtorCollection GetEndorsementsListDetails(long paymentListId, int pageSize, int page, OrderDto[] orderings);
    }
}
