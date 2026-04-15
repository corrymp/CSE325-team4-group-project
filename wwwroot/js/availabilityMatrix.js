window.availabilityMatrix = {
    init: (dotNetRef, tableId, enableClick) => {
        console.log("init called, tableId:", tableId, "enableClick:", enableClick);
        const table = document.getElementById(tableId);
        if (!table) {
            console.error("Table not found:", tableId);
            return;
        }
        console.log("Table found. Number of cells with data-slot-id:", table.querySelectorAll('td[data-slot-id]').length);
        
        // Wheel handler (unchanged)
        table.addEventListener('wheel', (e) => {
            if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) return;
            if (e.deltaY !== 0 && !e.shiftKey) {
                e.preventDefault();
                window.scrollBy(0, e.deltaY);
            }
        }, { passive: false });

        if (enableClick) {
            console.log("Attaching click handler to table");
            table.addEventListener('click', (e) => {
                console.log("Click detected on table");
                const cell = e.target.closest('td[data-slot-id]');
                if (cell) {
                    const slotId = cell.dataset.slotId;
                    console.log("Found cell with slotId:", slotId);
                    dotNetRef.invokeMethodAsync('OnSelectionChanged', [slotId])
                        .catch(err => console.error("Invoke error:", err));
                } else {
                    console.log("Clicked element is not a cell with data-slot-id");
                }
            });
        } else {
            console.log("Click handler disabled for read-only matrix");
        }
    }
};