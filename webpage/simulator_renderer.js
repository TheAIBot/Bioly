'use strict';

var gl;
var boardProgram;
var dropProgram;
var electrodeSize;
var eletrodeVerticiesCount;
var electrodeCount;

const ELECTRODE_OFF_COLOR = vec4(0.8, 0.8, 0.8, 1.0);
const ELECTRODE_ON_COLOR  = vec4(0.4, 0.4, 0.4, 1.0);

window.onload = function init()
{
    const canvas = document.getElementById("simulatorCanvas");
	const parentWidth = canvas.parentNode.clientWidth;
	const parentHeight = canvas.parentNode.clientHeight;
	const canvasSize = Math.min(parentWidth, parentHeight);
	
	canvas.width  = canvasSize;
	canvas.height = canvasSize;
    
    gl = WebGLUtils.setupWebGL(canvas);
    if (!gl) 
    {
        alert("failed to load webgl2");
    }
    
    gl.viewport(0, 0, canvasSize, canvasSize);
    gl.clearColor(1, 1, 1, 1.0);
    
    boardProgram = initShaders(gl, "board-vertex-shader", "board-fragment-shader");    
	dropProgram  = initShaders(gl, "drop-vertex-shader", "drop-fragment-shader");
	
	setupBoard(11, 11);
	render();
}

function setupBoard(width, height)
{
	gl.useProgram(boardProgram);
	
    let boardData = createBoardVertexData(width, height);
	electrodeSize = boardData.electrodeSize;
	
    var electrodeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, electrodeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodeVerticies), gl.STATIC_DRAW);
	
    const vElectrode = gl.getAttribLocation(boardProgram, "vElectrode");
    gl.vertexAttribPointer(vElectrode, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vElectrode);
	
	
	var electrodePositionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, electrodePositionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodePositions), gl.STATIC_DRAW);
	
    const vPosition = gl.getAttribLocation(boardProgram, "vPosition");
    gl.vertexAttribPointer(vPosition, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vPosition);
	gl.vertexAttribDivisor(vPosition, 1);
	
	
	var electrodeColorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, electrodeColorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodeColors), gl.STATIC_DRAW);
	
    const vColor = gl.getAttribLocation(boardProgram, "vColor");
    gl.vertexAttribPointer(vColor, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vColor);
	gl.vertexAttribDivisor(vColor, 1);
	
	return boardData;
}

function createBoardVertexData(width, height)
{	
	const borderSize = 0.10;
	const boardSize = 2 - (borderSize * 2);
	
	//ratio between electrode size and electrode spacing
	const ratioForSpace = 0.1;
	const electrodeSize =  (boardSize  / Math.max(width, height)) * (1 - ratioForSpace);
	const topLeftX = (-boardSize / 2) + (electrodeSize / 2) + (electrodeSize / 2) * ratioForSpace;
	const topLeftY = ( boardSize / 2) - (electrodeSize / 2) - (electrodeSize / 2) * ratioForSpace;
	
	let boardData = {};
	boardData.electrodeSize = electrodeSize;
	boardData.electrodeVerticies = createElectrodeVerticies(electrodeSize);
	boardData.electrodePositions = createElectrodePositions(width, height, topLeftX, topLeftY, ratioForSpace, electrodeSize);
	boardData.electrodeColors    = createElectrodeColors(width, height);
	
	return boardData;
}

function createElectrodeVerticies(electrodeSize)
{
	let verticies = [];
	verticies.push(vec2(-electrodeSize / 2,  electrodeSize / 2));
	verticies.push(vec2( electrodeSize / 2,  electrodeSize / 2));
	verticies.push(vec2(-electrodeSize / 2, -electrodeSize / 2));
	
	verticies.push(vec2(-electrodeSize / 2, -electrodeSize / 2));
	verticies.push(vec2( electrodeSize / 2,  electrodeSize / 2));
	verticies.push(vec2( electrodeSize / 2, -electrodeSize / 2));
	
	return verticies;
}

