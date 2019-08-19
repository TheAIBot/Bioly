"use strict";

//everything is 0 indexed except for the commands

var newCommands = [];
var errorMessages = [];

var electrodeSize;
var electrodes;
var drops;
var dropInputs;
var dropOutputs;
var boardWidth;
var boardHeight;
var areas;
var newestVersion = 0;

const LEFT_NEIGHBOR_INDEX  = 0;
const RIGHT_NEIGHBOR_INDEX = 1;
const ABOVE_NEIGHBOR_INDEX = 2;
const BELOW_NEIGHBOR_INDEX = 3;

var ELECTRODE_SIZE_IN_CM = 1;
var DROP_DISTANCE_PER_SEC_IN_CM = 600;
var DEFAULT_DROP_SIZE_IN_CM = 1;
var STRICT_MODE_ENABLED = true;
var didGraphicsChange = false;
var updatesPerUdate = 1;
var runningSimulatorIntervalID = null;
var UPDATES_PER_SECOND = 60;

//setel 1 2 3 4 5 6 7  8 9 10
//clrel 1 2 3 4 5  6 7 8 9 10
//clra
//--[[unique simulator commands]]--
//show_area (string)id (int)x (int)y (int)width (int)height (float)r (float)g (float)b
//remove_area (string)id

function startSimulator(width, height, inputs, outputs, enabledElectrodes)
{
	ELECTRODE_SIZE_IN_CM        = getElectrodeSizeSetting();
	DROP_DISTANCE_PER_SEC_IN_CM = getDropletSpeedSetting();
	DEFAULT_DROP_SIZE_IN_CM     = getDropletSizeSetting();
	STRICT_MODE_ENABLED         = getUseSimulatorStrictModeSetting();
	UPDATES_PER_SECOND          = getSimulatorUPSSetting();
	
	newCommands = [];
	errorMessages = [];
	
	boardWidth = width;
	boardHeight = height;
	dropInputs = inputs;
	dropOutputs = outputs;
	
	let electrodeData = setupBuffers(width, height, enabledElectrodes);
	electrodeSize = electrodeData.electrodeSize;

	prepareElectrodes(width, height, electrodeData.electrodePositions);
	prepareInputs();
	
	drops = [];
	areas = [];
	
	if (runningSimulatorIntervalID != null)
	{
		clearInterval(runningSimulatorIntervalID);
	}
	
	let timeBetweenUpdates = 1000 / UPDATES_PER_SECOND;
	
	if (timeBetweenUpdates < 10)
	{
		updatesPerUdate = Math.ceil(10 / timeBetweenUpdates);
		timeBetweenUpdates = 10;
	}
	else
	{
		updatesPerUdate = 1;
	}
	
	runningSimulatorIntervalID = setInterval(updateLoop, timeBetweenUpdates);
	didGraphicsChange = true;
}

function prepareElectrodes(width, height, electrodePositions)
{
	electrodes = [];
	for(var i = 0; i < width * height; i++)
	{
		let electrode = {};
		electrode.position = electrodePositions[i];
		electrode.isOn = false;
		electrode.neighbors = [];
		
		electrodes.push(electrode);
	}
	
	//add neighbors
	for(var i = 0; i < width * height; i++)
	{
		const electrode = electrodes[i];
		
		//left electrode
		if ((i % width) - 1 >= 0)
		{
			electrode.neighbors.push(electrodes[i - 1]);
		}
		else
		{
			electrode.neighbors.push(null);
		}
		//right electrode
		if ((i % width) + 1 < width)
		{
			electrode.neighbors.push(electrodes[i + 1]);
		}
		else
		{
			electrode.neighbors.push(null);
		}
		//above electrode
		if (i - width >= 0)
		{
			electrode.neighbors.push(electrodes[i - width]);
		}
		else
		{
			electrode.neighbors.push(null);
		}
		//below electrode
		if (i + width < width * height)
		{
			electrode.neighbors.push(electrodes[i + width]);
		}
		else
		{
			electrode.neighbors.push(null);
		}
	}
}

function prepareInputs()
{
	for(var i = 0; i < dropInputs.length; i++)
	{
		dropInputs[i].canSpawn = [true, true, true, true];
	}
}

function addCommand(command)
{
	newCommands.push(command);
}

