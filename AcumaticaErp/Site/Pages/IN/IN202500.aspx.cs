using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Web.UI;

public partial class Page_IN202500 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{

		var mainContainer = this.tab.Items.Items.First(ti => String.Equals(ti.Key, "Subitems", StringComparison.OrdinalIgnoreCase)).TemplateContainer.Controls;
		
		if (mainContainer.Count != 0) 
			mainContainer[1].Visible = false;

		PXGraph graph = this.ds.DataGraph;
		List<string> views = (from view in graph.Views where view.Key.StartsWith("SubItem_") select view.Key).ToList();
		PXSplitContainer parentContainer = null;

		List<object> segments = null;
		if (graph.Views.ContainsKey("DimensionsSubItem"))
			segments = graph.Views["DimensionsSubItem"].SelectMulti();

		for(int i=0; i<views.Count; i++)
		{
			ControlCollection controls = mainContainer;
			if (i == views.Count - 1)
			{
				if (parentContainer != null)
					controls = parentContainer.TemplateContainer2.Controls;
			}
			else
			{
				PXSplitContainer container = new PXSplitContainer();
				container.ID = "splitSubItem" + i;
				container.SplitterPosition = 300;
				container.Height = 600;
				container.SkinID = "Horizontal";
				container.AutoSize.Enabled = true;
				container.AutoSize.Container = DockContainer.Window;				

				if (parentContainer == null)				
					mainContainer.Add(container);				
				else
					parentContainer.TemplateContainer2.Controls.Add(container);
				container.ApplyStyleSheetSkin(this.Page);

				controls = container.TemplateContainer1.Controls;
				parentContainer = container;
			}
				

			PXGrid grid = new PXGrid();
			grid.ID = "gridSubItem" + i;
			grid.DataSourceID = this.ds.ID;			
			grid.Levels.Add(new PXGridLevel()
				{
				DataMember = views[i]
				});
			grid.AutoSize.Enabled = true;
			grid.Height = new Unit(150, UnitType.Pixel);
			grid.Width = new Unit(100,UnitType.Percentage);
			grid.Style.Add(HtmlTextWriterStyle.ZIndex, "100");
			grid.SkinID = "ShortList";
			//grid.ActionBar.ActionsVisible = false;
			//grid.ActionBar.PagerVisible = ActionVisible.False;
			grid.Columns.Add(new PXGridColumn()
			{
				DataField = "Active",
				Type = GridColumnType.CheckBox,
				AllowCheckAll = true,
				Width = new Unit(75, UnitType.Pixel)
			});
			grid.Columns.Add(new PXGridColumn()
					{
					DataField = "Value", 
					Width = new Unit(100, UnitType.Pixel)					
					});
			grid.Columns.Add(new PXGridColumn()
			{
				DataField = "Descr",
				Width = new Unit(250, UnitType.Pixel)
			});			
			if (segments != null && segments.Count > i)
			{
				grid.Caption = ((Segment) segments[i]).Descr;
				grid.CaptionVisible = true;
			}			
			controls.Add(grid);
			grid.ApplyStyleSheetSkin(this.Page);
			grid.AutoAdjustColumns = false;
		}
	}
}
