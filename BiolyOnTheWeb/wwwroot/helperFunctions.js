function setGraph(nodes, edges)
{
    nodes = eval(nodes);
    edges = eval(edges);

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
					'min-zoomed-font-size': 10,
					'content': 'data(label)',
					'text-opacity': 0.5,
					'text-valign': 'center',
					'text-halign': 'right',
					'background-color': '#11479e',
					'text-wrap': 'wrap'
				}
			},
			{
				selector: 'node.hidden',
				style: 
				{
					'display': 'none'
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
					'min-zoomed-font-size': 10,
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
		wheelSensitivity: 0.2,
		pixelRatio: 1,

		elements: 
		{
		nodes: nodes,
		edges: edges
		},
	});
}

function loadWorkspace(xmlText) 
{
	workspace.removeChangeListener(onWorkspaceChanged);
	
	const xml = Blockly.Xml.textToDom(xmlText);
	Blockly.Xml.domToWorkspace(xml, workspace);
	
	setTimeout(function() {
		workspace.addChangeListener(onWorkspaceChanged);
	}, 100);
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
    e.className += " active";
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

function ShowUnexpectedError(unexpectedError)
{
    const unexpectedErrorTextDisplay = document.getElementById("errorTextDiv");
    unexpectedErrorTextDisplay.innerHTML = unexpectedError.replace(/</g, "&lt");
	const unexpectedErrorsDisplay = document.getElementById("showErrors");
	unexpectedErrorsDisplay.style.display = "block";
}

function ClearErrors()
{
	const allBlocks = workspace.getAllBlocks();
	for(var i = 0; i < allBlocks.length; i++)
	{
		allBlocks[i].setWarningText(null);
	}
	workspace.highlightBlock(null);
	
	const unexpectedErrorsDisplay = document.getElementById("showErrors");
	unexpectedErrorsDisplay.style.display = "none";
}

//[
//	concentrations[]
//]
function ShowDropletsInformation(inputNames, droplets)
{	
	let html = "<tr>";
	
	for(var i = 0; i < inputNames.length; i++)
	{
		html += "<th>" + inputNames[i] + "</th>";
	}
	
	html += "</tr>";
	
	for(var i = 0; i < droplets.length; i++)
	{
		html += "<tr>";
		
		for(var z = 0; z < droplets[i].length; z++)
		{
			html += "<td style='text-align: right;'>" + (droplets[i][z] * 100).toFixed(2) + "%</td>";
		}
		
		html += "</tr>";
	}
	
	const table = document.getElementById("dropsTable");
	table.innerHTML = html;
}











