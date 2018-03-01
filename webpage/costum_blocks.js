Blockly.Blocks["start"] = {
	init: function() {
		this.jsonInit({
			"message0": "start %1",
			"args0": [
				{
					"type": "input_statement",
					"name": "program"
				}
			],
			"colour": 0,
			"tooltip": "Starts a bio program"
		});
	}
};

Blockly.Blocks["input"] = {
	init: function() {
		this.jsonInit({
			"message0": "new input",
			"args0": [
			],
			"message1": "name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "inputName",
					"variable": "input_fluid_name"
				}
			],
			"message2": "amount %1 %2",
			"args2": [
				{
					"type": "field_number",
					"name": "inputAmount",
					"check": "Number"
				},
				{
					"type": "field_dropdown",
					"name": "inputUnit",
					"options": [
						["drops", "0"],
						["ml", "1"]
					]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getInput"] = {
	init: function() {
		this.jsonInit({
			"message0": "input %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "inputName",
					"variable": "input_fluid_name"
				}
			],
			"output": "InputType",
			"colour": 40,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["fluid"] = {
	init: function() {
		this.jsonInit({
			"message0": "new fluid",
			"args0": [
			],
			"message1": "name %1 %2",
			"args1": [
				{
					"type": "field_variable",
					"name": "fluidName",
					"variable": "fluid_name"
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType", "FluidOperator"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 80,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getFluid"] = {
	init: function() {
		this.jsonInit({
			"message0": "fluid %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "fluidName",
					"variable": "fluid_name"
				}
			],
			"output": "FluidType",
			"colour": 80,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["mix"] = {
	init: function() {
		this.jsonInit({
			"message0": "mix",
			"args0": [
			],
			"message1": "a %1",
			"args1": [
				{
					"type": "input_value",
					"name": "inputFluidA",
					"check": ["InputType", "FluidType"]
				}
			],
			"message2": "b %1",
			"args2": [
				{
					"type": "input_value",
					"name": "inputFluidB",
					"check": ["InputType", "FluidType"]
				}
			],
			"output": "FluidOperator",
			"colour": 120,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["split"] = {
	init: function() {
		this.jsonInit({
			"message0": "split",
			"args0": [
			],
			"message1": "fluid %1",
			"args1": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"message2": "amount %1",
			"args2": [
				{
					"type": "field_number",
					"name": "fluidAmount",
					"check": "Number"
				}
			],
			"output": "FluidOperator",
			"colour": 160,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["heat"] = {
	init: function() {
		this.jsonInit({
			"message0": "heat",
			"args0": [
			],
			"message1": "fluid %1",
			"args1": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"message2": "temperature %1",
			"args2": [
				{
					"type": "field_number",
					"name": "temperature",
					"check": "Number"
				}
			],
			"message3": "time %1",
			"args3": [
				{
					"type": "field_number",
					"name": "time",
					"check": "Number"
				}
			],
			"output": "FluidOperator",
			"colour": 200,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["waste"] = {
	init: function() {
		this.jsonInit({
			"message0": "waste %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 240,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["output"] = {
	init: function() {
		this.jsonInit({
			"message0": "output %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["sensor"] = {
	init: function() {
		this.jsonInit({
			"message0": "sensor %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"output": "Number",
			"colour": 320,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["fluidArray"] = {
	init: function() {
		this.jsonInit({
			"message0": "new fluid array",
			"args0": [
			],
			"message1": "name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name"
				}
			],
			"message2": "length %1",
			"args2": [
				{
					"type": "input_value",
					"name": "arrayLength",
					"check": "Number"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["setFLuidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name"
				}
			],
			"message1": "set index %1 to %2",
			"args1": [
				{
					"type": "field_number",
					"name": "arrayName"
				},
				{
					"type": "input_value",
					"name": "value",
					"check": ["InputType", "FluidType", "FluidOperator"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getFLuidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name"
				}
			],
			"message1": "get index %1",
			"args1": [
				{
					"type": "field_number",
					"name": "arrayName"
				}
			],
			"output": "FluidType",
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};









