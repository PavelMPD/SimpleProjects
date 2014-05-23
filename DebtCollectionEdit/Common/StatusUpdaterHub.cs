using Microsoft.AspNet.SignalR;

namespace DebtCollection.Common
{
    public class StatusUpdateHub : Hub
    {
        public static void StatusUpdated(int oldStatusOrderId, int newStatusOrderId, int debtorsCount)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<StatusUpdateHub>();
            dynamic clients = context.Clients.All;
            clients.RefreshStatusCounters(oldStatusOrderId, newStatusOrderId, debtorsCount);
        }
    }
}