function updateLoop()
{	
	for(let i = 0; i < updatesPerUdate; i++)
	{
		if(newCommands.length > 0)
		{
			executeCommand(newCommands[0]);
			newCommands.splice(0, 1);
		}
		try
		{
			spawnInputDrops();
			splitDrops();
			removeDrops();
			updateDropPositions();
			mergeDrops();
		}
		catch(error)
		{
			console.log(error);
			clearInterval(runningSimulatorIntervalID);
			return;
		}
	}
}

function updateGraphics()
{	
    if (didGraphicsChange)
	{
		didGraphicsChange = false;
		
		try
		{
			updateDropData(drops);
			updateAreaData(areas);
			render(drops.length, areas.length);
		}
		catch(error) 
		{
			console.log(error);
		}
	}
	
	window.requestAnimFrame(updateGraphics);
}

function executeCommand(command)
{
	const splittedCommand = command.split(" ");
	const commandType = splittedCommand[0];
	if(commandType == "setel")
	{
		for(var i = 1; i < splittedCommand.length; i++)
		{
			let number = parseInt(splittedCommand[i]);
			turnElectrodeOn(number);
		}
		didGraphicsChange = true;
	}
	else if(commandType == "clrel")
	{
		for(var i = 1; i < splittedCommand.length; i++)
		{
			let number = parseInt(splittedCommand[i]);
			turnElectrodeOff(number);
		}
		didGraphicsChange = true;
	}
	else if(commandType == "clra")
	{
		for(var i = 1; i <= electrodes.length; i++)
		{
			turnElectrodeOff(i);
		}
		didGraphicsChange = true;
	}
	else if(commandType == "show_area")
	{
		const id = splittedCommand[1];
		const x      = parseInt(splittedCommand[2]);
		const y      = parseInt(splittedCommand[3]);
		const width  = parseInt(splittedCommand[4]);
		const height = parseInt(splittedCommand[5]);
		const r      = parseFloat(splittedCommand[6]);
		const g      = parseFloat(splittedCommand[7]);
		const b      = parseFloat(splittedCommand[8]);
		addArea(id, x, y, width, height, r, b, g);
		didGraphicsChange = true;
	}
	else if(commandType == "remove_area")
	{
		const id = splittedCommand[1];
		removeArea(id);
		didGraphicsChange = true;
	}
	else
	{
		throw "Unknown command type: " + commandType;
	}
}

function turnElectrodeOn(number)
{
	electrodeIndexCheck(number)
	drawElectrodeOn(number - 1);
	electrodes[number - 1].isOn = true;
}

function turnElectrodeOff(number)
{
	electrodeIndexCheck(number)
	drawElectrodeOff(number - 1);
	electrodes[number - 1].isOn = false;
}

function electrodeIndexCheck(number)
{
	if (!Number.isInteger(number))
	{
		throw "Electrode index was not a number. Was instead: " + number;
	}
	else if (number < 1 || number > electrodes.length)
	{
		throw "Electrode index was outside the bounds 1.." + electrodes.length + ". Number was: " + number;
	}
}

function addArea(id, x, y, width, height, r, g, b)
{
	const newArea = {};
	newArea.id = id;
	newArea.position = vec2(electrodes[0].position[0] + (x + (width  / 2)) * electrodeSize + (x + (width  / 2) - 1) * electrodeSize * ratioForSpace - electrodeSize / 2 + electrodeSize * ratioForSpace / 2,
							electrodes[0].position[1] - (y + (height / 2)) * electrodeSize - (y + (height / 2) - 1) * electrodeSize * ratioForSpace + electrodeSize / 2 - electrodeSize * ratioForSpace / 2);
	const widthSize  = (width  * electrodeSize + (width  - 1) * electrodeSize * ratioForSpace + ((electrodeSize * ratioForSpace) / 2)) / electrodeSize;
	const heightSize = (height * electrodeSize + (height - 1) * electrodeSize * ratioForSpace + ((electrodeSize * ratioForSpace) / 2)) / electrodeSize;
	newArea.size = vec2(widthSize, heightSize);
	newArea.color = vec3(r, g, b);
	
	areas.push(newArea);
}

function removeArea(id)
{
	for(var i = 0; i < areas.length; i++)
	{
		if(id == areas[i].id)
		{
			areas.splice(i, 1);
		}
	}
}

