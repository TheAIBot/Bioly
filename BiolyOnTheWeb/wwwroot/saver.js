const quickSaveLoadFileName = "__qsl__";

function quickSave() {
    window.localStorage[quickSaveLoadFileName] = getWorkspaceAsXml();
}

function quickLoad() {
    const xmlString = window.localStorage[quickSaveLoadFileName];

    Blockly.mainWorkspace.clear();
    loadWorkspace(xmlString);
}