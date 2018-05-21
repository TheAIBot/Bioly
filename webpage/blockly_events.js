var lastUpdateTime = new Date();
function onWorkspaceChanged(event)
{
	const currentTime = new Date();
	//if(lastUpdateTime.getMilliseconds() + 50 < currentTime.getMilliseconds())
	//{
		webUpdater.update(getWorkspaceAsXml());
	//}
	lastUpdateTime = new Date();
	
	if(event.type == Blockly.Events.BLOCK_CHANGE)
	{
		onBlockChanged(event);
	}
}
workspace.addChangeListener(onWorkspaceChanged);

function onBlockChanged(event)
{
	const block = workspace.getBlockById(event.blockId);
	if(block && block.type === "inlineProgram")
	{
		block
	}
}