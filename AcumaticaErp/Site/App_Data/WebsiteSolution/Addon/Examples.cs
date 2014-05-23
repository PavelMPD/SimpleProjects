using PX.Data;


#region Overriding existing Graph


namespace PX.Objects.GL
{
    /// <summary>
    /// To override behavior of existin graph
    /// we need to introduce a new class inherited from the graph.
    /// The name of class is the same as name of the graph, with the prefix 'Cst_'
    /// And this class must be declared in the same namespace as original graph.
    /// If all conditions are satisfied, this class will be automatically used by the system
    /// instead of original graph
    /// </summary>
    public class Cst_JournalEntry: JournalEntry
    {
        protected override void Batch_RowSelected(PX.Data.PXCache cache, PX.Data.PXRowSelectedEventArgs e)
        {
            base.Batch_RowSelected(cache, e);
        }
    }

}

#endregion


#region Declaring new Graph


public class SampleGraph: PXGraph<SampleGraph, FilterRow>
{
    
    protected virtual void _(Events.RowSelected<FilterRow> e)
    {
        
    }
}


public class FilterRow : IBqlTable
{

}

#endregion