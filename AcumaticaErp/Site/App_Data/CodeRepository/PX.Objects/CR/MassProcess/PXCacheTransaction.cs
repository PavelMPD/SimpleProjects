using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{
	class PXCacheTransaction : IUnitOfWork
	{
		private PXTransactionScope _dbTransactionScope = new PXTransactionScope();
		private readonly IPXTransactionCache[] _caches;
		private bool _commited = false;
		private bool _rolledBack = false;


		public PXCacheTransaction(List<PXCache> caches)
		{
			var transactionCaches = new List<IPXTransactionCache>();

			PXGraph graph = caches.Select(c => c.Graph).FirstOrDefault();
			foreach (PXCache cache in caches)
			{
				if(cache.Graph != graph) throw new ArgumentException("All caches in transaction must belong to the same graph");

				Type extCacheConcreteType =
								typeof(PXTransactionCache<>).MakeGenericType(cache.GetType().GetGenericArguments());

				var extCache = (IPXTransactionCache)Activator.CreateInstance(extCacheConcreteType, cache.Graph, cache);
				extCache.LoadIntoGraph(cache.Graph);
				extCache.Backup();
				transactionCaches.Add(extCache);
			}
			_caches = transactionCaches.ToArray();
		}

		#region IUnitOfWork Members

		//persists caches's graphs
		public void Commit()
		{
			try
			{
				_caches.ForEach(c => c.Commit());

				_dbTransactionScope.Complete();

				_caches.Distinct(c => c.Graph)
				       .ForEach(c => c.Graph.SelectTimeStamp());
				_commited = true;
				_rolledBack = false;

			}
			catch
			{
				Rollback();
				throw;
			}
		}

		public void Rollback()
		{
		  _caches.ForEach(c => c.Restore());
		  _dbTransactionScope.Dispose();
		  _dbTransactionScope = new PXTransactionScope();
		  _commited = false;
		  _rolledBack = true;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (!_commited && !_rolledBack) Rollback();

			_dbTransactionScope.Dispose();
		}

		#endregion
	}
}
