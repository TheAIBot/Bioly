Blockly.Blocks['mix'] = {
  init: function() {
    this.jsonInit({
      "message0": "mix %1 and %2",
      "args0": [
        {
          "type": "input_value",
          "name": "VALUE",
          "check": "String"
        },
		{
          "type": "input_value",
          "name": "VALUddE",
          "check": "String"
        }
      ],
	  "previousStatement": null,
	  "nextStatement": null,
      "output": "Number",
      "colour": 160,
      "tooltip": "Returns number of letters in the provided text.",
      "helpUrl": "http://www.w3schools.com/jsref/jsref_length_string.asp"
    });
  }
};