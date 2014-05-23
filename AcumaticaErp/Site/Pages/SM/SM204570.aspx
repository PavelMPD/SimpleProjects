<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM204570.aspx.cs" Inherits="Pages_SM_SM204570"
	EnableViewState="False" EnableViewStateMac="False" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.SourceBrowser" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="actionConvertPage" Visible="False"/>
		    <px:PXDSCallbackCommand Name="actionViewFile" Visible="False" RepaintControls="Bound"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="FormHidden" Style="display: none" DataSourceID="ds"
		DataMember="Filter" Width="100%" AllowAutoHide="False">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
			<px:PXTextEdit SuppressLabel="True" ID="edGeneratedDacSource" runat="server" DataField="GeneratedDacSource"
				TextMode="MultiLine" ReadOnly="True" />
            
           <px:PXTextEdit SuppressLabel="True" ID="edSourceFile" runat="server" DataField="SourceFile"
				TextMode="MultiLine" ReadOnly="True" />
			<px:PXTextEdit SuppressLabel="True" ID="EditGraphSource" runat="server" DataField="ReadonlyEventSource"
				Height="99px" TextMode="MultiLine" Font-Names="Courier New" Font-Size="10pt"
				ReadOnly="True" Wrap="False" SelectOnFocus="False">
				<Padding Left="10px" />
			</px:PXTextEdit>
			<px:PXTextEdit SuppressLabel="True" ID="edAspxCode" runat="server" DataField="AspxCode"
				TextMode="MultiLine" ReadOnly="True" />
			<px:PXTextEdit SuppressLabel="True" ID="EditTableOfContent" runat="server" DataField="TableOfContent"
				ReadOnly="True" /></Template>
		<ClientEvents Initialize="InitEventEditor" />
	</px:PXFormView>
	<script>
		var px_all2;
		function IndexObjects()
		{
			if (px_all2)
				return;
			px_all2 = {};
			for (var n in px_all)
			{
				var names = n.split("_");
				var s = names[names.length - 1];
				px_all2[s] = px_all[n];
			}
		}

		function GetObject(id)
		{
			IndexObjects();
			return px_all2[id];
		}
		var IsInitEvents = false;
		function InitEventEditor(a, b)
		{
			//debugger;
			if (IsInitEvents)
				return;

			IsInitEvents = true;
			a.events.addEventHandler("afterRepaint", UpdateSourceCode);

		}

		function UpdateCode(srcId, destId)
		{

			var target = document.getElementById(destId);
			if (!target)
			{
				alert("SourceCodePlaceholder not found " + destId);
				return;
			}
			var src = GetObject(srcId);
			if (!src)
			{
				alert("Edit control not found " + srcId);
				return;
			}

			var html = src.getValue();
			if (html == null)
				html = "";

			if ("outerHTML" in target)
			{
				var tag = target.nodeName;
				target.innerHTML = "<span></span>";
				target.firstChild.outerHTML = "<" + tag + ">" + html + "</" + tag + ">";

			}
			else
			{
				target.innerHTML = html;

			}

		}
		function UpdateSourceCode()
		{

			UpdateCode("EditGraphSource", "Pre1");
			UpdateCode("edGeneratedDacSource", "Pre2");
			UpdateCode("edSourceFile", "Pre3");
			UpdateCode("edAspxCode", "Pre4");

			UpdateCode("EditTableOfContent", "Span1");
			//ActivateCsEditor();

			//	target = document.getElementById("Span1");
			//	html = document.getElementById(editTableOfContentId).value;
			//	target.innerHTML = html;


		}

		function ControlAutoResize(elem, ax, x, ay, y)
		{

			px_cm.registerAutoSize({ element: elem, ID: elem.id },
		{ autoSize: {
			enabled: true
		, container: 0
		, valign: ay
		, bottom: y
		, align: ax
		, right: x
		, dockMethod: 2
		}
		});

		}


		function toggleRegion(targetId)
		{
			var link = document.getElementById("link" + targetId);
			var target = document.getElementById(targetId);
			var disp = target.style.display;
			var isHidden = (disp == "none");
			target.style.display = isHidden ? "" : "none";
			link.className = isHidden ? "csharpregion-start" : "csharpregion";

		}

		function expandParentRegions(id)
		{
			var target = document.getElementById(id);
			while (target && target.style)
			{
				if (target.style.display == "none")
				{
					toggleRegion(target.id);
					//target.style.display = "";

				}

				target = target.parentNode;
			}
		}



	</script>
	<px:PXTab ID="PXTab1" runat="server" DataMember="Filter" DataSourceID="ds" Width="100%"
		 AutoRepaint="False" SelectedIndex="1"
		Style="bottom: 0px;"
		AllowFocus="False" RepaintOnDemand="False" AllowAutoHide="False">
		<Items>
			<px:PXTabItem Text="Page Aspx">
				<Template>
					<px:PXPanel ID="PXPanel4" runat="server">
						<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" Merge="True"/>
						<px:PXSelector ID="PXSelector1" runat="server" CommitChanges="True" DataField="ScreenID"/>
						<px:PXButton runat="server" ID="BtnConvert">
							<AutoCallBack Target="ds" Command="actionConvertPage"/>
						</px:PXButton>
						
					</px:PXPanel>
					<div id="PageAspxCodeContainer" runat="server" style="right: 0px;
						margin: 9px; position: absolute; left: 0px; top: 40px; bottom: 0px; font-size: 10pt;
						font-family: 'Courier New', Monospace; background-color: white; border: gray 1px solid;
						padding: 5px; cursor: text; overflow: auto;">
						<pre id="Pre4"></pre>
					</div>
