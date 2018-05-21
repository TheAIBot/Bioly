function setGraph(nodes, edges)
{
	window.graphDiv = cytoscape(
	{
		container: document.getElementById('graphDiv'),

		boxSelectionEnabled: false,
		autounselectify: true,

		layout: 
		{
			name: 'dagre'
		},

		style: 
		[
			{
				selector: 'node',
				style: 
				{
					'content': 'data(label)',
					'text-opacity': 0.5,
					'text-valign': 'center',
					'text-halign': 'right',
					'background-color': '#11479e',
					'text-wrap': 'wrap'
				}
			},
			{
				selector: ':parent',
				style: 
				{
					'background-opacity': 0.333	
				}
			},
			{
				selector: 'edge',
				style: 
				{
					'content': 'data(label)',
					'curve-style': 'bezier',
					'width': 4,
					'target-arrow-shape': 'triangle',
					'line-color': '#ffffff',
					'target-arrow-color': '#ffffff'
				}
			},
			{
				selector: 'edge.haystack',
				style: 
				{
					'curve-style': 'haystack',
					'display': 'none'
				}
			}
		],

		elements: 
		{
		nodes: nodes,
		edges: edges
		},
	});
}

function loadWorkspace(xmlText) {
	const xml = Blockly.Xml.textToDom(xmlText);
	Blockly.Xml.domToWorkspace(xml, workspace);
}

function getWorkspaceAsXml()
{
	const xml = Blockly.Xml.workspaceToDom(workspace);
	return Blockly.Xml.domToText(xml);
}

function openTab(e, tabName) 
{
    const tabs = document.getElementsByClassName("tabItemContent");
    for (var i = 0; i < tabs.length; i++) 
	{
        tabs[i].style.display = "none";
    }

    const tablinks = document.getElementsByClassName("tabLink");
    for (var i = 0; i < tablinks.length; i++) 
	{
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    // Show the current tab, and add an "active" class to the button that opened the tab
    document.getElementById(tabName).style.display = "block";
    e.currentTarget.className += " active";
}

function ShowBlocklyErrors(errorInfo)
{
	//{
	//	id,
	//	message
	//}
	const allBlocks = workspace.getAllBlocks();
	for(var i = 0; i < errorInfo.length; i++)
	{
		for(var k = 0; k < allBlocks.length; k++)
		{
			if(errorInfo[i].id == allBlocks[k].id)
			{
				allBlocks.splice(k, 1);
				break;
			}
		}
	}
	for(var i = 0; i < allBlocks.length; i++)
	{
		allBlocks[i].setWarningText(null);
	}
	
	workspace.highlightBlock(null);
	for(var i = 0; i < errorInfo.length; i++)
	{
		const block = workspace.getBlockById(errorInfo[i].id);
		if(block)
		{
			block.setWarningText(errorInfo[i].message);
			workspace.highlightBlock(errorInfo[i].id, true);	
		}
	}
}

function ClearErrors()
{
	const allBlocks = workspace.getAllBlocks();
	for(var i = 0; i < allBlocks.length; i++)
	{
		allBlocks[i].setWarningText(null);
	}
	workspace.highlightBlock(null);
}

if(typeof(CefSharp) == "undefined") 
{
	startBlockly([{name: "crashes", inputs: ["fish","cake"], outputs: ["output"]},
				  {name: "ReassignFluid", inputs: ["input1","input2"], outputs: ["output"]},
				  {name: "SemiParallelMixing", inputs: ["H2O","CH3OH"], outputs: ["output"]},
				  {name: "SequentialMixing", inputs: ["input1","input2"], outputs: ["output"]},
				  {name: "SimpleFluidTransfer", inputs: ["H2O"], outputs: ["output"]},
				  {name: "SimpleInputOutput", inputs: ["H2O"], outputs: ["Bar"]}]);
}











