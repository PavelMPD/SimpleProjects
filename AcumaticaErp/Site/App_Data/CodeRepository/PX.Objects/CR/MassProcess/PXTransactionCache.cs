using System;
using System.Collections;
using System.Linq;
using System.Text;
using PX.Data;


namespace PX.Objects.CR.MassProcess
{
	class PXTransactionCache<T> : PXExtensableCache<T>,IPXTransactionCache where T : class,IBqlTable, new()
	{
		private CacheBackup _backup;

		public PXTransactionCache(PXGraph graph, PXCache<T> oldCache) : base(graph, oldCache)
		{
		}

		public void Backup()
		{
			_backup = new CacheBackup {Current = CreateCopy(this.Current)};
			Cached.Cast<object>()
				  .ForEach( i =>  _backup.Values.Add(new ObjectEntry(GetStatus(i),
																	CreateCopy(i))
													));

		}

		public void Restore()
		{
			Clear();
			try
			{
				foreach (var backupItem in _backup.Values)
				{
					switch (backupItem.Status)
					{
						case PXEntryStatus.Notchanged:
							PlaceNotChanged(backupItem.Object);
							break;
						case PXEntryStatus.Updated:
							PlaceUpdated((T) backupItem.Object);
							break;
						case PXEntryStatus.Inserted:
							PlaceInserted((T) backupItem.Object);
							break;
						case PXEntryStatus.Deleted:
							PlaceDeleted((T) backupItem.Object);
							break;
						case PXEntryStatus.InsertedDeleted:
							break;
						case PXEntryStatus.Held:
							break;
					}
				}
			}
			catch(Exception ex)
			{
				Clear();
				throw new Exception("Critical error occured in cache transaction rollback process: " + ex.Message,ex);
			}
		}

		#region IPXTransactionCache Members


		public void Commit()
		{
			Persist(PXDBOperation.Delete);
			Persist(PXDBOperation.Insert);
			Persist(PXDBOperation.Update);
			Cached.Cast<object>().ForEach(i=> this.SetStatus(i,PXEntryStatus.Notchanged));
		}

		#endregion
	}
}
