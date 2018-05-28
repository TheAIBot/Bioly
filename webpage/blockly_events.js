var workspace;

function startBlockly(programs)
{
	for(var i = 0; i < programs.length; i++)
	{
		inlineProgramPrograms.push(programs[i]);
	}
	
	workspace = Blockly.inject('blocklyDiv',
	{
		media: 'media/',
		toolbox: document.getElementById('toolbox'),
		zoom:
		{
			controls: true,
			wheel: true,
			startScale: 1.0,
			maxScale: 3,
			minScale: 0.1,
			scaleSpeed: 1.2
		}
	});
	
	workspace.addChangeListener(onWorkspaceChanged);
}

var lastUpdateTime = new Date();
function onWorkspaceChanged(event)
{
	const currentTime = new Date();
	//if(lastUpdateTime.getMilliseconds() + 50 < currentTime.getMilliseconds())
	//{
		webUpdater.update(getWorkspaceAsXml());
	//}
	lastUpdateTime = new Date();
}