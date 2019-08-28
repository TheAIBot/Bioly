﻿const quickSaveLoadFileName = "__qsl__";
const filesStorageFileName = "__fsf__";

function quickSave() {
    window.localStorage[quickSaveLoadFileName] = getWorkspaceAsXml();
}

function quickLoad() {
    const xmlString = window.localStorage[quickSaveLoadFileName];

    Blockly.mainWorkspace.clear();
    loadWorkspace(xmlString);
}

function getAllLocalBlocklyFileNames() {
    if (window.localStorage[filesStorageFileName] == null) {
        window.localStorage[filesStorageFileName] = JSON.stringify({});
    }

    return Object.keys(JSON.parse(window.localStorage[filesStorageFileName])).join("@");
}

function getLocalBlocklyFileContent(filename) {
    return window.localStorage[filesStorageFileName][filename];
}