function spawnInputDrops()
{
	for(var i = 0; i < dropInputs.length; i++)
	{
		const input = dropInputs[i];
		const neighbors = electrodes[input.index].neighbors;
		
		let electrodesOnCount = 0;
		for(var k = 0; k < neighbors.length; k++)
		{
			if(isElectrodeOn(neighbors[k]))
			{
				electrodesOnCount++;
				
				if (input.canSpawn[k])
				{
					spawnDrop(neighbors[k].position, 1, input.color);
					input.canSpawn[k] = false;
					didGraphicsChange = true;
				}
			}
			else
			{
				input.canSpawn[k] = true;
			}
		}
		
		if (electrodesOnCount > 1)
		{
			throw "Too many electrodes are turned on at an input";
		}
	}
}

function spawnDrop(position, amount, hue)
{
	const newDrop = {};
	newDrop.position = vec2(position[0], position[1]);
	newDrop.amount = amount;
	newDrop.size = getDropSize(newDrop.amount);
	newDrop.color = HSVtoRGB(hue, 0.45, 0.65, 0.5);
	newDrop.hue = hue;
	
	drops.push(newDrop);
}

//derived from https://stackoverflow.com/a/17243070
function HSVtoRGB(h, s, v, a)
{	
    let i = Math.floor(h * 6);
    let f = h * 6 - i;
    let p = v * (1 - s);
    let q = v * (1 - f * s);
    let t = v * (1 - (1 - f) * s);
	
    switch (i % 6)
	{
        case 0:
			return vec4(v, t, p, a);
        case 1:
			return vec4(q, v, p, a);
        case 2:
			return vec4(p, v, t, a);
        case 3:
			return vec4(p, q, v, a);
        case 4:
			return vec4(t, p, v, a);
        case 5:
			return vec4(v, p, q, a);
    }
}

function splitDrops()
{
	//drops are deleted so the array has to be iterated backwards
	let i = drops.length;
	while(i--)
	{
		const drop = drops[i];
		const electrode = getClosestElectrode(drop.position);
		
		if (STRICT_MODE_ENABLED && electrode.isOn)
		{
			continue;
		}
		
		const leftElectrode  = electrode.neighbors[LEFT_NEIGHBOR_INDEX];
		const rightElectrode = electrode.neighbors[RIGHT_NEIGHBOR_INDEX];
		const aboveElectrode = electrode.neighbors[ABOVE_NEIGHBOR_INDEX];
		const belowElectrode = electrode.neighbors[BELOW_NEIGHBOR_INDEX];
		
		const horizontalSplit = isElectrodeOn(leftElectrode)  && isElectrodeOn(rightElectrode);
		const verticalSplit   = isElectrodeOn(aboveElectrode) && isElectrodeOn(belowElectrode);
		
		if (horizontalSplit && verticalSplit)
		{
			throw "Too many electrodes are turned on next to a drop";
		}
		
		if (horizontalSplit || verticalSplit) 
		{
			if (drop.amount <= 1)
			{
				throw "Trying to split a drop that only has " + drop.amount + " drops in it. Atleast 2 drops woth of fluid is required before a drop can be splitted";
			}
			
			const electrodeA = horizontalSplit ? leftElectrode  : aboveElectrode;
			const electrodeB = horizontalSplit ? rightElectrode : belowElectrode;
			
			spawnDrop(electrodeA.position, drop.amount / 2, drop.hue);
			spawnDrop(electrodeB.position, drop.amount / 2, drop.hue);
			
			//delete drop that was splitted
			drops.splice(i, 1);
			didGraphicsChange = true;
		}
	}
}

function isElectrodeOn(electrode)
{
	return electrode && electrode.isOn;
}

function getDropSize(amount)
{
	return Math.sqrt(amount) * (DEFAULT_DROP_SIZE_IN_CM / ELECTRODE_SIZE_IN_CM);
}

function getClosestElectrode(position)
{
	let closest = null;
	let bestDistance = 1000000;
	for(var i = 0; i < electrodes.length; i++)
	{
		const distance = distanceAB(position, electrodes[i].position);
		if (distance < bestDistance)
		{
			closest = electrodes[i];
			bestDistance = distance;
		}
	}
	if (closest == null)
	{
		throw "There was somehow no closest electrode";
	}
	return closest;
}

function removeDrops()
{
	for(var i = 0; i < dropOutputs.length; i++)
	{
		const output = dropOutputs[i];
		const outputPosition = electrodes[output.index].position;
		
		//going through the array backwards so removed drops
		//won't mess with the index
		let dropIndex = drops.length;
		let dropsRemovedCount = 0;
		while (dropIndex--) {
			const drop = drops[dropIndex];
			const dropPoisition = drop.position;
			
			if (distanceAB(outputPosition, dropPoisition) <= electrodeSize * 0.1)
			{
				drops.splice(dropIndex, 1);
				dropsRemovedCount++;
				didGraphicsChange = true;
			}
		}
		
		if (dropsRemovedCount > 1)
		{
			throw "A single output can't remove more than one drop at a time. An output just removed " + dropsRemovedCount + " drops";
		}
	}
}

