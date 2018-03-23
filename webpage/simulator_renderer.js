'use strict';

var gl;
var boardGLData = {};
var dropGLData = {};

const ELECTRODE_OFF_COLOR = vec4(0.8, 0.8, 0.8, 1.0);
const ELECTRODE_ON_COLOR  = vec4(0.4, 0.4, 0.4, 1.0);
const DROP_POINT_COUNT = 100;

var currentZoom = 1;
var currentViewOffsetX = 0;
var currentViewOffsetY = 0;

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
    gl.clearColor(0, 0, 0, 0);
    
    boardGLData.program = initShaders(gl, "board-vertex-shader", "board-fragment-shader");    
	dropGLData.program  = initShaders(gl, "drop-vertex-shader", "drop-fragment-shader");
	
	gl.enable(gl.BLEND);
	gl.blendFunc(gl.SRC_COLOR, gl.DST_COLOR);
	
	/*
	const data = setupBoard(11, 11);
	setupDrops(data.electrodeSize / 2);
	updateDropData([{position: [0, 0], size: 4, color: [1, 0, 0, 0.5]}]);
	render(1);
	*/
	
	startSimulator(5, 5, [{index: 6, color: vec4(1, 0, 0, 0.5)}], []);

	canvas.addEventListener('mousemove', function(e)
	{
		if (e.buttons == 1)
		{
			const canvas = document.getElementById("simulatorCanvas");			
			offsetCurrentViewPosition((e.movementX / canvas.width) * 2, (-e.movementY / canvas.height) * 2);
		}
	});
	
	canvas.addEventListener('wheel', function(e)
	{
		changeZoom(e.deltaY / 1250);
		e.preventDefault();
	});
}

function setupBoard(width, height)
{
	gl.useProgram(boardGLData.program);
	
    let boardData = createBoardVertexData(width, height);
	
    boardGLData.electrodeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.electrodeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodeVerticies), gl.STATIC_DRAW);
    boardGLData.electrodePointer = gl.getAttribLocation(boardGLData.program, "vElectrode");
	
	boardGLData.positionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodePositions), gl.STATIC_DRAW);
    boardGLData.positionPointer = gl.getAttribLocation(boardGLData.program, "vPosition");
	
	boardGLData.colorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(boardData.electrodeColors), gl.DYNAMIC_DRAW);
    boardGLData.colorPointer = gl.getAttribLocation(boardGLData.program, "vColor");
	
	boardGLData.zoomPointer = gl.getUniformLocation(boardGLData.program, "zoom");
    gl.uniform1f(boardGLData.zoomPointer, currentZoom);
	
	boardGLData.viewOffsetPointer = gl.getUniformLocation(boardGLData.program, "viewOffset");
    gl.uniform2f(boardGLData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
	
	boardGLData.eletrodeVerticiesCount = boardData.electrodeVerticies.length;
	boardGLData.electrodeCount = width * height;
	
	return boardData;
}