<%--					<px:ClientScript runat="server" ID="ClientScript24">
						ControlAutoResize(this, 0, 15, 0, 15);
						//px_cm.registerAutoSize({element:this,ID:this.id}, {autoSize:{enabled:true,container:0,bottom:30,right:20,dockMethod:2}});					
					</px:ClientScript>--%>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Business logic">
				<Template>
					<px:PXPanel ID="PXPanel3" runat="server">
						<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
					    <px:PXSelector ID="PXSelector2" runat="server" CommitChanges="True" DataField="GraphName" DataSourceID="ds" AutoAdjustColumns="True" />
					</px:PXPanel>
					<px:PXSplitContainer AllowAutoHide="False" ID="SplitSource" runat="server" Orientation="Vertical" Width="100%" style="bottom: 0px;">
						<AutoSize Enabled="True" />
						<Template1>
							<div id="TableOfContentBounds" runat="server" 
								style="overflow: auto;
								padding-left: 10px;
								padding-top: 10px;
								position: absolute;
								left: 0px;
								top: 0px;
								right: 0px; 
								bottom: 0px; 
								border: gray 0px solid;
								background-color: white; text-decoration: none;">
								<span id="Span1"></span>
							</div>						
						</Template1>
						
						<Template2>
					<div id="SourceCodeContainer" runat="server" 
					style="width: 100%;
						
						height: 100%; font-size: 10pt; font-family: 'Courier New', Monospace;
						background-color: white; border: gray 0px solid; cursor: text;
						overflow: auto;">
						<pre id="Pre1"></pre>
					</div>							
						</Template2>
					</px:PXSplitContainer>


				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Data table declaration">
				<Template>
					<px:PXPanel ID="PXPanel2" runat="server">
						<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					    <px:PXSelector ID="PXSelector3" runat="server" CommitChanges="True" 
							DataField="TableName" />
					</px:PXPanel>
					<div id="dacCodeContainer" runat="server" 
					style="position: absolute;
						left: 0px; top: 40px; right: 0px; bottom: 0px; margin: 9px; font-size: 10pt;
						font-family: 'Courier New', Monospace; background-color: white; border: gray 1px solid;
						padding: 5px; cursor: text; overflow: auto;">
						<pre id="Pre2"></pre>
					</div>
<%--					<px:ClientScript runat="server" ID="ClientScript22">
						ControlAutoResize(this, 0, 15, 0, 15);
					</px:ClientScript>--%>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Find in Files">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server">
						<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					    <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="FindText" CommitChanges="True" />
					    <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					    <px:PXButton ID="PXButton1" runat="server" Text="Find">
							<AutoCallBack Command="Save" Target="PXTab1" />
					    </px:PXButton>
					</px:PXPanel>
					<px:PXGrid runat="server" ID="FormFindResults" DataSourceID="ds" Width="100%" Height="100%"
						Style="position: absolute; left: 0px; top: 49px;" 
						SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="ViewFindResults">
							    <Columns>
							        <px:PXGridColumn DataField="Name"  Width="250px" LinkCommand="actionViewFile"/>    
							        <px:PXGridColumn DataField="Line" Width="50px"/>    
							        <px:PXGridColumn DataField="Content" Width="600px"/>    
							    </Columns>
								
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
		<ClientEvents Initialize="UpdateSourceCode" />
	</px:PXTab>
    
    
    <px:PXSmartPanel runat="server" ID="PanelViewFile"
        Key="ViewFindResults"
        Caption="Source File" CaptionVisible="True"
        Width="800px" 
        Height="600px"
        ShowMaximizeButton="True"
        
        >
        
       				<div id="fileCodeContainer" runat="server" 
					style="position: absolute;
						left: 0px; top: 0px; right: 0px; bottom: 0px; margin: 9px; font-size: 10pt;
						font-family: 'Courier New', Monospace; background-color: white; border: gray 1px solid;
						padding: 5px; cursor: text; overflow: auto;">
						<pre id="Pre3"></pre>
					</div>
        

        

    </px:PXSmartPanel>
</asp:Content>
