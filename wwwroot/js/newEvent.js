
const clamp = (n,min,max) => Math.max(Math.min(max,n),min),Months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],DaysOfWeek = ["S", "M", "T", "W", "T", "F", "S"];
let topLeft = null,DateLoaded = null,sunday=null,IsMouseDownDates = false,IsMouseDownSideLabel = false,IsMouseDownTopLabel = false,AnchorRow = 0,AnchorColumn = 0,HoverRow = 0,HoverColumn = 0,WriteMode = "",Dates = [],xMousePos,yMousePos,xMouseRef,yMouseRef;

function Unique(InputArray) {return Array.from(new Set(InputArray)).filter(s=>s).sort()}
function ArraySearch(arr, obj) {return arr.includes(obj)}
function hide(el) { el.classList.add('none') }
function show(el) { el.classList.remove('none') }

function ShiftCalendar(Delta) {
    for (let Row = 1; Row <= 5; Row++) {
        let ThisWeek = false;
        for (let Column = 1; Column <= 7; Column++) {
            const el = document.getElementById("dateOf-" + Row + "-" + Column)
            const dateOfCell = el.value.split("-");
            const t = new Date(dateOfCell[0], dateOfCell[1] - 1, dateOfCell[2], 12, 0);
            t.setTime(t.getTime() + 60 * 60 * 24 * 1000 * Delta);
            const FullDateString = t.getFullYear() + "-" + (String(t.getMonth() + 1).padStart(2,0)) + "-" + (String(t.getDate()).padStart(2,0));
            el.value = FullDateString;
            if ((Row === 1) && (Column === 1)) topLeft = FullDateString;
            const el2 = document.getElementById("day-" + Row + "-" + Column);
            if (FullDateString === DateLoaded) {el2.style.fontWeight = "bold";ThisWeek = true}
            else el2.style.fontWeight = "normal";
            el2.innerHTML = t.getDate();
        }

        const dateOfCell = document.getElementById("dateOf-" + Row + "-1").value.split("-");
        const dateOfCell2 = document.getElementById("dateOf-" + Row + "-7").value.split("-");
        const MonthName = Months[dateOfCell[1]-1] + (dateOfCell[1] === dateOfCell2[1] ? '' : "/" + Months[dateOfCell2[1] - 1]);
        const Year = dateOfCell[0] === dateOfCell2[0] ? dateOfCell[0] : (dateOfCell[0] + "/" + dateOfCell2[0]);
        document.getElementById("month-" + Row).innerHTML = ThisWeek ? "<b>" + MonthName + "</b>" : MonthName;
        document.getElementById("year-" + Row).innerHTML = ThisWeek ? "<b>" + Year + "</b>" : Year;
    }

    for (let Column = 1; Column <= 7; Column++) {
        const dateOfCell = document.getElementById("dateOf-1-" + Column).value.split("-");
        const t = new Date(dateOfCell[0], dateOfCell[1] - 1, dateOfCell[2], 12, 0);
        document.getElementById("dayOfWeek-" + Column).innerHTML = DaysOfWeek[t.getDay()];
    }

    ReColor();
}

const emptyColor = '#ffdede';
const filledColor = '#070';
const emptyText = '#000';
const filledText = '#fff';

function ReColor() {
    for (let Row = 1; Row <= 5; Row++) for (let Column = 1; Column <= 7; Column++) {
        const day = document.getElementById('day-'+Row+'-'+Column).style;
        if (WriteMode != "" && (AnchorRow - Row) * (Row - HoverRow) >= 0 && (AnchorColumn - Column) * (Column - HoverColumn) >= 0) {
            if (WriteMode == "write") {
                day.backgroundColor = filledColor;
                day.color = filledText;
            }
            else {
                day.backgroundColor = emptyColor;
                day.color = emptyText;
            }
            day.border = '0px solid black';
            day.padding = '4px';
            day.margin = '0px';
        }
        else {
            day.border = '1px solid black';
            day.padding = '2px';
            day.margin = '1px';
            if (ArraySearch(Dates, document.getElementById("dateOf-" + Row + "-" + Column).value)) {
                day.backgroundColor = filledColor;
                day.color = filledText;
            }
            else {
                day.backgroundColor = emptyColor;
                day.color = emptyText;
            }
        }
    }
}


