using PX.Common;
using PX.Data;
using PX.SM;

namespace PX.Objects.SM
{
	public class NotificationService : INotificationService
	{
		private PXGraph _graph;

		public INotification Read(string notificationCD)
		{
			PXSelect<Notification,
					Where<Notification.notificationID, Equal<Required<Notification.notificationID>>>>.
				Clear(Graph);
			return notificationCD.With(_ => (Notification)PXSelect<Notification,
					Where<Notification.notificationID, Equal<Required<Notification.notificationID>>>>.
				Select(Graph, _));
		}

		private PXGraph Graph
		{
			get { return _graph ?? (_graph = new PXGraph()); }
		}
	}
}
