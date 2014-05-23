function OpenPageInfoContextMenu(e)
{

	if (!e.ctrlKey)
		return;

	__px(this).cancelEvent(e);


	var targetId = "";
	var dataCmd = "";
	for (var target = e.target;
					target != null;
					target = target.parentNode)
	{
		if (target.nodeName == "LABEL")
		{
			targetId = target.getAttribute("for");
			if (targetId != "" && targetId)
				break;
		}


		var cmd = target.getAttribute("data-cmd");
		if (cmd && cmd != "")
			dataCmd = cmd;


		if (target.id != "" && target.id != null)
		{
			targetId = target.id;
			break;
		}

	}

	var frame = GetPageInfoContextMenu();

	frame.src = "../../API/OpenItem.aspx?id=" + escape(targetId)
					+ "&cmd=" + escape(dataCmd)
					+ "&page=" + escape(window.location.href);

	frame.style.left = e.clientX + "px";
	frame.style.top = e.clientY + "px";
	frame.style.visibility = "visible";



	return false;

}

function GetPageInfoContextMenu()
{
	var menuId = "pageInfoContextMenu";
	var frame = document.getElementById(menuId);
	if (!frame)
	{

		frame = document.createElement("IFRAME");
		frame.setAttribute("id", menuId);
		document.body.appendChild(frame);
	}
	return frame;
}

function ClosePageInfo()
{
	var frame = GetPageInfoContextMenu();
	frame.style.visibility = "hidden";
}




function ClosePageInfoContextMenu(e)
{
	ClosePageInfo();

	if (e.ctrlKey)
		__px(this).cancelEvent(e);
}


