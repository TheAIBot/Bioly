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
					'curve-style': 'bezier',
					'width': 4,
					'target-arrow-shape': 'triangle',
					'line-color': '#9dbaea',
					'target-arrow-color': '#9dbaea'
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

function save() {
	const xml = Blockly.Xml.workspaceToDom(workspace);
	const xml_text = Blockly.Xml.domToText(xml);
	document.cookie = "dd=0" + xml_text;
}
function load() {
	const xml_text = document.cookie;
	const xml = Blockly.Xml.textToDom(xml_text);
	Blockly.Xml.domToWorkspace(xml, workspace);
}

var workspace = Blockly.inject('blocklyDiv',
{
	media: 'media/',
	toolbox: document.getElementById('toolbox')
});

function getWorkspaceAsXml()
{
	const xml = Blockly.Xml.workspaceToDom(workspace);
	return Blockly.Xml.domToText(xml);
}

var didWorkspaceChange = true;

function onWorkspaceChanged(event)
{
	didWorkspaceChange = true;
}
workspace.addChangeListener(onWorkspaceChanged);

function getIfWorkspaceChanged()
{
	const didChange = didWorkspaceChange;
	didWorkspaceChange = false;
	return didChange;
}

function openTab(e, tabName) {

    const tabs = document.getElementsByClassName("tabContent");
    for (var i = 0; i < tabs.length; i++) {
        tabs[i].style.display = "none";
    }

    const tablinks = document.getElementsByClassName("tabLink");
    for (var i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    // Show the current tab, and add an "active" class to the button that opened the tab
    document.getElementById(tabName).style.display = "block";
    e.currentTarget.className += " active";
}

document.getElementById("defaultTab").click();











