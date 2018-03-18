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
	setupDrops();
	
	electrodes = [];
	for(var i = 0; i < width * height; i++)
	{
		let electrode = {};
		electrode.position = boardData.electrodePositions[i];
		electrode.isOn = false;
		
		electrodes.push(electrode);
	}
	
	drops = [];
}


function updateLoop()
{
	if(newCommand != null)
	{
		executeCommand(newCommand);
	}
	
	//if new command then parse and execute command --done
	
	//spawn drops
	
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
		const index = dropInputs[i].index;
		let turnedOnElectrodesCount = 0;
		
		const left  = getLeftElectrodeIndex(index);
		const right = getRightElectrodeIndex(index);
		const above = getAboveElectrodeIndex(index);
		const below = getBelowElectrodeIndex(index);
		
		const eletrodesInRange = (left  == -1? 0 : 1) + 
								 (right == -1? 0 : 1) + 
								 (above == -1? 0 : 1) + 
								 (below == -1? 0 : 1);
		
		if (eletrodesInRange > 1)
		{
			throw "Too many electrodes are turned on at an input";
		}
		
		if (eletrodesInRange == 1)
		{
			const indexToSpawnDropOn = (left  == -1? 0 : 1) + 
									   (right == -1? 0 : 1) + 
									   (above == -1? 0 : 1) + 
									   (below == -1? 0 : 1);
			
			let dropPosition = electrode[i].position;
		}
	}
}

function getLeftElectrodeIndex(index)
{
	let x = (index % boardWidth) - 1;
	//electrodes in the first column can't
	//get the left electrode because it would
	//be the electrode from the last column
	//or an indexoutofbounds error
	if (x < 0)
	{
		return -1;
	}
	
	return index - 1;
}

function getRightElectrodeIndex(index)
{
	let x = (index % boardWidth) + 1;
	//electrodes in the last column can't
	//get the right electrode because it would
	//be the electrode from the first column
	//or an indexoutofbounds error
	if (x >= boardWidth)
	{
		return -1;
	}
	
	return index + 1;
}

function getAboveElectrodeIndex(index)
{
	let y = (index / boardHeight) + 1;
	if (y >= boardWidth)
	{
		return -1;
	}
	
	return index + boardWidth;
}

function getBelowElectrodeIndex(index)
{
	let y = (index / boardHeight) - 1;
	if (y < 0)
	{
		return -1;
	}
	
	return index - boardWidth;
}


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
//}