function createElectrodePositions(width, height, topLeftX, topLeftY, ratioForSpace, electrodeSize)
{
	let electrodePositions = [];
	
	for(var y = 0; y < height; y++)
	{
		const sumElectrodeHeight = electrodeSize * y;
		const sumElectrodeHeightSpace = ratioForSpace * sumElectrodeHeight;
		const posY = topLeftY - sumElectrodeHeight - sumElectrodeHeightSpace
		for(var x = 0; x < width; x++)
		{
			const sumElectrodeWidth = electrodeSize * x;
			const sumElectrodeWidthSpace = ratioForSpace * sumElectrodeWidth;
			const posX = topLeftX + sumElectrodeWidth + sumElectrodeWidthSpace;
			
			electrodePositions.push(vec2(posX, posY));
		}
	}
	
	return electrodePositions;
}

function createElectrodeColors(width, height)
{
	let colors = [];
	
	for(var x = 0; x < width * height; x++)
	{
		colors.push(ELECTRODE_OFF_COLOR);
	}
	
	return colors;
}

function setupDrops()
{
	gl.useProgram(dropProgram);
	
	let dropVerticies = createDropVerticies(electrodeSize);
	
    var dropBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, dropBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(dropVerticies), gl.STATIC_DRAW);
	
    const vDrop = gl.getAttribLocation(dropProgram, "vDrop");
    gl.vertexAttribPointer(vDrop, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vDrop);
	
	
	var dropPositionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, dropPositionBuffer);
    //gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodePositions), gl.STATIC_DRAW);
	
    const vPosition = gl.getAttribLocation(dropProgram, "vPosition");
    gl.vertexAttribPointer(vPosition, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vPosition);
	gl.vertexAttribDivisor(vPosition, 1);
	
	
	var dropSizeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, dropSizeBuffer);
    //gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodePositions), gl.STATIC_DRAW);
	
    const vSize = gl.getAttribLocation(dropProgram, "vSize");
    gl.vertexAttribPointer(vSize, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vSize);
	gl.vertexAttribDivisor(vSize, 1);
	
	
	var dropColorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, dropColorBuffer);
    //gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodeColors), gl.STATIC_DRAW);
	
    const vColor = gl.getAttribLocation(dropProgram, "vColor");
    gl.vertexAttribPointer(vColor, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vColor);
	gl.vertexAttribDivisor(vColor, 1);
}

function createDropVerticies(circleRadius)
{
	let verticies = [];
	verticies.push(vec2(0, 0));
	
	const angleBetweenPoints = (Math.PI / 180) * (360 / 99);
	for(var i = 0; i < 99; i++)
	{
		verticies.push(vec2(circleRadius * Math.cos(i * angleBetweenPoints), circleRadius * Math.sin(i * angleBetweenPoints)));
	}
	
	return verticies;
}

function renderBoard() 
{
	gl.useProgram(boardProgram);
    gl.drawArraysInstanced(gl.TRIANGLES, 0, eletrodeVerticiesCount, electrodeCount);
}

function renderDrops(dropCount)
{
	gl.useProgram(dropProgram);
	gl.drawArraysInstanced(gl.TRIANGLES, 0, 100, dropCount);
}

function render(eletrodeVerticiesCount, electrodeCount, dropCount)
{
	gl.clear(gl.COLOR_BUFFER_BIT);
	
	renderBoard(eletrodeVerticiesCount, electrodeCount);
	renderDrops(dropCount);
}

//void drawElectrodeOn (int electrodeNumber);
//void drawElectrodeOff(int electrodeNumber);

//int spawnDrop(x, y, size);
//void removeDrop(dropBumber);
//void changeDropPosition(dropNumber, x, y);

//render();


function drawElectrodeOn(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, electrodeColorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_ON_COLOR), 0, 4);
}

function drawElectrodeOff(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, electrodeColorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_OFF_COLOR), 0, 4);
}
