function distanceAB(a, b)
{
	const baX = a[0] - b[0];
	const baY = a[1] - b[1];
	return Math.sqrt(baX * baX + baY * baY);
}

function updateDropPositions()
{
	const distancePerUpdate = (DROP_DISTANCE_PER_SEC_IN_CM / UPDATES_PER_SECOND) * ELECTRODE_SIZE_IN_CM * electrodeSize;
	
	for(var i = 0; i < drops.length; i++)
	{
		const drop = drops[i];
		const nearbyDistance = electrodeSize * (drop.size + (ratioForSpace * 1.1));
		const nearbyElectrodes = getNearbyOnElectrodes(drop.position, nearbyDistance);
		
		if (STRICT_MODE_ENABLED && nearbyElectrodes.length > 1)
		{
			throw "Two or more electrodes are turned on near a drop";
		}
		
		if (nearbyElectrodes.length > 0)
		{
			let centerX = 0;
			let centerY = 0;
			for(var k = 0; k < nearbyElectrodes.length; k++)
			{
				const electrode = nearbyElectrodes[k];
				centerX += electrode.position[0];
				centerY += electrode.position[1];
			}
			
			centerX /= nearbyElectrodes.length;
			centerY /= nearbyElectrodes.length;
			
			let dx = centerX - drop.position[0];
			let dy = centerY - drop.position[1];
			const dVectorLength = Math.sqrt(dx * dx + dy * dy);
			
			if (dVectorLength > distancePerUpdate)
			{
				dx = dx * (distancePerUpdate / dVectorLength);
				dy = dy * (distancePerUpdate / dVectorLength);
			}
			
			drop.position[0] += dx;
			drop.position[1] += dy;
			
			if (dx != 0 || dy != 0)
			{
				didGraphicsChange = true;
			}
		}
	}
}

function getNearbyOnElectrodes(position, nearbyDistance)
{
	let nearbyElectrodes = [];
	for(var i = 0; i < electrodes.length; i++)
	{
		const electrode = electrodes[i];
		
		if (electrode.isOn)
		{
			const distance = distanceAB(position, electrode.position);
			if (distance <= nearbyDistance)
			{
				nearbyElectrodes.push(electrode);
			}
		}
	}
	
	return nearbyElectrodes;
}

function mergeDrops()
{
	const dropCount = drops.length;
	for(var i = 0; i < dropCount; i++)
	{
		const drop = drops[i];
		if(drop)
		{
			const dropRadius = (electrodeSize / 2) * drop.size;
			for(var k = i + 1; k < dropCount; k++)
			{
				const otherDrop = drops[k];
				if(otherDrop)
				{
					const otherDropRadius = (electrodeSize / 2) * otherDrop.size;
					const distance = distanceAB(drop.position, otherDrop.position);
					if (distance - dropRadius - otherDropRadius < electrodeSize / 2)
					{
						if (STRICT_MODE_ENABLED && drop.amount + otherDrop.amount > 2)
						{
							throw "Can't marge two drops where the resulting drop will have 3 or more drops worth of fluid.";
						}
						
						const newDropPos = vec2((drop.position[0] + otherDrop.position[0]) / 2, 
												(drop.position[1] + otherDrop.position[1]) / 2);
						const newDropHue = MixHues(drop.hue, otherDrop.hue);
						spawnDrop(newDropPos, drop.amount + otherDrop.amount, newDropHue);
						
						drops[i] = null;
						drops[k] = null;
						didGraphicsChange = true;
						break;
					}
				}
			}
		}
	}
	
	let index = drops.length;
	while(index--)
	{
		if (drops[index] == null)
		{
			drops.splice(index, 1);
		}
	}
}

function MixHues(hueA, hueB)
{
	if (Math.abs(hueA - hueB) < 0.5)
	{
		return (hueA + hueB) / 2;
	}
	else
	{
		return ((hueA + hueB + 1) / 2) % 1;
	}
}

//electrode
//{
//	position
//	isOn
//	neighbors
//}

//drop
//{
//	position
//	amount
//	size
//	color // rgba
//	hue
//}

//inputs
//{
//	index
//	canSpawn
//	color // a hue
//}

//outputs
//{
//	index
//}








