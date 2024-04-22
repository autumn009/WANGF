var theBuffer = "";
var limit = 10000;

function appendBuffer(str) {
    theBuffer = theBuffer + str;
    //console.log("a" + theBuffer.length+" "+str.length);
}

function clearBuffer() {
    theBuffer = "";
}

function getBuffer() {
    var s1 = theBuffer.substring(0, limit);
    theBuffer = theBuffer.substring(limit);
    //console.log("g" + s1.length +" "+ theBuffer.length);
    return s1;
}

function localStorageGetItemWrapper(key) {
    var r = localStorage.getItem(key);
    if (r) {
        theBuffer = r;
        return "OK"
    }
    return null;
}

function localStorageSetItemWrapper(key) {
    localStorage.setItem(key, theBuffer);
}

function localStorageRemoveItemWrapper(key) {
    localStorage.removeItem(key);
}

function localStorageClear() {
    localStorage.clear();
}

function localStorageEnumKeys() {
    return Object.keys(localStorage);
}

function setBackColorWrapper(color) {
    var elm = document.getElementsByTagName('body')[0];
    elm.style.background = color;
}
function setLineInputTextValue(s) {
    lineInputDefaltText = s;
}
function lineInputReturnClicked() {
    lineInputText.value = lineInputDefaltText;
}
function myalert(s) {
    alert(s);
}
function setWaitCursor() {
    document.body.style.cursor = 'wait';
}
function resetWaitCursor() {
    document.body.style.cursor = 'auto';
}
var result = [];
function recursion(item) {
    var tempArray = Array.prototype.slice.call(item.children);
    if (item && item.id) {
        //console.log(item);
        var val = "";
        if (item.tagName == "INPUT" && item.getAttribute("type").toLowerCase() == "checkbox" && item.checked) val = "True";
        else if (item.tagName == "INPUT" && item.getAttribute("type").toLowerCase() == "radio" && item.checked) val = "True";
        else if (item.tagName == "OPTION" && item.getAttribute("selected")) val = "True";
        else if (item.tagName == "INPUT" && item.getAttribute("type").toLowerCase() == "text") val = item.value;
        else if (item.tagName == "TEXTAREA") val = item.value;
        else {
            tempArray.forEach(recursion);
            return;
        }
        result.push("htmlform_" + item.id);
        result.push(val);
    }
    tempArray.forEach(recursion);
}
function collectResult() {
    result = [];
    recursion(document.body); //body ˆÈ‰º‘S•”
    return result;
}

//function downloadFileFromStream(fileName, contentStreamReference) {
//}
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}