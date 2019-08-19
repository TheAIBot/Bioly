'use strict';

var gl;
var ext;
var glData = 
{
	electrode: {},
	drop: {},
	area: {},
	areaBorder: {}
};

const ELECTRODE_OFF_COLOR     = vec4(0.85, 0.85, 0.85, 1.0);
const ELECTRODE_ON_COLOR      = vec4(0.7 , 0.7 , 0.7 , 1.0);
const ELECTRODE_HIDDEN_COLOR  = vec4(0.0 , 0.0 , 0.0 , 0.0);
const DROP_POINT_COUNT = 30;
//ratio between electrode size and electrode spacing
const ratioForSpace = 0.1;

var currentZoom = 1;
var currentViewOffsetX = 0;
var currentViewOffsetY = 0;

function initRender()
{
    const canvas = document.getElementById("simulatorCanvas");
	const parentWidth = canvas.parentNode.clientWidth;
	const parentHeight = canvas.parentNode.clientHeight;
	const canvasSize = Math.min(parentWidth, parentHeight);
	
	canvas.width  = canvasSize;
	canvas.height = canvasSize;
    
    gl = WebGLUtils.setupWebGL(canvas);
	ext = gl.getExtension('ANGLE_instanced_arrays');
    if (!gl) 
    {
        alert("failed to load webgl2");
    }
    
    gl.viewport(0, 0, canvasSize, canvasSize);
    gl.clearColor(0, 0, 0, 0);
    
    glData.program = initShaders(gl, "vertex-shader", "fragment-shader");
	gl.useProgram(glData.program);
	
	gl.enable(gl.BLEND);
	gl.blendFunc(gl.SRC_COLOR, gl.DST_COLOR);
	
	startSimulator(5, 5, [{index: 6, color: vec4(1, 0, 0, 0.5)}], [], [true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, ]);
	addCommand("show_area 'asd' 0 0 3 3 0 1 0.1");
}

function setupBuffers(width, height, enabledElectrodes)
{
	glData.shapePointer = gl.getAttribLocation(glData.program, "vShape");
	glData.positionPointer = gl.getAttribLocation(glData.program, "vPosition");
	glData.sizePointer = gl.getAttribLocation(glData.program, "size");
	glData.colorPointer = gl.getAttribLocation(glData.program, "vColor");
				
	glData.zoomPointer = gl.getUniformLocation(glData.program, "zoom");
    gl.uniform1f(glData.zoomPointer, currentZoom);
	
	glData.viewOffsetPointer = gl.getUniformLocation(glData.program, "viewOffset");
    gl.uniform2f(glData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
	
	const electrodeData = setupElectrodeBuffers(width, height, enabledElectrodes);
	setupDropBuffers(electrodeData.electrodeSize / 2);
	setupAreaBuffers(electrodeData.electrodeSize);
	setupAreaBorderBuffers(electrodeData.electrodeSize);
	
	return electrodeData;
}

function setupElectrodeBuffers(width, height, enabledElectrodes)
{	
    let electrodeData = createElectrodeVertexData(width, height, enabledElectrodes);
	
    glData.electrode.shapeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.shapeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(electrodeData.electrodeVerticies), gl.STATIC_DRAW);

	glData.electrode.positionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(electrodeData.electrodePositions), gl.STATIC_DRAW);

	glData.electrode.sizeBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.sizeBuffer);
	gl.bufferData(gl.ARRAY_BUFFER, flatten(electrodeData.electrodeSizes), gl.STATIC_DRAW);
	
	glData.electrode.colorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(electrodeData.electrodeColors), gl.DYNAMIC_DRAW);
	
	glData.electrode.shapeVerticiesCount = electrodeData.electrodeVerticies.length;
	glData.electrode.electrodesCount = width * height;
	
	return electrodeData;
}

