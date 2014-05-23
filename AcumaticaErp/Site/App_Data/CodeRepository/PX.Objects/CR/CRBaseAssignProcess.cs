using System;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	public class PXPrimaryGraphCollection
	{
		public PXPrimaryGraphCollection(PXGraph graph)
		{
			Graph = graph;
		}

		private readonly PXGraph Graph;
		private readonly Dictionary<Type, PXGraph> Graphs = new Dictionary<Type, PXGraph>();

		public PXGraph this[IBqlTable row]
		{
			get 
			{ 
				Type itemType = row.GetType();
				PXCache cache = Graph.Caches[itemType];
				Type graphType;
				object copy = cache.CreateCopy(row);
				PXPrimaryGraphAttribute.FindPrimaryGraph(cache, ref copy, out graphType);
				return this[graphType];
			}
		}

		public PXGraph this[Type graphType]
		{
			get
			{
				PXGraph graph = null;
				if (graphType != null && !Graphs.TryGetValue(graphType, out graph))
				{
					graph = PXGraph.CreateInstance(graphType);
					PXDBAttributeAttribute.Activate(graph.Views[graph.PrimaryView].Cache);
					Graphs[graphType] = graph;
				}
				if (graph != null) graph.Clear();
				return graph;
			}
		}
	}

	public interface IMassProcess<in TPrimary>
		where TPrimary : class, IBqlTable, new()
	{
		void ProccessItem(PXGraph graph, TPrimary item);
	}

	public abstract class CRBaseMassProcess<TGraph, TPrimary> : PXGraph<TGraph>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new() 
		where TPrimary : class, IBqlTable, new()
	{
		public PXCancel<TPrimary> Cancel;
		private readonly PXPrimaryGraphCollection primaryGraph;

		protected CRBaseMassProcess()
		{
			primaryGraph = new PXPrimaryGraphCollection(this);
			if (ProcessingDataMember == null)
				throw new PXException(string.Format("{0} is not processing graph", typeof(TGraph).FullName));

			Actions["Schedule"].SetVisible(false);
			foreach (Type table in ProcessingDataMember.View.BqlSelect.GetTables())
			{
				PXDBAttributeAttribute.Activate(Caches[table]);
			}

			ProcessingDataMember.SetParametersDelegate(delegate
				{
					bool result = AskParameters();
					Unload();
					TGraph process;
					using (new PXPreserveScope())
						process = CreateInstance<TGraph>();
					
					ProcessingDataMember.SetProcessDelegate(item =>
						{
							PXGraph graph = primaryGraph[item];
							if(graph == null)
								throw new PXException(ErrorMessages.CantDetermineGraphType);
							process.ProccessItem(graph, item);
							graph.Actions.PressSave();
						});
					return result;
				});

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(Caches[typeof(BAccount)], Messages.BAccountCD);
		}

		private PXProcessing<TPrimary> _items;
		public PXProcessing<TPrimary> ProcessingDataMember 
		{
			get
			{
				return _items ?? (_items = (typeof(TGraph).GetFields(BindingFlags.Public | BindingFlags.Instance)
														 .Where(
															 field =>
															 typeof(PXProcessing<TPrimary>).IsAssignableFrom(field.FieldType))
														 .Select(field => (PXProcessing<TPrimary>)field.GetValue(this)))
					.FirstOrDefault()); 
			}
		}

		public abstract void ProccessItem(PXGraph graph, TPrimary item);
		protected virtual bool AskParameters()
		{
			return true;
		}
	}


	public abstract class CRBaseAssignProcess<TGraph, TPrimary, TAssignmentMapField> : CRBaseMassProcess<TGraph, TPrimary>, IMassProcess<TPrimary>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new() 
		where TPrimary : class, IBqlTable, IAssign, new() 
		where TAssignmentMapField : IBqlField
	{
		protected  EPAssignmentProcessHelper<TPrimary> processor;

		protected CRBaseAssignProcess()
		{
			processor = CreateInstance<EPAssignmentProcessHelper<TPrimary>>();
		}

		public override void ProccessItem(PXGraph graph, TPrimary item)
		{
			int? assingmentMapId = GetAssignmentMapId(graph);
			if (assingmentMapId == null)
				throw new PXException(Messages.AssignmentMapIdEmpty);

			PXCache<TPrimary> cache = (PXCache<TPrimary>)graph.Caches[typeof(TPrimary)];
			TPrimary upd = PXCache<TPrimary>.CreateCopy(item);

			if (!processor.Assign(upd, assingmentMapId))
				throw new PXException(Messages.AssignmentError);
			cache.Update(upd);
		}

		private static int? GetAssignmentMapId(PXGraph graph)
		{
			BqlCommand search = (BqlCommand)Activator.CreateInstance(BqlCommand.Compose(typeof(Search<>), typeof(TAssignmentMapField)));
			PXView view = new PXView(graph, true, BqlCommand.CreateInstance(search.GetSelectType()));
			object row = view.SelectSingle();
			return row.With(_ => (int?)view.Cache.GetValue(_, ((IBqlSearch)search).GetField().Name));
		}
	}
}
