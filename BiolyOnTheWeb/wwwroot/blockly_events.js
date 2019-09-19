var workspace;

function startBlockly(programs)
{
    programs = eval(programs);

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
async function onWorkspaceChanged(event)
{
	if (alreadyWaitingForUpdate)
	{
		return;
	}
	
    alreadyWaitingForUpdate = true;
    await DotNet.invokeMethodAsync("BiolyOnTheWeb", "BlocklyUpdated")

    //crude way to prevent duplicate requests
    setTimeout(function () {
        alreadyWaitingForUpdate = false;
    }, 200);
}