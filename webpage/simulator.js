"use strict";

var command = null;
var errorMessage = "";

var boardData;
var electrodes;
var drops;
var dropInputs;
var dropOutputs;

function startSimulator(width, height, inputs, outputs)
{
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
	if(command != null)
	{
		const splittedCommand = command.split(" ");
		const commandType = splittedCommand[0];
		if(commandType == "setel")
		{
			
		}
		else if(commandType == "clrel")
		{
			
		}
		else if(commandType == "clra")
		{
			
		}
		else
		{
			errorMessage += "Unknown command type: " + commandType;
		}
	}
	
	//if new command then parse and execute command
	
	//split drops
	
	//update drop positions
	
	//merge drops
	
	//render
}


//drop
//{
//	amount
//	index
//
//}








