function setGraph(nodes, edges)
{
	window.graphDiv = cytoscape({
	  container: document.getElementById('graphDiv'),

	  boxSelectionEnabled: false,
	  autounselectify: true,

	  layout: {
		name: 'dagre'
	  },

	  style: [
		{
		  selector: 'node',
		  style: {
			'content': 'data(id)',
			'text-opacity': 0.5,
			'text-valign': 'center',
			'text-halign': 'right',
			'background-color': '#11479e'
		  }
		},

		{
		  selector: 'edge',
		  style: {
			'curve-style': 'bezier',
			'width': 4,
			'target-arrow-shape': 'triangle',
			'line-color': '#9dbaea',
			'target-arrow-color': '#9dbaea'
		  }
		}
	  ],

	  elements: {
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













