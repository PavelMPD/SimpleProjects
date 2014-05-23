<%@ Page Title="Code Editor" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM204580.aspx.cs" Inherits="Pages_SM_SM204580"
	EnableViewStateMac="False" EnableViewState="False" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.CodeMirror-wrapping
		{
			background-color: White;
			border: 1px solid gray;
			margin: 0px;
			border-right-width: 0px;
			border-left-width: 0px;
		}
		.CodeMirror-line-numbers
		{
			font-family: monospace;
			font-size: 10pt;
			color: gray;
			padding: .4em;
			background-color: #eee;
			border-right: 1px solid gray;
			text-align: right;
			padding-left: 15pt;
		}
	</style>
	<script type="text/javascript" language="javascript">
		window.isClientDirty = true;
		var IsCodeEditorWindow = true;
		function FireResize()
		{
			px_cm.notifyOnResize();
		}
	</script>
	<script src='<%=GetScriptName("codemirror.js")%>' type="text/javascript">
	</script>
	<script type="text/javascript" language="javascript">

		window.IsEditorActive = false;
		window.proxy = null;
		function ActivateCsEditor(editor) {
			if (window.IsEditorActive)
				return;
			window.IsEditorActive = true;

 
			var config = {
				parserfile: ["<%=GetScriptName("tokenizecsharp.js")%>", 
					"<%=GetScriptName("parsecsharp.js")%>"],
			
				stylesheet: "<%=GetScriptName("csharpcolors.css")%>",
				//path: "cseditor/",
				height: "100%",
				autoMatchParens: true,
				textWrapping: false,
				lineNumbers: true,
				tabMode: "shift",
				enterMode: "keep",
				content: editor.getValue(),
				basefiles: [
					"<%=GetScriptName("util.js")%>",
					"<%=GetScriptName("stringstream.js")%>",
					"<%=GetScriptName("select.js")%>",
					"<%=GetScriptName("undo.js")%>",
					"<%=GetScriptName("editor.js")%>",
					"<%=GetScriptName("tokenize.js")%>"]
        
			//  "util.js", "stringstream.js", "select.js", "undo.js", "editor.js", "tokenize.js"],

       

			};

			var p = document.getElementById("SourcePlacehoder");

			var replace = function(newElement)
			{
				p.appendChild(newElement);

			};
			window.proxy = new CodeMirror(replace, config);

			editor.onCallback = function() 
			{
				if(window.proxy.editor != null)
				{
					var code = window.proxy.getCode();
					editor.updateValue(code);

				}

        

			};
    

			editor.baseRepaintText = editor.repaintText;
			editor.repaintText = function(v) 
			{
        
				if (v === null || v === undefined)
					v = "";
				if(window.proxy.editor != null)
					window.proxy.setCode(v);
				editor.baseRepaintText(v);
			};

    
    

			window.proxy.wrapping.id = "csproxywrapping";
    

		                                  }


		function JumpLine(file, line) {

			if (!IsEditorActive)
				return false;
		
			var combo = GetObject("edFileName");
			var comboValue = combo.getValue();
			if (comboValue != file) {
				combo.updateValue(file);
				return false; 
		
			                        }

			var lineHandle = proxy.nthLine(line);
			if (!!lineHandle)
				proxy.jumpToLine(lineHandle);

			return true;
		                              }


		var px_all2;
		function IndexObjects()
		{
			if(px_all2)
				return;
			px_all2 = {};
			for(var n in px_all)
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

		function HideCompilerPanel()
		{
			var c = GetObject("PanelCompiler");
			c.hide();
	

		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.SM.GraphCodeFiles"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionNewFile" 
                RepaintControls="Bound" 
                BlockPage="true"
				Visible="false" 
                CommitChanges="True" 
                 />
			<px:PXDSCallbackCommand Name="actionValidate" RepaintControls="None" BlockPage="true"
				CommitChanges="True" RepaintControlsIDs="FormFilter" PostData="Page" PostDataControls="True" />
			<px:PXDSCallbackCommand Name="actionVisualStudio" RepaintControls="None" BlockPage="true"
				CommitChanges="True" RepaintControlsIDs="FormFilter" PostData="Page" PostDataControls="True" />
			<px:PXDSCallbackCommand Name="actionNewFileDlg" PopupPanel="PanelNewFile" />
		</CallbackCommands>
		<ClientEvents ButtonClick="HideCompilerPanel" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="FormFilter" runat="server" DataMember="Filter" DataSourceID="ds"
		Width="100%"  Caption="Project">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True"/>
			
			<px:PXSelector CommitChanges="True" runat="server" DataField="ProjectID" ID="edWorkingProject"
				AutoAdjustColumns="True" DataSourceID="ds"/>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True"/>
			
			<px:PXSelector CommitChanges="True" ID="edFileName" runat="server" DataField="ObjectID"
				AutoRefresh="True" AutoAdjustColumns="True" DataSourceID="ds" />
		</Template>
	</px:PXFormView>
	<px:PXFormView ID="FormEditContent" runat="server" DataMember="Files" DataSourceID="ds"
		Style="position: absolute; left: 0px; top: 0px; width: 200px; display: none;
		visibility: hidden">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXTextEdit SuppressLabel="True" Height="20px" ID="EventEditBox" runat="server"
				DataField="FileContent" TextMode="MultiLine" Font-Names="Courier New" Font-Size="10pt"
				Wrap="False" SelectOnFocus="False">
				<ClientEvents Initialize="ActivateCsEditor" />
			</px:PXTextEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="PanelSource" runat="server" Style="width: 100%; height: 300px;"
		RenderVisible="True" Position="Original" AllowMove="False" AllowResize="False"
		AutoSize-Enabled="True" AutoSize-Container="Window" Overflow="Hidden"
		 SkinID="Transparent"
		>
		<div id="SourcePlacehoder" style="width: 100%; height: 100%;">
		</div>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelCompiler" runat="server" Style="height: 250px; width: 100%;"
		CaptionVisible="True" Caption="Output" IFrameName="InnerCompiler" InnerPageUrl="~/Controls/Publish.aspx?compile=yes"
		RenderIFrame="True" Position="Original" WindowStyle="Flat" AutoReload="True"
		AllowMove="False" Key="ViewValidate" ClientEvents-AfterHide="FireResize" ClientEvents-AfterShow="FireResize"
		BlockPage="False" ClientEvents-BeforeLoad="FireResize">
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelNewFile" runat="server" Caption="Create Code File" 
		CaptionVisible="True" Height="120px" Width="442px" Style="z-index: 101; left: 90px;
		position: static; top: 3100px">
		<px:PXFormView ID="FormNewFile" runat="server" CaptionVisible="False" DataMember="FilterNewFile"
			DataSourceID="ds" Style="z-index: 103; left: 18px; position: static; top: 9px"
			Width="396px" AutoRepaint="False" BorderStyle="None" SkinID="Transparent">
			<ContentStyle BorderStyle="None" />
		    <Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDropDown CommitChanges="True" ID="edFileTemplateName" runat="server" AllowNull="False"
					DataField="FileTemplateName" Required="True" />
				<px:PXTextEdit ID="edFileClassName" runat="server" DataField="FileClassName" Required="True" />
				<px:PXCheckBox runat="server" ID="edGenerateDac" DataField="GenerateDacMembers" />
				<px:PXLayoutRule runat="server" StartRow="true" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="ButtonOk" runat="server" DialogResult="OK" Text="OK">
						<AutoCallBack Target="ds" Command="actionNewFile" />
					</px:PXButton>
					<px:PXButton ID="ButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
</asp:Content>