function AddHighlightedDates(HighlightedDates) {
    let DateNum = -1;
    while (DateNum < HighlightedDates.length) {
        Dates.push(HighlightedDates[DateNum]);
        DateNum++;
    }
    Dates = Unique(Dates);
    console.log(Dates);
    document.getElementById("possibleDates").value = Dates.join(",");
}

function RemoveHighlightedDates(HighlightedDates) {
    Dates = Dates.filter(d => !HighlightedDates.includes(d));
    document.getElementById("possibleDates").value = Dates.join(",");
    console.log(Dates)
}

document.addEventListener('mouseup',()=>{
    if (WriteMode != "") {
        const HighlightedDates = [];
        for (let Row = 1; Row < 6; Row++) {
            for (let Column = 1; Column <= 7; Column++) {
                if ((AnchorRow - Row) * (Row - HoverRow) >= 0 && (AnchorColumn - Column) * (Column - HoverColumn) >= 0) 
                    HighlightedDates.push(document.getElementById("dateOf-" + Row + "-" + Column).value);
                const el = document.getElementById("day-" + Row + "-" + Column).style;
                el.border = '1px solid black';
                el.padding = '2px';
                el.margin = '1px';
            }
        }

        WriteMode == "write" ? AddHighlightedDates(HighlightedDates) : RemoveHighlightedDates(HighlightedDates);
    }

    WriteMode = "";
    IsMouseDownDates = false;
    IsMouseDownSideLabel = false;
    IsMouseDownTopLabel = false;
});

document.addEventListener('mousemove',e=>{
    xMousePos = e.pageX;
    yMousePos = e.pageY;
    if (IsMouseDownSideLabel) {
        if (yMousePos > yMouseRef + 20) {
            yMouseRef = yMousePos;
            ShiftCalendar(-7);
        }
        else if (yMousePos < yMouseRef - 20) {
            yMouseRef = yMousePos;
            ShiftCalendar(+7);
        }
    }

    if (IsMouseDownTopLabel) {
        const dateOfCell = document.getElementById("dateOf-1-1").value.split("-");
        const t = new Date(dateOfCell[0], dateOfCell[1] - 1, dateOfCell[2], 12, 0);
        if (xMousePos > xMouseRef + 20) {
            xMouseRef = xMousePos;
            if (t.getDay() == 1) ShiftCalendar(-1);
        }
        else if (xMousePos < xMouseRef - 20) {
            xMouseRef = xMousePos;
            if (t.getDay() == 0) ShiftCalendar(+1);
        }
    }
});



function prevDef(e){e.preventDefault()}
const Calendar = document.getElementById('calendar');
Calendar.addEventListener('touchstart',prevDef);
Calendar.addEventListener('touchmove',prevDef);
Calendar.onselectstart=()=>false;
Calendar.onmousedown=()=>false;

function MouseDownSideLabel() { IsMouseDownSideLabel = true; yMouseRef = yMousePos; }

function RowLabelTouchStart(event) {
    event.preventDefault();
    yMouseRef = event.targetTouches[0].pageY;
}

function RowLabelTouchEnd(e) { document.onmouseup() }

function RowLabelTouchMove(event) {
    event.preventDefault();
    yMousePos = event.targetTouches[0].pageY;

    if (yMousePos > yMouseRef + 20) {
        yMouseRef = yMousePos;
        ShiftCalendar(-7);
    }
    else if (yMousePos < yMouseRef - 20) {
        yMouseRef = yMousePos;
        ShiftCalendar(+7);
    }
}

document.querySelectorAll('.dateSide').forEach((el,i) => {
    el.addEventListener('mousedown',MouseDownSideLabel);
    el.addEventListener('touchstart',RowLabelTouchStart);
    el.addEventListener('touchcancel',RowLabelTouchEnd);
    el.addEventListener('touchend',RowLabelTouchEnd);
    el.addEventListener('touchmove',RowLabelTouchMove);
});

function MouseOverDate(Row, Column) {
    if (!IsMouseDownDates) return;
    HoverRow = Row;
    HoverColumn = Column;
    ReColor();
}

function MouseDownDate(Row, Column) {
    IsMouseDownDates = true;
    AnchorRow = HoverRow = Row;
    AnchorColumn = HoverColumn = Column;
    WriteMode = ArraySearch(Dates, document.getElementById("dateOf-" + Row + "-" + Column).value) ? "erase" : "write";
    ReColor();
}

