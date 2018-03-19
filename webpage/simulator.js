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

function startSimulator(width, height, inputs, outputs)
{
	boardWidth = width;
	boardHeight = height;
	dropInputs = inputs;
	dropOutputs = outputs;
	
	boardData = setupBoard(width, height);
	setupDrops(boardData.electrodeSize / 2);

	prepareElectrodes(width, height);
	
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
		
		if (i % width == (i - 1) % width)
		{
			electrode.neighbors.push(electrodes[i - 1]);
		}
		if (i % width == (i + 1) % width)
		{
			electrode.neighbors.push(electrodes[i + 1]);
		}
		if (i - width >= 0)
		{
			electrode.neighbors.push(electrodes[i - width]);
		}
		if (i + width < width * height)
		{
			electrode.neighbors.push(electrodes[i + width]);
		}
	}
}

function updateLoop()
{
	if(newCommand != null)
	{
		executeCommand(newCommand);
	}
	
	spawnInputDrops();
	
	//if new command then parse and execute command --done
	
	//spawn drops --done
	
	//split drops

	//remove drops
	
	//update drop positions
	
	//merge drops
	
	//render
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
	for(var i = 0; i < )
	{
		const input = dropInputs[i];
		const neighbors = electrodes[input.index].neighbors;
		
		let eletrodesInRange = 0;
		let electrodeIndex = -1;
		for(var k = 0; k < neighbors.length; k++)
		{
			const electrode = neighbors[k];
			if(electrode.isOn)
			{
				eletrodesInRange++;
				electrodeIndex = k;
			}
		}
		
		if (eletrodesInRange > 1)
		{
			throw "Too many electrodes are turned on at an input";
		}
		
		if (eletrodesInRange == 1)
		{						
			let newDrop = {};
			newDrop.position = electrodes[electrodeIndex].position;
			newDrop.amount = 1;
			newDrop.size =  1;
			newDrop.color = input.color;
			
			drops.push(newDrop);
		}
	}
}

function splitDrops()
{
	for(var i = 0; i < drops.length; i++)
	{
		const drop = drops[i];
		const electrode = getClosestElectrode(drop.position);
		
		
	}
}

function getClosestElectrode(position)
{
	let closest = null;
	left bestDistance = 1000000;
	for(var i = 0; i < electrodes.length; i++)
	{
		const electrodePosition = electrodes[i].position;
		const distance = Math.sqrt(Math.abs(position[0] - electrodePosition[0]) + Math.abs(position[1] - electrodePosition[1]));
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
//	color
//}








