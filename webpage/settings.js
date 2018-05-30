function getSettings()
{
	const settingsElements = document.getElementsByClassName("settingInput");
	const settings = [];
	for(var i = 0; i < settingsElements.length; i++)
	{
		settings.push(settingsElements[i].id + " = " + settingsElements[i].value);
	}
	
	return settings.join("\n");
}

//{
//	id,
//	value
//}
function setSettings(settings)
{
	for(var i = 0; i < settings.length; i++)
	{
		const setting = settings[i];
		const settingElement = document.getElementById(setting.id);
		settingElement.value = setting.value;
	}
}