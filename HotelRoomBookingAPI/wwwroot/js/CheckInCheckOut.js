let currentOccupantsData = [];

function openCheckInCheckOutModal(bookingId) {
    // Clear previous data
    const tableBody = document.querySelector('#occupantStatusTable tbody');
    tableBody.innerHTML = '<tr><td colspan="5" class="text-center">Loading...</td></tr>';

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('checkInCheckOutModal'));
    modal.show();

    // Fetch occupants
    fetch(`/Reservations/GetOccupants?bookingId=${bookingId}`)
        .then(response => response.json())
        .then(data => {
            currentOccupantsData = data;
            renderOccupantsTable();
        })
        .catch(error => {
            console.error('Error fetching occupants:', error);
            tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error loading data.</td></tr>';
        });
}

function renderOccupantsTable() {
    const tableBody = document.querySelector('#occupantStatusTable tbody');
    tableBody.innerHTML = '';

    if (currentOccupantsData.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">No occupants found.</td></tr>';
        return;
    }

    currentOccupantsData.forEach((occupant, index) => {
        const row = document.createElement('tr');

        // Handle case sensitivity (PascalCase vs camelCase)
        const id = occupant.bookingOccupantId || occupant.BookingOccupantId;
        const name = occupant.fullName || occupant.FullName;
        const isCheckedIn = occupant.isCheckedIn !== undefined ? occupant.isCheckedIn : occupant.IsCheckedIn;
        const isCheckedOut = occupant.isCheckedOut !== undefined ? occupant.isCheckedOut : occupant.IsCheckedOut;

        const isTerminal = isCheckedOut;

        const checkInDisabled = isTerminal ? 'disabled' : '';
        const checkOutDisabled = (!isCheckedIn || isTerminal) ? 'disabled' : '';

        // HTML Generation
        row.innerHTML = `
            <td>${index + 1}</td>
            <td>${name}</td>
            <td>
                <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" id="checkIn_${id}" 
                        ${isCheckedIn ? 'checked' : ''} ${checkInDisabled}
                        onchange="handleStatusToggle(${id}, 'checkin')">
                    <label class="form-check-label" for="checkIn_${id}">
                        ${isCheckedIn ? 'Checked In' : 'Not Checked In'}
                    </label>
                </div>
            </td>
            <td>
                <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" id="checkOut_${id}" 
                        ${isCheckedOut ? 'checked' : ''} ${checkOutDisabled}
                        onchange="handleStatusToggle(${id}, 'checkout')">
                    <label class="form-check-label" for="checkOut_${id}">
                        ${isCheckedOut ? 'Checked Out' : 'Not Checked Out'}
                    </label>
                </div>
            </td>
        `;
        tableBody.appendChild(row);
    });
}

function handleStatusToggle(id, type) {
    const occupant = currentOccupantsData.find(o => (o.bookingOccupantId || o.BookingOccupantId) === id);
    if (!occupant) return;

    // Helper properties (since we might modify them)
    // We should normalize them on load, but for now we access safely
    // Actually, to update state, we need to know WHICH property to update.
    // Let's normalize data on fetch instead? Or just support both.

    // Simplest: Check both, set both (or sets standardized one)
    let isCheckedIn = occupant.isCheckedIn !== undefined ? occupant.isCheckedIn : occupant.IsCheckedIn;
    let isCheckedOut = occupant.isCheckedOut !== undefined ? occupant.isCheckedOut : occupant.IsCheckedOut;

    if (type === 'checkin') {
        const newState = !isCheckedIn;
        // Update both to be safe
        occupant.isCheckedIn = newState;
        occupant.IsCheckedIn = newState;

        if (!newState) {
            occupant.isCheckedOut = false;
            occupant.IsCheckedOut = false;
        }
        renderOccupantsTable();
    } else if (type === 'checkout') {
        if (!isCheckedOut) {
            const name = occupant.fullName || occupant.FullName;
            const confirmed = confirm(`Are you sure you want to check out ${name}? This will lock the status.`);
            if (confirmed) {
                occupant.isCheckedOut = true;
                occupant.IsCheckedOut = true;

                occupant.isCheckedIn = true;
                occupant.IsCheckedIn = true;
                renderOccupantsTable();
            } else {
                renderOccupantsTable();
            }
        } else {
            occupant.isCheckedOut = false;
            occupant.IsCheckedOut = false;
            renderOccupantsTable();
        }
    }
}

function saveOccupantStatuses() {
    const saveBtn = document.querySelector('#checkInCheckOutModal .btn-success');
    const originalText = saveBtn.textContent;
    saveBtn.disabled = true;
    saveBtn.textContent = 'Saving...';

    const updatePromises = currentOccupantsData.map(occupant => {
        const id = occupant.bookingOccupantId || occupant.BookingOccupantId;
        const isCheckedIn = occupant.isCheckedIn !== undefined ? occupant.isCheckedIn : occupant.IsCheckedIn;
        const isCheckedOut = occupant.isCheckedOut !== undefined ? occupant.isCheckedOut : occupant.IsCheckedOut;

        const payload = {
            occupantId: id,
            isCheckedIn: isCheckedIn,
            isCheckedOut: isCheckedOut
        };

        return fetch('/Reservations/UpdateOccupantStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        }).then(response => {
            const name = occupant.fullName || occupant.FullName;
            if (!response.ok) throw new Error(`Failed to update ${name}`);
            return response;
        });
    });

    Promise.all(updatePromises)
        .then(() => {
            alert('Occupant statuses saved successfully!');
            const modalEl = document.getElementById('checkInCheckOutModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            location.reload();
        })
        .catch(error => {
            console.error('Error saving:', error);
            alert('Some updates failed. Please try again.');
        })
        .finally(() => {
            saveBtn.disabled = false;
            saveBtn.textContent = originalText;
        });
}