function createBoardVertexData(width, height)
{	
	const borderSize = 0.10;
	const boardSize = 2 - (borderSize * 2);
	
	//ratio between electrode size and electrode spacing
	const ratioForSpace = 0.1;
	const electrodeSize = ((boardSize - (boardSize  / Math.max(width, height)) * ratioForSpace * (Math.max(width, height) - 1))  / Math.max(width, height));
	const topLeftX = -((electrodeSize * width + electrodeSize * (width - 1) * ratioForSpace) / 2) + (electrodeSize / 2);
	const topLeftY = ((electrodeSize * height + electrodeSize * (height - 1) * ratioForSpace) / 2) - (electrodeSize / 2);
	
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

function setupDrops(dropRadius)
{
	gl.useProgram(dropGLData.program);
	
	let dropVerticies = createDropVerticies(dropRadius);
	
    dropGLData.dropBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.dropBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(dropVerticies), gl.STATIC_DRAW);
    dropGLData.dropPointer = gl.getAttribLocation(dropGLData.program, "vDrop");
	
	dropGLData.positionBuffer = gl.createBuffer();
    dropGLData.positionPointer = gl.getAttribLocation(dropGLData.program, "vPosition");
	
	dropGLData.sizeBuffer = gl.createBuffer();
    dropGLData.sizePointer = gl.getAttribLocation(dropGLData.program, "size");
	
	dropGLData.colorBuffer = gl.createBuffer();
    dropGLData.colorPointer = gl.getAttribLocation(dropGLData.program, "vColor");
	
	dropGLData.zoomPointer = gl.getUniformLocation(dropGLData.program, "zoom");
    gl.uniform1f(dropGLData.zoomPointer, currentZoom);
	
	dropGLData.viewOffsetPointer = gl.getUniformLocation(dropGLData.program, "viewOffset");
    gl.uniform2f(dropGLData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
}

function createDropVerticies(circleRadius)
{
	let verticies = [];
	verticies.push(vec2(0, 0));
	
	const missingPoints = DROP_POINT_COUNT - 2;
	const angleBetweenPoints = (Math.PI / 180) * (360 / missingPoints);
	for(var i = 0; i < missingPoints; i++)
	{
		verticies.push(vec2(circleRadius * Math.cos(i * angleBetweenPoints), circleRadius * Math.sin(i * angleBetweenPoints)));
	}
	verticies.push(vec2(circleRadius * Math.cos(0), circleRadius * Math.sin(0)));
	
	return verticies;
}

function renderBoard() 
{
	gl.useProgram(boardGLData.program);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.electrodeBuffer);
    gl.vertexAttribPointer(boardGLData.electrodePointer, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(boardGLData.electrodePointer);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.positionBuffer);
    gl.vertexAttribPointer(boardGLData.positionPointer, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(boardGLData.positionPointer);
	gl.vertexAttribDivisor(boardGLData.positionPointer, 1);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.colorBuffer);
    gl.vertexAttribPointer(boardGLData.colorPointer, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(boardGLData.colorPointer);
	gl.vertexAttribDivisor(boardGLData.colorPointer, 1);
	
    gl.drawArraysInstanced(gl.TRIANGLES, 0, boardGLData.eletrodeVerticiesCount, boardGLData.electrodeCount);
}

function renderDrops(dropCount)
{
	gl.useProgram(dropGLData.program);
	
	gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.dropBuffer);
	gl.vertexAttribPointer(dropGLData.dropPointer, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(dropGLData.dropPointer);

	gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.positionBuffer);
	gl.vertexAttribPointer(dropGLData.positionPointer, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(dropGLData.positionPointer);
	gl.vertexAttribDivisor(dropGLData.positionPointer, 1);

	gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.sizeBuffer);
	gl.vertexAttribPointer(dropGLData.sizePointer, 1, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(dropGLData.sizePointer);
	gl.vertexAttribDivisor(dropGLData.sizePointer, 1);

	gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.colorBuffer);
	gl.vertexAttribPointer(dropGLData.colorPointer, 4, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(dropGLData.colorPointer);
	gl.vertexAttribDivisor(dropGLData.colorPointer, 1);
	
	gl.drawArraysInstanced(gl.TRIANGLE_FAN, 0, DROP_POINT_COUNT, dropCount);
}

function drawElectrodeOn(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.colorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_ON_COLOR), 0, 4);
}

function drawElectrodeOff(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, boardGLData.colorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_OFF_COLOR), 0, 4);
}

function updateDropData(drops)
{
	var dropPositions = new Float32Array(drops.length * 2);
	var dropSizes     = new Float32Array(drops.length * 1);
	var dropColors    = new Float32Array(drops.length * 4);
	
	for(var i = 0; i < drops.length; i++)
	{
		const drop = drops[i];
		
		dropPositions[i * 2 + 0] = drop.position[0];
		dropPositions[i * 2 + 1] = drop.position[1];
		
		dropSizes[i * 1 + 0] = drop.size;
		
		dropColors[i * 4 + 0] = drop.color[0];
		dropColors[i * 4 + 1] = drop.color[1];
		dropColors[i * 4 + 2] = drop.color[2];
		dropColors[i * 4 + 3] = drop.color[3];
	}
	
    gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropPositions, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.sizeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropSizes, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, dropGLData.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropColors, gl.DYNAMIC_DRAW);
}

function render(dropCount)
{
	gl.clear(gl.COLOR_BUFFER_BIT);
	
	renderBoard();
	renderDrops(dropCount);
}

function offsetCurrentViewPosition(x, y)
{
	currentViewOffsetX += x;
	currentViewOffsetY += y;
	
	gl.useProgram(dropGLData.program);
    gl.uniform2f(dropGLData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
	
	gl.useProgram(boardGLData.program);
    gl.uniform2f(boardGLData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
}

function changeZoom(zoom)
{
	currentZoom += zoom;
	
	gl.useProgram(dropGLData.program);
    gl.uniform1f(dropGLData.zoomPointer, currentZoom);
	
	gl.useProgram(boardGLData.program);
    gl.uniform1f(boardGLData.zoomPointer, currentZoom);
}


























