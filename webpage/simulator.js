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

const ELECTRODE_SIZE_IN_CM = 1;
const DROP_DISTANCE_PER_SEC_IN_CM = 50;
const UPDATES_PER_SECOND = 60;

//setel 1 2 3 4 5 6 7  8 9 10
//clrel 1 2 3 4 5  6 7 8 9 10
//clra
//--[[unique simulator commands]]--
//show_area (string)id (int)x (int)y (int)width (int)height (float)r (float)g (float)b
//remove_area (string)id

function startSimulator(width, height, inputs, outputs)
{
	newCommands = [];
	errorMessages = [];
	
	boardWidth = width;
	boardHeight = height;
	dropInputs = inputs;
	dropOutputs = outputs;
	
	let electrodeData = setupBuffers(width, height);
	electrodeSize = electrodeData.electrodeSize;

	prepareElectrodes(width, height, electrodeData.electrodePositions);
	prepareInputs();
	
	drops = [];
	areas = [];
	
	newestVersion = newestVersion + 1;
	updateLoop(newestVersion);
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

function updateLoop(version)
{
	if(newestVersion != version)
	{
		return;
	}
	
	if(newCommands.length > 0)
	{
		executeCommand(newCommands[0]);
		newCommands.splice(0, 1);
	}
	
	spawnInputDrops();
	splitDrops();
	removeDrops();
	updateDropPositions();
	mergeDrops();
	
	updateDropData(drops);
	updateAreaData(areas);
	render(drops.length, areas.length);
	
	window.requestAnimFrame(function()
	{
		updateLoop(version);
	});
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
	}
	else if(commandType == "clrel")
	{
		for(var i = 1; i < splittedCommand.length; i++)
		{
			let number = parseInt(splittedCommand[i]);
			turnElectrodeOff(number);
		}
	}
	else if(commandType == "clra")
	{
		for(var i = 1; i <= electrodes.length; i++)
		{
			turnElectrodeOff(i);
		}
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
	}
	else if(commandType == "remove_area")
	{
		const id = splittedCommand[1];
		removeArea(id);
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

function spawnDrop(position, amount, color)
{
	const newDrop = {};
	newDrop.position = vec2(position[0], position[1]);
	newDrop.amount = amount;
	newDrop.size = getDropSize(newDrop.amount);
	newDrop.color = color;
	
	drops.push(newDrop);
}

function splitDrops()
{
	//drops are deleted so the array has to be iterated backwards
	let i = drops.length;
	while(i--)
	{
		const drop = drops[i];
		const electrode = getClosestElectrode(drop.position);
		
		if (electrode.isOn)
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
				throw "Trying to split a drop that only has " + drop.amount + " drops in it";
			}
			
			const electrodeA = horizontalSplit ? leftElectrode  : aboveElectrode;
			const electrodeB = horizontalSplit ? rightElectrode : belowElectrode;
			
			spawnDrop(electrodeA.position, drop.amount / 2, drop.color);
			spawnDrop(electrodeB.position, drop.amount / 2, drop.color);
			
			//delete drop that was splitted
			drops.splice(i, 1);
		}
	}
}

function isElectrodeOn(electrode)
{
	return electrode && electrode.isOn;
}

function getDropSize(amount)
{
	return Math.sqrt(amount);
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
		const nearbyDistance = electrodeSize * 2;//don't do this for now * drop.size;
		const nearbyElectrode = getSingleNearbyOnElectrode(drop.position, nearbyDistance);
		
		if (nearbyElectrode)
		{
			let dx = nearbyElectrode.position[0] - drop.position[0];
			let dy = nearbyElectrode.position[1] - drop.position[1];
			const dVectorLength = Math.sqrt(dx * dx + dy * dy);
			
			if (dVectorLength > distancePerUpdate)
			{
				dx = dx * (distancePerUpdate / dVectorLength);
				dy = dy * (distancePerUpdate / dVectorLength);
			}
			
			drop.position[0] += dx;
			drop.position[1] += dy;
		}
		
	}
}

function getSingleNearbyOnElectrode(position, nearbyDistance)
{
	let nearbyElectrode = null;
	for(var i = 0; i < electrodes.length; i++)
	{
		const electrode = electrodes[i];
		
		if (electrode.isOn)
		{
			const distance = distanceAB(position, electrode.position);
			if (distance <= nearbyDistance)
			{
				if (nearbyElectrode == null)
				{
					nearbyElectrode = electrode;	
				}
				else 
				{
					throw "Two or more electrodes are turned on near a drop";
				}
			}
		}
	}
	
	return nearbyElectrode;
}

function mergeDrops()
{
	const dropCount = drops.length;
	for(var i = 0; i < dropCount / 2; i++)
	{
		const drop = drops[i];
		const dropRadius = (electrodeSize / 2) * drop.size;
		if(drop)
		{
			for(var k = i + 1; k < dropCount; k++)
			{
				const otherDrop = drops[k];
				if(otherDrop)
				{
					const otherDropRadius = (electrodeSize / 2) * otherDrop.size;
					if(otherDrop)
					{
						const distance = distanceAB(drop.position, otherDrop.position);
						if (distance - dropRadius - otherDropRadius < electrodeSize / 2)
						{
							const newDropPos = vec2((drop.position[0] + otherDrop.position[0]) / 2, 
													(drop.position[1] + otherDrop.position[1]) / 2);
							const newDropColor = vec4((drop.color[0] + otherDrop.color[0]) / 2, 
													  (drop.color[1] + otherDrop.color[1]) / 2, 
													  (drop.color[2] + otherDrop.color[2]) / 2, 
													  (drop.color[3] + otherDrop.color[3]) / 2);
							spawnDrop(newDropPos, drop.amount + otherDrop.amount, newDropColor);
							
							drops[i] = null;
							drops[k] = null;
							break;
						}
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
//	color
//}

//inputs
//{
//	index
//	canSpawn
//	color
//}

//outputs
//{
//	index
//}