function createElectrodeVertexData(width, height, enabledElectrodes)
{	
	const borderSize = 0.10;
	const boardSize = 2 - (borderSize * 2);
	
	const electrodeSize = boardSize / (Math.max(width, height) + ratioForSpace * (Math.max(width, height) - 1));
	const topLeftX = -((electrodeSize * width  + electrodeSize * (width  - 1) * ratioForSpace) / 2) + (electrodeSize / 2);
	const topLeftY =  ((electrodeSize * height + electrodeSize * (height - 1) * ratioForSpace) / 2) - (electrodeSize / 2);
	
	let boardData = {};
	boardData.electrodeSize = electrodeSize;
	boardData.electrodeVerticies = createElectrodeVerticies(electrodeSize);
	boardData.electrodePositions = createElectrodePositions(width, height, topLeftX, topLeftY, ratioForSpace, electrodeSize);
	boardData.electrodeSizes     = createElectrodeSizes(width, height);
	boardData.electrodeColors    = createElectrodeColors(width, height, enabledElectrodes);
	
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

function createElectrodeSizes(width, height)
{
	let sizes = [];
	
	for(var x = 0; x < width * height; x++)
	{
		sizes.push(vec2(1, 1));
	}
	
	return sizes;
}

function createElectrodeColors(width, height, enabledElectrodes)
{
	let colors = [];
	
	for(var x = 0; x < width * height; x++)
	{
		if (enabledElectrodes[x])
		{
			colors.push(ELECTRODE_OFF_COLOR);
		}
		else
		{
			colors.push(ELECTRODE_HIDDEN_COLOR);
		}
	}
	
	return colors;
}

function setupDropBuffers(dropRadius)
{	
	const dropVerticies = createDropVerticies(dropRadius);
	
    glData.drop.shapeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.drop.shapeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(dropVerticies), gl.STATIC_DRAW);
	
	glData.drop.positionBuffer = gl.createBuffer();
	glData.drop.sizeBuffer = gl.createBuffer();
	glData.drop.colorBuffer = gl.createBuffer();
	
	glData.drop.shapeVerticiesCount = DROP_POINT_COUNT;
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

function setupAreaBuffers(electrodeSize)
{
	const electrodeVerticies = createElectrodeVerticies(electrodeSize);
	
	glData.area.shapeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.area.shapeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(electrodeVerticies), gl.STATIC_DRAW);
	
	glData.area.positionBuffer = gl.createBuffer();
	glData.area.sizeBuffer = gl.createBuffer();
	glData.area.colorBuffer = gl.createBuffer();
	
	glData.area.shapeVerticiesCount = electrodeVerticies.length;
}

function setupAreaBorderBuffers(electrodeSize)
{
	const borderVerticies = createAreaBorder(electrodeSize);
	
	glData.areaBorder.shapeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.areaBorder.shapeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(borderVerticies), gl.STATIC_DRAW);
	
	glData.areaBorder.positionBuffer = glData.area.positionBuffer;
	glData.areaBorder.sizeBuffer = glData.area.sizeBuffer;
	glData.areaBorder.colorBuffer = gl.createBuffer();
	
	glData.areaBorder.shapeVerticiesCount = borderVerticies.length;
	
}

function createAreaBorder(electrodeSize)
{
	const borderPos = electrodeSize / 2;
	const borderSize = (electrodeSize * ratioForSpace) / 64;
	
	const verticies = [];
	//left 1
	verticies.push(vec2( borderPos - borderSize,  borderPos - borderSize));
	verticies.push(vec2( borderPos + borderSize,  borderPos - borderSize));
	verticies.push(vec2( borderPos + borderSize, -borderPos - borderSize));
	//left 2
	verticies.push(vec2( borderPos - borderSize, -borderPos - borderSize));
	verticies.push(vec2( borderPos + borderSize, -borderPos - borderSize));
	verticies.push(vec2( borderPos - borderSize,  borderPos - borderSize));
	
	//right 1
	verticies.push(vec2(-borderPos - borderSize,  borderPos - borderSize));
	verticies.push(vec2(-borderPos + borderSize,  borderPos - borderSize));
	verticies.push(vec2(-borderPos + borderSize, -borderPos + borderSize));
	//right 2
	verticies.push(vec2(-borderPos - borderSize, -borderPos + borderSize));
	verticies.push(vec2(-borderPos + borderSize, -borderPos + borderSize));
	verticies.push(vec2(-borderPos - borderSize,  borderPos - borderSize));
	
	//top 1
	verticies.push(vec2( borderPos + borderSize,  borderPos - borderSize));
	verticies.push(vec2( borderPos + borderSize,  borderPos + borderSize));
	verticies.push(vec2(-borderPos - borderSize,  borderPos - borderSize));
	//top 2
	verticies.push(vec2(-borderPos - borderSize,  borderPos - borderSize));
	verticies.push(vec2(-borderPos - borderSize,  borderPos + borderSize));
	verticies.push(vec2( borderPos + borderSize,  borderPos + borderSize));
	
	//bottom 1
	verticies.push(vec2( borderPos - borderSize, -borderPos - borderSize));
	verticies.push(vec2( borderPos - borderSize, -borderPos + borderSize));
	verticies.push(vec2(-borderPos - borderSize, -borderPos - borderSize));
	//bottom 2
	verticies.push(vec2(-borderPos - borderSize, -borderPos - borderSize));
	verticies.push(vec2(-borderPos - borderSize, -borderPos + borderSize));
	verticies.push(vec2( borderPos - borderSize, -borderPos + borderSize));
	
	return verticies;
}

