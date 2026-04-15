window.availabilityMatrix = {
    init: (dotNetRef, tableId, enableClick) => {
        console.log("init called, tableId:", tableId, "enableClick:", enableClick);
        const table = document.getElementById(tableId);
        if (!table) {
            console.error("Table not found:", tableId);
            return;
        }
        console.log("Table found");
        
        // Prevent horizontal scroll on wheel
        table.addEventListener('wheel', (e) => {
            if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) return;
            if (e.deltaY !== 0 && !e.shiftKey) {
                e.preventDefault();
                window.scrollBy(0, e.deltaY);
            }
        }, { passive: false });
        
        // Only attach click handler if enabled
        if (enableClick) {
            table.addEventListener('click', (e) => {
                const cell = e.target.closest('td[data-slot-id]');
                if (cell) {
                    const slotId = cell.dataset.slotId;
                    console.log("Cell clicked, slotId:", slotId);
                    dotNetRef.invokeMethodAsync('OnSelectionChanged', [slotId]);
                }
            });
        } else {
            console.log("Click handler disabled for read-only matrix");
        }
    }
};