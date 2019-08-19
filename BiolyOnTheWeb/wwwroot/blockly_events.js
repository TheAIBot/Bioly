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
            scaleSpeed: 1.2,
            sounds: false
		}
	});
	
	workspace.addChangeListener(onWorkspaceChanged);
}

var alreadyWaitingForUpdate = false
function onWorkspaceChanged(event)
{
	if (alreadyWaitingForUpdate)
	{
		return;
	}
	
    alreadyWaitingForUpdate = true;
    DotNet.invokeMethodAsync("BiolyOnTheWeb", "BlocklyUpdated")
        .then(data =>
        {
            setTimeout(function () {
                alreadyWaitingForUpdate = false;
            }, 100);
        });
}