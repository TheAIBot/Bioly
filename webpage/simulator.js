'use strict';

var gl;
var boardVerticies;
var program;

window.onload = function init()
{
    const canvas = document.getElementById("simulatorCanvas");
	const parentWidth = canvas.parentNode.clientWidth;
	const parentHeight = canvas.parentNode.clientHeight;
	const canvasSize = Math.min(parentWidth, parentHeight);
	
	canvas.width = canvasSize;
	canvas.height = canvasSize;
    
    gl = WebGLUtils.setupWebGL(canvas);
    if (!gl) 
    {
        alert("failed to load webgl");
    }
    
    gl.viewport(0, 0, canvasSize, canvasSize);
    gl.clearColor(1, 1, 1, 1.0);
    
    //NOW DRAW SOME SHIT
    program = initShaders(gl, "vertex-shader", "fragment-shader");
    gl.useProgram(program);    
    
}

function setupSimulator(width, height)
{
    let verticies = setupBoard(width, height);
    
    var vertexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(verticies), gl.STATIC_DRAW);
	
    const vPosition = gl.getAttribLocation(program, "vPosition");
    gl.vertexAttribPointer(vPosition, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vPosition);
	
	/*
	var colorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(verticies), gl.STATIC_DRAW);
	
    const vColor = gl.getAttribLocation(program, "vColor");
    gl.vertexAttribPointer(vColor, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vColor);
    */
    render(verticies.length);
	
}

function setupBoard(width, height)
{
	let boardVerticies = [];
	
	const borderSize = 0.05;
	const boardSize = 2 - (borderSize * 2);
	const topLeftX = (-boardSize / 2);
	const topLeftY = (boardSize / 2);
	
	
	//ratio between electrode size and electrode spacing
	const ratioForSpace = 0.1;
	const electrodeSize =  (boardSize  / Math.max(width, height) ) * (1 - ratioForSpace);
	
	for(var y = 0; y < height; y++)
	{
		const sumElectrodeHeight = electrodeSize * y;
		const sumElectrodeHeightSpace = ratioForSpace * y * electrodeSize;
		const topY = topLeftY - sumElectrodeHeight - sumElectrodeHeightSpace
		for(var x = 0; x < width; x++)
		{
			const sumElectrodeWidth = electrodeSize * x;
			const sumElectrodeWidthSpace = ratioForSpace * x * electrodeSize;
			const topX = topLeftX + sumElectrodeWidth + sumElectrodeWidthSpace;
			
			const topLeft     = vec2(topX                , topY);
			const topRight    = vec2(topX + electrodeSize, topY);
			const bottomLeft  = vec2(topX                , topY + electrodeSize);
			const bottomRight = vec2(topX + electrodeSize, topY + electrodeSize);
			
			//add two triangles that together make the square electrode
			boardVerticies.push(topLeft   , topRight, bottomLeft);
			boardVerticies.push(bottomLeft, topRight, bottomRight);
		}
	}
	
	return boardVerticies;
}

function render(numPoints) 
{
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.TRIANGLES, 0, numPoints);
}


function updateLoop()
{
	//if new command then parse and execute command
	
	//detect errors
	
	//update drop positions
	
	//marge or split drops
}