function renderBuffers(drawMode, buffers, shapesCount)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, buffers.shapeBuffer);
	gl.vertexAttribPointer(glData.shapePointer, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(glData.shapePointer);

	gl.bindBuffer(gl.ARRAY_BUFFER, buffers.positionBuffer);
	gl.vertexAttribPointer(glData.positionPointer, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(glData.positionPointer);
	ext.vertexAttribDivisorANGLE(glData.positionPointer, 1);

	gl.bindBuffer(gl.ARRAY_BUFFER, buffers.sizeBuffer);
	gl.vertexAttribPointer(glData.sizePointer, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(glData.sizePointer);
	ext.vertexAttribDivisorANGLE(glData.sizePointer, 1);

	gl.bindBuffer(gl.ARRAY_BUFFER, buffers.colorBuffer);
	gl.vertexAttribPointer(glData.colorPointer, 4, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(glData.colorPointer);
	ext.vertexAttribDivisorANGLE(glData.colorPointer, 1);
	
	ext.drawArraysInstancedANGLE(drawMode, 0, buffers.shapeVerticiesCount, shapesCount);
}

function drawElectrodeOn(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.colorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_ON_COLOR), 0, 4);
}

function drawElectrodeOff(electrodeNumber)
{
	gl.bindBuffer(gl.ARRAY_BUFFER, glData.electrode.colorBuffer);
	gl.bufferSubData(gl.ARRAY_BUFFER, electrodeNumber * 4 * 4, flatten(ELECTRODE_OFF_COLOR), 0, 4);
}

function updateDropData(drops)
{
	var dropPositions = new Float32Array(drops.length * 2);
	var dropSizes     = new Float32Array(drops.length * 2);
	var dropColors    = new Float32Array(drops.length * 4);
	
	for(var i = 0; i < drops.length; i++)
	{
		const drop = drops[i];
		
		dropPositions[i * 2 + 0] = drop.position[0];
		dropPositions[i * 2 + 1] = drop.position[1];
		
		dropSizes[i * 2 + 0] = drop.size;
		dropSizes[i * 2 + 1] = drop.size;
		
		dropColors[i * 4 + 0] = drop.color[0];
		dropColors[i * 4 + 1] = drop.color[1];
		dropColors[i * 4 + 2] = drop.color[2];
		dropColors[i * 4 + 3] = drop.color[3];
	}
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.drop.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropPositions, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.drop.sizeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropSizes, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.drop.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, dropColors, gl.DYNAMIC_DRAW);
}

function updateAreaData(areas)
{
	var areaPositions    = new Float32Array(areas.length * 2);
	var areaSizes        = new Float32Array(areas.length * 2);
	var areaColors       = new Float32Array(areas.length * 4);
	var areaBordorColors = new Float32Array(areas.length * 4);
	
	for(var i = 0; i < areas.length; i++)
	{
		const area = areas[i];
		
		areaPositions[i * 2 + 0] = area.position[0];
		areaPositions[i * 2 + 1] = area.position[1];
		
		areaSizes[i * 2 + 0] = area.size[0];
		areaSizes[i * 2 + 1] = area.size[1];
		
		areaColors[i * 4 + 0] = area.color[0];
		areaColors[i * 4 + 1] = area.color[1];
		areaColors[i * 4 + 2] = area.color[2];
		areaColors[i * 4 + 3] = 0.3;
		
		areaBordorColors[i * 4 + 0] = area.color[0];
		areaBordorColors[i * 4 + 1] = area.color[1];
		areaBordorColors[i * 4 + 2] = area.color[2];
		areaBordorColors[i * 4 + 3] = 0.7;
	}
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.area.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, areaPositions, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.area.sizeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, areaSizes, gl.DYNAMIC_DRAW);
	
    gl.bindBuffer(gl.ARRAY_BUFFER, glData.area.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, areaColors, gl.DYNAMIC_DRAW);
	
	gl.bindBuffer(gl.ARRAY_BUFFER, glData.areaBorder.colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, areaBordorColors, gl.DYNAMIC_DRAW);
}

function render(dropCount, areasCount)
{
	gl.clear(gl.COLOR_BUFFER_BIT);
	
	gl.blendFunc(gl.SRC_COLOR, gl.DST_COLOR);
	renderBuffers(gl.TRIANGLES, glData.electrode, glData.electrode.electrodesCount);
	renderBuffers(gl.TRIANGLES, glData.area, areasCount);
	gl.blendFunc(gl.ONE, gl.ZERO);
	renderBuffers(gl.TRIANGLE_FAN, glData.drop, dropCount);
	//renderBuffers(gl.TRIANGLES, glData.areaBorder, areasCount);
}

function offsetCurrentViewPosition(x, y)
{
	currentViewOffsetX += x;
	currentViewOffsetY += y;
	
    gl.uniform2f(glData.viewOffsetPointer, currentViewOffsetX, currentViewOffsetY);
}

function changeZoom(zoom)
{
	currentZoom += zoom;
	currentZoom = Math.max(0.1, currentZoom);
    gl.uniform1f(glData.zoomPointer, currentZoom);
}


























