function getSettings()
{
	const settingsElements = document.getElementsByClassName("settingInput");
	const settings = [];
	for(var i = 0; i < settingsElements.length; i++)
	{
		if(settingsElements[i].type == "number")
		{
			settings.push(settingsElements[i].id + " = " + settingsElements[i].value);
		}
		else
		{
			settings.push(settingsElements[i].id + " = " + settingsElements[i].checked);
		}
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
		if(setting.value === true || setting.value === false)
		{
			settingElement.checked = setting.value;
		}
		else
		{
			settingElement.value = setting.value;
		}
	}
}

function getDropletSpeedSetting()
{
	return document.getElementById("dropletSpeedSetting").value;
}
function getDropletSizeSetting()
{
	return document.getElementById("dropletSizeSetting").value;
}
function getElectrodeSizeSetting()
{
	return document.getElementById("electrodeSizeSetting").value;
}
function getUseSimulatorStrictModeSetting()
{
	return document.getElementById("useSimulatorStrictModeSetting").checked;
}
function getSimulatorUPSSetting()
{
	return document.getElementById("simulatorUPSSetting").value;
}

/*
function updateAvailableComPorts(comPorts)
{
	const combobox = document.getElementById("comPortSetting");
	const options = combobox.options;
	const currentSelectedOptionName = options[options.selectedIndex].name;
	
	//remove existing options
	for(var i = options.length - 1; i >= 0; i--)
	{
		options.remove(i);
	}
}
*/