function DateTouchStart(event) {
    let target = event.target;
    while (target.nodeType != 1) target = target.parentNode;
    target.onmousedown = null;
    target.onmouseover = null;
    document.onmousemove = null;
    const parts = target.id.split("-");
    MouseDownDate(parts[1], parts[2]);
    event.preventDefault();
    xMouseRef = event.targetTouches[0].pageX;
    yMouseRef = event.targetTouches[0].pageY;
}

function DateTouchEnd(e) { document.onmouseup() }

function DateTouchMove(event) {
    event.preventDefault();
    xMousePos = event.targetTouches[0].pageX;
    yMousePos = event.targetTouches[0].pageY;
    HoverRow = clamp(Number(AnchorRow) + Math.floor((yMousePos - yMouseRef) / 23),1,5);
    HoverColumn = clamp(Number(AnchorColumn) + Math.floor((xMousePos - xMouseRef) / 23),1,7);
    ReColor();
}

document.querySelectorAll('.dateBox').forEach(el => {
    const row = el.dataset.row;
    const col = el.dataset.col;
    el.addEventListener('mouseover',()=>MouseOverDate(row,col));
    el.addEventListener('mousedown',()=>MouseDownDate(row,col));
});

function ColumnLabelTouchStart(event) {
    event.preventDefault();
    xMouseRef = event.targetTouches[0].pageX;
}

function ColumnLabelTouchEnd(e) { document.onmouseup() }

function ColumnLabelTouchMove(event) {
    var dateOfCell = document.getElementById("dateOf-1-1").value.split("-");
    var date = new Date(dateOfCell[0], dateOfCell[1] - 1, dateOfCell[2], 12, 0);
    event.preventDefault();
    xMousePos = event.targetTouches[0].pageX;
    
    if (xMousePos > xMouseRef + 20) {
        xMouseRef = xMousePos;
        if (date.getDay() == 1) ShiftCalendar(-1);
    }
    else if (xMousePos < xMouseRef - 20) {
        xMouseRef = xMousePos;
        if (date.getDay() == 0) ShiftCalendar(+1);
    }
}

function MouseDownTopLabel() { IsMouseDownTopLabel = true; xMouseRef = xMousePos; }

document.querySelectorAll('.cal-week').forEach((el,i)=>{
    el.id = `dayOfWeek-${i+1}`;
    el.addEventListener('touchstart', ColumnLabelTouchStart);
    el.addEventListener('touchcancel', ColumnLabelTouchEnd);
    el.addEventListener('touchend', ColumnLabelTouchEnd);
    el.addEventListener('touchmove', ColumnLabelTouchMove);
    el.addEventListener('mousedown', MouseDownTopLabel);
});


function ShiftToDate(NewTopLeftDate, ShiftToLeft) {
    const OldStartTimeString = document.getElementById("dateOf-1-1").value.split("-");
    const NewStartTimeString = NewTopLeftDate.split("-");
    const ShiftDays = (
        (new Date(NewStartTimeString[0], NewStartTimeString[1] - 1, NewStartTimeString[2], 12, 0)).getTime() - 
        (new Date(OldStartTimeString[0], OldStartTimeString[1] - 1, OldStartTimeString[2], 12, 0)).getTime()
    ) / (60 * 60 * 24 * 1000);

    ShiftToLeft ? ShiftCalendar(ShiftDays) : ShiftCalendar(7 * Math.floor(ShiftDays / 7));
}

document.querySelector('#jumpToDate button').addEventListener('click',e=>{
    e.preventDefault();
    ShiftToDate(DateLoaded,false)
})

document.getElementById('form').onsubmit=()=>{
    if (document.getElementById("newEventName").value == "New Event Name") {
        alert("Your event cannot be called \"New Event Name\"");
        return false;
    }

    const pattern = /-/;

    if (!Dates.some(d=>d.match(pattern))) {
        alert("You must select at least one date.");
        return false;
    }

    return true;
}


function PageLoaded() {
    DateLoaded=Calendar.dataset.loaded;
    sunday = Calendar.dataset.sunday;
    ShiftToDate(sunday, true);
    Dates = document.getElementById("possibleDates").value.split("|");
    ReColor();
}

addEventListener('load',PageLoaded);


