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

function getWorkspaceAsXml()
{
	const xml = Blockly.Xml.workspaceToDom(workspace);
	return Blockly.Xml.domToText(xml);
}
	