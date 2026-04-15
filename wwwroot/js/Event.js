{// code adapted from when2meet pending in-house implementation
document.ondragstart = function () { return false; };
document.onmouseup = SelectStop;
var ChangeToAvailable = false;
var IsMouseDown = false;
var PeopleNames = [];
var PeopleIDs = [];
var UserID = 0;
var TimeOfSlot = [];
var AvailableIDs = [];
var AvailableAtSlot = [];
var FromCol = -1; var ToCol = -1;
var FromRow = -1; var ToRow = -1;
var select;
const dayOfWeekNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

const format = new Intl.DateTimeFormat(0, { second: '2-digit', minute: '2-digit', hour: '2-digit', day: '2-digit', weekday: 'short', month: 'short', year: 'numeric', timeZoneName: 'short' });

function hide(el) { el.classList.add('none') }
function show(el) { el.classList.remove('none') }

function PageLoaded() {
    select = document.getElementById("ParticipantTimeZone");
    const timezones = moment.tz.names();
    const timezone = moment.tz.guess();
    show(select);
    document.getElementById("YourTimeZone").innerHTML = 'Your Time Zone: ';
    for (var i = 0; i < timezones.length; i++) {
        const opt = timezones[i];
        const el = document.createElement("option");
        el.textContent = opt;
        el.value = opt;
        select.appendChild(el);
        if (timezone == opt) select.options[i].selected = true;
    }
    if (timezone != "America/Los_Angeles") LoadAvailabilityGrids();
}

function LoadAvailabilityGrids() {
    const participantTimeZone = select.options[select.selectedIndex].value;
    var parameters = "36075795-knaOh&id=36075795&code=knaOh&participantTimeZone=" + encodeURIComponent(participantTimeZone);
    function onSuccess(t) {
        document.getElementById("AvailabilityGrids").innerHTML = t.responseText;
        if (UserID) {
            hide(document.getElementById("SignIn"));
            show(document.getElementById("YouGrid"));
            document.getElementById("UserName").innerHTML = PeopleNames[PeopleIDs.indexOf(UserID)];
            ReColorIndividual();
        }
    }
    setTimeout(() => { onSuccess({ responseText: document.getElementById("AvailabilityGrids").innerHTML }) }, 1000);
    console.log(parameters);
    //new A_jax.Request("AvailabilityGrids.php", {method: "post",parameters,asynchronous: true,onSuccess});
}

function ReColorIndividual() {
    for (var i = 0; i < TimeOfSlot.length; i++) {
        [ColA,ColB] = FromCol < ToCol ? [FromCol,ToCol] : [ToCol,FromCol];
        [RowA,RowB] = FromRow < ToRow ? [FromRow,ToRow] : [ToRow,FromRow];

        const currentElement = document.getElementById("YouTime" + TimeOfSlot[i]);

        if (currentElement) {
            const dataCol = Number(currentElement.getAttribute("data-col"));
            const dataRow = Number(currentElement.getAttribute("data-row"));
            const NewColor = ChangeToAvailable ? '#339900' : "#ffdede";
            if (ColA <= dataCol && dataCol <= ColB && RowA <= dataRow && dataRow <= RowB && IsMouseDown) {
                currentElement.style.background = NewColor;
                currentElement.style.borderColor = NewColor;
                if (ColA == dataCol) currentElement.style.borderLeftColor = "black";
                if (ColB == dataCol) currentElement.style.borderRightColor = "black";
                if (RowA == dataRow) currentElement.style.borderTopColor = "black";
                if (RowB == dataRow) currentElement.style.borderBottomColor = "black";
            }
            else if (AvailableAtSlot[i].indexOf(UserID) !== -1) {
                currentElement.style.background = "#339900";
                currentElement.style.borderColor = "black";
            }
            else {
                currentElement.style.background = "#ffdede";
                currentElement.style.borderColor = "black";
            }
        }
    }
}

function ReColorGroup() {
    var MinAvailable = Infinity;
    var MaxAvailable = 0;
    var SelfIsAvailable = false;
    for (var i = 0; i < AvailableAtSlot.length; i++) {
        if (AvailableAtSlot[i].length < MinAvailable) MinAvailable = AvailableAtSlot[i].length;
        if (AvailableAtSlot[i].length > MaxAvailable) MaxAvailable = AvailableAtSlot[i].length;
        if (AvailableAtSlot[i].indexOf(UserID) != -1) SelfIsAvailable = true;
    }
    if ((AvailableIDs.indexOf(UserID) !== -1) && (!SelfIsAvailable)) {
        SplitSpot = AvailableIDs.indexOf(UserID);
        AvailableIDs.splice(SplitSpot, 1);
    }
    else if ((AvailableIDs.indexOf(UserID) === -1) && SelfIsAvailable) AvailableIDs.push(UserID);

    document.getElementById("MinAvailable").innerHTML = MinAvailable + "/" + AvailableIDs.length;
    document.getElementById("MaxAvailable").innerHTML = MaxAvailable + "/" + AvailableIDs.length;

    TableStart = '<table width=100 height=10 cellpadding=0 cellspacing=0 class="b"><tr>';
    TableEnd = "</tr></table>";
    TableContent = "";

    let Red, Green, Blue;

    for (let i = MinAvailable; i <= MaxAvailable; i++) {
        Red = Green = Blue = Math.round(255 / 2);
        if (MinAvailable != MaxAvailable) {
            Red = Math.round(((204) * (MaxAvailable - i) / (MaxAvailable - MinAvailable)) + 51);
            Green = Math.round(((102) * (MaxAvailable - i) / (MaxAvailable - MinAvailable)) + 153);
            Blue = Math.round(((255) * (MaxAvailable - i) / (MaxAvailable - MinAvailable)) + 0);
        }
        TableContent += `<td style="background-color:#${(Red * 256 * 256 + Green * 256 + Blue).toString(16)}">&nbsp;</td>`;
    }

    document.getElementById("GroupKey").innerHTML = TableStart + TableContent + TableEnd;

    for (let i = 0; i < AvailableAtSlot.length; i++) {
        Red = Green = Blue = Math.round(255 / 2);
        if (MinAvailable != MaxAvailable) {
            Red = Math.round(((204) * (MaxAvailable - AvailableAtSlot[i].length) / (MaxAvailable - MinAvailable)) + 51);
            Green = Math.round(((102) * (MaxAvailable - AvailableAtSlot[i].length) / (MaxAvailable - MinAvailable)) + 153);
            Blue = Math.round(((255) * (MaxAvailable - AvailableAtSlot[i].length) / (MaxAvailable - MinAvailable)) + 0);
        }
        document.getElementById("GroupTime" + TimeOfSlot[i]).style.background = "#" + (Red * 256 * 256 + Green * 256 + Blue).toString(16);
    }
}

function formatTime(time, timezone) {
    const date = new Date(time * 1000);
    const m = moment.tz(date, timezone);

    var HH = m.format('H').padStart(2, 0);
    const ap = (HH >= 12) ? "PM" : "AM";

    if (HH > 12) HH = HH - 12;
    if (HH == 0) HH = 12;

    const wd = dayOfWeekNames[m.format('e')].slice(0, 3);
    const d = m.format('D');
    const month = monthNames[m.format('M') - 1].substring(0, 3);
    const year = m.format('YYYY');
    const min = m.format('mm').slice(-2).padStart(2, 0);
    const tz = date.toLocaleDateString(0, { timeZoneName: 'short' }).slice(-3);

    //    "Fri     17     Apr     2026     05  :  00  :00 PM PDT"
    return `${wd} ${d} ${month} ${year} ${HH}:${min}:00 ${ap} ${tz}<br>${format.format(date)}`;
}

function showSlot(Time, TimeString = "") {
    var e = select;
    var timezone = e[e.selectedIndex].value;

    if (isNaN(Time) || (Time == 0)) {
        document.getElementById("AvailableDate").innerHTML = "&nbsp;";
        document.getElementById("Available").innerHTML = "";
        document.getElementById("Unavailable").innerHTML = "";
        document.getElementById("AvailableFraction").innerHTML = "0/" + AvailableIDs.length;
        return;
    }

    AvailableList = "";
    UnavailableList = "";
    AvailableCount = 0;
    Count = 0;
    for (var i = 0; i < AvailableIDs.length; i++) {
        Count++;
        var found = (-1 != AvailableAtSlot[TimeOfSlot.indexOf(Time)].indexOf(AvailableIDs[i]));
        if (found) {
            AvailableList += PeopleNames[PeopleIDs.indexOf(AvailableIDs[i])] + "<br>";
            AvailableCount++;
        }
        else UnavailableList += PeopleNames[PeopleIDs.indexOf(AvailableIDs[i])] + "<br>";
    }
    document.getElementById("Available").innerHTML = AvailableList;
    document.getElementById("Unavailable").innerHTML = UnavailableList;
    document.getElementById("AvailableFraction").innerHTML = AvailableCount + "/" + Count;
    var timeString = formatTime(Time, timezone);
    document.getElementById("AvailableDate").innerHTML = timeString + '<br>' + TimeString
    show(document.getElementById("SlotAvailability"));
    hide(document.getElementById("LeftPanel"));
}

function showSlotByTouch(event) {
    if (event.touches.length !== 1) return;
    var touch = event.touches[0];
    var elem = document.elementFromPoint(touch.clientX, touch.clientY);
    var Time = Number(elem.id.replace('GroupTime', ''));
    showSlot(Time);
}

function showSlotByTouchMove(event) {
    if (event.touches.length === 1) event.preventDefault();
    showSlotByTouch(event);
}

function restoreLeftSide(e) {
    hide(document.getElementById("SlotAvailability"));
    show(document.getElementById("LeftPanel"));
}

function setFromHere(e) {
    var timeOfSlot = Number(e.target.getAttribute("data-time"));
    var slotID = TimeOfSlot.indexOf(timeOfSlot);
    ChangeToAvailable = (-1 == AvailableAtSlot[slotID].indexOf(UserID));
    IsMouseDown = true;
    ToCol = Number(e.target.getAttribute("data-col"));
    ToRow = Number(e.target.getAttribute("data-row"));
    FromCol = ToCol;
    FromRow = ToRow;
    ReColorIndividual();
}

function setToHere(e) {
    if (!IsMouseDown) return
    ToCol = Number(e.target.getAttribute("data-col"));
    ToRow = Number(e.target.getAttribute("data-row"));
    ReColorIndividual();
}

function setFtomHereByTouch(event) {
    if (event.touches.length !== 1) return true;
    setFromHere(event);
}

function setToHereByTouch(event) {
    if(event.touches.length !== 1) return true;
    event.preventDefault();
    var touch = event.touches[0];
    var elem = document.elementFromPoint(touch.clientX, touch.clientY);
    if (!elem.hasAttribute("data-col")) return;
    ToCol = Number(elem.getAttribute("data-col"));
    ToRow = Number(elem.getAttribute("data-row"));
    ReColorIndividual();
}

function selectStopByTouch(e) {
    SelectStop();
}

function SelectStop() {
    if (!IsMouseDown) return;
    var TimesToToggle = [];
    var binaryAvailability = "";

    for (var i = 0; i < TimeOfSlot.length; i++) {
        if (FromCol < ToCol) { ColA = FromCol; ColB = ToCol; } else { ColA = ToCol; ColB = FromCol; }
        if (FromRow < ToRow) { RowA = FromRow; RowB = ToRow; } else { RowA = ToRow; RowB = FromRow; }
        var currentElement = document.getElementById("YouTime" + TimeOfSlot[i]);
        if (currentElement) {
            var dataCol = Number(currentElement.getAttribute("data-col"));
            var dataRow = Number(currentElement.getAttribute("data-row"));
            var WithinX = ((ColA <= dataCol) && (dataCol <= ColB));
            var WithinY = ((RowA <= dataRow) && (dataRow <= RowB));
            if (WithinX && WithinY) {
                TimesToToggle.push(TimeOfSlot[i]);
                if (ChangeToAvailable && (-1 == AvailableAtSlot[i].indexOf(UserID))) AvailableAtSlot[i].push(UserID);
                if ((!ChangeToAvailable) && (-1 != AvailableAtSlot[i].indexOf(UserID))) {
                    SplitSpot = AvailableAtSlot[i].indexOf(UserID);
                    AvailableAtSlot[i].splice(SplitSpot, 1);
                }
            }
            if (-1 != AvailableAtSlot[i].indexOf(UserID)) binaryAvailability += "1";
            else binaryAvailability += "0";
        }
    }

    var parameters = "person=" + UserID + "&event=36075795" + "&slots=" + TimesToToggle.join(",") + "&availability=" + binaryAvailability + "&password=" + encodeURIComponent(document.getElementById('password').value) + "&ChangeToAvailable=" + ChangeToAvailable;
    //new A_jax.Request("SaveTimes.php", { method: "post", parameters, asynchronous: true, onSuccess: function (t) {} });
    console.log(parameters);

    IsMouseDown = false;
    FromCol = -1; ToCol = -1; FromRow = -1; ToRow = -1;
    ReColorIndividual();
    ReColorGroup();
}


function setMouseLeaveEvent(el){
    el.addEventListener('mouseleave', restoreLeftSide);
}

function setAllEvents(el){
    el.addEventListener('mousedown',    setFromHere); 
    el.addEventListener('mouseover',    setToHere); 
    el.addEventListener('touchstart',   setFtomHereByTouch); 
    el.addEventListener('touchmove',    setToHereByTouch); 
    el.addEventListener('touchend',     selectStopByTouch);
}

function setTimeEvents(el){
    const time = Number(el.dataset.time);
    el.id = 'GroupTime' + time;
    el.addEventListener('mouseover', ()=>showSlot(time));
    el.addEventListener('mouseout', restoreLeftSide);
    el.addEventListener('touchstart', showSlotByTouch);
    el.addEventListener('touchmove', showSlotByTouchMove);
    el.addEventListener('touchend', restoreLeftSide);
}

let loaded;
let interact;
let hover;
function load(type){
    if(loaded)return;
    if(type=='interact') {
        interact=1
    }else if(type=='hover'){
        hover=1
    }
    if(interact&&hover){
        loaded=1;
        document.querySelectorAll('div.GroupGrid').forEach(setMouseLeaveEvent);
        document.querySelectorAll('.setAllEvents').forEach(setAllEvents);
        document.querySelectorAll('.setEvents').forEach(setTimeEvents);
    }
}
}