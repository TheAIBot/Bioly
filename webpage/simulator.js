"use strict";

//everything is 0 indexed except for the commands

var newCommand = null;
var errorMessages = [];

var boardData;
var electrodes;
var drops;
var dropInputs;
var dropOutputs;
var boardWidth;
var boardHeight;

const LEFT_NEIGHBOR_INDEX  = 0;
const RIGHT_NEIGHBOR_INDEX = 1;
const ABOVE_NEIGHBOR_INDEX = 2;
const BELOW_NEIGHBOR_INDEX = 3;

const ELECTRODE_SIZE_IN_CM = 1;
const DROP_DISTANCE_PER_SEC = 20;

function startSimulator(width, height, inputs, outputs)
{
	boardWidth = width;
	boardHeight = height;
	dropInputs = inputs;
	dropOutputs = outputs;
	
	boardData = setupBoard(width, height);
	setupDrops(boardData.electrodeSize / 2);

	prepareElectrodes(width, height);
	prepareInputs();
	
	drops = [];
}

function prepareElectrodes(width, height)
{
	electrodes = [];
	for(var i = 0; i < width * height; i++)
	{
		let electrode = {};
		electrode.position = boardData.electrodePositions[i];
		electrode.isOn = false;
		electrode.neighbors = [];
		
		electrodes.push(electrode);
	}
	
	//add neighbors
	for(var i = 0; i < width * height; i++)
	{
		const electrode = electrodes[i];
		
		//left electrode
		if (i % width == (i - 1) % width)
		{
			electrode.neighbors.push(electrodes[i - 1]);
		}
		else
		{
			electrode.neighbors.push(null);
		}
		//right electrode
		if (i % width == (i + 1) % width)
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

function updateLoop()
{
	if(newCommand != null)
	{
		executeCommand(newCommand);
	}
	
	spawnInputDrops();
	
	splitDrops();
	
	removeDrops();
	
	
	
	render(drops.length);
	
	//if new command then parse and execute command --done
	
	//spawn drops --done
	
	//split drops --done

	//remove drops --done
	
	//update drop positions
	
	//merge drops
	
	//render --done
}

function executeCommand(command)
{
	const splittedCommand = newCommand.split(" ");
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
		for(var i = 1; i <= boardData.electrodePositions.length; i++)
		{
			turnElectrodeOff(i);
		}
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
	boardData.electrodePositions[number - 1].isOn = true;
}

function turnElectrodeOff(number)
{
	electrodeIndexCheck(number)
	drawElectrodeOff(number - 1);
	boardData.electrodePositions[number - 1].isOn = false;
}

function electrodeIndexCheck(number)
{
	if (!Number.isInteger(number))
	{
		throw "Electrode index was not a number. Was instead: " + splittedCommand[i];
	}
	else if (number < 1 || number > boardData.electrodePositions.length)
	{
		throw "Electrode index was outside the bounds 1.." + boardData.electrodePositions.length + ". Number was: " + number;
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
	newDrop.size =  getDropSize(newDrop.amount);
	newDrop.color = color;
	
	drops.push(newDrop);
}

function splitDrops()
{
	for(var i = 0; i < drops.length; i++)
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
		}
	}
}

function isElectrodeOn(electrode)
{
	return electrode && electrode.isOn;
}

function getDropSize(amount)
{
	return Math.sqrt(amount) / Math.sqrt(Math.PI);
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
		while (dropIndex-- >= 0) {
			const drop = drops[dropIndex];
			const dropPoisition = electrodes[drop.index].position;
			
			if (distanceAB(outputPosition, dropPoisition) <= boardData.electrodeSize * 0.1)
			{
				drops.splice(dropIndex,1);
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
	const ba0 = a[0] - b[0];
	const ba1 = a[1] - b[1];
	return Math.sqrt(ba0 * ba0 + ba1 * ba1);
}

function updateDropPositions()
{
	for(var i = 0; i < drops.length; i++)
	{
		const drop = drops[i];
		
		
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








